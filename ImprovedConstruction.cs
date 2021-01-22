using HarmonyLib;
using Jobs;
using Jobs.Implementations.Construction;
using ModLoaderInterfaces;
using NetworkUI;
using NetworkUI.AreaJobs;
using NetworkUI.Items;
using Newtonsoft.Json.Linq;
using Pipliz;
using Pipliz.JSON;
using Shared;
using System;
using System.Reflection;

namespace Improved_Construction
{
    [ModLoader.ModManager]
    public class ImprovedConstruction : IOnConstructCommandTool, IOnPlayerPushedNetworkUIButton, IOnPlayerSelectedTypePopup
    {
        static string MODPATH;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, "Wingdings.ImprovedConstruction.OnAssemblyLoaded")]
        public static void OnAssemblyLoaded(string path)
        {
            // Get a nicely formatted version of our mod directory.
            MODPATH = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            Log.Write("Better Chat initialized in " + MODPATH);

            //var harmony = new Harmony("Nick.BetterChat.Harmony");
            //harmony.PatchAll(Assembly.GetExecutingAssembly());
            ConstructionArea.RegisterLoader((IConstructionLoader)new ReplacerSpecialLoader());
            Players.Player player = null;
            NetworkMenuManager.TriggerTypeSelectionPopup(player, 640, 480, EAreaItemSelectionFilter.ComboBuildable, null);



            //CommandToolManager.AreaDescriptions.Add("wingdings.replacer", (IToolDescription)new ConstructionAreaToolSettings("popup.tooljob.replacer", "wingdings.replacer", "wingdings.replacer", EConstructionKind.Digger, 0, EAreaItemSelectionFilter.ComboDiggable));
            CustomConstructionAreaJobDefinition deff = new CustomConstructionAreaJobDefinition();
            AreaJobTracker.RegisterAreaJobDefinition(deff);
        }

        public void OnConstructCommandTool(Players.Player p, NetworkMenu menu, string menuName)
        {
            menu.Items.Add((IItem)new EmptySpace(20));
            ButtonCallback button = new ButtonCallback("wingdings.replacer", new LabelData("popup.tooljob.replacer", ELabelAlignment.Default, 16, LabelData.ELocalizationType.Sentence), 200, 45, ButtonCallback.EOnClickActions.None, (JToken)null, 0.0f, 0.0f, true);
            CommandToolManager.GenerateTwoColumnCenteredRow(menu, button, CommandToolManager.GetButtonTool(p, "constructionjob", "popup.tooljob.constructionjob", 200, true));
        }

        public void OnPlayerPushedNetworkUIButton(ButtonPressCallbackData data)
        {
            Log.Write("Network button pushed!" + data.ButtonIdentifier);
            if (!(data.ButtonIdentifier == "wingdings.replacer"))
                return;
            //NetworkMenuManager.CloseServerPopup(data.Player);
            JSONNode payload = new JSONNode(NodeType.Object).SetAs<int>("wingdings.construction.selection", 1);
            NetworkMenuManager.TriggerTypeSelectionPopup(data.Player, 640, 480, EAreaItemSelectionFilter.ComboDiggable, payload);
            Log.Write("Sent!");

        }

        public void OnPlayerSelectedTypePopup(Players.Player player, ushort typeSelected, JSONNode payload)
        {
            int result;
            if (!payload.TryGetAs<int>("wingdings.construction.selection", out result))
                return;

            switch (result)
            {
                case 1:

                    {
                        Log.Write("Got selection 1");
                        payload.SetAs<int>("wingdings.construction.selection1", typeSelected);
                        payload.SetAs<int>("wingdings.construction.selection", 2);
                        //NetworkMenuManager.CloseServerPopup(player);
                        NetworkMenu menu = new NetworkMenu();
                        NetworkMenuManager.SendServerPopup(player, menu);
                        NetworkMenuManager.TriggerTypeSelectionPopup(player, 640, 480, EAreaItemSelectionFilter.ComboBuildable, payload);
                        Log.Write("Sent For Round 2!");
                        break;
                    }
                case 2:
                    {
                        Log.Write("Got selection 2");

                        ConstructionAreaToolSettings data1 = new ConstructionAreaToolSettings("wingdings.test", "wingdings.customconstruction", "pipliz.digger", EConstructionKind.Digger);

                        GenericCommandToolSettings data =  new GenericCommandToolSettings()
                        {
                            JSONData = payload,
                            Maximum2DBlockCount = 100,
                            Minimum2DBlockCount = 6,
                            Maximum3DBlockCount = 1000,
                            Minimum3DBlockCount = 6,
                            MaximumHeight = 100,
                            MinimumHeight = 1,
                            OneAreaOnly = true,
                            Key = "wingdings.customconstruction",
                            NPCTypeKey = "Wingdings.test2",
                            TranslationKey = "Wingdings.test3"
                        };
                        CommandToolManager.StartCommandToolSelection(player, data1);

                        break;
                    }
                default:
                    {
                        Log.WriteError("Unexpected case! " + payload.ToString());
                        break;
                    }
            }
            Log.Write(payload.ToString());

        }


    }
}
