using Abp.Application.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using KTGKNhom8.ToeicExams.Dto;

namespace KTGKNhom8.Toeic
{
    public interface IExamAppService : IApplicationService
    {
        Task SaveParsedExamAsync(ParsedExamDto input);
        Task CreateExamFromWordAsync(byte[] fileBytes, string fileName);
        Task<List<ExamListItemDto>> GetAllExamsAsync();
        Task<ExamDetailDto> GetExamDetailAsync(int examId);
        Task<ExamResultDto> SubmitExamAsync(SubmitExamDto input);
        Task<ExamResultDto> GetExamResultAsync(int attemptId);
        Task UpdateExamAsync(int examId, string title, string description);
        Task DeleteExamAsync(int examId);
    }
}