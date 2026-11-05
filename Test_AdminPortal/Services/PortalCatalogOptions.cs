namespace Test_AdminPortal.Services;

public sealed class PortalCatalogOptions
{
    public List<PortalToolOptions> Tools { get; set; } = [];

    public void Validate()
    {
        var routes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var tool in Tools)
        {
            if (string.IsNullOrWhiteSpace(tool.Title))
                throw new InvalidOperationException("Every portal catalog tool must have a title.");

            if (string.IsNullOrWhiteSpace(tool.Route) ||
                !tool.Route.StartsWith("/", StringComparison.Ordinal) ||
                tool.Route.StartsWith("//", StringComparison.Ordinal) ||
                tool.Route.Contains("://", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Portal catalog route for '{tool.Route}' must be an internal path beginning with '/'.");
            }

            if (!routes.Add(tool.Route))
                throw new InvalidOperationException($"Portal catalog route '{tool.Route}' is listed more than once.");

            if (tool.RequiredPermission?.StartsWith("perm:", StringComparison.OrdinalIgnoreCase) == true)
                throw new InvalidOperationException(
                    $"Portal catalog permission for '{tool.Title}' must be a permission name, not a policy name.");
        }
    }
}

public sealed class PortalToolOptions
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string RequiredPermission { get; set; } = string.Empty;
    public string Category { get; set; } = "Tools";
    public string IconText { get; set; } = "TO";
}