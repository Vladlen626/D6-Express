using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Dice
{
	[Serializable]
	public sealed class ItemDisabledVisual
	{
		[SerializeField] private float scaleMultiplier = 0.9f;
		[SerializeField] private Color outlineColor = new(0.38f, 0.38f, 0.38f, 1f);
		[SerializeField] private Material disabledMaterial;
		[SerializeField] private Renderer[] renderers;

		private readonly Dictionary<Renderer, Material[]> originalMaterialsByRenderer = new();
		private bool validated;

		public float ScaleMultiplier => Mathf.Max(0.01f, scaleMultiplier);
		public Color OutlineColor => outlineColor;

		public void EnsureConfiguredOrThrow()
		{
			if (validated)
			{
				return;
			}

			ValidateConfigOrThrow();
			validated = true;
		}

		public void Apply()
		{
			EnsureConfiguredOrThrow();

			for (int i = 0; i < renderers.Length; i++)
			{
				var renderer = renderers[i];
				if (!renderer)
				{
					continue;
				}

				var sourceMaterials = renderer.sharedMaterials ?? Array.Empty<Material>();
				if (!originalMaterialsByRenderer.ContainsKey(renderer))
				{
					originalMaterialsByRenderer[renderer] = (Material[])sourceMaterials.Clone();
				}

				renderer.sharedMaterials = BuildDisabledMaterials(sourceMaterials.Length);
			}
		}

		public void Restore()
		{
			foreach (var pair in originalMaterialsByRenderer)
			{
				if (!pair.Key)
				{
					continue;
				}

				pair.Key.sharedMaterials = pair.Value;
			}
		}

		public void AutoAssignMeshRenderers(Transform root)
		{
			if (!root)
			{
				throw new InvalidOperationException(
					"[ItemDisabledVisual] Root transform for renderer auto-assignment is not assigned.");
			}

			var meshRenderers = root.GetComponentsInChildren<MeshRenderer>(true);
			renderers = new Renderer[meshRenderers.Length];
			for (int i = 0; i < meshRenderers.Length; i++)
			{
				renderers[i] = meshRenderers[i];
			}

			ResetRuntimeCache();
		}

		private Material[] BuildDisabledMaterials(int length)
		{
			var disabledMaterials = new Material[length];
			for (int i = 0; i < length; i++)
			{
				disabledMaterials[i] = disabledMaterial;
			}

			return disabledMaterials;
		}

		private void ValidateConfigOrThrow()
		{
			if (!disabledMaterial)
			{
				throw new InvalidOperationException(
					"[ItemDisabledVisual] Disabled material is not assigned.");
			}

			if (renderers == null || renderers.Length == 0)
			{
				throw new InvalidOperationException(
					"[ItemDisabledVisual] Target renderers are not assigned.");
			}

			var uniqueRenderers = new HashSet<Renderer>();
			for (int i = 0; i < renderers.Length; i++)
			{
				var renderer = renderers[i];
				if (!renderer)
				{
					continue;
				}

				if (!uniqueRenderers.Add(renderer))
				{
					continue;
				}

				var sourceMaterials = renderer.sharedMaterials ?? Array.Empty<Material>();
				if (sourceMaterials.Length == 0)
				{
					continue;
				}
			}
		}

		private void ResetRuntimeCache()
		{
			originalMaterialsByRenderer.Clear();
			validated = false;
		}
	}
}
