using BlockTypes;
using ExtendedBuilder.Jobs;
using Jobs.Implementations.Construction;
using Pipliz;
using Shared.Networking;
using static ItemTypes;

namespace Improved_Construction
{
	static class GhostHelper
	{
		private static int MAX_GHOST_BLOCKS = 1000;

		public static void FillGhost(StructureIterator bpi, ItemType fillBlock = null)
		{
			if (fillBlock == null)
				fillBlock = ItemTypes.GetType("ghost");

			int blocks = 0;
			while (blocks <= MAX_GHOST_BLOCKS) // This is to move past air.
			{
				if (!bpi.MoveNext())
					return;

				var adjX = bpi.CurrentPosition.x - bpi.location.x;
				var adjY = bpi.CurrentPosition.y - bpi.location.y;
				var adjZ = bpi.CurrentPosition.z - bpi.location.z;
				var block = bpi.BuilderSchematic.GetBlock(adjX, adjY, adjZ);
				var buildType = ItemTypes.GetType(block);
				if (buildType == null)
				{
					Log.WriteError("Block " + block + " not found!");
					return;
				}
				if (block == BuiltinBlocks.Indices.air)
					continue; //Skip over air blocks in the model

				if (World.TryGetTypeAt(bpi.CurrentPosition, out ushort foundTypeIndex))
				{
					if (foundTypeIndex != BuiltinBlocks.Indices.air)
						continue;
					SendGhostBlock(null, bpi.CurrentPosition, fillBlock);
				}

			}
		}

		public static void SendGhostBlock(Colony colony, Vector3Int position, ItemType type)
		{
			using (ByteBuilder data = ByteBuilder.Get())
			{
				data.Write(ClientMessageType.BlockChange);
				data.WriteVariable(position);
				data.WriteVariable(type.ItemIndex);
				Players.SendToNearbyDrawDistance(position, data, 200, NetworkMessageReliability.Unreliable);
			}
			//Log.Write("Sending ghost block!" + position.ToString());
		}
	}
}
