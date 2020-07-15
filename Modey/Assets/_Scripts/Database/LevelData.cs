namespace MANA3DGames
{
    [System.Serializable]
    public class LevelData
    {
        public int levelID;
        public int landID;

        public int unlocked;
        public int stars;
        public int score;

        [System.NonSerialized]
        public int coinsReward;
        [System.NonSerialized]
        public int bonusRewardID;


        public void CopyValues( LevelData data )
        {
            levelID = data.levelID;
            landID = data.landID;

            unlocked = data.unlocked;
            stars = data.stars;
            score = data.score;
        }
    }
}