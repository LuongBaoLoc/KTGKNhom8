using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Collections.Generic;
using KTGKNhom8.Controllers;
using KTGKNhom8.ToeicExams;
using KTGKNhom8.ToeicExams.Dto;
using KTGKNHOM8.Toeic;
using KTGKNhom8.Toeic;

namespace KTGKNhom8.Web.Controllers
{
    public class ExamController : KTGKNhom8ControllerBase
    {
        private readonly WordParserService _wordParserService;
        private readonly PdfParserService _pdfParserService;
        private readonly IExamAppService _examAppService;

        public ExamController(
            WordParserService wordParserService,
            PdfParserService pdfParserService,
            IExamAppService examAppService)
        {
            _wordParserService = wordParserService;
            _pdfParserService = pdfParserService;
            _examAppService = examAppService;
        }

        public async Task<IActionResult> Index()
        {
            var exams = await _examAppService.GetAllExamsAsync();
            return View(exams);
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessUpload(IFormFile fileUpload)
        {
            if (fileUpload == null || fileUpload.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file hợp lệ.";
                return RedirectToAction("Upload");
            }

            try
            {
                string fileExtension = Path.GetExtension(fileUpload.FileName).ToLower();
                ParsedExamDto parsedExam = null;

                using (var stream = fileUpload.OpenReadStream())
                {
                    if (fileExtension == ".docx")
                    {
                        parsedExam = _wordParserService.ParseExamFromWord(stream);
                    }
                    else if (fileExtension == ".pdf")
                    {
                        parsedExam = _pdfParserService.ParseExamFromPdf(stream);
                    }
                    else if (fileExtension == ".png" || fileExtension == ".jpeg" || fileExtension == ".jpg")
                    {
                        TempData["ErrorMessage"] = "Hệ thống đang phát triển tính năng đọc ảnh (AI OCR). Vui lòng dùng Word hoặc PDF trước nhé!";
                        return RedirectToAction("Upload");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Hệ thống chỉ hỗ trợ bóc tách file Word (.docx) và PDF (.pdf).";
                        return RedirectToAction("Upload");
                    }

                    if (parsedExam != null)
                    {
                        await _examAppService.SaveParsedExamAsync(parsedExam);
                        TempData["SuccessMessage"] = $"Thành công! Đã bóc tách và lưu đề thi '{parsedExam.Title}' (Gồm {parsedExam.Questions.Count} câu hỏi).";
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi xử lý file: " + ex.Message;
                return RedirectToAction("Upload");
            }
        }

        public async Task<IActionResult> TakeExam(int id)
        {
            try
            {
                var exam = await _examAppService.GetExamDetailAsync(id);
                return View(exam);
            }
            catch
            {
                TempData["ErrorMessage"] = "Không tìm thấy đề thi";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitExam(SubmitExamDto input)
        {
            try
            {
                var result = await _examAppService.SubmitExamAsync(input);
                return RedirectToAction("ViewResult", new { id = result.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi submit bài thi: " + ex.Message;
                return RedirectToAction("TakeExam", new { id = input.ExamId });
            }
        }

        public async Task<IActionResult> ViewResult(int id)
        {
            try
            {
                var result = await _examAppService.GetExamResultAsync(id);
                return View(result);
            }
            catch
            {
                TempData["ErrorMessage"] = "Không tìm thấy kết quả thi";
                return RedirectToAction("Index");
            }
        }
    }
}
