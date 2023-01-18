using Chatting;
using ExtendedBuilder.Persistence;
using Jobs;
using Jobs.Implementations.Construction;
using Newtonsoft.Json.Linq;
using NPC;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Text;

namespace Improved_Construction.CopyTool
{
	public  class CopyJobDefinition : AbstractAreaJobDefinition
	{
		public CopyJobDefinition()
		{
			Identifier = "wingdings.copytool";
		}

		public override IAreaJob CreateAreaJob(Colony owner, Vector3Int min, Vector3Int max)
		{
			Log.Write("Created Copy Area {0} - {1}", min, max);
			//TODO Copy area using Min and Max Coords
			return new CopyAreaJob(this, owner, min, max, null);
		}

		public override IAreaJob LoadAreaJob(Colony owner, Vector3Int min, Vector3Int max, NPCID? npcID, JObject miscData)
		{
			//No need to load - Job will go away once area is copied.
			return null;
		}
	}

	public class CopyAreaJob : AbstractAreaJob, IAreaJobSubArguments
	{
		Blueprint blueprint;
		public CopyAreaJob(IAreaJobDefinition definition, Colony owner, Vector3Int min, Vector3Int max, NPCID? npcID) : base(definition, owner, min, max, npcID)
		{
			Log.Write("Created Copy Area Job {0} - {1}", min, max);
			blueprint = new Blueprint(min, max);


		}

		public override void CalculateSubPosition()
		{
			throw new NotImplementedException();
		}

		public override void OnNPCAtJob(ref NPCBase.NPCState state)
		{
			throw new NotImplementedException();
		}

		JObject args;
		public void SetArgument(JObject args)
		{
			this.args = args;

			//Save Blueprint

			
			JToken argName = args["wingdings.copy.name"];
			if(argName == null)
			{
				Log.Write("Error saving blueprint - Name was not provided");
				return;
			}
			string name = argName.Value<string>();
			//blueprint.Save(name);
			StructureManager.SaveStructure(blueprint, name);
			//TODO Send notice that blueprint was saved
			Chat.SendToConnected("Blueprint <b>" + name + "</b> saved!", EChatSendOptions.LogAll);

		}

		public bool createSchematic()
		{

			return false;
		}

		public override bool IsValid { get => false; protected set => base.IsValid = value; }
	}


}
