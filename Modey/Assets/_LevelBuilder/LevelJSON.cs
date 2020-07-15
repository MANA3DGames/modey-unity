using System.Collections.Generic;

namespace MANA3DGames
{
	[System.Serializable]
	public class LevelJSON
	{
		public int landID;
		public int levelID;

		public float stepTime;
		public float nextSpawnDelay;

		public int[] colorBlocksCount;
		public int[] boostersCount;

		public int spawnBombAfter;

		public List<ReservedCell> reservedCells;
	}
}