using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class CreatePastelGlobalVolumeTool
{
	private const string VolumeName = "Global Volume - Pastel Warm";

	[MenuItem("Tools/Create Pastel Global Volume (URP)")]
	private static void CreateOrUpdate()
	{
		Volume volume = FindOrCreateVolume();
		SetupVolume(volume);
	}

	private static Volume FindOrCreateVolume()
	{
		Volume[] volumes = Object.FindObjectsOfType<Volume>(true);
		foreach (Volume v in volumes)
		{
			if (v.isGlobal && v.gameObject.name == VolumeName)
			{
				return v;
			}
		}

		GameObject go = new GameObject(VolumeName);
		Undo.RegisterCreatedObjectUndo(go, "Create Global Volume");

		Volume volume = go.AddComponent<Volume>();
		volume.isGlobal = true;
		volume.priority = 10f;
		volume.weight = 1f;

		volume.sharedProfile = ScriptableObject.CreateInstance<VolumeProfile>();

		return volume;
	}

	private static void SetupVolume(Volume volume)
	{
		VolumeProfile profile = volume.sharedProfile;
		if (profile == null)
		{
			profile = ScriptableObject.CreateInstance<VolumeProfile>();
			volume.sharedProfile = profile;
		}

		// Tonemapping
		Tonemapping tonemapping = GetOrAdd<Tonemapping>(profile);
		tonemapping.mode.Override(TonemappingMode.ACES);
		tonemapping.active = true;

		// Color Adjustments
		ColorAdjustments color = GetOrAdd<ColorAdjustments>(profile);
		color.postExposure.Override(0.25f);
		color.contrast.Override(-10f);
		color.saturation.Override(-12f);
		color.active = true;

		// White Balance
		WhiteBalance wb = GetOrAdd<WhiteBalance>(profile);
		wb.temperature.Override(22f);
		wb.tint.Override(-4f);
		wb.active = true;

		// Lift / Gamma / Gain
		LiftGammaGain lgg = GetOrAdd<LiftGammaGain>(profile);
		lgg.lift.Override(new Vector4(0.035f, 0.025f, 0.015f, 0f));
		lgg.gamma.Override(new Vector4(0.030f, 0.015f, -0.005f, 0f));
		lgg.gain.Override(new Vector4(0.015f, 0.010f, 0.000f, 0f));
		lgg.active = true;

		// Bloom (очень мягкий)
		Bloom bloom = GetOrAdd<Bloom>(profile);
		bloom.intensity.Override(0.25f);
		bloom.threshold.Override(1.05f);
		bloom.scatter.Override(0.55f);
		bloom.tint.Override(Color.white);
		bloom.active = true;

		EditorUtility.SetDirty(volume);
		EditorUtility.SetDirty(profile);
	}

	private static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
	{
		if (profile.TryGet(out T existing))
		{
			return existing;
		}

		return profile.Add<T>(true);
	}
}