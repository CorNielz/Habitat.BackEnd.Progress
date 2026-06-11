namespace Habitat.BackEnd.Progress.Application.Common;

public sealed record PaginationRequest(int Page = 1, int PageSize = 20)
{
    public int SafePage => Page < 1 ? 1 : Page;
    public int SafePageSize => PageSize switch
    {
        < 1 => 20,
        > 100 => 100,
        _ => PageSize
    };

    public int Offset => (SafePage - 1) * SafePageSize;
}
