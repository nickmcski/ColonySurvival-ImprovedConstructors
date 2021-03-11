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
						Size = new Vector3(0.56f, 0.205f, 1.115f),
						Offset = new Vector3(0.0f, -0.645f, -0.145f),
						EulerAngles = new Vector3(0.0f, 0.0f, 0.0f)

					},
					new TransportManager.Box()
					{
						Size = new Vector3(1.1f, 0.205f, 3.46f),
						Offset = new Vector3(0.0f, 1.83f, -1.85f),
						EulerAngles = new Vector3(0.0f, 0.0f, 0.0f)

					},
					new TransportManager.Box()
					{
						Size = new Vector3(9.05f, 0.205f, 0.75f),
						Offset = new Vector3(0.0f, 1.83f, 0.25f),
						EulerAngles = new Vector3(0.0f, 0.0f, 0.0f)
					},
					new TransportManager.Box()
					{
						Size = new Vector3(6.68f, 0.205f, 0.75f),
						Offset = new Vector3(0.0f, 1.83f, 1f),
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
			ServerManager.FileTable.StartLoading(meshPath, ECachedFileType.Mesh);
		  Dozer.DozerType = MeshedObjectType.Register(new MeshedObjectTypeSettings("Dozer", meshPath, "neutral")
			{
				colliders = BoxColliders.Select<TransportManager.Box, RotatedBounds>((Func<TransportManager.Box, RotatedBounds>)(box => box.ToRotatedBounds)).ToList<RotatedBounds>(),
				InterpolationLooseness = 1.5f,
				sendUpdateRadius = 500
			});
		}

		public void OnPlayerClicked(Players.Player player, PlayerClickedData click)
		{
			Log.Write("CLICK" + click.HitType.ToString());
			if (click.IsHoldingButton || (click.ClickType != PlayerClickedData.EClickType.Right || click.OnBuildCooldown) || (click.HitType != PlayerClickedData.EHitType.Block || (int)click.TypeSelected != (int)ItemTypes.GetType("ghost").ItemIndex))
				return;
			Log.WriteWarning("CLICKED!");
			click.ConsumedType = PlayerClickedData.EConsumedType.UsedAsTool;
			Dozer.CreateGlider(click.GetExactHitPositionWorld() + new Vector3(0.0f, 0.75f, 0.0f), Quaternion.Euler(0,-90,0), Dozer.CreateVehicleDescription(MeshedObjectID.GetNew()), player);
		}

		public static DozerTransport CreateDozerPlacer(Players.Player player, Vector3 PlaceLocation, Quaternion rotation, SelectedArea area, ConstructionPlacer placer)
		{
			DozerTransport dozerPlacer = Dozer.CreateGlider(PlaceLocation, rotation, Dozer.CreateVehicleDescription(MeshedObjectID.GetNew()), player);
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
			DozerMover mover = new DozerMover(spawnPosition, rotation, playerInside);
			Log.Write("The Mover is created!" + mover.ToString());
			DozerTransport vehicle1 = new DozerTransport(mover, vehicle, new InventoryItem(ItemTypes.GetType("ghost").ItemIndex, 1));
			mover.SetParent(vehicle1);
			TransportManager.RegisterTransport((TransportManager.ITransportVehicle)vehicle1);
			Log.Write("Created Dozer!" + vehicle1.ToString());
			MeshedObjectManager.Attach(playerInside, vehicle);
			return vehicle1;
		}
		public static MeshedVehicleDescription CreateVehicleDescription(MeshedObjectID ID)
		{
			return new MeshedVehicleDescription(new ClientMeshedObject(Dozer.DozerType, ID), new Vector3(0.0f, 1.25f, 0.0f), false);
		}

	}
}
