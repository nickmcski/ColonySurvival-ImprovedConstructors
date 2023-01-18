using Chatting;
using Jobs;
using ModLoaderInterfaces;
using NetworkUI;
using NetworkUI.AreaJobs;
using NetworkUI.Items;
using Newtonsoft.Json.Linq;
using Pipliz;
using System.Collections.Generic;
using System.Text;

namespace Improved_Construction.CopyTool
{
	[ModLoader.ModManager]
	[ChatCommandAutoLoader]
	public class CopyUI : IOnPlayerPushedNetworkUIButton, IChatCommand
	{
		public void OnPlayerPushedNetworkUIButton(ButtonPressCallbackData data)
		{
			if(data.ButtonIdentifier == "wingdings.copytool"){
				SendCopyMenu(data.Player);
				return;
			}

			if (data.ButtonIdentifier == "wingdings.blueprint.copy")
			{
				string newName = "";
				JToken newNameValue = data.Storage["windings.blueprint.newName"];
				if(newNameValue != null)
				{
					newName = (string) newNameValue;
				}

				if (newName == "")
				{
					//TODO Error handling
					newName = data.Player.Name + " - " + Pipliz.Random.Next(100000).ToString(); //Name followed by random numbers.
				}

				Log.Write("New Name: " + newName);


				//TODO Send Copy Job Selection.
				JObject args = new JObject();
				args["wingdings.copy.name"] = newName;
				int limt = data.Player.ActiveColonyGroup?.DiggerSizeLimit ?? 1000;
				GenericCommandToolSettings copyData = new GenericCommandToolSettings()
				{
					JSONData = args,
					Maximum2DBlockCount = limt,
					Minimum2DBlockCount = 1,
					Maximum3DBlockCount = limt,
					Minimum3DBlockCount = 1,
					MaximumHeight = 100,
					MinimumHeight = 1,
					OneAreaOnly = true,
					Key = "wingdings.copytool",
					NPCTypeKey = "pipliz.digger",
					TranslationKey = "wingdings.tooljob.copy"
				};
				CommandToolManager.StartCommandToolSelection(data.Player, copyData);

				return;
			}
		}

		public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
		{
			switch (splits[0])
			{
				case "/copy":
					SendCopyMenu(player);
					return true;
			}
			return false;
		}

		public static void SendCopyMenu(Players.Player player)
		{
			NetworkMenu menu = new NetworkMenu();
			menu.LocalStorage["header"] = "Copy Tool";
			menu.Width = 600; //TODO Tweak size
			menu.Height = 200;

			InputField inputField = new InputField("windings.blueprint.newName", 200, 45);
			ButtonCallback button = new ButtonCallback("wingdings.blueprint.copy", new LabelData("wingdings.blueprint.copytext", ELabelAlignment.Default, 16, LabelData.ELocalizationType.Sentence), 200, 45, ButtonCallback.EOnClickActions.ClosePopup, (JToken)null, 0.0f, 0.0f, true);

			CommandToolManager.GenerateTwoColumnCenteredRow(menu, inputField, button);

			NetworkMenuManager.SendServerPopup(player, menu);
		}
	}
}
