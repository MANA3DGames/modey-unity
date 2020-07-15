[System.Serializable]
public class TestDev
{
    public bool ClearAll;
    public bool DisableStartupTimer;
    public bool StartGameplay;
    public int LandID;
    public int LevelID;


    static System.Diagnostics.Stopwatch _watch;

    public static void StartWatch()
    {
        _watch = new System.Diagnostics.Stopwatch();
        _watch.Start();
    }

    public static void StopWatch()
    {
        _watch.Stop();
        UnityEngine.Debug.Log( _watch.Elapsed.TotalSeconds );
    }
}