using Abp.Domain.Entities;
using System.Collections.Generic;

namespace KTGKNhom8.ToeicExams
{
    public class Passage : Entity<int>
    {
        public int ExamPartId { get; set; }
        public string Content { get; set; } // Nội dung đoạn văn
        
        public virtual ExamPart ExamPart { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
    }
}