using ExtendedBuilder.Jobs;
using ExtendedBuilder.Persistence;
using Jobs;
using Newtonsoft.Json.Linq;
using NPC;
using Pipliz;
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
	public class SelectedArea : IAreaJob
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

		public SelectedArea(Vector3Int loc1, Vector3Int loc2, JToken args) : this(loc1, loc2)
		{
			this.args = args;
		}

		public Vector3Int pos1 = Vector3Int.maximum;
		public Vector3Int pos2 = Vector3Int.maximum;
		public JToken args;
		public Vector3Int Minimum { get; internal set; }
		public Vector3Int Maximum { get; internal set; }
		public AreaJobIndex AreaJobIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public bool IsDirty { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


		public NPCBase NPC => throw new NotImplementedException();

		public Colony Owner => throw new NotImplementedException();

		public IAreaJobDefinition Definition => throw new NotImplementedException();

		public bool IsValid => throw new NotImplementedException();

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
			Minimum = Vector3Int.Min(pos1, pos2);
			Maximum = Vector3Int.Max(pos1, pos2);
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

			var center = (Minimum + Maximum) / 2;
			var offset = (Maximum - Minimum) / 2;

			Minimum = new Vector3Int(center.x - offset.z, Minimum.y, center.z - offset.x);
			Maximum = new Vector3Int(center.x + offset.z, Maximum.y, center.z + offset.x);
			pos1 = Minimum;
			pos2 = Maximum;
			//TODO Shift corners around centerpoint
		}

		public AreaHighlight GetAreaHighlight() //Change to IAreaJob
		{
			return new AreaHighlight(this);

			//cornerMin, cornerMax, Shared.EAreaMeshType.AutoSelectActive, Shared.EServerAreaType.Default
		}

		public void Move(Vector3Int offset)
		{
			//TODO
			pos1 += offset;
			pos2 += offset;
			UpdateCorner();
		}

		public void OnRemove()
		{
			throw new NotImplementedException();
		}

		public JToken GetMiscSaveData()
		{
			throw new NotImplementedException();
		}
	}
}
