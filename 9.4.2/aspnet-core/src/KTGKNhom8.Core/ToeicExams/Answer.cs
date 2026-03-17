using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace KTGKNhom8.ToeicExams 
{
    public class Answer : Entity<int>
    {
        public int QuestionId { get; set; }
        
        public string Label { get; set; } // Chứa chữ A, B, C hoặc D
        
        public string Content { get; set; } // Nội dung của đáp án
        
        public bool IsCorrect { get; set; } // Đánh dấu đáp án đúng (True/False)
    }
}