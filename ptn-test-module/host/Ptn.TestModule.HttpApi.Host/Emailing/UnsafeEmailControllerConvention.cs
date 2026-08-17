using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Piton.Emailing.Controllers.Emailing;

namespace Ptn.TestModule.Emailing;

// islevi: Piton.Emailing paketindeki yetkisiz ham gonderim controller'ini composition host yuzeyinden cikarir.
// sistemdeki gorevi: Yetkili provider ve sablon controller'lari compose edilirken anonim e-posta gonderimini kapali tutar.
public sealed class UnsafeEmailControllerConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (controller.ControllerType == typeof(EmailController))
        {
            controller.Actions.Clear();
        }
    }
}
