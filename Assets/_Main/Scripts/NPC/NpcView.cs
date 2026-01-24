using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NpcView : CharacterView
{
	[SerializeField]
	private List<Transform> hatsTransforms;
	
	[SerializeField]
	private Animator animator;
	
	public Animator Animator => animator;

	void Start()
	{
		/*if (hatsTransforms.Count == 0)
		{
			return;
		}

		foreach (var hatsTransform in hatsTransforms)
		{
			hatsTransform.gameObject.SetActive(false);
		}

		var hatIndex = Random.Range(0, hatsTransforms.Count);
		hatsTransforms[hatIndex].gameObject.SetActive(true);*/
	}
}