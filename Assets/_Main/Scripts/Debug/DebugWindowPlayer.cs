using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Main.Scripts.Dice;
using ImGuiNET;
using PlatformCore.Services.Factory;
using UnityEngine;

// todo нужно не хранить все категории в одном файле
public class DebugWindowPlayer : DebugWindowModel
{
	private readonly Run run;
	private readonly PlayerModel playerModel;
	private readonly DiceGameModel diceGameModel;
	private readonly PlayerView playerView;
	private readonly ConfigService configService;
	private readonly GlobalNotificationService notificationService;
	private int cashInputBuffer;
	private int questIdBuffer;
	private string diceIdBuffer;
	private int modifierIdxBuffer;
	private string notificationMessageBuffer = string.Empty;
	private int diceIdxBuffer;

	private string[] modifierIds = Array.Empty<string>();
	private string[] diceIds = Array.Empty<string>();

	private Dictionary<string, ItemCatalogEntry> catalog;

	public override string Id => "Player";

	public override async Task Preload()
	{
		await base.Preload();

		catalog = await configService.GetConfigsAsync<ItemCatalogEntry>(ResourcePaths.Json.items_catalog);
		diceIds = catalog.Where(x => x.Value.typeEnum == ItemCatalogType.Dice).Select(x => x.Key).ToArray();
		modifierIds = catalog.Where(x => x.Value.typeEnum == ItemCatalogType.ModifierItem).Select(x => x.Key).ToArray();
	}

	public DebugWindowPlayer(Run run, PlayerModel playerModel, PlayerView playerView, ConfigService configService, GlobalNotificationService notificationService, DiceGameModel diceGameModel)
	{
		this.run = run;
		this.playerModel = playerModel;
		this.playerView = playerView;
		this.configService = configService;
		this.notificationService = notificationService;
		this.diceGameModel = diceGameModel;
	}

