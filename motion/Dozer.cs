using MeshedObjects;
using ModLoaderInterfaces;
using Pipliz;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transport;
using UnityEngine;

namespace Improved_Construction.motion
{
	[ModLoader.ModManager]
	class Dozer : IAfterSelectedWorld, IOnPlayerClicked, IOnAssemblyLoaded
	{
		public static MeshedObjectType DozerType;
		public static String MODPATH;


		public List<TransportManager.Box> BoxColliders = new List<TransportManager.Box>()
								{
										new TransportManager.Box()
										{
												Size = new Vector3(1.115f, 0.56f, 1.115f),
												Offset = new Vector3(0.0f, -0.56f, -0.145f),
												EulerAngles = new Vector3(0.0f, 0.0f, 0.0f)

										}
								};

		public void OnAssemblyLoaded(string path)
		{
			// Get a nicely formatted version of our mod directory.
			MODPATH = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
		}

		//public void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> types)
		//{
		//	Log.Write("After Base Types?!?!");
		//}

		public void AfterSelectedWorld()
		{
			Log.Write("Loaded the Dozer!");
			string meshPath = MODPATH + "/meshes/Dozer.ply";
			FileTable.FileID fileID = ServerManager.FileTable.StartLoading(meshPath, ECachedFileType.MeshPly);
			MeshedObjectTypeSettings meshSettings = new MeshedObjectTypeSettings("Dozer", fileID, "neutral")
			{
				colliders = new List<ObjectCollider>(),
				InterpolationLooseness = 1.5f,
				sendUpdateRadius = 500
			};
			meshSettings.colliders.Add(new ObjectCollider(BoxColliders[0].ToRotatedBounds));
			Dozer.DozerType = MeshedObjectType.Register(meshSettings);
		}

		public void OnPlayerClicked(Players.Player player, PlayerClickedData click)
		{
			//Log.Write("CLICK" + click.HitType.ToString());
			if (click.IsHoldingButton || (click.ClickType != PlayerClickedData.EClickType.Right || click.OnBuildCooldown) || (click.HitType != PlayerClickedData.EHitType.Block || (int)click.TypeSelected != (int)ItemTypes.GetType("ghost").ItemIndex))
				return;
			//Log.WriteWarning("CLICKED!");
			click.ConsumedType = PlayerClickedData.EConsumedType.UsedAsTool;
			Dozer.CreateGlider(click.GetExactHitPositionWorld() + new Vector3(0.0f, 0.75f, 0.0f), Quaternion.Euler(0, -90, 0), DozerTransport.CreateVehicleDescription(MeshedObjectID.GetNew()), player);
		}

		public static DozerTransport CreateDozerPlacer(Players.Player player, Vector3 PlaceLocation, Quaternion rotation, SelectedArea area, ConstructionPlacer placer)
		{
			DozerTransport dozerPlacer = Dozer.CreateGlider(PlaceLocation, rotation, DozerTransport.CreateVehicleDescription(MeshedObjectID.GetNew()), player);
			dozerPlacer.setArea(area);
			dozerPlacer.setPlacer(placer);

			return dozerPlacer;
		}


		public static DozerTransport CreateGlider(
				Vector3 spawnPosition,
				Quaternion rotation,
				MeshedVehicleDescription vehicle,
				Players.Player playerInside)
		{

			//Log.Write("Start creating Dozer");
			DozerSettings settings = new DozerSettings();
			DozerTransport vehicle1 = DozerTransport.CreateDozer(spawnPosition, rotation, vehicle, playerInside, settings);
			//Log.Write("Created Dozer!" + vehicle1.ToString());
			MeshedObjectManager.Attach(playerInside, vehicle);
			return vehicle1;
		}

	}
}
