using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI; 
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KTGKNhom8.ToeicExams.Dto;
using KTGKNhom8.ToeicExams;

namespace KTGKNhom8.Toeic
{
    public class ExamAppService : ApplicationService, IExamAppService
    {
        private readonly IRepository<Exam> _examRepository;
        private readonly IRepository<ExamPart> _partRepository;
        private readonly IRepository<Question> _questionRepository;
        private readonly IRepository<Answer> _answerRepository;

        public ExamAppService(
            IRepository<Exam> examRepository,
            IRepository<ExamPart> partRepository,
            IRepository<Question> questionRepository,
            IRepository<Answer> answerRepository)
        {
            _examRepository = examRepository;
            _partRepository = partRepository;
            _questionRepository = questionRepository;
            _answerRepository = answerRepository;
        }

        // 1. HÀM MỚI THÊM: Dành cho Controller gọi sau khi bóc tách xong Word/PDF
        public async Task SaveParsedExamAsync(ParsedExamDto input)
        {
            // 1. Lưu thông tin Đề thi vào bảng Exams
            var examId = await _examRepository.InsertAndGetIdAsync(new Exam
            {
                Title = string.IsNullOrEmpty(input.Title) ? "Đề thi TOEIC" : input.Title,
                DurationMinutes = input.DurationMinutes,
                Description = "Được import tự động từ Web"
            });

            // 2. Tạo một ExamPart mặc định (Part 5) để chứa các câu hỏi này
            var partId = await _partRepository.InsertAndGetIdAsync(new ExamPart 
            { 
                ExamId = examId, 
                PartType = 5 
            });

            // 3. Lưu danh sách Câu hỏi và Đáp án
            foreach (var qDto in input.Questions)
            {
                var questionId = await _questionRepository.InsertAndGetIdAsync(new Question
                {
                    ExamPartId = partId,
                    QuestionNumber = qDto.QuestionNumber,
                    Content = qDto.Content,
                    IsShuffle = qDto.IsShuffle
                });

                // Lưu 4 đáp án A, B, C, D (Kiểm tra IsCorrect dựa vào CorrectAnswer)
                await _answerRepository.InsertAsync(new Answer { QuestionId = questionId, Label = "A", Content = qDto.OptionA, IsCorrect = (qDto.CorrectAnswer == "A") });
                await _answerRepository.InsertAsync(new Answer { QuestionId = questionId, Label = "B", Content = qDto.OptionB, IsCorrect = (qDto.CorrectAnswer == "B") });
                await _answerRepository.InsertAsync(new Answer { QuestionId = questionId, Label = "C", Content = qDto.OptionC, IsCorrect = (qDto.CorrectAnswer == "C") });
                await _answerRepository.InsertAsync(new Answer { QuestionId = questionId, Label = "D", Content = qDto.OptionD, IsCorrect = (qDto.CorrectAnswer == "D") });
            }
        }

        // 2. HÀM CŨ: Giữ lại để code không báo lỗi với Interface
        public async Task CreateExamFromWordAsync(byte[] fileBytes, string fileName)
        {
            using (var memoryStream = new MemoryStream(fileBytes))
            {
                using (var wordDoc = WordprocessingDocument.Open(memoryStream, false))
                {
                    var body = wordDoc.MainDocumentPart.Document.Body;
                    string fullText = string.Join("\n", body.Elements<Paragraph>().Select(p => p.InnerText));

                    var titleMatch = Regex.Match(fullText, @"\[EXAM_TITLE\](.*)");
                    if (!titleMatch.Success) throw new UserFriendlyException("Lỗi Format: Thiếu thẻ [EXAM_TITLE]");

                    var timeMatch = Regex.Match(fullText, @"\[EXAM_TIME\]\s*(\d+)");
                    if (!timeMatch.Success) throw new UserFriendlyException("Lỗi Format: Thiếu thẻ [EXAM_TIME]");

                    var examId = await _examRepository.InsertAndGetIdAsync(new Exam
                    {
                        Title = titleMatch.Groups[1].Value.Trim(),
                        DurationMinutes = int.Parse(timeMatch.Groups[1].Value.Trim()),
                        Description = "Được import từ file: " + fileName
                    });

                    var part5Match = Regex.Match(fullText, @"\[PART:5\](.*?)(\[PART:6\]|$)", RegexOptions.Singleline);
                    if (part5Match.Success)
                    {
                        var partId = await _partRepository.InsertAndGetIdAsync(new ExamPart { ExamId = examId, PartType = 5 });
                        var qMatches = Regex.Matches(part5Match.Value, @"\[Q:(\d+)\](.*?)\[KEY:([A-D])\]", RegexOptions.Singleline);
                        
                        foreach (Match qMatch in qMatches)
                        {
                            var qNumber = int.Parse(qMatch.Groups[1].Value);
                            var qBody = qMatch.Groups[2].Value; 
                            var correctKey = qMatch.Groups[3].Value.Trim().ToUpper(); 

                            var qContentMatch = Regex.Match(qBody, @"^(.*?)(?=\[A\])", RegexOptions.Singleline);
                            string qText = qContentMatch.Success ? qContentMatch.Groups[1].Value.Trim() : "Lỗi đọc nội dung";

                            var questionId = await _questionRepository.InsertAndGetIdAsync(new Question
                            {
                                ExamPartId = partId,
                                QuestionNumber = qNumber,
                                Content = qText,
                                IsShuffle = true 
                            });

                            string[] labels = { "A", "B", "C", "D" };
                            foreach (var label in labels)
                            {
                                var ansMatch = Regex.Match(qBody, $@"\[{label}\](.*?)(?=\[[A-D]\]|$)", RegexOptions.Singleline);
                                if (!ansMatch.Success) throw new UserFriendlyException($"Lỗi Format: Câu {qNumber} thiếu đáp án [{label}]");

                                await _answerRepository.InsertAsync(new Answer
                                {
                                    QuestionId = questionId,
                                    Label = label,
                                    Content = ansMatch.Groups[1].Value.Trim(),
                                    IsCorrect = (label == correctKey)
                                });
                            }
                        }
                    }
                }
            }
        }
    }
}