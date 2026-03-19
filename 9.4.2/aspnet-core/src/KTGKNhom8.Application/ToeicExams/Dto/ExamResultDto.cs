using System;
using System.Collections.Generic;

namespace KTGKNhom8.ToeicExams.Dto
{
    public class ExamResultDto
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public double ScorePercentage { get; set; }
        public int TimeSpent { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<QuestionResultDto> QuestionResults { get; set; } = new List<QuestionResultDto>();
    }

    public class QuestionResultDto
    {
        public int QuestionId { get; set; }
        public int QuestionNumber { get; set; }
        public string Content { get; set; }
        public string UserAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
    }
}
