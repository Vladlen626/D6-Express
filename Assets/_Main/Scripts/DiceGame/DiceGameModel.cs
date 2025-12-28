using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceGameModel
	{
		private const int TARGET_SCORE = 4000;
		public event Action OnBankedPointsChanged;
		public event Action OnTargetPointsChanged;
		public event Action OnGameConditionPassed;
		
		public int BankedPoints { get; private set; }
		public int TargetPoints { get; private set; }
		public int BankedDiceCount => _occupiedBankedPositions.Count;

		public int ActiveDiceCount => _occupiedActivePositions.Count;

		private Transform[] _activePositions;
		private Transform[] _bankedPositions;

		private HashSet<Transform> _occupiedActivePositions = new HashSet<Transform>();
		private HashSet<Transform> _occupiedBankedPositions = new HashSet<Transform>();
		
		public DiceGameModel(Transform[] activePositions, Transform[] bankedPositions)
		{
			_activePositions = activePositions;
			_bankedPositions = bankedPositions;

			_occupiedActivePositions.Clear();
			_occupiedBankedPositions.Clear();
			
			TargetPoints = TARGET_SCORE;
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

		public void AddBankedPoints(int points)
		{
			BankedPoints += points;
			CheckPointsCondition();
			OnBankedPointsChanged?.Invoke();
		}

		public void Reset()
		{
			ResetAllPositions();
			BankedPoints = 0;
			TargetPoints = 0;
			OnBankedPointsChanged?.Invoke();
			OnTargetPointsChanged?.Invoke();
		}

		private void CheckPointsCondition()
		{
			if (BankedPoints >= TARGET_SCORE)
			{
				OnGameConditionPassed?.Invoke();
			}
		}
	}
}