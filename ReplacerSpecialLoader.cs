using Jobs.Implementations.Construction;
using Jobs.Implementations.Construction.Iterators;
using Pipliz;
using Pipliz.JSON;

namespace Improved_Construction
{
  public class ReplacerSpecialLoader : IConstructionLoader
  {
    public static Pipliz.Collections.SortedList<Colony, ReplaceType> selectionMap = new Pipliz.Collections.SortedList<Colony, ReplaceType>();

    public string JobName
    {
      get
      {
        return "wingdings.replacer";
      }
    }

    public void ApplyTypes(ConstructionArea area, JSONNode node)
    {
            ReplaceType type = GetSelection(area.Owner);
            area.ConstructionType = (IConstructionType) new ReplacerSpecial(type);
            area.IterationType = (IIterationType) new TopToBottom(area);
    }

    public void SaveTypes(ConstructionArea area, JSONNode node)
    {
    }

    public static void RegisterSelection(Colony colony, ReplaceType type)
        {
            int foundIndex;
            if (selectionMap.Contains(colony, out foundIndex))
            {
                selectionMap.SetValueAtIndex(foundIndex, type);
            }
            else
            {
                selectionMap.Add(colony, type);
            }
        }

    public static ReplaceType GetSelection(Colony colony)
        {
            int foundIndex;
            if (selectionMap.Contains(colony, out foundIndex))
            {
                return selectionMap.GetValueAtIndex(foundIndex);
            }
            else
            {
                Log.WriteError("Getting a selection that has not been made!");
                return new ReplaceType();
            }
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
        }
  }
}
