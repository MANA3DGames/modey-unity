
namespace MANA3DGames
{
    [System.Serializable]
    public class ReservedCell
    {
        public int type;
        public int colorId;
        public int rowIndex;
        public int roomIndex;

        public ReservedCell()
        {
        }

        public ReservedCell( int type, int colorID, int rowIndex, int roomIndex )
        {
            this.type       = type;
            this.colorId    = colorID;
            this.rowIndex   = rowIndex;
            this.roomIndex  = roomIndex;
        }
    }
}