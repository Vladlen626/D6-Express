using System;
using PlatformCore.Services.UI;
using UnityEngine;
using UnityEngine.UI;

public class UISettings : UIBaseElement
{
	[Header("Controls")]
	[SerializeField] private Slider _master;
	[SerializeField] private Slider _music;
	[SerializeField] private Slider _sfx;

	public event Action<float> OnMasterChanged;
	public event Action<float> OnMusicChanged;
	public event Action<float> OnSfxChanged;
	public event Action OnCloseClicked;
	public event Action OnMainMenuClicked;

	protected override void OnAwake()
	{
		_master.onValueChanged.AddListener(v => OnMasterChanged?.Invoke(v));
		_music.onValueChanged.AddListener(v => OnMusicChanged?.Invoke(v));
		_sfx.onValueChanged.AddListener(v => OnSfxChanged?.Invoke(v));
	}

	public void SetValues(float master, float music, float sfx)
	{
		_master.SetValueWithoutNotify(master);
		_music.SetValueWithoutNotify(music);
		_sfx.SetValueWithoutNotify(sfx);
	}

	public void Close()
	{
		OnCloseClicked?.Invoke();
	}

	public void MainMenu()
	{
		OnMainMenuClicked?.Invoke();
	}
}