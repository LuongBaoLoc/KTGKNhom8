using Abp.Domain.Entities;
using System.Collections.Generic;

namespace KTGKNhom8.ToeicExams
{
    public class ExamPart : Entity<int>
    {
        public int ExamId { get; set; }
        public int PartType { get; set; } // 5, 6, hoặc 7
        
        public virtual Exam Exam { get; set; }
        public virtual ICollection<Passage> Passages { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
    }
}