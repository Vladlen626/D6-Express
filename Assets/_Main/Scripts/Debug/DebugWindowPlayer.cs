using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
	private string notificationMessageBuffer = string.Empty;
	private int diceIdxBuffer;

	private Dictionary<string, DiceConfig> diceConfig;

	public override string Id => "Player";

	public override async Task Preload()
	{
		await base.Preload();

		diceConfig = await configService.GetConfigsAsync<DiceConfig>(ResourcePaths.Json.dice_types);
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

		ImGui.End();
	}
}