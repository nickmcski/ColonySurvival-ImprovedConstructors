using ExtendedBuilder.Persistence;
using Jobs.Implementations.Construction;
using Pipliz;

namespace ExtendedBuilder.Jobs
{
	public class StructureIterator : IIterationType
	{
		protected ConstructionArea area;
		protected Vector3Int positionMin;
		protected Vector3Int positionMax;

		protected Vector3Int cursor;
		protected Vector3Int iterationChunkLocation;

		public Vector3Int location;
		public Structure.Rotation rotation;


		public string SchematicName { get; private set; }
		public Structure BuilderSchematic { get; private set; }

		public StructureIterator(ConstructionArea area, string schematicName, Structure.Rotation rotation)
		{
			this.area = area;
			this.rotation = rotation;

			positionMin = area.Minimum;
			positionMax = area.Maximum;
			this.location = area.Minimum;

			iterationChunkLocation = positionMin;
			cursor = positionMin;

			SchematicName = schematicName;

			BuilderSchematic = StructureManager.GetStructure(schematicName);


			if (BuilderSchematic == null)
			{
				Chatting.Chat.SendToConnected("<color=red>SchematicIterator: Structure " + schematicName + " not found </color>");

				return;
			}
			BuilderSchematic.Rotate(rotation);
		}

		public Vector3Int CurrentPosition { get { return cursor; } }

		public bool IsInBounds(Vector3Int location)
		{
			return location.x >= positionMin.x && location.x <= positionMax.x
					&& location.y >= positionMin.y && location.y <= positionMax.y
					&& location.z >= positionMin.z && location.z <= positionMax.z;
		}

		public bool MoveNext()
		{
			var next = cursor.Add(1, 0, 0);

			if (next.x > positionMax.x)
			{
				next.x = positionMin.x;
				next = next.Add(0, 0, 1);
			}

			if (next.z > positionMax.z)
			{
				next.z = positionMin.z;
				next = next.Add(0, 1, 0);
			}

			cursor = next;

			if (IsInBounds(cursor))
				return true;
			else
				return false;

		}
	}
}
