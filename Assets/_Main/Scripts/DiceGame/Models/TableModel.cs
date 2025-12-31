using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class TableModel
	{
		public event Action OnBankedPointsChanged;
		public event Action OnTurnPointsChanged;
		public event Action OnPreviewPointsChanged;
		public int TurnPoints { get; private set; }
		public int PreviewPoints { get; private set; }
		public int BankedPoints { get; private set; }

		private Transform[] _activePositions;
		private Transform[] _bankedPositions;

		private HashSet<Transform> _occupiedActivePositions = new HashSet<Transform>();
		private HashSet<Transform> _occupiedBankedPositions = new HashSet<Transform>();
		
		public TableModel(Transform[] activePositions, Transform[] bankedPositions)
		{
			Reset();
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
		
		public void ResetAllPositions()
		{
			_occupiedActivePositions.Clear();
			_occupiedBankedPositions.Clear();
		}

		public void AddBankedPoints(int points)
		{
			BankedPoints += points;
			OnBankedPointsChanged?.Invoke();
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

		public void Reset()
		{
			ResetAllPositions();
			SetTurnPoints(0);
			SetPreviewPoints(0);
			BankedPoints = 0;
			OnBankedPointsChanged?.Invoke();
		}
	}
}