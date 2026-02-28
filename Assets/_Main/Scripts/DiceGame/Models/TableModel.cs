using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class TableModel
	{
		public event Action OnUpdateUI;
		public event Action OnDisableButtons;
		public event Action<int, int> OnPlayerBankedPointsChanged;
		public event Action<int, int> OnEnemyBankedPointsChanged;
		public event Action<int, int> OnTurnPointsChanged;
		public event Action<int, int> OnPreviewPointsChanged;
		public int TurnPoints { get; private set; }
		public int PreviewPoints { get; private set; }
		public int PlayerBankedPoints { get; private set; }
		public int EnemyBankedPoints {get; private set;}
		public int ActiveSlotsCount => _activePositions?.Length ?? 0;
		public int BankedSlotsCount => _bankedPositions?.Length ?? 0;

		public bool isFirstRoll { get; set; } = true;

		private Transform[] _activePositions;
		private Transform[] _bankedPositions;

		private HashSet<Transform> _occupiedActivePositions = new HashSet<Transform>();
		private HashSet<Transform> _occupiedBankedPositions = new HashSet<Transform>();
		
		public TableModel(Transform[] activePositions, Transform[] bankedPositions)
		{
			_activePositions = activePositions;
			_bankedPositions = bankedPositions;
		}
		
		public Transform GetFreeActivePosition()
		{
			if (_activePositions == null)
			{
				return null;
			}

			foreach (var pos in _activePositions)
			{
				if (!_occupiedActivePositions.Contains(pos))
				{
					_occupiedActivePositions.Add(pos);
					return pos;
				}
			}
			
			return null;
		}


		public Transform GetFreeBankedPosition()
		{
			if (_bankedPositions == null)
			{
				return null;
			}

			foreach (var pos in _bankedPositions)
			{
				if (!_occupiedBankedPositions.Contains(pos))
				{
					_occupiedBankedPositions.Add(pos);
					return pos;
				}
			}
			
			return null;
		}
		
		public void ReleaseActivePosition(Transform position)
		{
			_occupiedActivePositions.Remove(position);
		}

		public void ReleaseBankedPosition(Transform position)
		{
			_occupiedBankedPositions.Remove(position);
		}

		public bool IsActivePosition(Transform position)
		{
			if (!position || _activePositions == null)
			{
				return false;
			}

			for (int i = 0; i < _activePositions.Length; i++)
			{
				if (_activePositions[i] == position)
				{
					return true;
				}
			}

			return false;
		}

		public bool IsBankedPosition(Transform position)
		{
			if (!position || _bankedPositions == null)
			{
				return false;
			}

			for (int i = 0; i < _bankedPositions.Length; i++)
			{
				if (_bankedPositions[i] == position)
				{
					return true;
				}
			}

			return false;
		}
		
		public void ResetAllPositions()
		{
			_occupiedActivePositions.Clear();
			_occupiedBankedPositions.Clear();
		}

		public void AddBankedPointsForPlayer(int points)
		{
			SetPlayerBankedPoints(PlayerBankedPoints + points);
		}
		
		public void AddBankedPointsForEnemy(int points)
		{
			SetEnemyBankedPoints(EnemyBankedPoints + points);
		}

		public void SetPlayerBankedPoints(int points)
		{
			var oldValue = PlayerBankedPoints;
			PlayerBankedPoints = points;
			OnPlayerBankedPointsChanged?.Invoke(oldValue, PlayerBankedPoints);
		}
		
		public void SetEnemyBankedPoints(int points)
		{
			var oldValue = EnemyBankedPoints;
			EnemyBankedPoints = points;
			OnEnemyBankedPointsChanged?.Invoke(oldValue, EnemyBankedPoints);
		}

		public void SetPreviewPoints(int points)
		{
			var oldValue = PreviewPoints;
			PreviewPoints = points;
			OnPreviewPointsChanged?.Invoke(oldValue, PreviewPoints);
		}
		
		private void SetTurnPoints(int points)
		{
			var oldValue = TurnPoints;
			TurnPoints = points;
			OnTurnPointsChanged?.Invoke(oldValue, TurnPoints);
		}
		public void AddTurnPoints(int points)
		{
			SetTurnPoints(TurnPoints + points);
		}

		public void SendUpdateUI()
		{
			OnUpdateUI?.Invoke();
		}

		public void DisableButtons()
		{
			OnDisableButtons?.Invoke();
		}

		public void ResetTurn()
		{
			isFirstRoll = true;
			ResetAllPositions();
			SetTurnPoints(0);
			SetPreviewPoints(0);
		}

		public void Reset()
		{
			ResetAllPositions();
			SetTurnPoints(0);
			SetPreviewPoints(0);
			SetPlayerBankedPoints(0);
			SetEnemyBankedPoints(0);
		}
	}
}
