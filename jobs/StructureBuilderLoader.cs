using Jobs.Implementations.Construction;
using ExtendedBuilder.Persistence;
using Pipliz.JSON;
using ModLoaderInterfaces;

namespace ExtendedBuilder.Jobs
{
	[ModLoader.ModManager]
	public class StructureBuilderLoader : IConstructionLoader, IAfterItemTypesDefined
	{
		public static readonly string NAME = "Khanx.ExtendedBuilder.Jobs.Construction.SchematicBuilder";

		public string JobName => NAME;

		public void ApplyTypes(ConstructionArea area, JSONNode node)
		{
			if (node == null)
				return;

			if (node.TryGetAs(NAME + ".StructureName", out string schematic)
					 && node.TryGetAs(NAME + ".Rotation", out Structure.Rotation rotation))
			{
				area.IterationType = new StructureIterator(area, schematic, rotation);
				area.ConstructionType = new StructureBuilder();
			}
		}

		public void SaveTypes(ConstructionArea area, JSONNode node)
		{
			var itt = area.IterationType as StructureIterator;

			if (itt != null)
			{
				node.SetAs(NAME + ".StructureName", itt.SchematicName);
				node.SetAs(NAME + ".Rotation", itt.rotation);

				node.SetAs(NAME + ".LocationX", itt.location.x);
				node.SetAs(NAME + ".LocationY", itt.location.y);
				node.SetAs(NAME + ".LocationZ", itt.location.z);

			}
		}


		public void AfterItemTypesDefined()
		{
			ConstructionArea.RegisterLoader(new StructureBuilderLoader());
		}
	}
}
