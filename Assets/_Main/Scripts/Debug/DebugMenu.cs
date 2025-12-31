using System.Collections.Generic;

public class DebugMenuModel
{
    public readonly IEnumerable<DebugMenuItem> items;
    public readonly string path;

    public DebugMenuModel(string path, params DebugMenuItem[] items)
    {
        this.path = path;
        this.items = items;
    }
}
