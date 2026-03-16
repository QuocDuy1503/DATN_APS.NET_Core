using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DATN_TMS.Controllers;
using DATN_TMS.Services;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class ChamDiemBaoCaoController : BaseChamDiemBaoCaoController
    {
        public ChamDiemBaoCaoController(IChamDiemBaoCaoService service) : base(service) { }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessionRole = HttpContext.Session.GetString("Role");
            var isGV = User?.Identity?.IsAuthenticated == true &&
                       (User.IsInRole("GIANG_VIEN") || User.IsInRole("BO_MON") || User.IsInRole("BCN_KHOA"));
            var isGVBySession = sessionRole == "GIANG_VIEN" || sessionRole == "BO_MON" || sessionRole == "BCN_KHOA";

            if (!isGV && !isGVBySession)
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "" });
                return;
            }
            base.OnActionExecuting(context);
        }
    }
}
