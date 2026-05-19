namespace Mantenimiento.Application.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Items    { get; init; } = [];
    public int TotalCount          { get; init; }
    public int Page                { get; init; }
    public int PageSize            { get; init; }
    public int TotalPages          => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage    => Page > 1;
    public bool HasNextPage        => Page < TotalPages;

    public static PagedResult<T> Create(IEnumerable<T> source, int page, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page     = Math.Max(page, 1);
        var list = source.ToList();
        return new PagedResult<T>
        {
            Items      = list.Skip((page - 1) * pageSize).Take(pageSize),
            TotalCount = list.Count,
            Page       = page,
            PageSize   = pageSize
        };
    }
}
