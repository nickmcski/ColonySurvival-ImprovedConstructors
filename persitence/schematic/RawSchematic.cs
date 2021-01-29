using fNbt;

namespace ExtendedBuilder.Persistence
{
    public class RawSchematicSize
    {
        public int XMax { get; set; }
        public int YMax { get; set; }
        public int ZMax { get; set; }

        public override string ToString()
        {
            return $"Max Bounds: [{XMax}, {YMax}, {ZMax}]";
        }
    }

    public class RawSchematic : RawSchematicSize
    {
        public string Materials { get; set; }
        public byte[] Blocks { get; set; }
        public byte[] Data { get; set; }
        public SchematicBlock[,,] CSBlocks { get; set; }
        public TileEntity[,,] TileEntities { get; set; }

        public RawSchematic(NbtFile nbtFile)
        {
            var rootTag = nbtFile.RootTag;

            foreach (NbtTag tag in rootTag.Tags)
            {
                switch (tag.Name)
                {
                    case "Width": //Short
                        XMax = tag.IntValue + 1;
                        break;
                    case "Height": //Short
                        YMax = tag.IntValue + 1;
                        break;
                    case "Length": //Short
                        ZMax = tag.IntValue + 1;
                        break;
                    case "Materials": //String
                        Materials = tag.StringValue;
                        break;
                    case "Blocks": //ByteArray
                        Blocks = tag.ByteArrayValue;
                        break;
                    case "Data": //ByteArray
                        Data = tag.ByteArrayValue;
                        break;
                    case "Entities": //List
                        break; //Ignore
                    case "Icon": //Compound
                        break; //Ignore
                    case "CSBlocks":
                        CSBlocks = GetCSBlocks(this, tag, new SchematicBlock[XMax + 1, YMax + 1, ZMax + 1]);
                        break;
                    case "SchematicaMapping": //Compound
                        tag.ToString();
                        break; //Ignore
                    default:
                        break;
                }
            }
        }

        private static SchematicBlock[,,] GetCSBlocks(RawSchematic raw, NbtTag csBlockTag, SchematicBlock[,,] list)
        {
            NbtList csBlocks = csBlockTag as NbtList;

            if (csBlocks != null)
            {
                foreach (NbtCompound compTag in csBlocks)
                {
                    NbtTag xTag = compTag["x"];
                    NbtTag yTag = compTag["y"];
                    NbtTag zTag = compTag["z"];
                    NbtTag idTag = compTag["id"];
                    SchematicBlock block = new SchematicBlock()
                    {
                        X = xTag.IntValue,
                        Y = yTag.IntValue,
                        Z = zTag.IntValue,
                        BlockID = idTag.StringValue
                    };

                    if (string.IsNullOrEmpty(block.BlockID))
                        block.BlockID = "0";

                    list[xTag.IntValue, yTag.IntValue, zTag.IntValue] = block;
                }
            }

            for (int Y = 0; Y <= raw.YMax; Y++)
            {
                for (int Z = 0; Z <= raw.ZMax; Z++)
                {
                    for (int X = 0; X <= raw.XMax; X++)
                    {
                        if (list[X, Y, Z] == null)
                            list[X, Y, Z] = new SchematicBlock()
                            {
                                X = X,
                                Y = Y,
                                Z = Z,
                                BlockID = BlockTypes.BuiltinBlocks.Types.air.Name
                            };
                    }
                }
            }

            return list;
        }

        public override string ToString()
        {
            return $"Max Bounds: [{XMax}, {YMax}, {ZMax}] CSBlock Count: {CSBlocks.LongLength} Blocks Count: {Blocks.LongLength}";
        }
    }
}
