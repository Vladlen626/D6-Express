using System.Collections.Generic;
using System.Threading.Tasks;

public class DebugMenuModel
{
    public readonly IEnumerable<DebugMenuItem> items;
    public readonly string path;

    public async Task Preload()
    {
        foreach (var item in items)
        {
            await item.Preload();
        }
    }

    public DebugMenuModel(string path, params DebugMenuItem[] items)
    {
        this.path = path;
        this.items = items;
    }
}