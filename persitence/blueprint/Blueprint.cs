using Pipliz;
using System.Collections.Generic;
using BlockTypes;
using System.IO;

namespace ExtendedBuilder.Persistence
{
    public class Blueprint : Structure
    {
        private int xSize;
        private int ySize;
        private int zSize;
        public Dictionary<ushort, string> types = new Dictionary<ushort, string>();
        private ushort[,,] blocks;
        public Vector3Int playerMod;

        public override int GetMaxX() { return xSize - 1; }

        public override int GetMaxY() { return ySize - 1; }

        public override int GetMaxZ() { return zSize - 1; }

        public override void Rotate()
        {
            ushort[,,] newBlocks = new ushort[zSize + 1, ySize + 1, xSize + 1];

            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < zSize; x++)
                {
                    for (int z = 0; z < xSize; z++)
                    {
                        int newX = z;
                        int newZ = zSize - (x + 1);

                        string type = ItemTypes.IndexLookup.GetName(blocks[z, y, x]);
                        switch (type.Substring(type.Length - 2))
                        {
                            case "x+":
                                type = type.Replace("x+", "z+");
                                break;
                            case "x-":
                                type = type.Replace("x-", "z-");
                                break;
                            case "z+":
                                type = type.Replace("z+", "x-");
                                break;
                            case "z-":
                                type = type.Replace("z-", "x+");
                                break;
                            default:
                                break;
                        }

                        newBlocks[newZ, y, newX] = ItemTypes.GetType(type).ItemIndex;
                    }
                }
            }

            blocks = newBlocks;

            int tmpSize = xSize;
            xSize = zSize;
            zSize = tmpSize;
        }

        public override ushort GetBlock(int x, int y, int z)
        {
            return blocks[x, y, z];
        }

        public Blueprint(Structure structure) : base(structure)
        {
            xSize = structure.GetMaxX();
            ySize = structure.GetMaxY();
            zSize = structure.GetMaxZ();

            blocks = new ushort[xSize + 1, ySize + 1, zSize + 1];

            for (int x = 0; x <= xSize; x++)
            {
                for (int y = 0; y <= ySize; y++)
                {
                    for (int z = 0; z <= zSize; z++)
                    {
                        Vector3Int newPos = new Vector3Int(x, y, z);
                        ushort type = structure.GetBlock(x, y, z);

                        if (!types.ContainsKey(type))
                            types.Add(type, ItemTypes.IndexLookup.GetName(type));

                        blocks[x, y, z] = type;
                    }
                }
            }

            playerMod = new Vector3Int(0, 0, 0);
        }

        public Blueprint(string file) : base(file)
        {
            byte[] binaryBlueprint = File.ReadAllBytes(file);

            using (ByteReader raw = ByteReader.Get(binaryBlueprint))
            {
                playerMod = raw.ReadVariableVector3Int();
                int typesC = raw.ReadVariableInt();

                xSize = raw.ReadVariableInt();
                ySize = raw.ReadVariableInt();
                zSize = raw.ReadVariableInt();

                //From one world to another
                Dictionary<ushort, ushort> typesTransformation = new Dictionary<ushort, ushort>();

                using (ByteReader compressed = raw.ReadCompressed())
                {
                    for (int i = 0; i < typesC; i++)
                    {
                        ushort type_index = compressed.ReadVariableUShort();
                        string type_name = compressed.ReadString();

                        ushort new_type_index;

                        if (!ItemTypes.IndexLookup.TryGetIndex(type_name, out new_type_index))
                            new_type_index = BuiltinBlocks.Indices.missingerror;

                        typesTransformation.Add(type_index, new_type_index);
                        types.Add(new_type_index, type_name);
                    } //type

                    blocks = new ushort[xSize, ySize, zSize];

                    for (int x = 0; x < xSize; x++)
                    {
                        for (int y = 0; y < ySize; y++)
                        {
                            for (int z = 0; z < zSize; z++)
                            {
                                blocks[x, y, z] = typesTransformation.GetValueOrDefault(compressed.ReadVariableUShort(), BuiltinBlocks.Indices.missingerror);
                            }
                        }
                    }

                } //ByteReader compressed 
            }
        }

        public override void Save(string name)
        {
            using (ByteBuilder builder = ByteBuilder.Get())
            {
                builder.WriteVariable(playerMod);

                builder.WriteVariable(types.Count);

                builder.WriteVariable(xSize);
                builder.WriteVariable(ySize);
                builder.WriteVariable(zSize);

                using (ByteBuilder compressed = ByteBuilder.Get())
                {
                    foreach (var key in types.Keys)
                    {
                        compressed.WriteVariable(key);
                        compressed.Write(types[key]);
                    }

                    for (int x = 0; x < xSize; x++)
                    {
                        for (int y = 0; y < ySize; y++)
                        {
                            for (int z = 0; z < zSize; z++)
                            {
                                compressed.WriteVariable(blocks[x, y, z]);
                            }
                        }
                    }

                    builder.WriteCompressed(compressed);
                }

                File.WriteAllBytes(StructureManager.Blueprint_FOLDER + name + ".b", builder.ToArray());
            }
        }
    }
}
