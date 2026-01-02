using ImGuiNET;
using UImGui;

public abstract class DebugWindowModel
{
    protected bool isOpen;

    public abstract string Id { get; }

    public void Open()
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;
        UImGuiUtility.Layout += OnLayout;
    }

    public void Close()
    {
        if (!isOpen)
        {
            return;
        }
        
        isOpen = false;
        UImGuiUtility.Layout -= OnLayout;
    }

    protected abstract void OnLayout(UImGui.UImGui uImGui);
}

public class DebugWindowPlayer : DebugWindowModel
{
    private readonly PlayerModel playerModel;
    private readonly PlayerView playerView;

    public override string Id => "Player";

    public DebugWindowPlayer(PlayerModel playerModel, PlayerView playerView)
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

        // Здесь можно добавить элементы управления для отладки игрока

        ImGui.End();
    }
}