using UnityEngine;

namespace MANA3DGames
{
    public class Row
    {
        public int index;
        Room[] rooms;

        public int GetRowCount { get { return rooms.Length; } }


        public Row( int count, int rowIndex, float yPos, GameObject emptyCellPrefab, Transform Parent )
        {
            rooms = new Room[count];

            float xPos = -2.88f;
            for ( int roomIndex = 0; roomIndex < count; roomIndex++ )
            {
                GameObject emptyGO = GameObject.Instantiate<GameObject>( emptyCellPrefab, new Vector2( xPos, yPos ), Quaternion.identity ) as GameObject;
                emptyGO.transform.parent = Parent;

                rooms[roomIndex] = new Room( roomIndex, rowIndex, new Vector2( xPos, yPos ), emptyGO );
                xPos += 0.64f;
            }
        }


        public bool IsAllFilled( bool noHardBlock = true )
        {
            foreach ( var room in rooms )
            {
                if ( room.IsEmpty || ( noHardBlock && room.cell.ColorID == 5 ) )
                    return false;
            }

            return true;
        }

        public bool IsAllSameFilled()
        {
            int id = -1;
            if ( !rooms[0].IsEmpty )
                id = rooms[0].cell.ColorID;
            else
                return false;

            foreach ( var room in rooms )
            {
                if ( room.IsEmpty || id != room.cell.ColorID )
                    return false;
            }

            return true;
        }

        
        public bool IsAllEmpty()
        {
            foreach ( var room in rooms )
            {
                if ( !room.IsEmpty )
                    return false;
            }

            return true;
        }

        public bool IsRoomEmpty( int roomIndex )
        {
            return rooms[roomIndex].IsEmpty;
        }

        public bool IsFace( int roomIndex )
        {
            return rooms[roomIndex].cell != null && rooms[roomIndex].cell.IsFace;
        }

        public bool HasFace()
        {
            foreach ( var room in rooms )
            {
                if ( room.cell != null && room.cell.IsFace )
                    return true;
            }

            return false;
        }

        public Cell GetFace( Cell notThisCell = null )
        {
            foreach ( var room in rooms )
            {
                if ( room.cell != null && room.cell.IsFace && notThisCell != null && notThisCell != room.cell )
                    return room.cell;
            }

            return null;
        }


        public void SetAt( int index, Cell cell, bool forcePosition = false )
        {
            if ( cell != null && cell.room != null && cell.room.cell != null )
                cell.room.cell = null;

            rooms[index].cell = cell;
            cell.SetInRoom( rooms[index], forcePosition );
        }

        public void SetTemporaryAt( int index, Cell cell, bool forcePosition = false )
        {
            cell.SetInRoom( rooms[index], forcePosition );
        }



        public void FreeRoom( int index )
        {
            rooms[index].FreeRoom();
        }

        public void DestroyRoom( int index )
        {
            rooms[index].Destroy();
        }



        public Vector2 GetPosition( int roomIndex )
        {
            return rooms[roomIndex].Position;
        }

        public Room GetRoom( int id )
        {
            return rooms[id];
        }

        public int GetColorID( int index )
        {
            if ( !rooms[index].IsEmpty )
                return rooms[index].cell.ColorID;

            return -1;
        }
    }
}
