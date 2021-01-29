using System.Linq;

namespace ExtendedBuilder.Persistence
{
    public class SchematicBlock
    {
        static SchematicBlock()
        {
            Air = new SchematicBlock();
        }

        public static SchematicBlock Air { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public string BlockID { get; set; } = "Air";

        public override string ToString()
        {
            return string.Format("ID: {3}, X: {0}, Y: {1}, Z: {2}", X, Y, Z, BlockID);
        }
    }
}
