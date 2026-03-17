using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;

namespace KTGKNhom8.ToeicExams
{
    public class Exam : FullAuditedEntity<int>
    {
        public string Title { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; }
        public bool IsPublished { get; set; }

        public virtual ICollection<ExamPart> Parts { get; set; }
    }
}