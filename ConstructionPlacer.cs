﻿using Chatting;
using ExtendedBuilder.Jobs;
using ExtendedBuilder.Persistence;
using Jobs;
using Jobs.Implementations.Construction;
using ModLoaderInterfaces;
using NetworkUI;
using NetworkUI.Items;
using Newtonsoft.Json.Linq;
using Pipliz;
using Pipliz.JSON;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Players;

namespace Improved_Construction
{
	[ModLoader.ModManager]
	[ChatCommandAutoLoader]
	class ConstructionPlacer : IChatCommand, IOnPlayerPushedNetworkUIButton, IOnSendAreaHighlights, IOnConstructTooltipUI
	{
		public static void SendSelectionMenu(Players.Player player)
		{
			NetworkMenu menu = new NetworkMenu();
			menu.LocalStorage.SetAs<string>("header", "Pick Menu");
			menu.Width = 600;
			menu.Height = 300;

			Table table = new Table(900, 300);
			table.AutoExpandHeight = false;
			table.ExternalMarginHorizontal = 3f;

			List<(IItem, int)> header = new List<(IItem, int)>();
			header.Add((new Label("Structure Name"), 100));
			header.Add((new Label("Size"), 100));
			HorizontalRow headerRow = new HorizontalRow(header, -1, 0.0f, 0.0f, 4f);
			table.Header = headerRow;
			table.Rows = new List<IItem>();

			foreach (KeyValuePair<string,Structure> structure in StructureManager._structures)
			{
				List<(IItem, int)> row = new List<(IItem, int)>();
				row.Add((new Label(structure.Key), 200));
				row.Add((new Label(structure.Value.GetSizeString()), 100));
				row.Add((GetButtonItem(structure.Key), 100));
				HorizontalRow horizontalRow = new HorizontalRow(row, 30);
				table.Rows.Add(new BackgroundColor(horizontalRow, -1, 20, 0.0f, 0.0f, 4f, 4f, Table.ITEM_BG_COLOR));
			}

			menu.Items.Add(table);
			NetworkMenuManager.SendServerPopup(player, menu);
		}

		public static IItem GetButtonItem(string structureName)
		{
			//TODO Check to see player has unlocked this size structure
			LabelData text = new LabelData("Select", ELabelAlignment.Default, 16, LabelData.ELocalizationType.None);
			JToken ButtonPayload = new JObject()
			{
				{
					"key",
					(JToken) structureName
				}
			};
			return (IItem)new ButtonCallback("wingdings.construction.structure", text, 110, 30, ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload, 0.0f, 0.0f, true)
			{
				TriggerHoverCallback = true
		  };
		}
			public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
		{
			switch (splits[0])
			{
				case "/load":
					SendSelectionMenu(player);
					return true;
				case "/apply":
					Apply(player);
					return true;
				case "/show":
					Show(player);
					return true;
				case "/chunk":
					ClearChunk(player);
					return true;
			}
			return false;
		}

		public void OnPlayerPushedNetworkUIButton(ButtonPressCallbackData data)
		{
			if (!(data.ButtonIdentifier == "wingdings.construction.structure"))
				return;
			string structureName = data.ButtonPayload["key"].Value<string>();
			Chat.Send(data.Player, "Got Structure! " + structureName);

			Structure structure = StructureManager.GetStructure(structureName);
			if (structure == null)
			{
				Chat.Send(data.Player, "Null structure");
				return;
			}
			Vector3Int location = new Vector3Int(data.Player.Position);
			Vector3Int maxSize;

			maxSize = location.Add(structure.GetMaxX(), structure.GetMaxY(), structure.GetMaxZ());


			JSONNode args = new JSONNode();
			args.SetAs("constructionType", StructureBuilderLoader.NAME);

			args.SetAs(StructureBuilderLoader.NAME + ".StructureName", structureName);
			args.SetAs(StructureBuilderLoader.NAME + ".Rotation", Structure.Rotation.Front);
			
			//args.SetAs(StructureBuilderLoader.NAME + ".LocationX", location.x);
			//args.SetAs(StructureBuilderLoader.NAME + ".LocationY", location.y);
			//args.SetAs(StructureBuilderLoader.NAME + ".LocationZ", location.z);

			//AreaJobTracker.CreateNewAreaJob("pipliz.constructionarea", args, data.Player.ActiveColony, location, maxSize);
			//AreaJobTracker.SendData(data.Player);
			setSelection(data.Player, location, maxSize, args);
		}

