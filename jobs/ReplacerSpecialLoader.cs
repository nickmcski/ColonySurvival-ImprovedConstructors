using Jobs.Implementations.Construction;
using Jobs.Implementations.Construction.Iterators;
using Newtonsoft.Json.Linq;
using Pipliz;

namespace Improved_Construction
{
	public class ReplacerSpecialLoader : IConstructionLoader
	{
		public static Pipliz.Collections.SortedList<Colony, ReplaceType> selectionMap = new Pipliz.Collections.SortedList<Colony, ReplaceType>();

		public string JobName
		{
			get
			{
				return "wingdings.customconstruction";
			}
		}

		public void ApplyTypes(ConstructionArea area, JObject node)
		{
			Log.Write("Appling Types " + node.ToString());
			ReplaceType type = new ReplaceType(node);
			area.ConstructionType = (IConstructionType)new ReplacerSpecial(type);
			area.IterationType = (IIterationType)new TopToBottom(area);
		}

		public void SaveTypes(ConstructionArea area, JObject node)
		{
		}

		public struct ReplaceType
		{
			public ItemTypes.ItemType dig;
			public ItemTypes.ItemType place;
			public ReplaceType(ItemTypes.ItemType dig, ItemTypes.ItemType place)
			{
				this.dig = dig;
				this.place = place;
			}

			public ReplaceType(JObject node)
			{
				ushort dig;
				ushort place;
				node.TryGetAs<ushort>("wingdings.construction.selection1", out dig);
				node.TryGetAs<ushort>("wingdings.construction.selection2", out place);

				this.dig = ItemTypes.GetType(dig);
				this.place = ItemTypes.GetType(place);

			}
		}
	}
}
