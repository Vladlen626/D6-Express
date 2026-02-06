using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlatformCore.Services;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Central scoring engine. Loads defaults from JSON config, applies runtime overrides from persistent storage,
	/// evaluates dice combinations, and exposes mutation APIs for runtime editing in builds.
	/// </summary>
	public class DiceScoringService
	{
		private const string ConfigResourcePath = "DiceScoringConfig";
		private const string ConfigFileName = "DiceScoringConfig.json";
		private const string OverridesFileName = "DiceScoringOverrides.json";
		private const string ComboUpgradeConfigResourcePath = "ComboUpgradesConfig";
		private const string ComboUpgradeConfigFileName = "ComboUpgradesConfig.json";

		private static DiceScoringService instance;
		public static DiceScoringService Instance => instance ??= new DiceScoringService();

		private readonly List<ComboRuleDefinition> activeRules = new();
		private readonly List<ComboRuleDefinition> addedRules = new();
		private readonly HashSet<string> disabledRuleIds = new();
		private readonly Dictionary<string, int> baseScoreOverrides = new();
		private readonly StraightCombination straightCombination;
		private readonly Dictionary<string, ComboUpgradeConfig> comboUpgradeConfigs = new();
		private readonly Dictionary<string, ComboUpgradeState> comboUpgradeStates = new();
		private StraightConfig straightConfig;
		private ComboUpgradeConfigRoot upgradeBundle;

		private DiceScoringService()
		{
			LoadDefaults();
			LoadOverrides();
			upgradeBundle = LoadComboUpgradeConfig();
			straightConfig = upgradeBundle?.Straight;
			straightCombination = new StraightCombination(
				straightConfig,
				new StraightRuntimeState(straightConfig?.Defaults),
				msg => Debug.Log(msg))
			{
				DebugLogging = straightConfig?.Upgrade?.Debug ?? false
			};
			BuildComboUpgradeDictionary(upgradeBundle);
			InitializeDefaultUpgradeStates();
		}

		#region Public API ----------------------------------------------------

		public DiceCombinationResult Evaluate(int[] values)
		{
			var result = new DiceCombinationResult
			{
				Combinations = new List<DiceCombinationEntry>()
			};

			if (values == null || values.Length == 0)
			{
				result.RemainingCounts = new int[7];
				return result;
			}

			var remaining = new int[7];
			foreach (var value in values)
			{
				if (value >= 1 && value <= 6)
				{
					remaining[value]++;
				}
			}

			foreach (var rule in activeRules)
			{
				if (!rule.Enabled || string.IsNullOrWhiteSpace(rule.Id))
				{
					continue;
				}

				switch (rule.RuleType)
				{
					case ComboRuleType.Straight:
						ApplyStraightRule(rule, remaining, result.Combinations);
						break;
					case ComboRuleType.OfAKind:
						ApplyOfAKindRule(rule, remaining, result.Combinations);
						break;
					case ComboRuleType.Single:
						ApplySingleRule(rule, remaining, result.Combinations);
						break;
				}
			}

			result.RemainingCounts = remaining;
			return result;
		}

		public bool HasTrash(int[] values)
		{
			var eval = Evaluate(values);
			for (int face = 1; face <= 6; face++)
			{
				if (eval.RemainingCounts != null && eval.RemainingCounts[face] > 0)
				{
					return true;
				}
			}

			return false;
		}

		public int CalculateTotalScore(DiceCombinationResult result)
		{
			if (result.Combinations == null)
			{
				return 0;
			}

			int total = 0;
			foreach (var entry in result.Combinations)
			{
				total += entry.FinalScore;
			}

			return total;
		}

		public string GetDisplayName(string id, DiceCombination combination = DiceCombination.None)
		{
			if (!string.IsNullOrWhiteSpace(id))
			{
				var rule = activeRules.FirstOrDefault(r => r.Id == id);
				if (rule != null && !string.IsNullOrWhiteSpace(rule.DisplayName))
				{
					return rule.DisplayName;
				}
			}

			if (combination != DiceCombination.None)
			{
				return combination switch
				{
					DiceCombination.Straight_1_6 => "Straight 1-6",
					DiceCombination.Straight_1_5 => "Straight 1-5",
					DiceCombination.Straight_2_6 => "Straight 2-6",
					DiceCombination.StraightLength4 => "Straight (4)",
					DiceCombination.StraightLength5 => "Straight (5)",
					DiceCombination.StraightLength6 => "Straight (6)",
					DiceCombination.ThreeOfAKind => "Three of a kind",
					DiceCombination.FourOfAKind => "Four of a kind",
					DiceCombination.FiveOfAKind => "Five of a kind",
					DiceCombination.SixOfAKind => "Six of a kind",
					DiceCombination.SingleOnes => "Single ones",
					DiceCombination.SingleFives => "Single fives",
					_ => combination.ToString()
				};
			}

			return string.IsNullOrWhiteSpace(id) ? string.Empty : id;
		}

		/// <summary>Set/override the base score for a specific combo id (e.g., "straight_1_6" or "3kind_6"). Persisted to disk.</summary>
		public void UpdateBaseScore(string combinationId, int newBaseScore)
		{
			if (string.IsNullOrWhiteSpace(combinationId))
			{
				return;
			}

			baseScoreOverrides[combinationId] = Mathf.Max(0, newBaseScore);
			SaveOverrides();
		}

		/// <summary>Add a new rule or replace an existing rule with the same id. Persisted to disk.</summary>
		public void AddOrReplaceRule(ComboRuleDefinition definition)
		{
			if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
			{
				return;
			}

			RemoveRule(definition.Id, persist: false);
			addedRules.Add(Clone(definition));
			RebuildActiveRules();
			SaveOverrides();
		}

		/// <summary>Remove a rule by id. Base rules are only disabled; added rules are deleted. Persisted to disk.</summary>
		public void RemoveRule(string ruleId, bool persist = true)
		{
			if (string.IsNullOrWhiteSpace(ruleId))
			{
				return;
			}

			addedRules.RemoveAll(r => r.Id == ruleId);
			disabledRuleIds.Add(ruleId);
			RebuildActiveRules();

			if (persist)
			{
				SaveOverrides();
			}
		}

		/// <summary>Reorder rules by the provided id list; any missing ids are appended in their previous order. Persisted to disk.</summary>
		public void ReorderRules(List<string> orderedIds)
		{
			if (orderedIds == null || orderedIds.Count == 0)
			{
				return;
			}

			var newOrder = new List<ComboRuleDefinition>();
			foreach (var id in orderedIds)
			{
				var rule = activeRules.FirstOrDefault(r => r.Id == id);
				if (rule != null && !newOrder.Contains(rule))
				{
					newOrder.Add(rule);
				}
			}

			foreach (var rule in activeRules)
			{
				if (!newOrder.Contains(rule))
				{
					newOrder.Add(rule);
				}
			}

			activeRules.Clear();
			activeRules.AddRange(newOrder);
			SaveOverrides();
		}

		/// <summary>Reload defaults and overrides. Discards non-persisted changes.</summary>
		public void ReloadDefaults()
		{
			activeRules.Clear();
			addedRules.Clear();
			disabledRuleIds.Clear();
			baseScoreOverrides.Clear();

			LoadDefaults();
			LoadOverrides();
		}

		/// <summary>Returns snapshot of active rules (order-sensitive).</summary>
		public IReadOnlyList<ComboRuleDefinition> GetActiveRules() => activeRules;

		public StraightDefaults GetStraightDefaults() => straightConfig?.Defaults;
		public StraightUpgradeConfig GetStraightUpgradeConfig() => straightConfig?.Upgrade;
		public StraightRuntimeState GetStraightState() => straightCombination.Snapshot();
		public ComboUpgradeConfig GetComboUpgradeConfig(string comboId)
		{
			return comboUpgradeConfigs.TryGetValue(comboId, out var cfg) ? cfg : null;
		}

		public ComboUpgradeState GetComboUpgradeState(string comboId)
		{
			return comboUpgradeStates.TryGetValue(comboId, out var state) ? state.Clone() : null;
		}

		public void SetStraightState(StraightRuntimeState state)
		{
			straightCombination.LoadState(state, logClamp: straightCombination.DebugLogging);
		}

		public StraightUpgradeOutcome[] GetStraightUpgradeOutcomes()
		{
			return straightConfig?.Upgrade?.Outcomes;
		}

		public ComboUpgradeOutcome[] GetComboUpgradeOutcomes(string comboId)
		{
			return GetComboUpgradeConfig(comboId)?.Outcomes;
		}

		public void ResetStraightToDefaults()
		{
			straightCombination.ResetToDefaults(straightConfig?.Defaults);
		}

		private void InitializeDefaultUpgradeStates()
		{
			comboUpgradeStates["straight"] = new ComboUpgradeState
			{
				Min = straightCombination.MinLen,
				Max = straightCombination.MaxLen,
				ScoreBonus = straightCombination.ScoreBonus
			};

			// Default OfAKind state mirrors current rule min/max with zero bonus
			comboUpgradeStates["ofakind"] = new ComboUpgradeState
			{
				Min = 3,
				Max = 6,
				ScoreBonus = 0
			};
		}

		public StraightUpgradeOutcome ApplyStraightUpgradeOutcome(int face, ILoggerService logger = null, Notifications notifications = null, Run run = null)
		{
			var outcomes = straightConfig?.Upgrade?.Outcomes;
			if (outcomes == null || outcomes.Length == 0)
			{
				return null;
			}

			var outcome = outcomes.FirstOrDefault(o => o.Face == face) ?? outcomes[0];
			var before = straightCombination.Snapshot();
			straightCombination.Adjust(outcome);
			var after = straightCombination.Snapshot();

			logger?.Log($"[StraightUpgrade] Die face {face} -> Δ(min {outcome.DeltaMinLen}, max {outcome.DeltaMaxLen}, bonus {outcome.DeltaScoreBonus}). Result: min={after.MinLen}, max={after.MaxLen}, bonus={after.ScoreBonus}");
			if (notifications != null)
			{
				var note = $"Straight upgraded! Min {before.MinLen}->{after.MinLen}, Max {before.MaxLen}->{after.MaxLen}, Bonus {before.ScoreBonus}->{after.ScoreBonus}";
				notifications.Add(new Notifications.Notification
				{
					message = note
				});
				logger?.Log(note);
			}

			if (run != null)
			{
				run.SetStraightState(after);
				comboUpgradeStates["straight"] = new ComboUpgradeState
				{
					Min = after.MinLen,
					Max = after.MaxLen,
					ScoreBonus = after.ScoreBonus
				};
			}

			return outcome;
		}

		public ComboUpgradeOutcome ApplyGenericUpgradeOutcome(string comboId, int face, ILoggerService logger = null, Notifications notifications = null, Run run = null)
		{
			if (!comboUpgradeConfigs.TryGetValue(comboId, out var cfg))
			{
				return null;
			}

			var outcomes = cfg.Outcomes;
			if (outcomes == null || outcomes.Length == 0)
			{
				return null;
			}

			var outcome = outcomes.FirstOrDefault(o => o.Face == face) ?? outcomes[0];
			var state = GetComboUpgradeState(comboId) ?? new ComboUpgradeState();
			var before = state.Clone();

			state.Min += outcome.DeltaMin;
			state.Max += outcome.DeltaMax;
			state.ScoreBonus += outcome.DeltaScoreBonus;

			var constraints = cfg.Constraints ?? new ComboUpgradeConstraints { MinLowerBound = 1, MaxUpperBound = 6 };
			state.Min = Mathf.Clamp(state.Min, constraints.MinLowerBound, constraints.MaxUpperBound);
			state.Max = Mathf.Clamp(state.Max, state.Min, constraints.MaxUpperBound);

			comboUpgradeStates[comboId] = state;

			logger?.Log($"[ComboUpgrade:{comboId}] Face {face} -> Δ(min {outcome.DeltaMin}, max {outcome.DeltaMax}, bonus {outcome.DeltaScoreBonus}). Result: min={state.Min}, max={state.Max}, bonus={state.ScoreBonus}");

			return outcome;
		}

		#endregion

		#region Rule application ---------------------------------------------

		private void ApplyStraightRule(ComboRuleDefinition rule, int[] remaining, List<DiceCombinationEntry> output)
		{
			// Variable-length straight logic driven by StraightCombination
			while (true)
			{
				if (!rule.Repeatable && output.Any(e => e.Id == rule.Id))
				{
					return;
				}

				if (!straightCombination.TryConsumeStraight(remaining, out var match))
				{
					return;
				}

				var baseScore = straightCombination.GetBaseScore(match.Length);
				var entryId = $"{rule.Id}_{match.Length}";
				var entry = new DiceCombinationEntry
				{
					Id = entryId,
					DisplayName = rule.DisplayName,
					Combination = MapToCombination(match.Length >= 4 ? $"straight_len_{match.Length}" : rule.Id),
					Face = match.StartFace,
					Count = match.Length,
					BaseScore = GetScoreWithOverrides(entryId, baseScore) + straightCombination.ScoreBonus
				};

				output.Add(entry);
			}
		}

		private void ApplyOfAKindRule(ComboRuleDefinition rule, int[] remaining, List<DiceCombinationEntry> output)
		{
			// Apply upgrade-adjusted min/max and score bonus
			var ofaState = GetComboUpgradeState("ofakind");
			var effectiveMin = ofaState?.Min > 0 ? ofaState.Min : rule.MinCount;
			var effectiveMax = ofaState?.Max > 0 ? ofaState.Max : rule.MaxCount;
			var extraBonus = ofaState?.ScoreBonus ?? 0;

			var faces = (rule.Faces != null && rule.Faces.Length > 0)
				? rule.Faces
				: new[] { 1, 2, 3, 4, 5, 6 };

			foreach (var face in faces)
			{
				while (remaining[face] >= effectiveMin)
				{
					if (!rule.Repeatable && output.Any(e => e.Id == rule.Id && e.Face == face))
					{
						break;
					}

					int take = remaining[face];
					if (effectiveMax > 0)
					{
						take = Math.Min(take, effectiveMax);
					}

					if (take < effectiveMin)
					{
						break;
					}

					remaining[face] -= take;

					int baseScore = ComputeOfAKindScore(rule, face, take);
					baseScore += extraBonus;
					var entryId = $"{rule.Id}_{face}_{take}";

					var entry = new DiceCombinationEntry
					{
						Id = entryId,
						DisplayName = rule.DisplayName,
						Combination = MapToCombination(rule.Id, take),
						Face = face,
						Count = take,
						BaseScore = GetScoreWithOverrides(entryId, baseScore)
					};

					output.Add(entry);
				}
			}
		}

		private void ApplySingleRule(ComboRuleDefinition rule, int[] remaining, List<DiceCombinationEntry> output)
		{
			var faces = (rule.Faces != null && rule.Faces.Length > 0)
				? rule.Faces
				: new[] { 1, 2, 3, 4, 5, 6 };

			foreach (var face in faces)
			{
				if (remaining[face] <= 0)
				{
					continue;
				}

				int count = remaining[face];
				remaining[face] = 0;

				int perDie = rule.BaseScore;
				if (rule.PerFaceScaling)
				{
					perDie *= face;
				}

				int baseScore = perDie * count;
				var entryId = $"{rule.Id}_{face}";

				var entry = new DiceCombinationEntry
				{
					Id = entryId,
					DisplayName = rule.DisplayName,
					Combination = MapToCombination(rule.Id),
					Face = face,
					Count = count,
					BaseScore = GetScoreWithOverrides(entryId, baseScore)
				};

				output.Add(entry);
			}
		}

		private int ComputeOfAKindScore(ComboRuleDefinition rule, int face, int count)
		{
			// If caller set BaseScore explicitly, honour it with optional scaling rules.
			if (rule.BaseScore > 0 || rule.PerFaceScaling)
			{
				int score = rule.BaseScore;
				if (rule.PerFaceScaling)
				{
					score *= face;
				}

				if (rule.DoublePerExtraAboveMin && count > rule.MinCount)
				{
					score *= (int)Mathf.Pow(2, count - rule.MinCount);
				}

				return score;
			}

			// Legacy behaviour: 1s are 1000, others face * BaseScorePerPip, doubling per extra.
			int baseScore = (face == 1) ? rule.BaseScoreForOne : face * rule.BaseScorePerPip;

			if (count > rule.MinCount && rule.DoublePerExtraAboveMin)
			{
				baseScore *= (int)Mathf.Pow(2, count - rule.MinCount);
			}

			return baseScore;
		}

		private int GetScoreWithOverrides(string id, int computedBaseScore)
		{
			if (!string.IsNullOrWhiteSpace(id) && baseScoreOverrides.TryGetValue(id, out var overrideScore))
			{
				return overrideScore;
			}

			return computedBaseScore;
		}

		private static DiceCombination MapToCombination(string ruleId, int count = 0)
		{
			return ruleId switch
			{
				"straight_1_6" => DiceCombination.Straight_1_6,
				"straight_1_5" => DiceCombination.Straight_1_5,
				"straight_2_6" => DiceCombination.Straight_2_6,
				"straight_len_4" => DiceCombination.StraightLength4,
				"straight_len_5" => DiceCombination.StraightLength5,
				"straight_len_6" => DiceCombination.StraightLength6,
				"ofakind" when count == 3 => DiceCombination.ThreeOfAKind,
				"ofakind" when count == 4 => DiceCombination.FourOfAKind,
				"ofakind" when count == 5 => DiceCombination.FiveOfAKind,
				"ofakind" when count >= 6 => DiceCombination.SixOfAKind,
				"single_ones" => DiceCombination.SingleOnes,
				"single_fives" => DiceCombination.SingleFives,
				_ => DiceCombination.None
			};
		}

		#endregion

		#region Defaults & overrides -----------------------------------------

		private void LoadDefaults()
		{
			var loaded = LoadRulesFromConfig();
			if (loaded != null && loaded.Count > 0)
			{
				activeRules.AddRange(loaded.Select(Clone));
				return;
			}

			activeRules.AddRange(BuildBuiltinDefaults());
		}

		private void LoadOverrides()
		{
			try
			{
				var path = GetOverridesPath();
				if (!File.Exists(path))
				{
					return;
				}

				var json = File.ReadAllText(path);
				var payload = JsonUtility.FromJson<OverridesPayload>(json);
				if (payload == null)
				{
					return;
				}

				baseScoreOverrides.Clear();
				if (payload.scoreOverrides != null)
				{
					foreach (var entry in payload.scoreOverrides)
					{
						if (!string.IsNullOrWhiteSpace(entry.Id))
						{
							baseScoreOverrides[entry.Id] = entry.Value;
						}
					}
				}

				disabledRuleIds.Clear();
				if (payload.disabledRuleIds != null)
				{
					foreach (var id in payload.disabledRuleIds)
					{
						if (!string.IsNullOrWhiteSpace(id))
						{
							disabledRuleIds.Add(id);
						}
					}
				}

				addedRules.Clear();
				if (payload.addedRules != null)
				{
					foreach (var def in payload.addedRules)
					{
						if (def != null && !string.IsNullOrWhiteSpace(def.Id))
						{
							addedRules.Add(def);
						}
					}
				}

				RebuildActiveRules();
			}
			catch (Exception ex)
			{
				Debug.LogError($"[DiceScoringService] Failed to load overrides: {ex.Message}");
			}
		}

		private ComboUpgradeConfigRoot LoadComboUpgradeConfig()
		{
			ComboUpgradeConfigRoot payload = null;

			// Persistent path first
			var persistentPath = Path.Combine(Application.persistentDataPath, ComboUpgradeConfigFileName);
			if (File.Exists(persistentPath))
			{
				payload = JsonUtility.FromJson<ComboUpgradeConfigRoot>(File.ReadAllText(persistentPath));
			}

			if (payload == null || payload.Upgrades == null || payload.Upgrades.Length == 0)
			{
				var textAsset = Resources.Load<TextAsset>(ComboUpgradeConfigResourcePath);
				if (textAsset != null)
				{
					payload = JsonUtility.FromJson<ComboUpgradeConfigRoot>(textAsset.text);
				}
			}

			return payload;
		}

		private void BuildComboUpgradeDictionary(ComboUpgradeConfigRoot payload)
		{
			if (payload?.Upgrades == null)
			{
				return;
			}

			comboUpgradeConfigs.Clear();
			foreach (var cfg in payload.Upgrades)
			{
				if (!string.IsNullOrWhiteSpace(cfg?.ComboId))
				{
					comboUpgradeConfigs[cfg.ComboId] = cfg;
				}
			}

			if (payload.Straight != null)
			{
				straightConfig = payload.Straight;
			}
		}

		private void SaveOverrides()
		{
			try
			{
				var payload = new OverridesPayload
				{
					scoreOverrides = baseScoreOverrides
						.Select(kv => new ScoreOverrideEntry { Id = kv.Key, Value = kv.Value })
						.ToList(),
					disabledRuleIds = disabledRuleIds.ToList(),
					addedRules = addedRules.Select(Clone).ToList()
				};

				var json = JsonUtility.ToJson(payload, true);
				var path = GetOverridesPath();
				File.WriteAllText(path, json);
				Debug.Log($"[DiceScoringService] Overrides saved to {path}");
			}
			catch (Exception ex)
			{
				Debug.LogError($"[DiceScoringService] Failed to save overrides: {ex.Message}");
			}
		}

		private void RebuildActiveRules()
		{
			var rebuilt = new List<ComboRuleDefinition>();

			// Base rules first (excluding disabled)
			foreach (var rule in activeRules.ToList())
			{
				if (disabledRuleIds.Contains(rule.Id))
				{
					continue;
				}
				rebuilt.Add(Clone(rule));
			}

			// Added rules override/append
			foreach (var rule in addedRules)
			{
				var existingIndex = rebuilt.FindIndex(r => r.Id == rule.Id);
				if (existingIndex >= 0)
				{
					rebuilt[existingIndex] = Clone(rule);
				}
				else
				{
					rebuilt.Add(Clone(rule));
				}
			}

			activeRules.Clear();
			activeRules.AddRange(rebuilt);
		}

		private static List<ComboRuleDefinition> BuildBuiltinDefaults()
		{
			return new List<ComboRuleDefinition>
			{
				new ComboRuleDefinition
				{
					Id = "straight_1_6",
					DisplayName = "Straight 1-6",
					RuleType = ComboRuleType.Straight,
					Faces = new [] { 1, 2, 3, 4, 5, 6 },
					BaseScore = 1500
				},
				new ComboRuleDefinition
				{
					Id = "straight_1_5",
					DisplayName = "Straight 1-5",
					RuleType = ComboRuleType.Straight,
					Faces = new [] { 1, 2, 3, 4, 5 },
					BaseScore = 500
				},
				new ComboRuleDefinition
				{
					Id = "straight_2_6",
					DisplayName = "Straight 2-6",
					RuleType = ComboRuleType.Straight,
					Faces = new [] { 2, 3, 4, 5, 6 },
					BaseScore = 750
				},
				new ComboRuleDefinition
				{
					Id = "ofakind",
					DisplayName = "N of a kind",
					RuleType = ComboRuleType.OfAKind,
					MinCount = 3,
					MaxCount = 6,
					PerFaceScaling = false,
					DoublePerExtraAboveMin = true,
					BaseScoreForOne = 1000,
					BaseScorePerPip = 100
				},
				new ComboRuleDefinition
				{
					Id = "single_ones",
					DisplayName = "Single ones",
					RuleType = ComboRuleType.Single,
					Faces = new [] {1},
					BaseScore = 100,
					PerFaceScaling = false
				},
				new ComboRuleDefinition
				{
					Id = "single_fives",
					DisplayName = "Single fives",
					RuleType = ComboRuleType.Single,
					Faces = new [] {5},
					BaseScore = 50,
					PerFaceScaling = false
				}
			};
		}

		private static ComboRuleDefinition Clone(ComboRuleDefinition src)
		{
			return new ComboRuleDefinition
			{
				Id = src.Id,
				DisplayName = src.DisplayName,
				RuleType = src.RuleType,
				Faces = src.Faces != null ? src.Faces.ToArray() : null,
				MinCount = src.MinCount,
				MaxCount = src.MaxCount,
				BaseScore = src.BaseScore,
				Repeatable = src.Repeatable,
				PerFaceScaling = src.PerFaceScaling,
				DoublePerExtraAboveMin = src.DoublePerExtraAboveMin,
				BaseScoreForOne = src.BaseScoreForOne,
				BaseScorePerPip = src.BaseScorePerPip,
				Enabled = src.Enabled
			};
		}

		private static string GetOverridesPath()
		{
			return Path.Combine(Application.persistentDataPath, OverridesFileName);
		}

		private List<ComboRuleDefinition> LoadRulesFromConfig()
		{
			// 1) Persistent path (allows shipping a writable base config alongside overrides).
			var persistentPath = Path.Combine(Application.persistentDataPath, ConfigFileName);
			if (File.Exists(persistentPath))
			{
				var json = File.ReadAllText(persistentPath);
				var payload = JsonUtility.FromJson<ConfigPayload>(json);
				if (payload?.rules != null && payload.rules.Count > 0)
				{
					return payload.rules;
				}
			}

			// 2) Resources fallback (read-only, packaged with build).
			var textAsset = Resources.Load<TextAsset>(ConfigResourcePath);
			if (textAsset != null)
			{
				var payload = JsonUtility.FromJson<ConfigPayload>(textAsset.text);
				if (payload?.rules != null && payload.rules.Count > 0)
				{
					return payload.rules;
				}
			}

			return null;
		}

		#endregion

		#region Payload models -----------------------------------------------

		[Serializable]
		private class OverridesPayload
		{
			public List<ScoreOverrideEntry> scoreOverrides;
			public List<string> disabledRuleIds;
			public List<ComboRuleDefinition> addedRules;
		}

		[Serializable]
		private class ConfigPayload
		{
			public List<ComboRuleDefinition> rules;
		}

		[Serializable]
		private class ScoreOverrideEntry
		{
			public string Id;
			public int Value;
		}

		#endregion
	}
}
