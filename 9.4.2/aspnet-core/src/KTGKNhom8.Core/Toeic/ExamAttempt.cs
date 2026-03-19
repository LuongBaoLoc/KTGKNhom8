using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;

namespace KTGKNHOM8.Toeic
{
    public class ExamAttempt : FullAuditedEntity<int>
    {
        public int ExamId { get; set; }
        public long UserId { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int TimeSpent { get; set; }
        
        public virtual Exam Exam { get; set; }
        public virtual ICollection<ExamAttemptAnswer> Answers { get; set; }
    }

    public class ExamAttemptAnswer : Entity<int>
    {
        public int ExamAttemptId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedAnswerId { get; set; }
        public bool IsCorrect { get; set; }

        public virtual ExamAttempt ExamAttempt { get; set; }
        public virtual Question Question { get; set; }
        public virtual Answer SelectedAnswer { get; set; }
    }
}
