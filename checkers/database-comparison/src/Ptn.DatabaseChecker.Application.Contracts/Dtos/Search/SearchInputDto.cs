using Volo.Abp.Application.Dtos;

namespace Ptn.DatabaseChecker.Dtos.Search;

// islevi: Metin aramasi yapan endpoint'lerin ortak input kontratini tasir.
// sistemdeki gorevi: Keyword ve sayfalama alanlari her search endpoint'inde tekrar tanimlanmaz.
public class SearchInputDto : PagedAndSortedResultRequestDto
{
    // Aranacak serbest metin; bos ise ilgili repository tum dokumanlari sayfali doner.
    public string? Keyword { get; set; }
}
