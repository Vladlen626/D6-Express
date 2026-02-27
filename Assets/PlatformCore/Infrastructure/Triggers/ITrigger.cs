using System;

namespace PlatformCore.Infrastructure.Triggers
{
	public interface ITrigger
	{
		event Action<string> OnTriggered;
		string triggerId { get; }

		void Trigger();
	}
}
