using ExtendedBuilder.Jobs;
using ExtendedBuilder.Persistence;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AreaJobTracker;
using Math = Pipliz.Math;

namespace Improved_Construction
{
	public class SelectedArea
	{
		public SelectedArea()
		{

		}

		public SelectedArea(Vector3Int loc1, Vector3Int loc2)
		{
			SetCorner1(loc1);
			SetCorner2(loc2);
		}

		public SelectedArea(Vector3Int loc1, Vector3Int loc2, JSONNode args) : this(loc1, loc2)
		{
			this.args = args;
		}

		public Vector3Int pos1 = Vector3Int.maximum;
		public Vector3Int pos2 = Vector3Int.maximum;
		public JSONNode args;
		public Vector3Int corner1 { get; internal set; }
		public Vector3Int corner2 { get; internal set; }

		public bool IsPos1Initialized() { return pos1 != Vector3Int.maximum; }
		public bool IsPos2Initialized() { return pos2 != Vector3Int.maximum; }

		public void SetCorner1(Vector3Int newPos)
		{
			pos1 = newPos;
			UpdateCorner();
		}

		public void SetCorner2(Vector3Int newPos)
		{
			pos2 = newPos;
			UpdateCorner();
		}

		public void UpdateCorner()
		{
			corner1 = Vector3Int.Min(pos1, pos2);
			corner2 = Vector3Int.Max(pos1, pos2);
		}

		public int GetXSize() { return Math.Abs(pos1.x - pos2.x) + 1; }
		public int GetYSize() { return Math.Abs(pos1.y - pos2.y) + 1; }
		public int GetZSize() { return Math.Abs(pos1.z - pos2.z) + 1; }
		public int GetSize() { return GetXSize() * GetYSize() * GetZSize(); }

		public void Rotate()
		{
			if (args == null)
				return;
			Structure.Rotation rotation;
			args.TryGetAs(StructureBuilderLoader.NAME + ".Rotation", out rotation);
			args.SetAs(StructureBuilderLoader.NAME + ".Rotation", Structure.RotateClockwise(rotation));

			//TODO Shift corners around centerpoint
		}

		public AreaHighlight GetAreaHighlight()
		{
			return new AreaHighlight(corner1, corner2, Shared.EAreaMeshType.AutoSelectActive, Shared.EServerAreaType.Default);
		}

		public void Move(Vector3Int offset)
		{
			//TODO
			pos1 += offset;
			pos2 += offset;
			UpdateCorner();
		}
	}
}
