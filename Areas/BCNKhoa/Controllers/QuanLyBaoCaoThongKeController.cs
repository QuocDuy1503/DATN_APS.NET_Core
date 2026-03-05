using System.Collections.Generic;
using System.Linq;
using DATN_TMS.Areas.BCNKhoa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyBaoCaoThongKeController : Controller
    {
        [HttpGet]
        public IActionResult Index(string? dotId)
        {
            var dotOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "dot1", Text = "HK2 - 2024-2025" },
                new SelectListItem { Value = "dot2", Text = "HK1 - 2024-2025" },
                new SelectListItem { Value = "dot3", Text = "HK2 - 2023-2024" }
            };

            var metrics = new Dictionary<string, (int sv, int detai, int task, int done)>
            {
                ["dot1"] = (sv: 150, detai: 45, task: 320, done: 260),
                ["dot2"] = (sv: 200, detai: 60, task: 410, done: 360),
                ["dot3"] = (sv: 120, detai: 40, task: 280, done: 210)
            };

            var summariesByDot = new Dictionary<string, List<DeTaiSummaryItem>>
            {
                ["dot1"] = new List<DeTaiSummaryItem>
                {
                    new DeTaiSummaryItem { TenDeTai = "RFID Library", SinhVien = "Nguyễn Văn A, Trần Thị B", TaskDone = 12, TaskTotal = 15 },
                    new DeTaiSummaryItem { TenDeTai = "AI X-Quang", SinhVien = "Lê Văn C", TaskDone = 9, TaskTotal = 12 },
                    new DeTaiSummaryItem { TenDeTai = "Cloud Admin", SinhVien = "Phạm Thị D, Hoàng Văn E", TaskDone = 7, TaskTotal = 10 },
                    new DeTaiSummaryItem { TenDeTai = "E-Commerce", SinhVien = "Vũ Thị F", TaskDone = 11, TaskTotal = 14 },
                    new DeTaiSummaryItem { TenDeTai = "DDoS ML", SinhVien = "Đặng Văn G, Bùi Thị H", TaskDone = 8, TaskTotal = 13 }
                },
                ["dot2"] = new List<DeTaiSummaryItem>
                {
                    new DeTaiSummaryItem { TenDeTai = "Smart Home IoT", SinhVien = "Đỗ Văn I", TaskDone = 14, TaskTotal = 16 },
                    new DeTaiSummaryItem { TenDeTai = "Blockchain Payment", SinhVien = "Hồ Thị K, Ngô Văn L", TaskDone = 10, TaskTotal = 12 },
                    new DeTaiSummaryItem { TenDeTai = "AR Navigation", SinhVien = "Dương Thị M", TaskDone = 9, TaskTotal = 11 },
                    new DeTaiSummaryItem { TenDeTai = "Medical AI", SinhVien = "Lý Văn N, Trương Thị O", TaskDone = 13, TaskTotal = 15 }
                },
                ["dot3"] = new List<DeTaiSummaryItem>
                {
                    new DeTaiSummaryItem { TenDeTai = "Chatbot CS", SinhVien = "Nguyễn Văn P", TaskDone = 6, TaskTotal = 10 },
                    new DeTaiSummaryItem { TenDeTai = "Data Warehouse", SinhVien = "Trần Thị Q, Lê Văn R", TaskDone = 9, TaskTotal = 13 },
                    new DeTaiSummaryItem { TenDeTai = "Mobile Banking", SinhVien = "Phạm Thị S", TaskDone = 7, TaskTotal = 12 }
                }
            };

            var selectedId = dotId ?? dotOptions.First().Value;
            var metric = metrics.TryGetValue(selectedId, out var m) ? m : metrics.First().Value;
            var summaries = summariesByDot.TryGetValue(selectedId, out var list) ? list : summariesByDot.First().Value;

            var model = new BaoCaoThongKeViewModel
            {
                SelectedDotId = selectedId,
                DotOptions = dotOptions,
                TongSinhVien = metric.sv,
                TongDeTai = metric.detai,
                TongTask = metric.task,
                TienDoPhanTram = metric.task == 0 ? 0 : (double)metric.done / metric.task * 100,
                DeTaiSummaries = summaries
            };

            return View(model);
        }
    }
}
