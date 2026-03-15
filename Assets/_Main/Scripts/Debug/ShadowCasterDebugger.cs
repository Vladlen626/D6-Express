using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public class ShadowCasterDebugger : MonoBehaviour
{
	[Header("Camera")]
	[SerializeField] private Camera targetCamera;

	[Header("Filter")]
	[SerializeField] private bool onlyVisibleForCamera = false;
	[SerializeField] private bool includeInactiveObjects = false;

	[Header("Logging")]
	[SerializeField, Min(0)] private int maxGroupsToPrint = 30;
	[SerializeField, Min(0)] private int maxRenderersToPrint = 100;
	[SerializeField] private bool printIndividualRenderers = true;

	[Header("Debug Draw")]
	[SerializeField] private bool drawGizmos = true;
	[SerializeField] private bool drawOnlyWhenSelected = true;
	[SerializeField] private Color gizmoColor = Color.yellow;

	private readonly List<Renderer> _lastMatchedRenderers = new List<Renderer>();

	[ContextMenu("Log Shadow Casters")]
	public void LogShadowCasters()
	{
		LogShadowCastersInternal(onlyVisibleForCamera);
	}

	[ContextMenu("Log Shadow Casters (All Objects)")]
	public void LogAllShadowCasters()
	{
		LogShadowCastersInternal(false);
	}

	private void LogShadowCastersInternal(bool filterByCamera)
	{
		Camera cameraToUse = filterByCamera ? GetTargetCamera() : null;

		if (filterByCamera && !cameraToUse)
		{
			Debug.LogWarning("ShadowCasterDebugger: targetCamera is not assigned and Camera.main was not found.");
			return;
		}

		_lastMatchedRenderers.Clear();

		FindObjectsInactive findObjectsInactive = includeInactiveObjects
			? FindObjectsInactive.Include
			: FindObjectsInactive.Exclude;
		Renderer[] allRenderers = FindObjectsByType<Renderer>(findObjectsInactive, FindObjectsSortMode.None);
		Plane[] frustumPlanes = null;

		if (filterByCamera)
		{
			frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cameraToUse);
		}

		Dictionary<string, GroupInfo> groups = new Dictionary<string, GroupInfo>();
		int totalShadowCapable = 0;

		foreach (Renderer renderer in allRenderers)
		{
			if (!renderer)
			{
				continue;
			}

			if (!renderer.enabled)
			{
				continue;
			}

			if (renderer.shadowCastingMode == ShadowCastingMode.Off)
			{
				continue;
			}

			totalShadowCapable++;

			if (filterByCamera && !GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds))
			{
				continue;
			}

			_lastMatchedRenderers.Add(renderer);

			string rootPath = GetPath(renderer.transform.root);

			if (!groups.TryGetValue(rootPath, out GroupInfo groupInfo))
			{
				groupInfo = new GroupInfo(rootPath);
				groups.Add(rootPath, groupInfo);
			}

			groupInfo.count++;
		}

		List<GroupInfo> sortedGroups = new List<GroupInfo>(groups.Values);
		sortedGroups.Sort((a, b) =>
		{
			int compareByCount = b.count.CompareTo(a.count);

			if (compareByCount != 0)
			{
				return compareByCount;
			}

			return string.CompareOrdinal(a.rootPath, b.rootPath);
		});

		StringBuilder sb = new StringBuilder(8192);
		sb.AppendLine("========== SHADOW CASTER DEBUG ==========");
		sb.AppendLine($"Only visible for camera: {filterByCamera}");
		sb.AppendLine($"Camera: {(cameraToUse ? cameraToUse.name : "None")}");
		sb.AppendLine($"Total renderers with shadows enabled: {totalShadowCapable}");
		sb.AppendLine($"Matched renderers: {_lastMatchedRenderers.Count}");
		sb.AppendLine($"Unique root groups: {sortedGroups.Count}");
		sb.AppendLine();

		sb.AppendLine("Top groups:");
		int groupsToPrint = Mathf.Min(Mathf.Max(0, maxGroupsToPrint), sortedGroups.Count);

		for (int i = 0; i < groupsToPrint; i++)
		{
			GroupInfo groupInfo = sortedGroups[i];
			sb.AppendLine($"{i + 1}. {groupInfo.rootPath} -> {groupInfo.count}");
		}

		if (printIndividualRenderers)
		{
			sb.AppendLine();
			sb.AppendLine("Top renderers:");

			List<RendererInfo> rendererInfos = new List<RendererInfo>(_lastMatchedRenderers.Count);

			foreach (Renderer renderer in _lastMatchedRenderers)
			{
				rendererInfos.Add(new RendererInfo(renderer));
			}

			rendererInfos.Sort((a, b) =>
			{
				int rootCompare = string.CompareOrdinal(a.rootPath, b.rootPath);

				if (rootCompare != 0)
				{
					return rootCompare;
				}

				return string.CompareOrdinal(a.fullPath, b.fullPath);
			});

			int renderersToPrint = Mathf.Min(Mathf.Max(0, maxRenderersToPrint), rendererInfos.Count);

			for (int i = 0; i < renderersToPrint; i++)
			{
				RendererInfo info = rendererInfos[i];
				sb.AppendLine(
					$"{i + 1}. [{info.rendererType}] {info.fullPath} | " +
					$"Shadows: {info.shadowMode} | Materials: {info.materialCount}"
				);
			}
		}

		sb.AppendLine("=========================================");

		Debug.Log(sb.ToString());
	}

	[ContextMenu("Disable Shadows On Last Matched")]
	public void DisableShadowsOnLastMatched()
	{
		int changed = 0;

		foreach (Renderer renderer in _lastMatchedRenderers)
		{
			if (!renderer)
			{
				continue;
			}

			if (renderer.shadowCastingMode == ShadowCastingMode.Off)
			{
				continue;
			}

			renderer.shadowCastingMode = ShadowCastingMode.Off;
			changed++;
		}

		Debug.Log($"ShadowCasterDebugger: disabled Cast Shadows on {changed} renderer(s) from the last result.");
	}

	private Camera GetTargetCamera()
	{
		if (targetCamera)
		{
			return targetCamera;
		}

		if (Camera.main)
		{
			return Camera.main;
		}

		return null;
	}

	private static string GetPath(Transform current)
	{
		if (!current)
		{
			return "NULL";
		}

		string path = current.name;

		while (current.parent)
		{
			current = current.parent;
			path = current.name + "/" + path;
		}

		return path;
	}

	private void OnDrawGizmos()
	{
		if (!drawGizmos)
		{
			return;
		}

		if (drawOnlyWhenSelected)
		{
			return;
		}

		DrawMatchedRendererGizmos();
	}

	private void OnDrawGizmosSelected()
	{
		if (!drawGizmos)
		{
			return;
		}

		if (!drawOnlyWhenSelected)
		{
			return;
		}

		DrawMatchedRendererGizmos();
	}

	private void DrawMatchedRendererGizmos()
	{
		Gizmos.color = gizmoColor;

		foreach (Renderer renderer in _lastMatchedRenderers)
		{
			if (!renderer)
			{
				continue;
			}

			Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
		}
	}

	private class GroupInfo
	{
		public readonly string rootPath;
		public int count;

		public GroupInfo(string rootPathValue)
		{
			rootPath = rootPathValue;
			count = 0;
		}
	}

	private class RendererInfo
	{
		public readonly string rootPath;
		public readonly string fullPath;
		public readonly string rendererType;
		public readonly ShadowCastingMode shadowMode;
		public readonly int materialCount;

		public RendererInfo(Renderer renderer)
		{
			rootPath = GetPath(renderer.transform.root);
			fullPath = GetPath(renderer.transform);
			rendererType = renderer.GetType().Name;
			shadowMode = renderer.shadowCastingMode;
			materialCount = renderer.sharedMaterials != null ? renderer.sharedMaterials.Length : 0;
		}
	}
}
