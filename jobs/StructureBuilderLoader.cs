using Jobs.Implementations.Construction;
using ExtendedBuilder.Persistence;
using ModLoaderInterfaces;
using Newtonsoft.Json.Linq;
using Pipliz;

namespace ExtendedBuilder.Jobs
{
	[ModLoader.ModManager]
	public class StructureBuilderLoader : IConstructionLoader, IAfterItemTypesDefined
	{
		public static readonly string NAME = "Khanx.ExtendedBuilder.Jobs.Construction.SchematicBuilder";

		public string JobName => NAME;

		public void AfterItemTypesDefined()
		{
			ConstructionArea.RegisterLoader(new StructureBuilderLoader());
		}

		public void ApplyTypes(ConstructionArea area, JObject node)
		{
			if (node == null)
				return;
			int rotation_int;
			if (node.TryGetAs(NAME + ".StructureName", out string schematic)
					 && node.TryGetAs(NAME + ".Rotation", out rotation_int))
			{
				Structure.Rotation rotation = (Structure.Rotation) rotation_int;
				area.IterationType = new StructureIterator(area, schematic, rotation);
				area.ConstructionType = new StructureBuilder();
			}
		}

		public void SaveTypes(ConstructionArea area, JObject node)
		{
			var itt = area.IterationType as StructureIterator;

			if (itt != null)
			{
				node.SetAs(NAME + ".StructureName", itt.SchematicName);
				node.SetAs(NAME + ".Rotation", (int)itt.rotation);

				node.SetAs(NAME + ".LocationX", itt.location.x);
				node.SetAs(NAME + ".LocationY", itt.location.y);
				node.SetAs(NAME + ".LocationZ", itt.location.z);

			}
		}
	}
}
