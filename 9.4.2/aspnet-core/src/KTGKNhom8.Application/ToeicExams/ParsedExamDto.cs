using System.Collections.Generic;

namespace KTGKNhom8.ToeicExams.Dto
{
    public class ParsedExamDto
    {
        public string Title { get; set; }
        public int DurationMinutes { get; set; }
        public List<ParsedQuestionDto> Questions { get; set; } = new List<ParsedQuestionDto>();
    }

    public class ParsedQuestionDto
    {
        public int QuestionNumber { get; set; }
        public string Content { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsShuffle { get; set; }
    }
}