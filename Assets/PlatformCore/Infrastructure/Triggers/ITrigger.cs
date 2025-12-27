using System;

namespace _Main.Scripts.SceneOrchestra
{
	public interface ITrigger
	{
		event Action<string> OnTriggered;
		string triggerId { get; }
		
		void Trigger();
	}
}