using UnityEngine;

namespace MANA3DGames
{
    public class Room
    {
        int roomIndex;
        public int RoomIndex { get { return roomIndex; } }

        int rowIndex;
        public int RowIndex { get { return rowIndex; } }

        Vector2 position;
        public Vector2 Position { get { return position; } }

        public Cell cell;
        public bool IsEmpty { get { return cell == null; } }


        GameObject emptyBlockRef;


        public Room( int roomIndex, int rowIndex, Vector2 position, GameObject emptyBlockRef )
        {
            this.roomIndex      = roomIndex;
            this.rowIndex       = rowIndex;
            this.position       = position;
            this.emptyBlockRef  = emptyBlockRef;
        }

        public void FreeRoom()
        {
            if ( cell != null )
            {
                cell.FreeMemory();
                cell = null;
            }
        }

        public void Destroy()
        {
            FreeRoom();
            GameObject.Destroy( emptyBlockRef );
        }
    }
}