	protected override void OnLayout(UImGui.UImGui uImGui)
	{
		if (!isOpen)
		{
			Close();
			return;
		}

		ImGui.SetNextWindowSizeConstraints(new Vector2(420, 600), new Vector2(float.MaxValue, float.MaxValue));

		if (!ImGui.Begin(Id, ref isOpen, ImGuiWindowFlags.MenuBar))
		{
			return;
		}

		if (ImGui.CollapsingHeader("Run Settings"))
		{
			int tickBuffer = run.Tick;
			ImGui.InputInt("Tick:", ref tickBuffer);
			if (tickBuffer != run.Tick)
			{
				run.RequestSetTick(tickBuffer);
			}

			int ticksPerDayBuffer = run.TicksPerDay;
			ImGui.InputInt("Ticks Per Day:", ref ticksPerDayBuffer);
			if (ticksPerDayBuffer != run.TicksPerDay)
			{
				run.SetTicksPerDay(ticksPerDayBuffer);
			}

			int dayBuffer = run.Day;
			ImGui.InputInt("Day:", ref dayBuffer);
			if (dayBuffer != run.Day)
			{
				run.RequestSetDay(dayBuffer);
			}

			int daysPerLevelBuffer = run.DaysPerLevel;
			ImGui.InputInt("Days Per Level:", ref daysPerLevelBuffer);
			if (daysPerLevelBuffer != run.DaysPerLevel)
			{
				run.SetDaysPerLevel(daysPerLevelBuffer);
			}

			int ticketPriceBuffer = run.TicketPrice;
			ImGui.InputInt("Ticket Price:", ref ticketPriceBuffer);
			if (ticketPriceBuffer != run.TicketPrice)
			{
				run.SetTicketPrice(ticketPriceBuffer);
			}

			int nextTicketPriceBuffer = run.NextTicketPrice;
			ImGui.InputInt("Next Ticket Price:", ref nextTicketPriceBuffer);
			if (nextTicketPriceBuffer != run.NextTicketPrice)
			{
				run.SetNextTicketPrice(nextTicketPriceBuffer);
			}

			if (ImGui.Button("Win Run"))
			{
				run.FinishRun(Run.FinishType.WIN);
			}

			if (ImGui.Button("Lose Run"))
			{
				run.FinishRun(Run.FinishType.LOSE);
			}
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

		if (ImGui.CollapsingHeader("Actions"))
		{
			ImGui.Text("Current Action:");
			ImGui.Separator();
			foreach (var item in playerView.Interactor.activeActions)
			{
				ImGui.Text(item.ToString());
			}
		}

		if (ImGui.CollapsingHeader("Quests"))
		{
			// if (ImGui.Button("Add random"))
			// {
			// 	var quest = QuestFactory.GenerateRandomQuest(playerView);
			// 	quest.RequestInProgress();
			// 	playerModel.Quests.Add(quest);
			// }

			if (ImGui.Button("Clear"))
			{
				playerModel.Quests.Clear();
			}
		}

		if (ImGui.CollapsingHeader("Inventory"))
		{
			var configsArray = diceIds;

			if (ImGui.TreeNode("Dices"))
			{
				foreach (var item in playerModel.InventoryModel.DiceIdList)
				{
					ImGui.Text(item);
				}

				ImGui.Separator();
				ImGui.TreePop();
			}

			if (configsArray.Length > 0 && ImGui.Combo("DiceId", ref diceIdxBuffer, configsArray, configsArray.Length))
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

		if (ImGui.CollapsingHeader("Notifications"))
		{
			ImGui.InputText("Message", ref notificationMessageBuffer, 999);

			if (ImGui.Button("Send"))
			{
				notificationService?.EnqueueToastRaw(notificationMessageBuffer);
			}
		}

		if (ImGui.CollapsingHeader("Modifiers"))
		{
			IModifier toRemove = null;

			if (ImGui.TreeNode("Active"))
			{
				var list = playerModel.InventoryModel.ModifiersModel.AllModifiers;

				for (int i = 0; i < list.Count; i++)
				{
					var item = list[i];
					if (item is not IModifierItem modifierItem)
						continue;

					if (!catalog.TryGetValue(modifierItem.Id, out var config))
						continue;

					ImGui.TextUnformatted(config.id);
					ImGui.SameLine();

					ImGui.PushID(i);
					if (ImGui.Button("X"))
					{
						toRemove = item;
					}
					ImGui.PopID();
				}

				if (toRemove != null)
					playerModel.InventoryModel.ModifiersModel.RemoveModifier(toRemove);

				ImGui.Separator();
				ImGui.TreePop();
			}


			if (modifierIds.Length > 0 && ImGui.Combo("Avaliable", ref modifierIdxBuffer, modifierIds, modifierIds.Length))
			{
			}

			if (ImGui.Button("Add Modifier"))
			{
				var id = modifierIds.Length > 0 ? modifierIds[modifierIdxBuffer] : null;
				playerModel.InventoryModel.AddModifierItem(id);
			}

			if (ImGui.Button("Clear Modifiers"))
			{
				playerModel.InventoryModel.RemoveAllModifierItems();
				playerModel.InventoryModel.ModifierItemsModel.Reset();
				playerModel.InventoryModel.ModifiersModel.ClearModifiers();
			}
		}

		if (ImGui.CollapsingHeader("Dice Game"))
		{
			ImGui.Text($"Is Dice Game Started: {diceGameModel.IsDiceGameStarted}");

			if (ImGui.Button("Win"))
			{
				diceGameModel.SetConditionPassed(
					DiceMatchResultReason.DebugForced,
					DiceMatchStage.Unknown,
					"debug_window");
			}

			if (ImGui.Button("Lose"))
			{
				diceGameModel.SetConditionFailed(
					DiceMatchResultReason.DebugForced,
					DiceMatchStage.Unknown,
					"debug_window");
			}
		}

		ImGui.End();
	}
}
