using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DATN_TMS.Controllers;
using DATN_TMS.Services;

namespace DATN_TMS.Areas.GV_BoMon.Controllers
{
    [Area("GV_BoMon")]
    public class ChamDiemBaoCaoController : BaseChamDiemBaoCaoController
    {
        public ChamDiemBaoCaoController(IChamDiemBaoCaoService service) : base(service) { }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessionRole = HttpContext.Session.GetString("Role");
            var isBoMon = User?.Identity?.IsAuthenticated == true &&
                          (User.IsInRole("BO_MON") || User.IsInRole("BCN_KHOA") || User.IsInRole("ADMIN"));
            var isBoMonBySession = sessionRole == "BO_MON" || sessionRole == "BCN_KHOA" || sessionRole == "ADMIN";

            if (!isBoMon && !isBoMonBySession)
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "" });
                return;
            }
            base.OnActionExecuting(context);
        }
    }
}
