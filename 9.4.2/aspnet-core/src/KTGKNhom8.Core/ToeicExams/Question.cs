using Abp.Domain.Entities;

namespace KTGKNhom8.ToeicExams
{
    public class Question : Entity<int>
    {
        public int ExamPartId { get; set; }
        public int? PassageId { get; set; } // Part 5 không có đoạn văn nên cho phép NULL
        public int QuestionNumber { get; set; } // Ví dụ: 101, 131
        public string Content { get; set; }
        
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        
        public string CorrectAnswer { get; set; } // A, B, C, D
        public bool IsShuffle { get; set; }

        public virtual ExamPart ExamPart { get; set; }
        public virtual Passage Passage { get; set; }
    }
}