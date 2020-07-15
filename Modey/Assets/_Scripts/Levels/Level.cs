using System.Collections.Generic;

namespace MANA3DGames
{
    public class Level
    {
        #region [Fixed Variables]

        int landID;
        public int LandID { get { return landID; } }
        int levelID;
        public int LevelID { get { return levelID; } }

        float stepTime;
        public float StepTime { get { return stepTime; } }

        float nextSpawnDelay;
        public float NextSpawnDelay { get { return nextSpawnDelay; } }

        ReservedCell[] reservedCells;
        public ReservedCell[] ReservedCells { get { return reservedCells; } }

        int[] colorBlocksCount;
        public int[] ColorBlocksCount { get { return colorBlocksCount; } }

        int[] boostersCount;
        public int[] BoostersCount { get { return boostersCount; } }

        int originalFaceCount;
        public int OriginalFaceCount { get { return originalFaceCount; } }

        int spawnBombAfter;
        public int SpawnBombAfter { get { return spawnBombAfter; } }

        #endregion

        #region [Updated Variables]

        int _legalFaceCount;
        public int LegalFaceCount { get { return _legalFaceCount; } }

        int _illegalFaceCount;
        public int IllegalFaceCount { get { return _illegalFaceCount; } }

        List<Face> _faces;
        public int FaceCount { get { return _faces.Count; } }

        #endregion


        #region [Constructor]

        public Level( int landID, int levelID, 
                      float stepTime, float nextSpawnDelay, 
                      ReservedCell[] reservedCells, 
                      int[] colorBlocksCount,
                      int[] boostersCount,
                      int spawnBombAfter )
        {
            this.landID = landID;
            this.levelID = levelID;

            this.stepTime = stepTime;
            this.nextSpawnDelay = nextSpawnDelay;

            this.reservedCells = reservedCells;

            this.colorBlocksCount = colorBlocksCount;
            this.boostersCount = boostersCount;

            foreach ( var cell in reservedCells )
            {
                if ( cell.type == 1 )
                    originalFaceCount++;
            }

            this.spawnBombAfter = spawnBombAfter;

            _faces = new List<Face>();
        }

        public void Destroy()
        {
            if ( _faces != null )
            {
                _faces.Clear();
                _faces = null;
            }
        }

        #endregion


        #region [Face Functions]

        public void AddFace( Face face )
        {
            _faces.Add( face );
        }

        public void RemoveFace( Face face, bool legal )
        {
            face.FreeMemory();
            _faces.Remove( face );
            face = null;

            if ( legal )
                _legalFaceCount++;
            else
                _illegalFaceCount++;
        }

        public bool ContainsFace( Face face )
        {
            return _faces.Contains( face );
        }

        public void UpdateFaces( Block block )
        {
            foreach ( var face in _faces )
            {
                face.LookAtBlock( block );
            }
        }


        public int GetFaceColorIDAtIndex( int index )
        {
            if ( _faces.Count == 0  )
                return 0;

            return _faces[index].ColorID;
        }

        #endregion


        #region [Event Handlers]

        public void OnGamePaused()
        {
            foreach ( var face in _faces )
                face.OnPauseGame();
        }

        public void OnGameResumed()
        {
            foreach ( var face in _faces )
                face.OnResumeGame();
        }

        #endregion
    }
}