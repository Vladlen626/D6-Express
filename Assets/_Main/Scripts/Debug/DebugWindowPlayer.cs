using ImGuiNET;
using UnityEngine;

public class DebugWindowPlayer : DebugWindowModel
{
	private readonly PlayerModel playerModel;
	private readonly PlayerView playerView;

	private int cashInputBuffer;
	private int questIdBuffer;

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

		ImGui.SetNextWindowSize(new Vector2(420, 300), 0);

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

		if (ImGui.CollapsingHeader("States"))
		{
			ImGui.Text("Current States:");
			ImGui.Separator();
			foreach (var item in playerModel.PlayerStateModel.CurrentStates)
			{
				ImGui.Text(item.ToString());
			}
		}

		if (ImGui.CollapsingHeader("Quests"))
		{
			if (ImGui.Button("Add random"))
			{
                var quest = QuestFactory.GenerateRandomQuest(playerView);
				quest.RequestStart();
                playerModel.Quests.Add(quest);
			}

			if (ImGui.Button("Clear"))
			{
				playerModel.Quests.Clear();
			}
		}

		ImGui.End();
	}
}