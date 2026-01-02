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

    private int cashInputBuffer;

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

        if (ImGui.CollapsingHeader("Cash"))
        {
            ImGui.Text($"Cash: {playerModel.InventoryModel.CashCount}");

            ImGui.InputInt("Amount", ref cashInputBuffer);

            if (ImGui.Button("Give cash"))
            {
                playerModel.InventoryModel.GiveCash(cashInputBuffer);
            }

            if (ImGui.Button("Take cash"))
            {
                playerModel.InventoryModel.TakeCash(cashInputBuffer);
            }

            if (ImGui.Button("Set cash"))
            {
                playerModel.InventoryModel.SetCash(cashInputBuffer);
            }
        }

        ImGui.End();
    }
}