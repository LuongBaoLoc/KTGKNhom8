using System.Collections.Generic;

namespace KTGKNhom8.ToeicExams.Dto
{
    public class ExamDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int TimeLimit { get; set; }
        public string Description { get; set; }
        public List<ExamPartDto> Parts { get; set; } = new List<ExamPartDto>();
    }

    public class ExamPartDto
    {
        public int Id { get; set; }
        public int PartType { get; set; }
        public string Directions { get; set; }
        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public int QuestionNumber { get; set; }
        public string Content { get; set; }
        public bool IsShuffle { get; set; }
        public int? PassageId { get; set; }
        public string PassageContent { get; set; }
        public List<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
    }

    public class AnswerDto
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string Content { get; set; }
    }
}
