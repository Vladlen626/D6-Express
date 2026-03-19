using System;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceItemTargetingView : MonoBehaviour
	{
		[SerializeField] private LineRenderer bodyRenderer;
		[SerializeField] private LineRenderer tipRenderer;
		[Min(2)]
		[SerializeField] private int bodySegments = 18;
		[SerializeField] private float sourceYOffset = 0.08f;
		[SerializeField] private float targetYOffset = 0.02f;
		[SerializeField] private float arcHeight = 0.22f;
		[SerializeField] private float arcHeightByDistance = 0.08f;
		[SerializeField] private float arcWaveAmplitude = 0.015f;
		[SerializeField] private float arcWaveFrequency = 3.2f;
		[Min(0.001f)]
		[SerializeField] private float bodyWidth = 0.035f;
		[SerializeField] private float tipLength = 0.15f;
		[Min(0.001f)]
		[SerializeField] private float tipBaseWidth = 0.1f;
		[Min(0f)]
		[SerializeField] private float tipBodyGap = 0.008f;
		[SerializeField] private LineAlignment bodyAlignment = LineAlignment.View;
		[SerializeField] private LineAlignment tipAlignment = LineAlignment.View;
		[SerializeField] private int bodyCapVertices = 0;
		[SerializeField] private int tipCapVertices = 2;
		[Min(1)]
		[SerializeField] private int dashCount = 12;
		[Min(4)]
		[SerializeField] private int dashResolutionPerSegment = 20;
		[Range(0.1f, 1f)]
		[SerializeField] private float dashFill = 0.56f;
		[SerializeField] private bool autoScaleDashCountByLength = true;
		[Min(0.01f)]
		[SerializeField] private float dashReferenceLength = 1f;
		[SerializeField] private float dashScrollSpeed = 1.4f;

		private static readonly int BaseMapPropertyId = Shader.PropertyToID("_BaseMap");
		private static readonly int MainTexPropertyId = Shader.PropertyToID("_MainTex");

		private Material bodyRuntimeMaterial;
		private Texture2D dashRuntimeTexture;
		private bool hasBaseMapProperty;
		private bool hasMainTexProperty;
		private int cachedDashCount = -1;
		private int cachedDashResolutionPerSegment = -1;
		private float cachedDashFill = -1f;
		private int cachedRuntimeDashCount = -1;
		private bool isInitialized;

		public float SourceYOffset => sourceYOffset;
		public float TargetYOffset => targetYOffset;

		public void EnsureConfiguredOrThrow()
		{
			if (!bodyRenderer)
			{
				throw new InvalidOperationException("[DiceItemTargetingView] Body LineRenderer reference is not assigned.");
			}

			if (!tipRenderer)
			{
				throw new InvalidOperationException("[DiceItemTargetingView] Tip LineRenderer reference is not assigned.");
			}

			if (bodySegments < 2)
			{
				throw new InvalidOperationException("[DiceItemTargetingView] Body segments must be >= 2.");
			}

			if (bodyRenderer.positionCount != bodySegments)
			{
				bodyRenderer.positionCount = bodySegments;
			}

			if (tipRenderer.positionCount != 2)
			{
				tipRenderer.positionCount = 2;
			}

			if (!bodyRenderer.sharedMaterial)
			{
				throw new InvalidOperationException("[DiceItemTargetingView] Body LineRenderer material is not assigned.");
			}

			if (!tipRenderer.sharedMaterial)
			{
				throw new InvalidOperationException("[DiceItemTargetingView] Tip LineRenderer material is not assigned.");
			}

			if (isInitialized)
			{
				return;
			}

			bodyRuntimeMaterial = new Material(bodyRenderer.sharedMaterial);
			bodyRenderer.material = bodyRuntimeMaterial;
			bodyRenderer.textureMode = LineTextureMode.Stretch;

			hasBaseMapProperty = bodyRuntimeMaterial.HasProperty(BaseMapPropertyId);
			hasMainTexProperty = bodyRuntimeMaterial.HasProperty(MainTexPropertyId);
			if (!hasBaseMapProperty && !hasMainTexProperty)
			{
				throw new InvalidOperationException("[DiceItemTargetingView] Body LineRenderer material must expose _BaseMap or _MainTex.");
			}

			RebuildDashTexture(force: true, dashCount);
			ApplyRendererShapeSettings();
			isInitialized = true;
		}

		public void SetVisible(bool visible)
		{
			EnsureConfiguredOrThrow();
			bodyRenderer.enabled = visible;
			tipRenderer.enabled = visible;
		}

		public void SetPoints(Vector3 sourceWorld, Vector3 targetWorld)
		{
			EnsureConfiguredOrThrow();
			ApplyRendererShapeSettings();

			sourceWorld.y += sourceYOffset;
			targetWorld.y += targetYOffset;

			var distance = Vector3.Distance(sourceWorld, targetWorld);
			var arcWave = Mathf.Sin(Time.unscaledTime * arcWaveFrequency) * arcWaveAmplitude;
			var finalArcHeight = arcHeight + distance * arcHeightByDistance + arcWave;
			var controlPoint = (sourceWorld + targetWorld) * 0.5f + Vector3.up * finalArcHeight;
			var derivativeAtEnd = targetWorld - controlPoint;
			if (derivativeAtEnd.sqrMagnitude <= 0.00001f)
			{
				derivativeAtEnd = targetWorld - sourceWorld;
			}

			if (derivativeAtEnd.sqrMagnitude <= 0.00001f)
			{
				derivativeAtEnd = Vector3.forward;
			}

			derivativeAtEnd.Normalize();

			var tipBackCenter = targetWorld - derivativeAtEnd * tipLength;
			var bodyEndPoint = tipBackCenter - derivativeAtEnd * tipBodyGap;

			var bodyLength = 0f;
			Vector3 previousPoint = sourceWorld;
			for (var i = 0; i < bodySegments; i++)
			{
				var t = i / (float)(bodySegments - 1);
				var point = EvaluateQuadraticBezier(sourceWorld, controlPoint, bodyEndPoint, t);
				bodyRenderer.SetPosition(i, point);
				if (i > 0)
				{
					bodyLength += Vector3.Distance(previousPoint, point);
				}

				previousPoint = point;
			}

			tipRenderer.SetPosition(0, tipBackCenter);
			tipRenderer.SetPosition(1, targetWorld);

			var runtimeDashCount = CalculateRuntimeDashCount(bodyLength);
			UpdateDashAnimation(runtimeDashCount);
			UpdateTipWidth();
			UpdateWidths();
		}

		private void OnDestroy()
		{
			if (bodyRuntimeMaterial)
			{
				Destroy(bodyRuntimeMaterial);
			}

			if (dashRuntimeTexture)
			{
				Destroy(dashRuntimeTexture);
			}
		}

		private static Vector3 EvaluateQuadraticBezier(Vector3 start, Vector3 control, Vector3 end, float t)
		{
			var oneMinusT = 1f - t;
			return oneMinusT * oneMinusT * start + 2f * oneMinusT * t * control + t * t * end;
		}

		private void UpdateDashAnimation(int runtimeDashCount)
		{
			RebuildDashTexture(force: false, runtimeDashCount);

			if (!bodyRuntimeMaterial)
			{
				return;
			}

			var offset = new Vector2(-Time.unscaledTime * dashScrollSpeed, 0f);
			if (hasBaseMapProperty)
			{
				bodyRuntimeMaterial.SetTextureOffset(BaseMapPropertyId, offset);
			}

			if (hasMainTexProperty)
			{
				bodyRuntimeMaterial.SetTextureOffset(MainTexPropertyId, offset);
			}
		}

		private void UpdateWidths()
		{
			bodyRenderer.startWidth = bodyWidth;
			bodyRenderer.endWidth = bodyWidth;
		}

		private void UpdateTipWidth()
		{
			tipRenderer.startWidth = tipBaseWidth;
			tipRenderer.endWidth = 0f;
		}

		private void RebuildDashTexture(bool force, int runtimeDashCount)
		{
			if (dashCount < 1)
			{
				throw new InvalidOperationException("[DiceItemTargetingView] Dash count must be >= 1.");
			}

			if (dashResolutionPerSegment < 4)
			{
				throw new InvalidOperationException("[DiceItemTargetingView] Dash resolution per segment must be >= 4.");
			}

			if (runtimeDashCount < 1)
			{
				throw new InvalidOperationException("[DiceItemTargetingView] Runtime dash count must be >= 1.");
			}

			if (!force &&
			    cachedDashCount == dashCount &&
			    cachedDashResolutionPerSegment == dashResolutionPerSegment &&
			    Mathf.Approximately(cachedDashFill, dashFill) &&
			    cachedRuntimeDashCount == runtimeDashCount)
			{
				return;
			}

			var width = runtimeDashCount * dashResolutionPerSegment;
			const int height = 2;

			if (!dashRuntimeTexture || dashRuntimeTexture.width != width || dashRuntimeTexture.height != height)
			{
				if (dashRuntimeTexture)
				{
					Destroy(dashRuntimeTexture);
				}

				dashRuntimeTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
				{
					wrapMode = TextureWrapMode.Repeat,
					filterMode = FilterMode.Bilinear
				};
			}

			var colors = new Color[width * height];
			for (var x = 0; x < width; x++)
			{
				var local = (x % dashResolutionPerSegment) / (float)(dashResolutionPerSegment - 1);
				var alpha = EvaluateDashAlpha(local, dashFill);
				var color = new Color(1f, 1f, 1f, alpha);

				for (var y = 0; y < height; y++)
				{
					colors[y * width + x] = color;
				}
			}

			dashRuntimeTexture.SetPixels(colors);
			dashRuntimeTexture.Apply(false, false);

			if (hasBaseMapProperty)
			{
				bodyRuntimeMaterial.SetTexture(BaseMapPropertyId, dashRuntimeTexture);
			}

			if (hasMainTexProperty)
			{
				bodyRuntimeMaterial.SetTexture(MainTexPropertyId, dashRuntimeTexture);
			}

			cachedDashCount = dashCount;
			cachedDashResolutionPerSegment = dashResolutionPerSegment;
			cachedDashFill = dashFill;
			cachedRuntimeDashCount = runtimeDashCount;
		}

		private void ApplyRendererShapeSettings()
		{
			bodyRenderer.alignment = bodyAlignment;
			tipRenderer.alignment = tipAlignment;
			bodyRenderer.numCapVertices = Mathf.Max(0, bodyCapVertices);
			tipRenderer.numCapVertices = Mathf.Max(0, tipCapVertices);
		}

		private static float EvaluateDashAlpha(float local, float fill)
		{
			if (local >= fill)
			{
				return 0f;
			}

			return 1f;
		}

		private int CalculateRuntimeDashCount(float bodyLength)
		{
			if (!autoScaleDashCountByLength)
			{
				return dashCount;
			}

			if (dashReferenceLength <= 0f)
			{
				throw new InvalidOperationException("[DiceItemTargetingView] Dash reference length must be > 0.");
			}

			var lengthScale = bodyLength / dashReferenceLength;
			var scaledCount = Mathf.RoundToInt(dashCount * lengthScale);
			if (scaledCount < dashCount)
			{
				return dashCount;
			}

			return scaledCount;
		}
	}
}
