using System;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	public class DiceItemTargetingView : MonoBehaviour
	{
		[SerializeField] private LineRenderer bodyRenderer;
		[SerializeField] private LineRenderer tipRenderer;
		[SerializeField] private int bodySegments = 18;
		[SerializeField] private float sourceYOffset = 0.08f;
		[SerializeField] private float targetYOffset = 0.02f;
		[SerializeField] private float arcHeight = 0.22f;
		[SerializeField] private float arcHeightByDistance = 0.08f;
		[SerializeField] private float tipLength = 0.15f;
		[SerializeField] private float tipHalfWidth = 0.06f;
		[SerializeField] private float dashScrollSpeed = 1.4f;
		[SerializeField] private float tipPulseAmplitude = 0.12f;
		[SerializeField] private float tipPulseFrequency = 7f;

		private static readonly int BaseMapPropertyId = Shader.PropertyToID("_BaseMap");
		private static readonly int MainTexPropertyId = Shader.PropertyToID("_MainTex");

		private Material bodyRuntimeMaterial;
		private float baseTipWidthMultiplier = 1f;
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

			if (tipRenderer.positionCount < 4)
			{
				tipRenderer.positionCount = 4;
			}

			if (isInitialized)
			{
				return;
			}

			if (!bodyRenderer.sharedMaterial)
			{
				throw new InvalidOperationException("[DiceItemTargetingView] Body LineRenderer material is not assigned.");
			}

			bodyRuntimeMaterial = new Material(bodyRenderer.sharedMaterial);
			bodyRenderer.material = bodyRuntimeMaterial;
			baseTipWidthMultiplier = tipRenderer.widthMultiplier;
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

			sourceWorld.y += sourceYOffset;
			targetWorld.y += targetYOffset;

			var distance = Vector3.Distance(sourceWorld, targetWorld);
			var finalArcHeight = arcHeight + distance * arcHeightByDistance;
			var controlPoint = (sourceWorld + targetWorld) * 0.5f + Vector3.up * finalArcHeight;

			for (var i = 0; i < bodySegments; i++)
			{
				var t = i / (float)(bodySegments - 1);
				var point = EvaluateQuadraticBezier(sourceWorld, controlPoint, targetWorld, t);
				bodyRenderer.SetPosition(i, point);
			}

			var lastSegmentStart = bodyRenderer.GetPosition(bodySegments - 2);
			var lastSegmentEnd = bodyRenderer.GetPosition(bodySegments - 1);
			var direction = lastSegmentEnd - lastSegmentStart;
			if (direction.sqrMagnitude <= 0.00001f)
			{
				tipRenderer.SetPosition(0, targetWorld);
				tipRenderer.SetPosition(1, targetWorld);
				tipRenderer.SetPosition(2, targetWorld);
				tipRenderer.SetPosition(3, targetWorld);
				return;
			}

			direction.Normalize();
			var normal = Vector3.Cross(direction, Vector3.up);
			if (normal.sqrMagnitude <= 0.00001f)
			{
				normal = Vector3.Cross(direction, Vector3.forward);
			}

			normal.Normalize();

			var tipBackCenter = targetWorld - direction * tipLength;
			var tipLeft = tipBackCenter + normal * tipHalfWidth;
			var tipRight = tipBackCenter - normal * tipHalfWidth;

			tipRenderer.SetPosition(0, tipLeft);
			tipRenderer.SetPosition(1, targetWorld);
			tipRenderer.SetPosition(2, tipRight);
			tipRenderer.SetPosition(3, tipLeft);

			UpdateDashAnimation();
			UpdateTipPulseAnimation();
		}

		private void OnDestroy()
		{
			if (bodyRuntimeMaterial)
			{
				Destroy(bodyRuntimeMaterial);
			}
		}

		private static Vector3 EvaluateQuadraticBezier(Vector3 start, Vector3 control, Vector3 end, float t)
		{
			var oneMinusT = 1f - t;
			return oneMinusT * oneMinusT * start + 2f * oneMinusT * t * control + t * t * end;
		}

		private void UpdateDashAnimation()
		{
			if (!bodyRuntimeMaterial)
			{
				return;
			}

			var offsetX = -Time.unscaledTime * dashScrollSpeed;
			var offset = new Vector2(offsetX, 0f);

			if (bodyRuntimeMaterial.HasProperty(BaseMapPropertyId))
			{
				bodyRuntimeMaterial.SetTextureOffset(BaseMapPropertyId, offset);
			}

			if (bodyRuntimeMaterial.HasProperty(MainTexPropertyId))
			{
				bodyRuntimeMaterial.SetTextureOffset(MainTexPropertyId, offset);
			}
		}

		private void UpdateTipPulseAnimation()
		{
			var pulse = 1f + Mathf.Sin(Time.unscaledTime * tipPulseFrequency) * tipPulseAmplitude;
			tipRenderer.widthMultiplier = baseTipWidthMultiplier * pulse;
		}
	}
}
