using Pipliz;
using System.Collections.Generic;
using System.IO;
using ModLoaderInterfaces;

namespace ExtendedBuilder.Persistence
{
	[ModLoader.ModManager]
	public class StructureManager : IOnAssemblyLoaded, IAfterItemTypesDefined
	{
		public static string MOD_FOLDER = @"";

		public static string Blueprint_FOLDER = "";
		public static string Schematic_FOLDER = "";

		public static Dictionary<string, Structure> _structures = new Dictionary<string, Structure>();

		public void OnAssemblyLoaded(string path)
		{
			MOD_FOLDER = Path.GetDirectoryName(path).Replace("\\", "/");

			Blueprint_FOLDER = MOD_FOLDER + "/Blueprints/";
			Schematic_FOLDER = MOD_FOLDER + "/Schematics/";
		}

		public void AfterItemTypesDefined()
		{
			LoadStructures();
		}

		public static void LoadStructures()
		{
			_structures.Clear();

			if (Directory.Exists(Blueprint_FOLDER))
			{
				string[] prefixFiles = Directory.GetFiles(Blueprint_FOLDER, "*.b", SearchOption.AllDirectories);
				Log.Write(string.Format("<color=blue>Loading blueprints: {0}</color>", prefixFiles.Length));

				foreach (string file in prefixFiles)
				{
					string blueprint_name = file.Substring(file.LastIndexOf("/") + 1).Trim().ToLower();
					blueprint_name = blueprint_name.Substring(0, blueprint_name.Length - 2);
					Structure structure = new Blueprint(file);
					_structures.Add(blueprint_name, structure);
					Log.Write(string.Format("<color=blue>Loaded blueprint: {0}</color>", blueprint_name));
				}

			}
			else
				Directory.CreateDirectory(Blueprint_FOLDER);

			if (Directory.Exists(Schematic_FOLDER))
			{
				string[] prefixFiles = Directory.GetFiles(Schematic_FOLDER, "*.csschematic", SearchOption.AllDirectories);
				Log.Write(string.Format("<color=blue>Loading schematics: {0}</color>", prefixFiles.Length));

				foreach (string file in prefixFiles)
				{
					string schematic_name = file.Substring(file.LastIndexOf("/") + 1).Trim().ToLower();
					schematic_name = schematic_name.Substring(0, schematic_name.Length - 12);

					if (_structures.ContainsKey(schematic_name))
					{
						Log.Write(string.Format("<color=red>The {0} schematic has not been added since a blueprint with the same name already exists.</color>", schematic_name));
						continue;
					}
					Structure structure = new Schematic(file);
					_structures.Add(schematic_name, structure);

					Log.Write(string.Format("<color=blue>Loaded blueprint: {0}</color>", schematic_name));
				}
			}
			else
				Directory.CreateDirectory(Schematic_FOLDER);
		}

		public static Structure GetStructure(string name)
		{
			Structure structure = null;

			if (!_structures.TryGetValue(name, out structure))
				return null;

			return structure;
		}

		public static bool SaveStructure(Structure structure, string name)
		{
			if (_structures.ContainsKey(name))
				return false;

			structure.Save(name);
			LoadStructures();

			return true;
		}
	}
}
