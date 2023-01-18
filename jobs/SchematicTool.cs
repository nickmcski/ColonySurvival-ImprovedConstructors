/*
using NetworkUI;
using NetworkUI.Items;
using Pipliz;
using Pipliz.JSON;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AreaJobTracker;

namespace Pandaros.SchematicBuilder.Jobs.Construction
{
    public enum SchematicClickType
    {
        Build,
        Archetect
    }



    [ModLoader.ModManager]
    public class SchematicMenu
    {
        private static readonly List<Schematic.Rotation> _rotation = new List<Schematic.Rotation>()
        {
            Schematic.Rotation.Front,
            Schematic.Rotation.Right,
            Schematic.Rotation.Back,
            Schematic.Rotation.Left
        };

        private static readonly string Selected_Schematic = GameLoader.NAMESPACE + ".SelectedSchematic";
        static readonly LocalizationHelper _localizationHelper = new LocalizationHelper(GameLoader.NAMESPACE, "buildertool");
        private static Dictionary<Players.Player, Tuple<SchematicClickType, string, Schematic.Rotation>> _awaitingClick = new Dictionary<Players.Player, Tuple<SchematicClickType, string, Schematic.Rotation>>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicMenu.OpenMenu")]
        public static void OpenMenu(Players.Player player, PlayerClickedData playerClickData)
        {
            if (ItemTypes.IndexLookup.TryGetIndex(SchematicTool.NAME, out var schematicItem) &&
                playerClickData.TypeSelected == schematicItem)
            {
                if (player.ActiveColony == null)
                {
                    Chatting.Chat.Send(player, _localizationHelper, "ErrorOpening");
                    return;
                }
                
                if (!_awaitingClick.ContainsKey(player))
                {
                    SendMainMenu(player);
                }
                else
                {
                    var tuple = _awaitingClick[player];
                    _awaitingClick.Remove(player);

                    switch (tuple.Item1)
                    {
                        case SchematicClickType.Build:
                            Vector3Int location = playerClickData.GetVoxelHit().BlockHit.Add(0, 1, 0);
                            var args = new JSONNode();
                            args.SetAs("constructionType", GameLoader.NAMESPACE + ".SchematicBuilder");
                            args.SetAs(SchematicBuilderLoader.NAME + ".SchematicName", tuple.Item2);
                            args.SetAs(SchematicBuilderLoader.NAME + ".Rotation", tuple.Item3);

                            if (SchematicReader.TryGetSchematic(tuple.Item2, player.ActiveColony.ColonyID, location, out var schematic))
                            {
                                if (tuple.Item3 >= Schematic.Rotation.Right)
                                    schematic.Rotate();

                                if (tuple.Item3 >= Schematic.Rotation.Back)
                                    schematic.Rotate();

                                if (tuple.Item3 >= Schematic.Rotation.Left)
                                    schematic.Rotate();

                                var maxSize = location.Add(schematic.XMax - 1, schematic.YMax - 1, schematic.ZMax - 1);
                                AreaJobTracker.CreateNewAreaJob("pipliz.constructionarea", args, player.ActiveColony, location, maxSize);
                                AreaJobTracker.SendData(player);
                            }

                            break;

                        case SchematicClickType.Archetect:

                            break;
                    }
                }
            }
        }

        private static void SendMainMenu(Players.Player player)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Schematic Menu");
            List<FileInfo> options = SchematicReader.GetSchematics(player);

            menu.Items.Add(new DropDown(new LabelData(_localizationHelper.GetLocalizationKey("Schematic"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Sentence), Selected_Schematic, options.Select(fi => fi.Name.Replace(".csschematic", "")).ToList()));
            menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".ShowBuildDetails", new LabelData(_localizationHelper.GetLocalizationKey("Details"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter, 18, LabelData.ELocalizationType.Sentence)));
            menu.LocalStorage.SetAs(Selected_Schematic, 0);
            menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".SetScemanticName", new LabelData(_localizationHelper.GetLocalizationKey("Save"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter, 18, LabelData.ELocalizationType.Sentence)));

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSendAreaHighlights, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicMenu.OnSendAreaHighlights")]
        static void OnSendAreaHighlights(Players.Player goal, List<AreaHighlight> list, List<ushort> showWhileHoldingTypes)
        {
            showWhileHoldingTypes.Add(ColonyBuiltIn.ItemTypes.BED.Id);

            if (ItemTypes.IndexLookup.StringLookupTable.TryGetItem(SchematicTool.NAME, out var item))
                showWhileHoldingTypes.Add(item.ItemIndex);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicMenu.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            switch (data.ButtonIdentifier)
            {
                case GameLoader.NAMESPACE + ".SetScemanticName":
                    NetworkMenu saveMenu = new NetworkMenu();
                    saveMenu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("SaveSchematic", data.Player));
                    saveMenu.Width = 600;
                    saveMenu.Height = 300;
                    saveMenu.ForceClosePopups = true;
                    saveMenu.Items.Add(new Label(new LabelData(_localizationHelper.GetLocalizationKey("SaveInstructions"), UnityEngine.Color.black)));
                    saveMenu.Items.Add(new InputField("Construction.SetArchitectArea"));
                    saveMenu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".SetArchitectArea", new LabelData(_localizationHelper.GetLocalizationKey("Start"), UnityEngine.Color.black)));

                    NetworkMenuManager.SendServerPopup(data.Player, saveMenu);
                    break;

                case GameLoader.NAMESPACE + ".SetArchitectArea":
                    NetworkMenuManager.CloseServerPopup(data.Player);
                    if (data.Storage.TryGetAs("Construction.SetArchitectArea", out string schematicName))
                    {
                        var colonySaves = GameLoader.Schematic_SAVE_LOC + $"\\{data.Player.ActiveColony.ColonyID}\\";

                        if (!Directory.Exists(colonySaves))
                            Directory.CreateDirectory(colonySaves);

                        var schematicFile = Path.Combine(colonySaves, schematicName + ".csschematic");

                        if (File.Exists(schematicFile))
                            File.Delete(schematicFile);

                        var metaDataSave = Path.Combine(GameLoader.Schematic_SAVE_LOC, schematicName + ".csschematic.metadata.json");

                        if (File.Exists(metaDataSave))
                            File.Delete(metaDataSave);

                        AreaJobTracker.StartCommandToolSelection(data.Player, new CommandToolTypeData()
                        {
                            AreaType = "pipliz.constructionarea",
                            LocaleEntry = _localizationHelper.LocalizeOrDefault("Architect", data.Player),
                            JSONData = new JSONNode().SetAs(ArchitectLoader.NAME + ".ArchitectSchematicName", schematicName).SetAs("constructionType", GameLoader.NAMESPACE + ".Architect"),
                            OneAreaOnly = true,
                            Maximum3DBlockCount = int.MaxValue,
                            Maximum2DBlockCount = int.MaxValue,
                            MaximumHeight = int.MaxValue,
                            MinimumHeight = 1,
                            Minimum2DBlockCount = 1,
                            Minimum3DBlockCount = 1
                        });
                    }

                    break;

                case GameLoader.NAMESPACE + ".ShowMainMenu":
                    SendMainMenu(data.Player);
                    break;

                case GameLoader.NAMESPACE + ".ShowBuildDetails":
                    List<FileInfo> options = SchematicReader.GetSchematics(data.Player);
                    var index = data.Storage.GetAs<int>(Selected_Schematic);

                    if (options.Count > index)
                    {
                        var selectedSchematic = options[index];

                        if (SchematicReader.TryGetSchematicMetadata(selectedSchematic.Name, data.Player.ActiveColony.ColonyID, out SchematicMetadata schematicMetadata))
                        {
                            if (schematicMetadata.Blocks.Count == 1 && schematicMetadata.Blocks.ContainsKey(BlockTypes.BuiltinBlocks.Indices.air))
                                Chatting.Chat.Send(data.Player, _localizationHelper, "invlaidSchematic");
                            {
                                NetworkMenu menu = new NetworkMenu();
                                menu.Width = 800;
                                menu.Height = 600;
                                menu.LocalStorage.SetAs("header", selectedSchematic.Name.Replace(".csschematic","") + " " + _localizationHelper.LocalizeOrDefault("Details", data.Player));

                                menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Height", data.Player) + ": " + schematicMetadata.MaxY, UnityEngine.Color.black)));
                                menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Width", data.Player) + ": " + schematicMetadata.MaxZ, UnityEngine.Color.black)));
                                menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Length", data.Player) + ": " + schematicMetadata.MaxX, UnityEngine.Color.black)));
                                menu.LocalStorage.SetAs(Selected_Schematic, selectedSchematic.Name);

                                List<ValueTuple<IItem, int>> headerItems = new List<ValueTuple<IItem, int>>();
                                headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData("  ", UnityEngine.Color.black)), 200));
                                headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Item", data.Player), UnityEngine.Color.black)), 200));
                                headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Required", data.Player), UnityEngine.Color.black)), 200));
                                headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("InStockpile", data.Player), UnityEngine.Color.black)), 200));
                                menu.Items.Add(new HorizontalRow(headerItems));

                                foreach (var kvp in schematicMetadata.Blocks)
                                {
                                    try
                                    {
                                        if (ItemTypes.TryGetType(kvp.Key, out ItemTypes.ItemType item))
                                        {
                                            var stockpileCount = 0;
                                            data.Player.ActiveColony.Stockpile.Items.TryGetValue(item.ItemIndex, out stockpileCount);

                                            List<ValueTuple<IItem, int>> items = new List<ValueTuple<IItem, int>>();
                                            items.Add(ValueTuple.Create<IItem, int>(new ItemIcon(kvp.Key), 200));
                                            items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(item.Name, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)), 200));
                                            items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(" x " + kvp.Value.Count, UnityEngine.Color.black)), 200));
                                            items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(" x " + stockpileCount, UnityEngine.Color.black)), 200));
                                            menu.Items.Add(new HorizontalRow(items));
                                        }
                                        else
                                            SchematicBuilderLogger.Log(ChatColor.orange, "Unknown item for schematic: {0}", kvp.Key);
                                    }
                                    catch (Exception ex)
                                    {
                                        SchematicBuilderLogger.LogError(ex);
                                    }
                                }

                                menu.Items.Add(new DropDown(new LabelData(_localizationHelper.GetLocalizationKey("Rotation"), UnityEngine.Color.black), Selected_Schematic + ".Rotation", _rotation.Select(r => r.ToString()).ToList()));
                                menu.Items.Add(new HorizontalSplit(new ButtonCallback(GameLoader.NAMESPACE + ".ShowMainMenu", new LabelData("Back", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)),
                                                                   new ButtonCallback(GameLoader.NAMESPACE + ".SetBuildArea", new LabelData("Build", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter))));
                                menu.LocalStorage.SetAs(Selected_Schematic + ".Rotation", 0);

                                NetworkMenuManager.SendServerPopup(data.Player, menu);
                            }
                        }
                    }

                    break;

                case GameLoader.NAMESPACE + ".SetBuildArea":
                    var scem = data.Storage.GetAs<string>(Selected_Schematic);
                    var rotation = data.Storage.GetAs<int>(Selected_Schematic + ".Rotation");

                    SchematicBuilderLogger.Log("Schematic: {0}", scem);

                    if (SchematicReader.TryGetSchematicMetadata(scem, data.Player.ActiveColony.ColonyID, out SchematicMetadata metadata))
                    {
                        if (metadata.Blocks.Count == 1 && metadata.Blocks.ContainsKey(BlockTypes.BuiltinBlocks.Indices.air))
                            PandaChat.Send(data.Player, _localizationHelper, "invlaidSchematic", ChatColor.red);
                        {
                            _awaitingClick[data.Player] = Tuple.Create(SchematicClickType.Build, scem, _rotation[rotation]);
                            PandaChat.Send(data.Player, _localizationHelper, "instructions");
                            NetworkMenuManager.CloseServerPopup(data.Player);
                        }
                    }
        
                    break;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSendAreaHighlights, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicMenu.AreaHighlighted")]
        public static void AreaHighlighted(Players.Player player, List<AreaJobTracker.AreaHighlight> list, List<ushort> showWhileHoldingTypes)
        {
            if (ItemTypes.IndexLookup.TryGetIndex(SchematicTool.NAME, out var schematicItem))
                showWhileHoldingTypes.Add(schematicItem);
        }
    }
}
*/