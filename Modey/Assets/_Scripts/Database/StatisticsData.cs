namespace MANA3DGames
{
    public class StatisticsData
    {
        public string name;
        public int score;
        public int rank;
        public int land;
        public int level;
        public int coins;

        public int[] starsCount;

        public int[] painters;
        public int[] boosters;

        public StatisticsData()
        {
            starsCount = new int[3];
            painters = new int[5];
            boosters = new int[3];
        }
    }
}
