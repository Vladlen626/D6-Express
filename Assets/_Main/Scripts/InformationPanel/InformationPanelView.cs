using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class InformationPanelView : MonoBehaviour
{
	[SerializeField] private InformationPanelStationView[] stations;

	[Header("Connections")] [SerializeField]
	private RectTransform connectionsRoot;

	[SerializeField] private Image segmentPrefab;

	[SerializeField] [Min(1f)] private float segmentLength = 30f;

	[SerializeField] [Min(1f)] private float segmentThickness = 18f;

	[SerializeField] [Min(0f)] private float segmentGap = 10f;

	[SerializeField] [Min(0f)] private float stationPadding = 12f;

	[Header("Progress Colors")] [SerializeField]
	private Color completedColor = new Color(0.2f, 0.9f, 0.3f, 1f);

	[SerializeField] private Color activeColor = new Color(1f, 0.85f, 0.2f, 1f);

	[SerializeField] private Color lockedColor = new Color(1f, 1f, 1f, 0.25f);

	[Header("Wave")] [SerializeField] private float waveScale = 1.2f;

	[SerializeField] private float waveDuration = 0.5f;

	[SerializeField] private float waveStagger = 0.05f;

	private readonly List<InformationPanelConnectionView> connections = new();

	public IReadOnlyList<InformationPanelStationView> Stations => stations;

	public void RefreshConnections()
	{
		connections.Clear();

		var root = ResolveConnectionsRoot();
		if (!root)
		{
			return;
		}

		var found = root.GetComponentsInChildren<InformationPanelConnectionView>(true);
		connections.AddRange(found);
		connections.Sort((a, b) => a.Index.CompareTo(b.Index));
	}

	public void SetProgress(int level)
	{
		if (connections.Count == 0)
		{
			RefreshConnections();
		}

		var connectionCount = connections.Count;
		var completedCount = Mathf.Clamp(level, 0, connectionCount);
		var hasActive = level >= 0 && level < connectionCount;

		for (int i = 0; i < connectionCount; i++)
		{
			var connection = connections[i];
			if (!connection)
			{
				continue;
			}

			if (i < completedCount)
			{
				connection.SetColor(completedColor);
				connection.StopWave();
				continue;
			}

			if (hasActive && i == level)
			{
				connection.SetColor(activeColor);
				connection.PlayWave(waveScale, waveDuration, waveStagger);
				continue;
			}

			connection.SetColor(lockedColor);
			connection.StopWave();
		}
	}

#if UNITY_EDITOR
	[ContextMenu("Apply Connections")]
	public void ApplyConnections()
	{
		var root = ResolveConnectionsRoot();
		if (!root)
		{
			Debug.LogWarning("[InformationPanelView] ConnectionsRoot is not assigned.", this);
			return;
		}

		Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Rebuild Station Connections");
		ClearConnections(root);

		if (stations == null || stations.Length < 2)
		{
			return;
		}

		int stationCount = Mathf.Min(GetRunRulesStationCount(), stations.Length);
		if (stationCount < 2)
		{
			return;
		}

		for (int i = 0; i < stationCount - 1; i++)
		{
			var from = stations[i];
			var to = stations[i + 1];
			if (!from || !to)
			{
				Debug.LogWarning("[InformationPanelView] Station view is missing, connection skipped.", this);
				continue;
			}

			CreateConnection(root, i, from.Position, to.Position);
		}

		EditorUtility.SetDirty(this);
		if (!Application.isPlaying)
		{
			EditorSceneManager.MarkSceneDirty(gameObject.scene);
		}
	}

	private int GetRunRulesStationCount()
	{
		var textAsset = Resources.Load<TextAsset>("Json/run_rules");
		if (!textAsset)
		{
			return stations != null ? stations.Length : 0;
		}

		try
		{
			var runs = JsonConvert.DeserializeObject<List<RunConfig>>(textAsset.text);
			if (runs != null && runs.Count > 0 && runs[0].levels != null)
			{
				return runs[0].levels.Length;
			}
		}
		catch
		{
			return stations != null ? stations.Length : 0;
		}

		return stations != null ? stations.Length : 0;
	}
#endif

	private RectTransform ResolveConnectionsRoot()
	{
		if (connectionsRoot)
		{
			return connectionsRoot;
		}

		return transform as RectTransform;
	}

#if UNITY_EDITOR
	private void ClearConnections(Transform root)
	{
		var children = new List<Transform>();
		foreach (Transform child in root)
		{
			children.Add(child);
		}

		for (int i = 0; i < children.Count; i++)
		{
			var child = children[i];
			if (!child)
			{
				continue;
			}

			DestroyImmediate(child.gameObject);
		}
	}

	private void CreateConnection(Transform root, int index, Vector3 from, Vector3 to)
	{
		var start = root.InverseTransformPoint(from);
		var end = root.InverseTransformPoint(to);
		var direction = end - start;
		var distance = direction.magnitude;
		if (distance <= Mathf.Epsilon)
		{
			return;
		}

		if (stationPadding > 0f)
		{
			var inset = Mathf.Min(stationPadding, distance * 0.5f);
			var norm = direction / distance;
			start += norm * inset;
			end -= norm * inset;
			direction = end - start;
			distance = direction.magnitude;
		}

		var count = GetSegmentCount(distance);
		if (count <= 0)
		{
			return;
		}

		var connectionRoot = new GameObject($"Connection_{index}", typeof(RectTransform),
			typeof(InformationPanelConnectionView));
		connectionRoot.transform.SetParent(root, false);
		var connectionView = connectionRoot.GetComponent<InformationPanelConnectionView>();
		connectionView.SetIndex(index);

		var step = segmentLength + segmentGap;
		var totalLength = (segmentLength * count) + (segmentGap * (count - 1));
		var startOffset = Mathf.Max(0f, (distance - totalLength) * 0.5f);
		var rotation = Quaternion.FromToRotation(Vector3.right, direction);
		var segments = new List<Image>(count);
		for (int i = 0; i < count; i++)
		{
			var segment = CreateSegment(connectionRoot.transform);
			var center = start + (direction.normalized * (startOffset + (segmentLength * 0.5f) + (i * step)));
			var rect = segment.rectTransform;
			rect.localPosition = center;
			rect.localRotation = rotation;
			rect.sizeDelta = new Vector2(segmentLength, segmentThickness);
			rect.localScale = Vector3.one;
			segment.color = lockedColor;
			segment.raycastTarget = false;
			segments.Add(segment);
		}

		connectionView.SetSegments(segments);
	}

	private int GetSegmentCount(float distance)
	{
		if (segmentLength <= 0f)
		{
			return 0;
		}

		var step = segmentLength + Mathf.Max(0f, segmentGap);
		if (step <= 0f)
		{
			return 0;
		}

		var count = Mathf.FloorToInt((distance + segmentGap) / step);
		return Mathf.Max(1, count);
	}

	private Image CreateSegment(Transform parent)
	{
		if (segmentPrefab)
		{
			var instance = Object.Instantiate(segmentPrefab, parent, false);
			return instance;
		}

		var go = new GameObject("StationConnectionSegment", typeof(RectTransform), typeof(Image));
		go.transform.SetParent(parent, false);
		var image = go.GetComponent<Image>();
		image.sprite = GetWhiteSprite();
		image.type = Image.Type.Simple;
		var rect = image.rectTransform;
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		return image;
	}

	private static Sprite whiteSprite;

	private static Sprite GetWhiteSprite()
	{
		if (whiteSprite)
		{
			return whiteSprite;
		}

		var texture = Texture2D.whiteTexture;
		whiteSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		return whiteSprite;
	}
#endif
}