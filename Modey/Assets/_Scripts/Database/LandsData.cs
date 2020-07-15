namespace MANA3DGames
{
    [System.Serializable]
    public class LandData
    {
        public LevelData[] levels;
    }

    [System.Serializable]
    public class CloudLandsData
    {
        public LandData[] lands;
        
        public CloudLandsData()
        {
            lands = new LandData[LevelManager.LAND_COUNT];
            for ( int i = 0; i < lands.Length; i++ )
            {
                lands[i] = new LandData();
                lands[i].levels = new LevelData[LevelManager.LEVEL_COUNT];
            }
        }


        public void UnlockLevel( int landID, int levelID )
        {
            if ( GetUnlockedState( landID, levelID ) > 0 )
                return;

            LevelData data = new LevelData();
            data.landID = 0;
            data.levelID = 0;
            data.score = 0;
            data.stars = 0;
            data.unlocked = 1;

            lands[landID].levels[levelID] = data;
        }

        public int GetUnlockedState( int landID, int levelID )
        {
            if ( lands[landID].levels[levelID] != null )
                return lands[landID].levels[levelID].unlocked;
            
            return 0;
        }

        public int GetCurrentLand()
        {
            for ( int i = LevelManager.LAND_COUNT - 1; i >= 0; i-- )
            {
                if ( lands[i] != null && 
                     lands[i].levels[0] != null &&
                     lands[i].levels[0].unlocked >= 1 )
                    return i;
            }
            return 0;
        }

        public int GetCurrentLevel( int landID )
        {
            for ( int lvl = 0; lvl < LevelManager.LEVEL_COUNT; lvl++ )
            {
                if ( lands[landID] != null && 
                     lands[landID].levels[lvl] != null &&
                     lands[landID].levels[lvl].unlocked < 1 )
                    return lvl-1;
            }
            return 29;
        }

        public int[] GetStarsCount()
        {
            int[] stars = new int[3];
            for ( int land = 0; land < LevelManager.LAND_COUNT; land++ )
            {
                for ( int lvl = 0; lvl < LevelManager.LEVEL_COUNT; lvl++ )
                {
                    int starCount = 0;
                    if ( lands[land] != null && 
                         lands[land].levels[lvl] != null )
                    {
                        starCount = lands[land].levels[lvl].stars;
                    }

                    if ( starCount > 0 )
                        stars[starCount-1]++;
                    else
                        break;
                }
            }
            return stars;
        }

        public int CalculateTotalScore()
        {
            int totalScore = 0;
            for ( int land = 0; land < LevelManager.LAND_COUNT; land++ )
            {
                if ( lands[land] == null || lands[land].levels == null )
                    break;

                for ( int lvl = 0; lvl < LevelManager.LEVEL_COUNT; lvl++ )
                {
                    if ( lands[land].levels[lvl] == null )
                        break;

                    totalScore += lands[land].levels[lvl].score;
                }
            }
            return totalScore;
        }
    }
}