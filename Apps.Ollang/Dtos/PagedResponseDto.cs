namespace Apps.Ollang.Dtos;

public class PagedResponseDto<T>
{
    public List<T> Data { get; set; } = [];

    public PageMetaDto Meta { get; set; } = new();
}
