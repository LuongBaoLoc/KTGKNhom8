using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KTGKNhom8.ToeicExams.Dto;
using KTGKNHOM8.Toeic;

namespace KTGKNhom8.Toeic
{
    public class ExamAppService : ApplicationService, IExamAppService
    {
        private readonly IRepository<Exam> _examRepository;
        private readonly IRepository<ExamPart> _partRepository;
        private readonly IRepository<Question> _questionRepository;
        private readonly IRepository<Answer> _answerRepository;
        private readonly IRepository<ExamAttempt> _attemptRepository;
        private readonly IRepository<ExamAttemptAnswer> _attemptAnswerRepository;

        public ExamAppService(
            IRepository<Exam> examRepository,
            IRepository<ExamPart> partRepository,
            IRepository<Question> questionRepository,
            IRepository<Answer> answerRepository,
            IRepository<ExamAttempt> attemptRepository,
            IRepository<ExamAttemptAnswer> attemptAnswerRepository)
        {
            _examRepository = examRepository;
            _partRepository = partRepository;
            _questionRepository = questionRepository;
            _answerRepository = answerRepository;
            _attemptRepository = attemptRepository;
            _attemptAnswerRepository = attemptAnswerRepository;
        }

        public async Task SaveParsedExamAsync(ParsedExamDto input)
        {
            var examId = await _examRepository.InsertAndGetIdAsync(new Exam
            {
                Title = string.IsNullOrEmpty(input.Title) ? "Đề thi TOEIC" : input.Title,
                TimeLimit = input.DurationMinutes,
                Description = "Được import tự động từ Web"
            });

            var partId = await _partRepository.InsertAndGetIdAsync(new ExamPart
            {
                ExamId = examId,
                PartType = 5
            });

            foreach (var qDto in input.Questions)
            {
                var questionId = await _questionRepository.InsertAndGetIdAsync(new Question
                {
                    ExamPartId = partId,
                    QuestionNumber = qDto.QuestionNumber,
                    Content = qDto.Content,
                    IsShuffle = qDto.IsShuffle
                });

                await _answerRepository.InsertAsync(new Answer { QuestionId = questionId, Label = "A", Content = qDto.OptionA, IsCorrect = (qDto.CorrectAnswer == "A") });
                await _answerRepository.InsertAsync(new Answer { QuestionId = questionId, Label = "B", Content = qDto.OptionB, IsCorrect = (qDto.CorrectAnswer == "B") });
                await _answerRepository.InsertAsync(new Answer { QuestionId = questionId, Label = "C", Content = qDto.OptionC, IsCorrect = (qDto.CorrectAnswer == "C") });
                await _answerRepository.InsertAsync(new Answer { QuestionId = questionId, Label = "D", Content = qDto.OptionD, IsCorrect = (qDto.CorrectAnswer == "D") });
            }
        }

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
                        TimeLimit = int.Parse(timeMatch.Groups[1].Value.Trim()),
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

        public async Task<List<ExamListItemDto>> GetAllExamsAsync()
        {
            var exams = await _examRepository.GetAllListAsync();
            var result = new List<ExamListItemDto>();

            foreach (var exam in exams)
            {
                var totalQuestions = await _questionRepository.CountAsync(q => q.ExamPart.ExamId == exam.Id);

                result.Add(new ExamListItemDto
                {
                    Id = exam.Id,
                    Title = exam.Title,
                    TimeLimit = exam.TimeLimit,
                    Description = exam.Description,
                    TotalQuestions = totalQuestions
                });
            }

            return result;
        }

        public async Task<ExamDetailDto> GetExamDetailAsync(int examId)
        {
            var exam = await _examRepository.FirstOrDefaultAsync(examId);
            if (exam == null)
                throw new UserFriendlyException($"Không tìm thấy đề thi {examId}");

            var examParts = (await _partRepository.GetAllListAsync()).Where(p => p.ExamId == examId).ToList();
            var partsDto = new List<ExamPartDto>();

            foreach (var part in examParts)
            {
                var questions = (await _questionRepository.GetAllListAsync()).Where(q => q.ExamPartId == part.Id).ToList();
                var questionsDto = new List<QuestionDto>();

                foreach (var question in questions)
                {
                    var answers = (await _answerRepository.GetAllListAsync()).Where(a => a.QuestionId == question.Id).ToList();
                    var answersDto = answers.Select(a => new AnswerDto
                    {
                        Id = a.Id,
                        Label = a.Label,
                        Content = a.Content
                    }).ToList();

                    questionsDto.Add(new QuestionDto
                    {
                        Id = question.Id,
                        QuestionNumber = question.QuestionNumber,
                        Content = question.Content,
                        IsShuffle = question.IsShuffle,
                        Answers = answersDto
                    });
                }

                partsDto.Add(new ExamPartDto
                {
                    Id = part.Id,
                    PartType = part.PartType,
                    Questions = questionsDto
                });
            }

            return new ExamDetailDto
            {
                Id = exam.Id,
                Title = exam.Title,
                TimeLimit = exam.TimeLimit,
                Description = exam.Description,
                Parts = partsDto
            };
        }

