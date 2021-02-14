using Jobs;
using Jobs.Implementations.Construction;
using ModLoaderInterfaces;
using NetworkUI;
using NetworkUI.AreaJobs;
using NetworkUI.Items;
using Newtonsoft.Json.Linq;
using Pipliz;
using Pipliz.JSON;
using Science;
using Shared;
using System;
using System.Reflection;

namespace Improved_Construction
{
	[ModLoader.ModManager]
	public class ImprovedConstruction : IOnConstructCommandTool, IOnPlayerPushedNetworkUIButton, IOnPlayerSelectedTypePopup, IOnAssemblyLoaded
	{
		public void OnAssemblyLoaded(string path)
		{
			//Register loader for Construction job callback
			ConstructionArea.RegisterLoader((IConstructionLoader)new ReplacerSpecialLoader());
			ConstructionArea.RegisterLoader((IConstructionLoader)new ShapeBuilderLoader("wingdings.walls"));
			ConstructionArea.RegisterLoader((IConstructionLoader)new ShapeBuilderLoader("wingdings.pyramid"));
			ConstructionArea.RegisterLoader((IConstructionLoader)new ShapeBuilderLoader("wingdings.circle"));

			//Override identifier so we can create our own callback using the Construction Job
			AreaJobTracker.RegisterAreaJobDefinition(new CustomConstructionAreaJobDefinition());
			CommandToolManager.AddAreaJobSettings(new ConstructionAreaToolSettings("wingdings.tooljob.walls", "wingdings.walls", "wingdings.builder.2d", EConstructionKind.Builder, 1, EAreaItemSelectionFilter.ComboBuildable));
			CommandToolManager.AddAreaJobSettings(new ConstructionAreaToolSettings("wingdings.tooljob.pyramid", "wingdings.pyramid", "wingdings.builder.3d", EConstructionKind.Builder, 1, EAreaItemSelectionFilter.ComboBuildable));
			CommandToolManager.AddAreaJobSettings(new ConstructionAreaToolSettings("wingdings.tooljob.circle", "wingdings.circle", "wingdings.builder.2d", EConstructionKind.Builder, 1, EAreaItemSelectionFilter.ComboBuildable));

			//dictionary.Add("pipliz.digger", ("popup.tooljob.diggera", "popup.tooljob.diggerb"));
			CommandToolManager.AddButtonTooltip("wingdings.tooljob.replacer", "wingdings.tooljob.replacera", "wingdings.tooljob.replacerb");
			CommandToolManager.AddButtonTooltip("wingdings.tooljob.walls", "wingdings.tooljob.wallsa", "wingdings.tooljob.wallsb");
			CommandToolManager.AddButtonTooltip("wingdings.tooljob.pyramid", "wingdings.tooljob.pyramida", "wingdings.tooljob.pyramidb");
			CommandToolManager.AddButtonTooltip("wingdings.tooljob.circle", "wingdings.tooljob.circlea", "wingdings.tooljob.circleb");
		}

		public void OnConstructCommandTool(Players.Player p, NetworkMenu menu, string menuName)
		{
			if (menuName != "popup.tooljob.construction")
				return;
			menu.Items.Add((IItem)new EmptySpace(20));

			bool unlocked = CommandToolManager.NPCAreaUnlocked(p, "pipliz.builder", out ScienceKey? _);
			ButtonCallback button = new ButtonCallback("wingdings.replacer", new LabelData("wingdings.tooljob.replacer", ELabelAlignment.Default, 16, LabelData.ELocalizationType.Sentence), 200, 45, ButtonCallback.EOnClickActions.None, (JToken)null, 0.0f, 0.0f, true)
			{
				Enabled = unlocked
			};

			ButtonCallback button2 = new ButtonCallback("windings.shapes", new LabelData("wingdings.tooljob.shapes", ELabelAlignment.Default, 16, LabelData.ELocalizationType.Sentence), 200, 45, ButtonCallback.EOnClickActions.None, (JToken)null, 0.0f, 0.0f, true)
			{
				Enabled = unlocked
			};
			CommandToolManager.GenerateTwoColumnCenteredRow(menu, button, button2);
		}

		public static void SendShapeMenu(Players.Player player)
		{
			NetworkMenu menuBase = CommandToolManager.GenerateMenuBase(player, true);
			CommandToolManager.GenerateTwoColumnCenteredRow(menuBase, CommandToolManager.GetButtonTool(player, "wingdings.tooljob.walls", "wingdings.tooljob.walls", 200, true), CommandToolManager.GetButtonTool(player, "wingdings.tooljob.pyramid", "wingdings.tooljob.pyramid", 200, true));
			menuBase.Items.Add((IItem)new EmptySpace(20));
			CommandToolManager.GenerateTwoColumnCenteredRow(menuBase, CommandToolManager.GetButtonTool(player, "wingdings.tooljob.circle", "wingdings.tooljob.circle", 200, true), (IItem)new EmptySpace(20));
			NetworkMenuManager.SendServerPopup(player, menuBase);
		}

		public void OnPlayerPushedNetworkUIButton(ButtonPressCallbackData data)
		{
			switch (data.ButtonIdentifier)
			{
				case "wingdings.replacer":
					JSONNode payload = new JSONNode(NodeType.Object).SetAs<int>("wingdings.construction.selection", 1);
					NetworkMenuManager.TriggerTypeSelectionPopup(data.Player, 640, 480, EAreaItemSelectionFilter.ComboDiggable, payload);
					return;
				case "windings.shapes":
					SendShapeMenu(data.Player);
					return;
				default:
					return;
			}
		}

		public void OnPlayerSelectedTypePopup(Players.Player player, ushort typeSelected, JSONNode payload)
		{
			int result;
			if (!payload.TryGetAs<int>("wingdings.construction.selection", out result))
				return;
			switch (result)
			{
				case 1:
					payload.SetAs<int>("wingdings.construction.selection1", typeSelected);
					payload.SetAs<int>("wingdings.construction.selection", 2);

					//Send blank menu. TriggerTypeSelectionPopup will only work when a menu is open
					NetworkMenu menu = new NetworkMenu();
					NetworkMenuManager.SendServerPopup(player, menu);

					NetworkMenuManager.TriggerTypeSelectionPopup(player, 640, 480, EAreaItemSelectionFilter.ComboBuildable, payload);
					break;
				case 2:
					payload.SetAs<int>("wingdings.construction.selection2", typeSelected);
					payload.SetAs<string>("constructionType", "wingdings.customconstruction");

					int limt = player.ActiveColony?.BuilderSizeLimit ?? Colony.BUILDER_LIMIT_START;

					GenericCommandToolSettings data = new GenericCommandToolSettings()
					{
						JSONData = payload,
						Maximum2DBlockCount = limt,
						Minimum2DBlockCount = 1,
						Maximum3DBlockCount = limt,
						Minimum3DBlockCount = 1,
						MaximumHeight = 100,
						MinimumHeight = 1,
						OneAreaOnly = true,
						Key = "wingdings.customconstruction",
						NPCTypeKey = "pipliz.digger",
						TranslationKey = "wingdings.tooljob.replacer"
					};
					CommandToolManager.StartCommandToolSelection(player, data);
					break;
				default:
					Log.WriteError("Unexpected case! " + payload.ToString());
					break;
			}
		}
	}
}
