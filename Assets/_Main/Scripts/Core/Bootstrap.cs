using PlatformCore.Core;

namespace _Main.Scripts.Core
{
	public class Bootstrap : BaseBootstrap
	{
		protected override BaseGameRoot CreateGameRoot()
		{
			return new GameRoot();
		}
	}
}