		public static Pipliz.Collections.SortedList<Player, SelectedArea> selectionTracker = new Pipliz.Collections.SortedList<Player, SelectedArea>();

		public static void setSelection(Player player, Vector3Int loc1, Vector3Int loc2, JSONNode args = null)
		{
			int foundIndex;
			if( selectionTracker.Contains(player, out foundIndex))
			{
				ref SelectedArea selected = ref selectionTracker.GetValueAtIndexRef(foundIndex);
				selected.SetCorner1(loc1);
				selected.SetCorner2(loc2);
			}
			else
			{
				selectionTracker.Add(player, new SelectedArea(loc1, loc2, args));
			}
			AreaJobTracker.SendData(player);
		}

		public void OnSendAreaHighlights(Player player, List<AreaJobTracker.AreaHighlight> highlights, List<ushort> showWhileHoldingTypes)
		{
			if (player == null)
				return;
			int foundIndex;
			if (!selectionTracker.Contains(player, out foundIndex))
				return;
			SelectedArea selectedArea = selectionTracker.GetValueAtIndex(foundIndex);
			//TODO Check if selection is valid

			highlights.Add(selectedArea.GetAreaHighlight());
		}

		public void Apply(Player player)
		{
			if (player == null)
				return;
			int foundIndex;
			if(!selectionTracker.Contains(player, out foundIndex))
			{
				Chat.Send(player, "You don't have valid selection");
				return;
			}
			SelectedArea selected = selectionTracker.GetValueAtIndex(foundIndex);

			JSONNode args = selected.args;
			args.SetAs("constructionType", StructureBuilderLoader.NAME);

			//args.SetAs(StructureBuilderLoader.NAME + ".LocationX", location.x);
			//args.SetAs(StructureBuilderLoader.NAME + ".LocationY", location.y);
			//args.SetAs(StructureBuilderLoader.NAME + ".LocationZ", location.z);

			AreaJobTracker.CreateNewAreaJob("pipliz.constructionarea", args, player.ActiveColony, selected.corner1, selected.corner2);
			selectionTracker.RemoveAt(foundIndex);

			AreaJobTracker.SendData(player);

		}

		public void Show(Player player)
		{
			if (player == null)
				return;
			int foundIndex;
			if (!selectionTracker.Contains(player, out foundIndex))
			{
				Chat.Send(player, "You don't have valid selection");
				return;
			}
			SelectedArea selected = selectionTracker.GetValueAtIndex(foundIndex);

			JSONNode args = selected.args;
			args.SetAs("constructionType", StructureBuilderLoader.NAME);

			//args.SetAs(StructureBuilderLoader.NAME + ".LocationX", location.x);
			//args.SetAs(StructureBuilderLoader.NAME + ".LocationY", location.y);
			//args.SetAs(StructureBuilderLoader.NAME + ".LocationZ", location.z);
			ConstructionArea area = new ConstructionArea(null, null, selected.corner1, selected.corner2);
			area.SetArgument(args);
			string structureName;
			if (!args.TryGetAs<string>(StructureBuilderLoader.NAME + ".StructureName", out structureName))
			{
				Log.WriteError("Could not get structure!");
				return;
			}
			Structure.Rotation rotation;
			if (!args.TryGetAs<Structure.Rotation>(StructureBuilderLoader.NAME + ".Rotation", out rotation))
			{
				Log.WriteError("Could not get rotation!");
				return;
			}

			StructureIterator iterator = new StructureIterator(area, structureName, rotation);
			GhostHelper.FillGhost(iterator);
		}

		public void OnConstructTooltipUI(Player player, ConstructTooltipUIData data)
		{
			if (data.hoverType != ETooltipHoverType.NetworkUiButton)
				return;

			Log.Write(data.hoverKey);
		}

		public void ClearChunk(Player player)
		{
			Chunk chunk = World.GetChunk(player.VoxelPosition.ToChunk());

			chunk.SendToReceivingPlayers(player);
			return;
		}

		//Load model size
		//Attach Mesh
		//Hook movement keys
		//Update position
		//Send new grid
		//Send new ghost

		//Apply, trigger OnSelectionMade

	}
}