using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlatformCore.Core
{
	public class PersistentSceneContext : MonoBehaviour
	{
		public Scene scene {get; private set;}
		public Transform StaticCanvas;
		public Transform DynamicCanvas;

		private void Awake()
		{
			scene = gameObject.scene;
		}
	}
}