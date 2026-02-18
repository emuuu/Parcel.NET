namespace ParcelNET.Docs.Models;

public sealed class ApiTypeDoc
{
    public required string TypeName { get; set; }
    public required string FullName { get; set; }
    public List<ApiMember> Members { get; set; } = [];
}

public sealed class ApiMember
{
    public required string Name { get; set; }
    public required string Kind { get; set; }
    public required string Type { get; set; }
    public string? Default { get; set; }
    public string? Description { get; set; }
}
