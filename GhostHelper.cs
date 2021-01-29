using BlockTypes;
using Jobs.Implementations.Construction;
using Pipliz;
using Shared.Networking;
using static ItemTypes;

namespace Improved_Construction
{
	static class GhostHelper
	{
		public static void FillGhost(ConstructionArea area, IIterationType iteration)
		{
			ItemType ghostBlock = ItemTypes.GetType("ghost");

			int num1 = 4096;
			while (num1-- > 0)
			{
				Vector3Int currentPosition = iteration.CurrentPosition;
				ushort val;
				if (World.TryGetTypeAt(currentPosition, out val))
				{
					if (val == (ushort)0 || (int)val == (int)BuiltinBlocks.Indices.water)
					{
						SendGhostBlock(area.Owner, currentPosition, ghostBlock);
					}
				}
				iteration.MoveNext();
			}
		}

		public static void SendGhostBlock(Colony colony, Vector3Int position, ItemType type)
		{
			using (ByteBuilder data = ByteBuilder.Get())
			{
				data.Write(ClientMessageType.BlockChange);
				data.WriteVariable(position);
				data.WriteVariable(type.ItemIndex);
				Players.SendToNearbyDrawDistance(position, data, 2000, NetworkMessageReliability.ReliableWithBuffering);
			}
			Log.Write("Sending ghost block!");
		}
	}
}
