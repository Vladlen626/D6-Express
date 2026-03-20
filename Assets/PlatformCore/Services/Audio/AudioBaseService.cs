using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FMOD;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;

namespace PlatformCore.Services.Audio
{
	public class AudioBaseService : IAudioService, IService
	{
		private readonly ILoggerService _logger;

		private EventInstance _currentMusic;
		private float _masterVolume = 0.8f;
		private float _musicVolume = 0.5f;
		private float _sfxVolume = 0.5f;
		private bool _isMuted;
		
		private Dictionary<string, EventInstance> _eventInstances = new ();
		private readonly HashSet<string> _prewarmedEvents = new(StringComparer.Ordinal);

		public bool IsMuted => _isMuted;
		public float MasterVolume => _masterVolume;
		public float MusicVolume => _musicVolume;
		public float SfxVolume => _sfxVolume;

		public AudioBaseService(ILoggerService logger)
		{
			_logger = logger;
		}

		public async UniTask PrewarmEventAsync(string eventPath)
		{
			if (string.IsNullOrWhiteSpace(eventPath) || _prewarmedEvents.Contains(eventPath))
			{
				return;
			}

			try
			{
				var eventDescription = RuntimeManager.GetEventDescription(eventPath);
				if (!eventDescription.isValid())
				{
					_logger?.LogError($"[AudioService] Failed to prewarm event, invalid description: {eventPath}");
					return;
				}

				var loadResult = eventDescription.loadSampleData();
				if (loadResult != RESULT.OK)
				{
					_logger?.Log($"[AudioService] Sample prewarm request returned {loadResult} for {eventPath}");
				}

				const int maxFramesToWait = 120;
				for (int frame = 0; frame < maxFramesToWait; frame++)
				{
					var stateResult = eventDescription.getSampleLoadingState(out var loadingState);
					if (stateResult != RESULT.OK)
					{
						_logger?.LogError($"[AudioService] Failed to check sample loading state for {eventPath}: {stateResult}");
						return;
					}

					if (loadingState == LOADING_STATE.LOADED)
					{
						_prewarmedEvents.Add(eventPath);
						return;
					}

					if (loadingState == LOADING_STATE.UNLOADED)
					{
						break;
					}

					await UniTask.Yield();
				}

				_prewarmedEvents.Add(eventPath);
			}
			catch (Exception ex)
			{
				_logger?.LogError($"[AudioService] Failed to prewarm event {eventPath}: {ex.Message}");
			}
		}

		public async UniTask PlayMusicAsync(string eventPath, float fadeTime = 1f)
		{
			_logger?.Log($"[AudioService] Playing music: {eventPath}");

			try
			{
				await PrewarmEventAsync(eventPath);

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

		public void PlaySoundParallel(string eventPath)
		{
			if (!_eventInstances.TryGetValue(eventPath, out var sound))
			{
				sound = RuntimeManager.CreateInstance(eventPath);
				_eventInstances.Add(eventPath, sound);
			}

			sound.start();
		}

		public void StopParallelSound(string eventPath)
		{
			if (!_eventInstances.TryGetValue(eventPath, out var sound))
			{
				_logger?.Log($"[AudioService] Failed to stop: {eventPath}");
				return;
			}

			sound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
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
				_currentMusic = new EventInstance();
			}
			catch (Exception ex)
			{
				_logger?.LogError($"[AudioService] Failed to stop music: {ex.Message}");
			}
		}

		// ReSharper disable Unity.PerformanceAnalysis
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

		// ReSharper disable Unity.PerformanceAnalysis
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
