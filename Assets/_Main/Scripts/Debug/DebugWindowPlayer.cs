using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImGuiNET;
using PlatformCore.Services.Factory;
using UnityEngine;

// todo нужно не хранить все категории в одном файле
public class DebugWindowPlayer : DebugWindowModel
{
	private readonly PlayerModel playerModel;
	private readonly PlayerView playerView;
	private readonly ConfigService configService;

	private int cashInputBuffer;
	private int questIdBuffer;
	private string diceIdBuffer;
	private int diceIdxBuffer;

	private Dictionary<string, DiceConfig> diceConfig;

	public override string Id => "Player";

	public override async Task Preload()
	{
		await base.Preload();

		diceConfig = await configService.GetConfigsAsync<DiceConfig>(ResourcePaths.Json.dice_types);
	}

	public DebugWindowPlayer(PlayerModel playerModel, PlayerView playerView, ConfigService configService)
	{
		this.playerModel = playerModel;
		this.playerView = playerView;
		this.configService = configService;
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


		if (ImGui.CollapsingHeader("Inventory"))
		{
			var configsArray = diceConfig.Keys.ToArray();

			if (ImGui.CollapsingHeader("Dices"))
			{
				foreach (var item in playerModel.InventoryModel.DiceIdList)
				{
					ImGui.Text(item);
				}
			}

			if (ImGui.Combo("DiceId", ref diceIdxBuffer, configsArray, diceConfig.Count))
			{
				diceIdBuffer = configsArray[diceIdxBuffer];
			}

			if (ImGui.Button("Add Dice"))
			{
				playerModel.InventoryModel.AddDice(diceIdBuffer);
			}

			if (ImGui.Button("Remove Dice"))
			{
				playerModel.InventoryModel.RemoveDice(diceIdBuffer);
			}

			if (ImGui.Button("Remove All Dices"))
			{
				playerModel.InventoryModel.RemoveAllDices();
			}
		}

		ImGui.End();
	}
}