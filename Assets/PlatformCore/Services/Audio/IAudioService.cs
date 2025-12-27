using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlatformCore.Services.Audio
{
	public interface IAudioService
	{
	UniTask PlayMusicAsync(string eventPath, float fadeTime = 0f);
	UniTask StopMusicAsync(float fadeTime = 0f);
	void SetMusicVolume(float volume);

	void PlaySound(string eventPath);
	void PlaySoundAt(string eventPath, Vector3 position);

	void SetMasterVolume(float volume);
	void SetSfxVolume(float volume);
	void SetMuted(bool muted);

	bool IsMuted { get; }
	}
}