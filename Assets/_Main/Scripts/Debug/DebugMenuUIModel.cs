using System.Collections.Generic;
using ImGuiNET;

public class DebugMenuUIModel
{
    private readonly IEnumerable<DebugMenuModel> items;

    public DebugMenuUIModel(params DebugMenuModel[] items)
    {
        this.items = items;
    }

    public void OnLayout(UImGui.UImGui uImGui)
    {
        if (ImGui.BeginMainMenuBar())
        {
            foreach (var menu in items)
            {
                if (ImGui.BeginMenu(menu.path))
                {
                    foreach (var item in menu.items)
                    {
                        if (ImGui.MenuItem(item.Path))
                        {
                            item.Execute();
                        }
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }
}