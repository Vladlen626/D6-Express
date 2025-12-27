using System;
using UnityEngine;

namespace _Main.Scripts.Core.Services
{
	public interface IInputService
	{
		public event Action OnJumpPressed;
		public event Action OnJumpReleased;
		public event Action OnPausePressed;
		public event Action OnInteractPressed;
		public Vector2 Move { get; }
		public Vector2 Look { get; }

		public bool IsJumping { get; }
		public bool IsSprinting { get; }
		public bool IsInteract { get; }

		void EnableAllInputs();
		void DisableAllInputs();

		void EnablePlayerInputs();
		void DisablePlayerInputs();

		void EnableUIInputs();
		void DisableUIInputs();
	}
}