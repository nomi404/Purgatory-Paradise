namespace CharlesEngine
{
    public static class CELogger
    {
        public const int VIDEOS = 1;
        public const int COMIC = 2;
        public const int DIALOG = 4;
        public const int PERSISTANCE = 8;
        public static int Flags = 1;

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(string msg, int tag = 0)
        {
            if ( tag == 0 || (Flags & tag) > 0)
                UnityEngine.Debug.Log(msg);
        }
    }
}