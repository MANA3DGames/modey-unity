namespace MANA3DGames
{
    [System.Serializable]
    public class UserData
    {
        public int coins;
        public int[] boosters;
        public int[] painters;

        [System.NonSerialized]
        public int[] paintersExtra;
        [System.NonSerialized]
        public int[] boosterExtra;
        //[System.NonSerialized]
        //public int totalScore;


        public UserData() { }
        public UserData( UserData data )
        {
            coins = data.coins;

            if ( data.boosters == null )
                data.boosters = new int[3];
            boosters = new int[data.boosters.Length];
            for ( int i = 0; i < boosters.Length; i++ )
                boosters[i] = data.boosters[i];

            if ( data.painters == null )
                data.painters = new int[5];
            painters = new int[data.painters.Length];
            for ( int i = 0; i < painters.Length; i++ )
                painters[i] = data.painters[i];

            if ( data.paintersExtra == null )
                data.paintersExtra = new int[5];
            paintersExtra = new int[data.paintersExtra.Length];
            for ( int i = 0; i < paintersExtra.Length; i++ )
                paintersExtra[i] = data.paintersExtra[i];

            if ( data.boosterExtra == null )
                data.boosterExtra = new int[3];
            boosterExtra = new int[data.boosterExtra.Length];
            for ( int i = 0; i < boosterExtra.Length; i++ )
                boosterExtra[i] = data.boosterExtra[i];

            //totalScore = data.totalScore;
        }
    }
}