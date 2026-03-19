using System.Collections.Generic;

namespace KTGKNhom8.ToeicExams.Dto
{
    public class SubmitExamDto
    {
        public int ExamId { get; set; }
        public int TimeSpent { get; set; }
        public List<UserAnswerDto> Answers { get; set; } = new List<UserAnswerDto>();
    }

    public class UserAnswerDto
    {
        public int QuestionId { get; set; }
        public int SelectedAnswerId { get; set; }
    }
}
