using Abp.Application.Services;
using System.Threading.Tasks;
using KTGKNhom8.ToeicExams.Dto;

namespace KTGKNhom8.Toeic
{
    public interface IExamAppService : IApplicationService
    {
        Task SaveParsedExamAsync(ParsedExamDto input);
        // Nhận file dưới dạng mảng byte để tránh phụ thuộc vào IFormFile của tầng Web
        Task CreateExamFromWordAsync(byte[] fileBytes, string fileName);
        
    }
}