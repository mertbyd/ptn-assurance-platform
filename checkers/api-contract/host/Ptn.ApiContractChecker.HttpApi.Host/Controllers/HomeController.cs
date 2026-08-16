using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Ptn.ApiContractChecker.Controllers;

// islevi: Host kok istegini Swagger arayuzune yonlendirir.
// sistemdeki gorevi: Gelistiricinin checker HTTP yuzeyine tek adimda ulasmasini saglar.
public class HomeController : AbpController
{
    public ActionResult Index()
    {
        return Redirect("~/swagger");
    }
}
