using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CenterParentOnChildren
{
	[MenuItem("Tools/Center Parent On Children", true)]
	private static bool ValidateSelection() => Selection.activeTransform != null;

	[MenuItem("Tools/Center Parent On Children")]
	private static void CenterSelected()
	{
		foreach (Transform parent in Selection.transforms)
			CenterOne(parent);
	}

	private static void CenterOne(Transform parent)
	{
		var children = parent.Cast<Transform>().ToArray();
		if (children.Length == 0) return;

		var renderers = parent.GetComponentsInChildren<Renderer>(true)
			.Where(r => r.transform != parent).ToArray();

		Bounds b = renderers.Length > 0
			? renderers.Aggregate(new Bounds(renderers[0].bounds.center, Vector3.zero), (bb, r) =>
			{
				bb.Encapsulate(r.bounds);
				return bb;
			})
			: children.Aggregate(new Bounds(children[0].position, Vector3.zero), (bb, c) =>
			{
				bb.Encapsulate(c.position);
				return bb;
			});

		Vector3 target = b.center;
		Vector3 delta = target - parent.position;
		if (delta.sqrMagnitude < 1e-12f) return;

		Undo.RecordObject(parent, "Center Parent");
		parent.position = target;

		foreach (var c in children)
		{
			Undo.RecordObject(c, "Center Parent");
			c.position -= delta;
		}
	}
}