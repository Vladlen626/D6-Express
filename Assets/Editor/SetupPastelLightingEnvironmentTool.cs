using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class SetupPastelLightingEnvironmentTool
{
	// Если хочешь, впиши сюда точное имя skybox-материала из проекта
	private const string SkyboxMaterialName = "ES000_Day_01";

	[MenuItem("Tools/Setup Pastel Lighting Environment")]
	private static void Setup()
	{
		TryAssignSkyboxByName(SkyboxMaterialName);
		TryAssignSunSourceDirectional();

		// Realtime Shadow Color (в Lighting окне называется именно так)
		// В Unity это RenderSettings.subtractiveShadowColor (используется и для смешанного света/теней)
		RenderSettings.subtractiveShadowColor = new Color(0.65f, 0.70f, 0.75f, 1f);

		// Environment Lighting: Gradient
		RenderSettings.ambientMode = AmbientMode.Trilight;

		// Под стиль референса: тёплый верх, нейтральная середина, тёплая земля
		RenderSettings.ambientSkyColor = new Color(0.95f, 0.90f, 0.84f, 1f);      // кремовый
		RenderSettings.ambientEquatorColor = new Color(0.78f, 0.78f, 0.74f, 1f);  // тёплый серый
		RenderSettings.ambientGroundColor = new Color(0.70f, 0.62f, 0.52f, 1f);   // песочный

		// Environment Reflections
		RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
		RenderSettings.defaultReflectionResolution = 512;
		RenderSettings.reflectionIntensity = 0.4f;
		RenderSettings.reflectionBounces = 5;

		// Other Settings
		RenderSettings.fog = false;

		RenderSettings.haloStrength = 0.5f;
		RenderSettings.flareFadeSpeed = 3f;
		RenderSettings.flareStrength = 1f;

		EditorApplication.QueuePlayerLoopUpdate();
		SceneView.RepaintAll();
	}

	private static void TryAssignSkyboxByName(string materialName)
	{
		if (string.IsNullOrWhiteSpace(materialName))
		{
			return;
		}

		// Ищем материал по имени в проекте
		string[] guids = AssetDatabase.FindAssets($"{materialName} t:Material");
		if (guids == null || guids.Length == 0)
		{
			return;
		}

		string path = AssetDatabase.GUIDToAssetPath(guids[0]);
		Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
		if (mat == null)
		{
			return;
		}

		RenderSettings.skybox = mat;
		EditorUtility.SetDirty(RenderSettings.skybox);
	}

	private static void TryAssignSunSourceDirectional()
	{
		// Если уже назначен солнце-источник, не трогаем
		if (RenderSettings.sun != null)
		{
			return;
		}

		Light[] lights = Object.FindObjectsOfType<Light>(true);
		for (int i = 0; i < lights.Length; i++)
		{
			if (lights[i] != null && lights[i].type == LightType.Directional)
			{
				RenderSettings.sun = lights[i];
				return;
			}
		}
	}
}