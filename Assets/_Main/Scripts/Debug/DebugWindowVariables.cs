using ImGuiNET;

public class DebugWindowVariables : DebugWindowModel
{
    private readonly PlayerModel playerModel;
    private readonly PlayerView playerView;

    public override string Id => "Debug Variables";

    public DebugWindowVariables(PlayerModel playerModel, PlayerView playerView)
    {
        this.playerModel = playerModel;
        this.playerView = playerView;
    }

    protected override void OnLayout(UImGui.UImGui uImGui)
    {
        if (!isOpen)
        {
            Close();
            return;
        }

        if (!ImGui.Begin(Id, ref isOpen, ImGuiWindowFlags.MenuBar))
        {
            return;
        }

        ImGui.Checkbox("Show Lose View", ref DebugVariables.ShowLoseView);
        ImGui.Checkbox("Show Win View", ref DebugVariables.ShowWinView);

        ImGui.End();
    }
}
