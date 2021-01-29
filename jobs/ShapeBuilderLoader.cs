using Jobs.Implementations.Construction;
using Jobs.Implementations.Construction.Types;
using Pipliz.JSON;
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
		}

		public void SaveTypes(ConstructionArea area, JSONNode node)
		{
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

		//TODO Add Sphere, Trianges
	}
}
