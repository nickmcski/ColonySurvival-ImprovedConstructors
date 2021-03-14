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
using static ExtendedBuilder.Persistence.Structure;
using Math = Pipliz.Math;

namespace Improved_Construction
{
	public class SelectedArea
	{
		public Rotation rotation = Structure.Rotation.Front;

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
		public Vector3Int cornerMin { get; internal set; }
		public Vector3Int cornerMax { get; internal set; }

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
			cornerMin = Vector3Int.Min(pos1, pos2);
			cornerMax = Vector3Int.Max(pos1, pos2);
		}

		public int GetXSize() { return Math.Abs(pos1.x - pos2.x) + 1; }
		public int GetYSize() { return Math.Abs(pos1.y - pos2.y) + 1; }
		public int GetZSize() { return Math.Abs(pos1.z - pos2.z) + 1; }
		public int GetSize() { return GetXSize() * GetYSize() * GetZSize(); }

		public void Rotate()
		{
			if (args == null)
				return;
			rotation = Structure.RotateClockwise(rotation);

			var center = (cornerMin + cornerMax) / 2;
			var offset = (cornerMax - cornerMin) / 2;

			cornerMin = new Vector3Int(center.x - offset.z, cornerMin.y, center.z - offset.x);
			cornerMax = new Vector3Int(center.x + offset.z, cornerMax.y, center.z + offset.x);
			pos1 = cornerMin;
			pos2 = cornerMax;
			//TODO Shift corners around centerpoint
		}

		public AreaHighlight GetAreaHighlight()
		{
			return new AreaHighlight(cornerMin, cornerMax, Shared.EAreaMeshType.AutoSelectActive, Shared.EServerAreaType.Default);
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
