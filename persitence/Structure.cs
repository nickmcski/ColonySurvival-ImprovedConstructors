using Pipliz;

namespace ExtendedBuilder.Persistence
{
    public abstract class Structure
    {
        public enum Rotation
        {
            Front,
            Right,
            Back,
            Left
        }

        protected Structure() { }

        protected Structure(string file) { }

        public abstract int GetMaxX();
        public abstract int GetMaxY();
        public abstract int GetMaxZ();

        public abstract ushort GetBlock(int x, int y, int z);
        public ushort GetBlock(Vector3Int pos)
        {
            return GetBlock(pos.x, pos.y, pos.z);
        }

        public abstract void Rotate();

        public void Rotate(Rotation r)
        {
            int rotation = 1;

            if (r == Rotation.Right)
                rotation = 1;

            if (r == Rotation.Back)
                rotation = 1;

            if (r == Rotation.Left)
                rotation = 1;

            if (r == Rotation.Front)
                return;

            for (int i = 0; i < rotation; i++)
                Rotate();
        }

        public abstract void Save(string name);
    }
}
