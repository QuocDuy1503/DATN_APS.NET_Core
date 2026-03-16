using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DATN_TMS.Controllers;
using DATN_TMS.Services;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class ChamDiemBaoCaoController : BaseChamDiemBaoCaoController
    {
        public ChamDiemBaoCaoController(IChamDiemBaoCaoService service) : base(service) { }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessionRole = HttpContext.Session.GetString("Role");
            var isBCNKhoa = User?.Identity?.IsAuthenticated == true &&
                            (User.IsInRole("BCN_KHOA") || User.IsInRole("ADMIN"));
            var isBCNKhoaBySession = sessionRole == "BCN_KHOA" || sessionRole == "ADMIN";

            if (!isBCNKhoa && !isBCNKhoaBySession)
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "" });
                return;
            }
            base.OnActionExecuting(context);
        }
    }
}
