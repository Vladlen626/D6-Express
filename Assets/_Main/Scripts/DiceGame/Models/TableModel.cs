using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class TableModel
	{
		public event Action OnPlayerBankedPointsChanged;
		public event Action OnEnemyBankedPointsChanged;
		public event Action OnTurnPointsChanged;
		public event Action OnPreviewPointsChanged;
		public int TurnPoints { get; private set; }
		public int PreviewPoints { get; private set; }
		public int PlayerBankedPoints { get; private set; }
		
		public int EnemyBankedPoints {get; private set;}

		public bool isFirstRoll { get; set; } = true;

		private Transform[] _activePositions;
		private Transform[] _bankedPositions;

		private HashSet<Transform> _occupiedActivePositions = new HashSet<Transform>();
		private HashSet<Transform> _occupiedBankedPositions = new HashSet<Transform>();
		
		public TableModel(Transform[] activePositions, Transform[] bankedPositions)
		{
			_activePositions = activePositions;
			_bankedPositions = bankedPositions;
			ResetAllPositions();
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
			PlayerBankedPoints = points;
			OnPlayerBankedPointsChanged?.Invoke();
		}
		
		public void SetEnemyBankedPoints(int points)
		{
			EnemyBankedPoints = points;
			OnEnemyBankedPointsChanged?.Invoke();
		}

		public void SetPreviewPoints(int points)
		{
			PreviewPoints = points;
			OnPreviewPointsChanged?.Invoke();
		}
		public void AddTurnPoints(int points)
		{
			SetTurnPoints(TurnPoints + points);
		}
		
		private void SetTurnPoints(int points)
		{
			TurnPoints = points;
			OnTurnPointsChanged?.Invoke();
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
		}
	}
}