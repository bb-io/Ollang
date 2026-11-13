namespace Apps.Ollang.Dtos;

public class PageMetaDto
{
    public int Page { get; set; }

    public int Take { get; set; }

    public int ItemCount { get; set; }

    public int PageCount { get; set; }

    public bool HasPreviousPage { get; set; }

    public bool HasNextPage { get; set; }
}
