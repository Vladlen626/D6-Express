using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using FMODUnity;

namespace PlatformCore.Services.Audio
{
	public class AudioBaseService : IAudioService, IService
	{
		private readonly ILoggerService _logger;

		private FMOD.Studio.EventInstance _currentMusic;
		private float _masterVolume = 0.5f;
		private float _musicVolume = 0.5f;
		private float _sfxVolume = 0.5f;
		private bool _isMuted = false;

		public bool IsMuted => _isMuted;

		public AudioBaseService(ILoggerService logger)
		{
			_logger = logger;
		}

		public async UniTask PlayMusicAsync(string eventPath, float fadeTime = 1f)
		{
			_logger?.Log($"[AudioService] Playing music: {eventPath}");

			try
			{
				if (_currentMusic.isValid())
				{
					_currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
					_currentMusic.release();
				}
				
				_currentMusic = RuntimeManager.CreateInstance(eventPath);
				_currentMusic.start();

				if (fadeTime > 0f)
				{
					await UniTask.Delay(TimeSpan.FromSeconds(fadeTime));
				}
			}
			catch (Exception ex)
			{
				_logger?.LogError($"[AudioService] Failed to play music: {ex.Message}");
			}
		}

		public async UniTask StopMusicAsync(float fadeTime = 1f)
		{
			await UniTask.Yield();
			if (!_currentMusic.isValid()) return;

			_logger?.Log("[AudioService] Stopping music");

			try
			{
				_currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

				_currentMusic.release();
				_currentMusic = new FMOD.Studio.EventInstance();
			}
			catch (Exception ex)
			{
				_logger?.LogError($"[AudioService] Failed to stop music: {ex.Message}");
			}
		}

		public void PlaySound(string eventPath)
		{
			try
			{
				RuntimeManager.PlayOneShot(eventPath);
			}
			catch (Exception ex)
			{
				_logger?.LogError($"[AudioService] Failed to play sound {eventPath}: {ex.Message}");
			}
		}

		public void PlaySoundAt(string eventPath, Vector3 position)
		{
			try
			{
				RuntimeManager.PlayOneShot(eventPath, position);
			}
			catch (Exception ex)
			{
				_logger?.LogError($"[AudioService] Failed to play sound at position {eventPath}: {ex.Message}");
			}
		}

		public void SetMasterVolume(float volume)
		{
			_masterVolume = Mathf.Clamp01(volume);
			ApplyVolume();
		}

		public void SetMusicVolume(float volume)
		{
			_musicVolume = Mathf.Clamp01(volume);
			ApplyVolume();
		}

		public void SetSfxVolume(float volume)
		{
			_sfxVolume = Mathf.Clamp01(volume);
			ApplyVolume();
		}

		public void SetMuted(bool muted)
		{
			_isMuted = muted;
			ApplyVolume();
			_logger?.Log($"[AudioService] Audio {(muted ? "muted" : "unmuted")}");
		}

		private void ApplyVolume()
		{
			try
			{
				float finalVolume = _isMuted ? 0f : _masterVolume;

				var masterBus = RuntimeManager.GetBus("bus:/");
				masterBus.setVolume(finalVolume);

				var musicBus = RuntimeManager.GetBus("bus:/Music");
				musicBus.setVolume(_musicVolume);

				var sfxBus = RuntimeManager.GetBus("bus:/SFX");
				sfxBus.setVolume(_sfxVolume);
			}
			catch (Exception ex)
			{
				_logger?.LogError($"[AudioService] Failed to apply volume: {ex.Message}");
			}
		}

		public void Dispose()
		{
			if (_currentMusic.isValid())
			{
				_currentMusic.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
				_currentMusic.release();
			}

			_logger?.Log("[AudioService] Disposed");
		}
	}
}