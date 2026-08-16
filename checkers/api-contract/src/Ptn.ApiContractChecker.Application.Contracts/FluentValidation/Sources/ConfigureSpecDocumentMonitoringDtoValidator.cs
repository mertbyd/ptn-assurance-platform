using FluentValidation;
using Ptn.ApiContractChecker.Constants.Sources;
using Ptn.ApiContractChecker.Dtos.Sources;
using Ptn.ApiContractChecker.ExceptionCodes.Sources;

namespace Ptn.ApiContractChecker.FluentValidation.Sources;

// islevi: Izleme isteginin aralik alanini zorunluluk ve sinir acisindan dogrular.
// sistemdeki gorevi: Entity invariantina dusmeden once cagirana kararli sinir hatasi dondurur; izleme kapatilirken aralik istemez.
public class ConfigureSpecDocumentMonitoringDtoValidator : AbstractValidator<ConfigureSpecDocumentMonitoringDto>
{
    public ConfigureSpecDocumentMonitoringDtoValidator()
    {
        // Aralik yalniz izleme acilirken anlamlidir; kapatma istegi aralik tasimak zorunda degildir.
        RuleFor(monitoring => monitoring.CheckIntervalMinutes)
            .NotNull().WithMessage(SpecSourceExceptionCodes.Validation.MonitoringIntervalRequired)
            .InclusiveBetween(
                SpecDocumentConsts.MinCheckIntervalMinutes,
                SpecDocumentConsts.MaxCheckIntervalMinutes)
            .WithMessage(SpecSourceExceptionCodes.Validation.MonitoringIntervalOutOfRange)
            .When(monitoring => monitoring.IsMonitored);
    }
}
