using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using KTGKNhom8.ToeicExams.Dto;
using Abp.Dependency;
using System.Collections.Generic;

namespace KTGKNhom8.ToeicExams
{
    public class WordParserService : ITransientDependency
    {
        public ParsedExamDto ParseExamFromWord(Stream fileStream)
        {
            // 1. Đọc toàn bộ text từ file Word
            string fullText = ReadTextFromWord(fileStream);

            // 2. Tiến hành bóc tách
            var examDto = new ParsedExamDto();

            // Tìm tiêu đề và thời gian
            examDto.Title = ExtractValue(fullText, @"\[EXAM_TITLE\](.*?)(?=\n|\r)");
            
            string timeStr = ExtractValue(fullText, @"\[EXAM_TIME\](.*?)(?=\n|\r)");
            if (int.TryParse(timeStr, out int time))
            {
                examDto.DurationMinutes = time;
            }

            // ==========================================
            // 3. ĐỌC CÁC ĐOẠN VĂN (PASSAGE)
            // ==========================================
            string passagePattern = @"\[PASSAGE_START\](.*?)\[PASSAGE_END\]";
            var passageMatches = Regex.Matches(fullText, passagePattern, RegexOptions.Singleline);
            
            foreach (Match match in passageMatches)
            {
                string passageContent = match.Groups[1].Value.Trim();
                
                // LƯU Ý: Nếu ParsedExamDto của bạn có danh sách Passages, hãy bỏ comment dòng dưới
                // examDto.Passages.Add(new ParsedPassageDto { Content = passageContent });
            }

            // ==========================================
            // 4. ĐỌC CÂU HỎI & FIX LỖI PART 6 (CÂU TRỐNG)
            // ==========================================
            // Regex này tìm từ [Q:số] cho đến [KEY:đáp_án]
            string questionPattern = @"\[Q:(\d+)\](?:(?:\s*\[SHUFFLE:\s*(TRUE|FALSE)\])?)(.*?)\[A\](.*?)\[B\](.*?)\[C\](.*?)\[D\](.*?)\[KEY:([A-D])\]";
            var matches = Regex.Matches(fullText, questionPattern, RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                var q = new ParsedQuestionDto
                {
                    QuestionNumber = int.Parse(match.Groups[1].Value.Trim()),
                    IsShuffle = match.Groups[2].Value.Trim().ToUpper() == "TRUE",
                    Content = match.Groups[3].Value.Trim(),
                    OptionA = match.Groups[4].Value.Trim(),
                    OptionB = match.Groups[5].Value.Trim(),
                    OptionC = match.Groups[6].Value.Trim(),
                    OptionD = match.Groups[7].Value.Trim(),
                    CorrectAnswer = match.Groups[8].Value.Trim()
                };

                // FIX LỖI 131: Nếu là Part 6 (không có câu hỏi), tự động gán chữ "Điền từ vào chỗ trống"
                if (string.IsNullOrEmpty(q.Content))
                {
                    q.Content = "Điền từ vào chỗ trống";
                }

                // Validate dữ liệu (Chỉ kiểm tra đáp án, nếu thiếu đáp án thì báo lỗi)
                if (string.IsNullOrEmpty(q.CorrectAnswer))
                {
                    throw new Exception($"Câu hỏi số {q.QuestionNumber} đang bị thiếu Đáp án (KEY). Vui lòng kiểm tra lại file Word.");
                }

                // Thêm câu hỏi vào danh sách
                examDto.Questions.Add(q);
            }

            return examDto;
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ ĐỌC FILE VÀ CẮT CHUỖI
        // ==========================================
        private string ReadTextFromWord(Stream stream)
        {
            var sb = new StringBuilder();
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream, false))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;
                foreach (var para in body.Elements<Paragraph>())
                {
                    sb.AppendLine(para.InnerText);
                }
            }
            return sb.ToString();
        }

        private string ExtractValue(string text, string pattern)
        {
            var match = Regex.Match(text, pattern);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }
    }
}