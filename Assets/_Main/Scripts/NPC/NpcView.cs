using System.Collections.Generic;
using UnityEngine;

public class NpcView : CharacterView
{
	[SerializeField]
	private List<Transform> hatsTransforms;

	void Start()
	{
		if (hatsTransforms.Count == 0)
		{
			hatsTransforms[0].gameObject.SetActive(true);
			return;
		}
		
		foreach (var hatsTransform in hatsTransforms)
		{
			hatsTransform.gameObject.SetActive(false);
		}

		var hatIndex = Random.Range(0, hatsTransforms.Count);
		hatsTransforms[hatIndex].gameObject.SetActive(true);
	}
}