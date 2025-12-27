using System;

namespace PlatformCore.Services
{
	public interface ILoggerService
	{
		// ReSharper disable Unity.PerformanceAnalysis
		void Log(string message);
		// ReSharper disable Unity.PerformanceAnalysis
		void LogWarning(string message);
		// ReSharper disable Unity.PerformanceAnalysis
		void LogError(string message);
		// ReSharper disable Unity.PerformanceAnalysis
		void LogException(Exception exception);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		void LogDebug(string message);
#endif
	}
}