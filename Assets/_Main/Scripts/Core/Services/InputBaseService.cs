using System;
using UnityEngine;

namespace _Main.Scripts.Core.Services
{
	public class InputBaseService : IInputService, ISyncInitializable
	{
		public event Action OnJumpPressed;
		public event Action OnJumpReleased;
		public event Action OnPausePressed;
		public event Action OnInteractPressed;
		public event Action OnInteractPerformed;
		public event Action<Vector2> OnMoved;
		public event Action<Vector2> OnLooked;

		public event Action OnDebugSwitchPressed;

		public event Action OnSpeechLineSkip;

		public Vector2 Move { get; private set; }
		public Vector2 Look { get; private set; }
		public bool IsJumping { get; private set; }
		public bool IsSprinting { get; private set; }
		public bool IsInteract { get; private set; }

		private InputSystem_Actions _actions;
		private Vector2 _moveVector;
		private Vector2 _lookInput;
		private bool _cancelInput;

		public void Initialize()
		{
			_actions = new InputSystem_Actions();

			BindActions();
			EnableAllInputs();
		}

		public void EnableAllInputs()
		{
			EnableUIInputs();
			EnablePlayerInputs();
			EnableDebugInputs();
		}

		public void DisableAllInputs()
		{
			DisableUIInputs();
			DisablePlayerInputs();
			DisableDebugInputs();
		}

		public void EnableUIInputs()
		{
			_actions.UI.Enable();
		}

		public void DisableUIInputs()
		{
			_actions.UI.Disable();
		}

		public void EnablePlayerInputs()
		{
			_actions.Player.Enable();
		}

		public void DisablePlayerInputs()
		{
			_actions.Player.Disable();
		}

		public void EnableCameraInputs()
		{
			_actions.Player.Look.Enable();
		}

		public void DisableCameraInputs()
		{
			_actions.Player.Look.Disable();
		}

		public void EnableDebugInputs()
		{
			_actions.Debug.Enable();
		}

		public void DisableDebugInputs()
		{
			_actions.Debug.Disable();
		}

		private void BindActions()
		{
			_actions.Player.Move.performed += ctx =>
			{
				Move = ctx.ReadValue<Vector2>();
				OnMoved?.Invoke(Move);
			};
			_actions.Player.Move.canceled += _ => { Move = Vector2.zero; };

			_actions.Player.Sprint.performed += _ => { IsSprinting = true; };
			_actions.Player.Sprint.canceled += _ => { IsSprinting = false; };

			_actions.Player.Look.performed += ctx =>
			{
				Look = ctx.ReadValue<Vector2>();
				OnLooked?.Invoke(Look);
			};
			_actions.Player.Look.canceled += _ => { Look = Vector2.zero; };

			_actions.Player.Interact.started += _ =>
			{
				IsInteract = true;
				OnInteractPressed?.Invoke();
			};

			_actions.Player.Interact.canceled += _ =>
			{
				IsInteract = false;
			};
			
			_actions.Player.Interact.performed += _ =>
			{
				OnInteractPerformed?.Invoke();
			};

			_actions.Player.Jump.started += _ =>
			{
				IsJumping = true;
				OnJumpPressed?.Invoke();
			};
			_actions.Player.Jump.canceled += _ =>
			{
				IsJumping = false;
				OnJumpReleased?.Invoke();
			};

			_actions.UI.Cancel.started += _ => OnPausePressed?.Invoke();

			_actions.Debug.Switch.started += _ =>
			{
				OnDebugSwitchPressed?.Invoke();
			};

			_actions.UI.SpeechLineSkip.started += _ =>
			{
				OnSpeechLineSkip?.Invoke();
			};
		}

		public void Dispose()
		{
			if (_actions != null)
			{
				_actions.Player.Disable();
				_actions.UI.Disable();
				_actions.Dispose();
				_actions = null;
			}
		}
	}
}