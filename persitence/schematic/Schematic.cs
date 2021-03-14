using Pipliz;
using BlockTypes;
using fNbt;
using System.IO;
using System.Collections.Generic;

namespace ExtendedBuilder.Persistence
{
	public class Schematic : Structure
	{
		public string Name { get; set; }
		private int XMax { get; set; }
		private int YMax { get; set; }
		private int ZMax { get; set; }
		/// <summary>Contains all usual blocks</summary>
		public SchematicBlock[,,] Blocks { get; set; }
		/// <summary>Contains TileEntities such as hoppers and chests</summary>
		public Vector3Int StartPos { get; set; }

		public Schematic(Structure structure) : base(structure)
		{
			Name = "";
			if (structure is Blueprint)
			{
				XMax = structure.GetMaxX() + 1;
				YMax = structure.GetMaxY() + 1;
				ZMax = structure.GetMaxZ() + 1;
			}
			else
			{
				XMax = structure.GetMaxX();
				YMax = structure.GetMaxY();
				ZMax = structure.GetMaxZ();
			}

			Blocks = new SchematicBlock[XMax + 1, YMax + 1, ZMax + 1];

			for (int Y = 0; Y <= YMax; Y++)
			{
				for (int Z = 0; Z <= ZMax; Z++)
				{
					for (int X = 0; X <= XMax; X++)
					{
						string blockid;
						if (!ItemTypes.IndexLookup.TryGetName(structure.GetBlock(new Vector3Int(X, Y, Z)), out blockid))
							blockid = "air";

						Blocks[X, Y, Z] = new SchematicBlock()
						{
							X = X,
							Y = Y,
							Z = Z,
							BlockID = blockid
						};
					}
				}
			}

		}

		public Schematic(string name, int x, int y, int z, Vector3Int corner1)
		{
			Name = name;
			XMax = x;
			YMax = y;
			ZMax = z;

			Blocks = new SchematicBlock[x + 1, y + 1, z + 1];

			for (int Y = 0; Y <= YMax; Y++)
			{
				for (int Z = 0; Z <= ZMax; Z++)
				{
					for (int X = 0; X <= XMax; X++)
					{
						string blockid;
						if (World.TryGetTypeAt(corner1 + new Vector3Int(X, Y, Z), out ItemTypes.ItemType type))
							blockid = type.Name;
						else
							blockid = "air";

						Blocks[X, Y, Z] = new SchematicBlock()
						{
							X = X,
							Y = Y,
							Z = Z,
							BlockID = blockid
						};
					}
				}
			}
		}


		public Schematic(string file) : base(file)
		{
			NbtFile nbtFile = new NbtFile(file);

			RawSchematic raw = new RawSchematic(nbtFile);

			Name = Path.GetFileNameWithoutExtension(nbtFile.FileName);
			XMax = raw.XMax;
			YMax = raw.YMax;
			ZMax = raw.ZMax;
			Blocks = raw.CSBlocks;
		}

		public SchematicBlock GetSBlock(int X, int Y, int Z)
		{
			SchematicBlock block = default(SchematicBlock);

			if (Y < YMax &&
					X < XMax &&
					Z < ZMax)
				block = Blocks[X, Y, Z];

			if (block == default(SchematicBlock))
				block = SchematicBlock.Air;

			return block;
		}

		public override void Rotate()
		{
			SchematicBlock[,,] newBlocks = new SchematicBlock[ZMax + 1, YMax + 1, XMax + 1];

			for (int y = 0; y <= YMax; y++)
			{
				for (int z = 0; z <= ZMax; z++)
				{
					for (int x = 0; x <= XMax; x++)
					{
						int newX = x;
						int newZ = (ZMax + 1) - (z + 1);

						if (Blocks[x, y, z].BlockID.Contains("z+"))
						{
							Blocks[x, y, z].BlockID = Blocks[x, y, z].BlockID.Replace("z+", "x-");
						}
						else if (Blocks[x, y, z].BlockID.Contains("z-"))
						{
							Blocks[x, y, z].BlockID = Blocks[x, y, z].BlockID.Replace("z-", "x+");
						}
						else if (Blocks[x, y, z].BlockID.Contains("x+"))
						{
							Blocks[x, y, z].BlockID = Blocks[x, y, z].BlockID.Replace("x+", "z+");
						}
						else if (Blocks[x, y, z].BlockID.Contains("x-"))
						{
							Blocks[x, y, z].BlockID = Blocks[x, y, z].BlockID.Replace("x-", "z-");
						}

						newBlocks[newZ, y, newX] = Blocks[x, y, z];
					}
				}
			}

			Blocks = newBlocks;

			int tmpSize = XMax;
			XMax = ZMax;
			ZMax = tmpSize;
		}

		public override string ToString()
		{
			return $"Name: {Name}  Max Bounds: [{XMax}, {YMax}, {ZMax}]";
		}

		public override int GetMaxX()
		{
			return XMax;
		}

		public override int GetMaxY()
		{
			return YMax;
		}

		public override int GetMaxZ()
		{
			return ZMax;
		}

		public override ushort GetBlock(int x, int y, int z)
		{
			var b = GetSBlock(x, y, z);

			ItemTypes.ItemType itemType;

			if (!ItemTypes.TryGetType(b.BlockID, out itemType))
			{
				itemType = BuiltinBlocks.Types.air;
			}

			return itemType.ItemIndex;
		}

		public override void Save(string name)
		{
			List<NbtTag> tags = new List<NbtTag>();

			tags.Add(new NbtInt("Width", XMax));
			tags.Add(new NbtInt("Height", YMax));
			tags.Add(new NbtInt("Length", ZMax));

			List<NbtTag> blocks = new List<NbtTag>();

			for (int Y = 0; Y < YMax; Y++)
			{
				for (int Z = 0; Z < ZMax; Z++)
				{
					for (int X = 0; X < XMax; X++)
					{
						NbtCompound compTag = new NbtCompound();
						compTag.Add(new NbtInt("x", X));
						compTag.Add(new NbtInt("y", Y));
						compTag.Add(new NbtInt("z", Z));
						compTag.Add(new NbtString("id", Blocks[X, Y, Z].BlockID));
						blocks.Add(compTag);
					}
				}
			}

			NbtList nbtList = new NbtList("CSBlocks", blocks);
			tags.Add(nbtList);

			NbtFile nbtFile = new NbtFile(new NbtCompound("CompoundTag", tags));
			var fileSave = Path.Combine(StructureManager.Schematic_FOLDER + name + ".csschematic");

			if (File.Exists(fileSave))
				File.Delete(fileSave);

			nbtFile.SaveToFile(fileSave, NbtCompression.GZip);

		}
	}
}
