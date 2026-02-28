using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using PlatformCore.Services.UI;

namespace PlatformCore.Core
{
	public class PersistentSceneContext : MonoBehaviour
	{
		public Scene scene {get; private set;}
		[SerializeField] private UICanvasEntry[] _uiCanvases;
		public UICanvasEntry[] UICanvases => _uiCanvases;

		private void Awake()
		{
			scene = gameObject.scene;
		}
	}
}