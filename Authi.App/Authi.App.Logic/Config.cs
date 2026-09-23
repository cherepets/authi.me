namespace Authi.App.Logic
{
    public static class Config
    {
        public const int ChunkMs = 1;
        public const int ChunkCount = 50;
        public const int UpdateMs = 30_000;         // 30 sec
        public const int SyncPeriodMs = 180_000;    //  3 min
        public const int LockTimeoutMs = 300_000;   //  5 min
    }
}
