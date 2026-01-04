using System;

namespace _Main.Scripts.Dice
{
	[Serializable]
	public class DiceConfig : BaseConfig
	{
		public string name;
		public int[] weights;
	}
}