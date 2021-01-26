using BlockTypes;
using Jobs.Implementations.Construction;
using Jobs.Implementations.Construction.Iterators;
using Jobs.Implementations.Construction.Types;
using Pipliz;
using Pipliz.JSON;
using Shared.Networking;
using static ItemTypes;

namespace Improved_Construction
{
    public class ShapeBuilderLoader : IConstructionLoader
    {
        private string _jobName;

        public ShapeBuilderLoader(string jobName)
        {
            this._jobName = jobName;
        }

        public string JobName
        {
            get
            {
                return this._jobName;
            }
        }

        public void ApplyTypes(ConstructionArea area, JSONNode node)
        {
            JSONNode node1;
            if (node == null || !node.TryGetChild("selectedTypes", out node1) || (node1.NodeType != NodeType.Array || node1.ChildCount <= 0))
                return;
            ItemTypes.ItemType type = ItemTypes.GetType(ItemTypes.IndexLookup.GetIndex(node1[0].GetAs<string>()));
            if (!(type != (ItemTypes.ItemType)null) || type.ItemIndex == (ushort)0)
                return;
            area.ConstructionType = (IConstructionType)new BuilderBasic(type);
            area.IterationType = GetIterationType(area);

            postLoad(area);
        }

        public void SaveTypes(ConstructionArea area, JSONNode node)
        {
        }

        private void postLoad (ConstructionArea area)
        {
            IIterationType iteration = GetIterationType(area);
            fillGhost(area, iteration);
        }

        private void fillGhost(ConstructionArea area, IIterationType iteration)
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
                        sendGhostBlock(area.Owner, currentPosition, ghostBlock);
                    }
                }
                iteration.MoveNext();
            }
        }

        private void sendGhostBlock (Colony colony, Vector3Int position, ItemType type)
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

        private IIterationType GetIterationType(ConstructionArea area)
        {
            switch (JobName)
            {
                case "wingdings.walls":
                    return new Walls(area);
                case "wingdings.pyramid":
                    return new Pyramid(area);
                case "wingdings.circle":
                    return new Circle(area);
            }
            return null;
        }
    }
}
