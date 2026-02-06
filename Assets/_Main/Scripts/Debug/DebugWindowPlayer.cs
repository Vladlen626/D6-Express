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
	private readonly PlayerView playerView;
	private readonly ConfigService configService;
	private readonly Notifications notifications;
	private int cashInputBuffer;
	private int questIdBuffer;
	private string diceIdBuffer;
	private int modifierIdxBuffer;
	private string notificationMessageBuffer = string.Empty;
	private int diceIdxBuffer;

	private readonly List<(string name, IModifier mod)> modifiers = new()
	{
		("Single One", new MultiplyKindOfModifiers(DiceCombination.SingleOnes, 0)),
		("Single Fives", new MultiplyKindOfModifiers(DiceCombination.SingleFives, 0)),
		("Straight 1 to 5", new MultiplyKindOfModifiers(DiceCombination.Straight_1_5, 0)),
		("Straight 2 to 6", new MultiplyKindOfModifiers(DiceCombination.Straight_2_6, 0)),
		("Straight 1 to 6", new MultiplyKindOfModifiers(DiceCombination.Straight_1_6, 0)),
		("Three of a Kind 1", new MultiplyKindOfModifiers(DiceCombination.ThreeOfAKind, 1)),
		("Three of a Kind 2", new MultiplyKindOfModifiers(DiceCombination.ThreeOfAKind, 2)),
		("Three of a Kind 3", new MultiplyKindOfModifiers(DiceCombination.ThreeOfAKind, 3)),
		("Three of a Kind 4", new MultiplyKindOfModifiers(DiceCombination.ThreeOfAKind, 4)),
		("Three of a Kind 5", new MultiplyKindOfModifiers(DiceCombination.ThreeOfAKind, 5)),
		("Three of a Kind 6", new MultiplyKindOfModifiers(DiceCombination.ThreeOfAKind, 6)),
		("Four of a Kind 1", new MultiplyKindOfModifiers(DiceCombination.FourOfAKind, 1)),
		("Four of a Kind 2", new MultiplyKindOfModifiers(DiceCombination.FourOfAKind, 2)),
		("Four of a Kind 3", new MultiplyKindOfModifiers(DiceCombination.FourOfAKind, 3)),
		("Four of a Kind 4", new MultiplyKindOfModifiers(DiceCombination.FourOfAKind, 4)),
		("Four of a Kind 5", new MultiplyKindOfModifiers(DiceCombination.FourOfAKind, 5)),
		("Four of a Kind 6", new MultiplyKindOfModifiers(DiceCombination.FourOfAKind, 6)),
		("Five of a Kind 1", new MultiplyKindOfModifiers(DiceCombination.FiveOfAKind, 1)),
		("Five of a Kind 2", new MultiplyKindOfModifiers(DiceCombination.FiveOfAKind, 2)),
		("Five of a Kind 3", new MultiplyKindOfModifiers(DiceCombination.FiveOfAKind, 3)),
		("Five of a Kind 4", new MultiplyKindOfModifiers(DiceCombination.FiveOfAKind, 4)),
		("Five of a Kind 5", new MultiplyKindOfModifiers(DiceCombination.FiveOfAKind, 5)),
		("Five of a Kind 6", new MultiplyKindOfModifiers(DiceCombination.FiveOfAKind, 6)),
		("Six of a Kind 1", new MultiplyKindOfModifiers(DiceCombination.SixOfAKind, 1)),
		("Six of a Kind 2", new MultiplyKindOfModifiers(DiceCombination.SixOfAKind, 2)),
		("Six of a Kind 3", new MultiplyKindOfModifiers(DiceCombination.SixOfAKind, 3)),
		("Six of a Kind 4", new MultiplyKindOfModifiers(DiceCombination.SixOfAKind, 4)),
		("Six of a Kind 5", new MultiplyKindOfModifiers(DiceCombination.SixOfAKind, 5)),
		("Six of a Kind 6", new MultiplyKindOfModifiers(DiceCombination.SixOfAKind, 6))
	};

	private Dictionary<string, DiceConfig> diceConfig;
	private Dictionary<string, ModifierUIConfig> modifiersConfig;

	public override string Id => "Player";

	public override async Task Preload()
	{
		await base.Preload();

		diceConfig = await configService.GetConfigsAsync<DiceConfig>(ResourcePaths.Json.dice_types);
		modifiersConfig = await configService.GetConfigsAsync<ModifierUIConfig>(ResourcePaths.Json.modifiers_ui);
	}

	public DebugWindowPlayer(Run run, PlayerModel playerModel, PlayerView playerView, ConfigService configService, Notifications notifications)
	{
		this.run = run;
		this.playerModel = playerModel;
		this.playerView = playerView;
		this.configService = configService;
		this.notifications = notifications;
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
			var configsArray = diceConfig.Keys.ToArray();

			if (ImGui.TreeNode("Dices"))
			{
				foreach (var item in playerModel.InventoryModel.DiceIdList)
				{
					ImGui.Text(item);
				}

				ImGui.Separator();
				ImGui.TreePop();
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

		if (ImGui.CollapsingHeader("Notifications"))
		{
			ImGui.InputText("Message", ref notificationMessageBuffer, 999);

			if (ImGui.Button("Send"))
			{
				notifications.Add(new Notifications.Notification()
				{
					message = notificationMessageBuffer
				});
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
					if (!modifiersConfig.TryGetValue(item.GetType().Name, out var config))
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


			if (ImGui.Combo("Avaliable", ref modifierIdxBuffer, modifiers.Select(x => x.name).ToArray(), diceConfig.Count))
			{
			}

			if (ImGui.Button("Add Modifier"))
			{
				var modifier = modifiers[modifierIdxBuffer].mod;
				playerModel.InventoryModel.ModifiersModel.AddModifier(modifier);
			}

			if (ImGui.Button("Clear Modifiers"))
			{
				playerModel.InventoryModel.ModifiersModel.ClearModifiers();
			}
		}

		ImGui.End();
	}
}