﻿using Chatting;
using Pipliz;

namespace ExtendedBuilder.Persistence
{
    public abstract class Structure
    {
		private Rotation currentRotation = Rotation.Front;
        public enum Rotation
        {
            Front,
            Right,
            Back,
            Left
        }

		public static Rotation RotateClockwise(Rotation rotation)
		{
			return ((rotation + 1) & Rotation.Left); //Rotate clockwise, bitwise with 3
		}

        protected Structure(Structure structure) { }

        protected Structure(string file) { }

		protected Structure() { }

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
			Chat.SendToConnected("Current: " + currentRotation.ToString());
			if (r == currentRotation) {
				Log.Write("Already Correct!");
				return;
			}
			else
			{
				Log.WriteError("Got another rotate!");
			}

						while(currentRotation != r)
			{
				currentRotation = RotateClockwise(currentRotation);
				Rotate();
				Chat.SendToConnected("Rotating Structure! " + currentRotation.ToString() + " Target: " + r.ToString());
			}
        }

        public abstract void Save(string name);

		public string GetSizeString()
		{
			return GetMaxX() + " x " + GetMaxY() + " x " + GetMaxZ();
		}
    }
}
