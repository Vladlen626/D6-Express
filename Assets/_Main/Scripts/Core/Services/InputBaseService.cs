using System;
using PlatformCore.Services;
using Unity.VisualScripting;
using UnityEngine;

namespace _Main.Scripts.Core.Services
{
	public class InputBaseService : IInputService, ISyncInitializable
	{
		private enum InputType
		{
			UI,
			PLAYER,
			DICE_GAME,
			DEBUG,
			SPEECH,
			LOOK
		}

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

		public Vector2 Move { get; private set; }
		public Vector2 Look { get; private set; }
		public bool IsJumping { get; private set; }
		public bool IsSprinting { get; private set; }
		public bool IsInteract { get; private set; }

		private InputSystem_Actions _actions;
		private Vector2 _moveVector;
		private Vector2 _lookInput;
		private bool _cancelInput;

		private readonly int[] lockCount = new int[Enum.GetValues(typeof(InputType)).Length];

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
			EnableSpeechInputs();
		}

		public void DisableAllInputs()
		{
			DisableUIInputs();
			DisablePlayerInputs();
			DisableDebugInputs();
			DisableSpeechInputs();
		}

		public void EnableDiceGameInputs()
		{
			if (TryUnlock(InputType.DICE_GAME))
			{
				_actions.DiceGame.Enable();
			}
		}

		public void DisableDiceGameInputs()
		{
			if (TryLock(InputType.DICE_GAME))
			{
				_actions.DiceGame.Disable();
			}
		}

		public void EnableUIInputs()
		{
			if (TryUnlock(InputType.UI))
			{
				_actions.UI.Enable();
			}
		}

		public void DisableUIInputs()
		{
			if (TryLock(InputType.UI))
			{
				_actions.UI.Disable();
			}
		}

		public void EnablePlayerInputs()
		{
			if (TryUnlock(InputType.PLAYER))
			{
				_actions.Player.Enable();
			}
		}

		public void DisablePlayerInputs()
		{
			if (TryLock(InputType.PLAYER))
			{
				_actions.Player.Disable();
			}
		}

		public void EnableCameraInputs()
		{
			if (TryUnlock(InputType.LOOK))
			{
				_actions.Player.Look.Enable();
			}
		}

		public void DisableCameraInputs()
		{
			if (TryLock(InputType.LOOK))
			{
				_actions.Player.Look.Disable();
			}
		}

		public void EnableDebugInputs()
		{
			if (TryUnlock(InputType.DEBUG))
			{
				_actions.Debug.Enable();
			}
		}

		public void DisableDebugInputs()
		{
			if (TryLock(InputType.DEBUG))
			{
				_actions.Debug.Disable();
			}
		}

		public void EnableSpeechInputs()
		{
			if (TryUnlock(InputType.SPEECH))
			{
				_actions.Speech.Enable();
			}
		}

		public void DisableSpeechInputs()
		{
			if (TryLock(InputType.SPEECH))
			{
				_actions.Speech.Disable();
			}
		}

		private bool TryLock(InputType type)
		{
			ref int c = ref lockCount[(int)type];
			c++;
			return c == 1;
		}

		private bool TryUnlock(InputType type)
		{
			ref int c = ref lockCount[(int)type];
			if (c == 0)
			{
				return true;
			}
			else
			{
				c--;
				return c == 0;
			}
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

			_actions.UI.Cancel.started += _ =>
			{
				OnUICancel?.Invoke();
			};

			_actions.UI.Submit.started += _ =>
			{
				OnUISubmit?.Invoke();
			};

			_actions.UI.Pause.started += _ =>
			{
				OnPausePressed?.Invoke();
			};

			_actions.Debug.Switch.started += _ =>
			{
				OnDebugSwitchPressed?.Invoke();
			};

			_actions.Speech.SkipLine.performed += _ =>
			{
				OnSpeechLineSkip?.Invoke();
			};

			_actions.Speech.Accept.performed += _ =>
			{
				OnSpeechAccept?.Invoke();
			};

			_actions.Speech.Decline.performed += _ =>
			{
				OnSpeechDecline?.Invoke();
			};

			_actions.Player.Fart.performed += _ =>
			{
				OnFarted?.Invoke();
			};

			_actions.DiceGame.Next.performed += _ =>
			{
				OnDiceGameNext?.Invoke();
			};

			_actions.DiceGame.Previous.performed += _ =>
			{
				OnDiceGamePrevious?.Invoke();
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