using Utils;

namespace Dungeon
{
    [System.Serializable]
    public class DungeonConfiguration 
    {
        public int Width;
        public int Height;
        public MinMax RoomSize;
        public float Sparseness;
        public int MaxDoorsPerRoom;
        public int MaxAttempts;
        public bool RemoveDeadEnds;
        public int Seed;

        public DungeonConfiguration()
        {
            Width = 10;
            Height = 10;
            MaxAttempts = 100;
            RoomSize = new MinMax(2, 5);
            Sparseness = 0.5f;
            MaxDoorsPerRoom = 2;
            Seed = -1;
        }
    }
}
