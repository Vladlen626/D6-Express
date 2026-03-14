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
		public event Action OnInteractPerformed;
		public event Action<Vector2> OnMoved;
		public event Action<Vector2> OnLooked;
		public event Action OnFarted;

		public event Action OnDiceGameNext;
		public event Action OnDiceGamePrevious;

		public event Action OnUISubmit;
		public event Action OnUICancel;
		
		public event Action OnDebugSwitchPressed;

		public event Action OnSpeechLineSkip;
		public event Action OnSpeechAccept;
		public event Action OnSpeechDecline;

		public Vector2 Move { get; }
		public Vector2 Look { get; }

		public bool IsJumping { get; }
		public bool IsSprinting { get; }
		public bool IsInteract { get; }

		void EnableAllInputs();
		void DisableAllInputs();

		void EnableCameraInputs();
		void DisableCameraInputs();

		void EnablePlayerInputs();
		void DisablePlayerInputs();

		void EnableUIInputs();
		void DisableUIInputs();

		void EnableSpeechInputs();
		void DisableSpeechInputs();

		void EnableDiceGameInputs();
		void DisableDiceGameInputs();
	}
}