        public async Task<ExamResultDto> SubmitExamAsync(SubmitExamDto input)
        {
            var exam = await _examRepository.FirstOrDefaultAsync(input.ExamId);
            if (exam == null)
                throw new UserFriendlyException("Không tìm thấy đề thi");

            // Fix: Tránh lỗi null nếu không có câu trả lời nào được chọn
            input.Answers = input.Answers ?? new List<UserAnswerDto>();

            var allQuestions = (await _questionRepository.GetAllListAsync()).ToList();
            var allAnswers = (await _answerRepository.GetAllListAsync()).ToList();

            // Fix: Tính tổng số câu hỏi THỰC TẾ của đề thi
            var totalExamQuestions = await _questionRepository.CountAsync(q => q.ExamPart.ExamId == input.ExamId);

            int correctCount = 0;
            var questionResults = new List<QuestionResultDto>();

            foreach (var userAnswer in input.Answers)
            {
                var question = allQuestions.FirstOrDefault(q => q.Id == userAnswer.QuestionId);
                var selectedAnswer = allAnswers.FirstOrDefault(a => a.Id == userAnswer.SelectedAnswerId);
                var correctAnswer = allAnswers.FirstOrDefault(a => a.QuestionId == userAnswer.QuestionId && a.IsCorrect);

                bool isCorrect = selectedAnswer?.IsCorrect ?? false;
                if (isCorrect) correctCount++;

                questionResults.Add(new QuestionResultDto
                {
                    QuestionId = userAnswer.QuestionId,
                    QuestionNumber = question?.QuestionNumber ?? 0,
                    Content = question?.Content,
                    UserAnswer = selectedAnswer?.Label,
                    CorrectAnswer = correctAnswer?.Label,
                    IsCorrect = isCorrect
                });
            }

            var attempt = new ExamAttempt
            {
                ExamId = input.ExamId,
                UserId = AbpSession.UserId ?? 1,
                TotalQuestions = totalExamQuestions, // Đã fix: Gán tổng câu thực tế thay vì mảng đáp án
                CorrectAnswers = correctCount,
                TimeSpent = input.TimeSpent
            };

            var attemptId = await _attemptRepository.InsertAndGetIdAsync(attempt);

            foreach (var userAnswer in input.Answers)
            {
                var selectedAnswer = allAnswers.FirstOrDefault(a => a.Id == userAnswer.SelectedAnswerId);
                await _attemptAnswerRepository.InsertAsync(new ExamAttemptAnswer
                {
                    ExamAttemptId = attemptId,
                    QuestionId = userAnswer.QuestionId,
                    SelectedAnswerId = userAnswer.SelectedAnswerId,
                    IsCorrect = selectedAnswer?.IsCorrect ?? false
                });
            }

            return new ExamResultDto
            {
                Id = attemptId,
                ExamId = input.ExamId,
                ExamTitle = exam.Title,
                CorrectAnswers = correctCount,
                TotalQuestions = totalExamQuestions,
                // Fix: Ngăn lỗi chia cho 0
                ScorePercentage = totalExamQuestions == 0 ? 0 : (double)correctCount / totalExamQuestions * 100,
                TimeSpent = input.TimeSpent,
                CreatedAt = DateTime.Now,
                QuestionResults = questionResults
            };
        }

        public async Task<ExamResultDto> GetExamResultAsync(int attemptId)
        {
            var attempt = await _attemptRepository.FirstOrDefaultAsync(attemptId);
            if (attempt == null)
                throw new UserFriendlyException("Không tìm thấy kết quả thi");

            var exam = await _examRepository.FirstOrDefaultAsync(attempt.ExamId);
            var attemptAnswers = (await _attemptAnswerRepository.GetAllListAsync()).Where(aa => aa.ExamAttemptId == attemptId).ToList();
            var allQuestions = (await _questionRepository.GetAllListAsync()).ToList();
            var allAnswers = (await _answerRepository.GetAllListAsync()).ToList();

            var questionResults = new List<QuestionResultDto>();
            foreach (var aa in attemptAnswers)
            {
                var question = allQuestions.FirstOrDefault(q => q.Id == aa.QuestionId);
                var selectedAnswer = allAnswers.FirstOrDefault(a => a.Id == aa.SelectedAnswerId);
                var correctAnswer = allAnswers.FirstOrDefault(a => a.QuestionId == aa.QuestionId && a.IsCorrect);

                questionResults.Add(new QuestionResultDto
                {
                    QuestionId = aa.QuestionId,
                    QuestionNumber = question?.QuestionNumber ?? 0,
                    Content = question?.Content,
                    UserAnswer = selectedAnswer?.Label,
                    CorrectAnswer = correctAnswer?.Label,
                    IsCorrect = aa.IsCorrect
                });
            }

            return new ExamResultDto
            {
                Id = attemptId,
                ExamId = attempt.ExamId,
                ExamTitle = exam?.Title,
                CorrectAnswers = attempt.CorrectAnswers,
                TotalQuestions = attempt.TotalQuestions,
                // Fix: Ngăn lỗi chia cho 0
                ScorePercentage = attempt.TotalQuestions == 0 ? 0 : (double)attempt.CorrectAnswers / attempt.TotalQuestions * 100,
                TimeSpent = attempt.TimeSpent,
                CreatedAt = attempt.CreationTime,
                QuestionResults = questionResults
            };
        }

        public async Task UpdateExamAsync(int examId, string title, string description)
        {
            var exam = await _examRepository.FirstOrDefaultAsync(examId);
            if (exam == null)
                throw new UserFriendlyException("Không tìm thấy đề thi");

            exam.Title = title;
            exam.Description = description;
            await _examRepository.UpdateAsync(exam);
        }

        public async Task DeleteExamAsync(int examId)
        {
            await _examRepository.DeleteAsync(examId);
        }
    }
}