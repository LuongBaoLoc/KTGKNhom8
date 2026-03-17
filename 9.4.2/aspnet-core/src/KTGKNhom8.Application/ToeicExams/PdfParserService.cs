using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Abp.Dependency;
using KTGKNhom8.ToeicExams.Dto;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace KTGKNhom8.ToeicExams
{
    public class PdfParserService : ITransientDependency
    {
        public ParsedExamDto ParseExamFromPdf(Stream fileStream)
        {
            // 1. Đọc toàn bộ text từ file PDF
            string fullText = ReadTextFromPdf(fileStream);

            // 2. Dùng lại thuật toán bóc tách y hệt như Word
            var examDto = new ParsedExamDto();

            examDto.Title = ExtractValue(fullText, @"\[EXAM_TITLE\](.*?)(?=\n|\r)");
            
            string timeStr = ExtractValue(fullText, @"\[EXAM_TIME\](.*?)(?=\n|\r)");
            if (int.TryParse(timeStr, out int time))
            {
                examDto.DurationMinutes = time;
            }

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

                examDto.Questions.Add(q);
            }

            if (examDto.Questions.Count == 0)
            {
                throw new Exception("Không tìm thấy câu hỏi nào. Vui lòng kiểm tra lại định dạng thẻ Tags trong file PDF.");
            }

            return examDto;
        }

        // Hàm lõi: Dùng PdfPig để rút trích chữ từ PDF
        private string ReadTextFromPdf(Stream stream)
        {
            var textBuilder = new StringBuilder();
            
            using (PdfDocument document = PdfDocument.Open(stream))
            {
                foreach (var page in document.GetPages())
                {
                    // Lấy text của từng trang và ghép lại
                    var pageText = ContentOrderTextExtractor.GetText(page, true);
                    textBuilder.AppendLine(pageText);
                }
            }
            
            return textBuilder.ToString();
        }

        private string ExtractValue(string text, string pattern)
        {
            var match = Regex.Match(text, pattern);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }
    }
}