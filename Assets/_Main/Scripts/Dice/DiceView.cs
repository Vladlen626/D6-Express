using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.UI;

namespace _Main.Scripts.Dice
{
	public class DiceView : MonoBehaviour
	{
		[SerializeField] private MeshRenderer[] sideMeshes;
		[SerializeField] private Transform model;
		[SerializeField] private Outline outline;
        
		[SerializeField] private float animSpeed = 0.15f;
		[SerializeField] private float yOffset = 0.02f;
        
		public UnityAction OnDiceClicked;
        
		public void SetSideMesh(int value)
		{
			foreach (var mesh in sideMeshes)
			{
				mesh.enabled = false;
			}

			if (value > 0 && value <= sideMeshes.Length)
			{
				sideMeshes[value - 1].enabled = true;
			}
		}

		public void UpdateChosenVisual(bool isChosen)
		{
			if (isChosen)
			{
				model.DOLocalMove(Vector3.up * yOffset, animSpeed);
				//outline.OutlineColor = Color.red;
			}
			else
			{
				model.DOLocalMove(Vector3.zero, animSpeed);
				//outline.OutlineColor = Color.black;
			}
		}
        
		public void PlayPressAnimation()
		{
			transform.DOScale(0.9f, animSpeed);
		}
        
		public void PlayReleaseAnimation()
		{
			transform.DOScale(1f, animSpeed);
		}
        
		private void OnMouseDown()
		{
			PlayPressAnimation();
		}
        
		private void OnMouseUp()
		{
			PlayReleaseAnimation();
			OnDiceClicked?.Invoke();
		}
	}
}