namespace EmployeeManagement.API.DTOs;

public class EmployeeQueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
    }

    public string? SearchTerm { get; set; }
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public string? EmploymentType { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "Id"; // "Id", "EmployeeCode", "FirstName", "LastName", "Email", "Department", "Designation", "Salary", "DateOfJoining"
    public string? SortOrder { get; set; } = "desc"; // "asc" or "desc"
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
