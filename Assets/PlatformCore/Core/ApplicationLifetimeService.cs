using System;
using System.Threading;
using UnityEngine;

namespace PlatformCore.Core
{
	public class ApplicationLifetimeService : IDisposable
	{
		private readonly CancellationTokenSource _applicationCts;
		
		public CancellationToken ApplicationLifetime => _applicationCts.Token;
        
		public ApplicationLifetimeService()
		{
			_applicationCts = new CancellationTokenSource();
			
			Application.quitting += OnApplicationQuitting;
		}
        
		private void OnApplicationQuitting()
		{
			Debug.Log("[ApplicationLifetime] Application quitting, cancelling all operations...");
			_applicationCts.Cancel();
		}
        
		public void Dispose()
		{
			Application.quitting -= OnApplicationQuitting;
			_applicationCts?.Cancel();
			_applicationCts?.Dispose();
		}
	}
}