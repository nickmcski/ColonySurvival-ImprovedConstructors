using MeshedObjects;
using Newtonsoft.Json.Linq;
using Pipliz;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transport;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEngine.PlayerLoop.PreLateUpdate;

namespace Improved_Construction.motion
{
	public class DozerTransport : TransportManager.ITransportVehicle
	{
		public static DozerTransport CreateDozer(Vector3 spawnPosition, Quaternion startRotation, MeshedVehicleDescription vehicle, Players.Player player, DozerSettings settings)
		{
			DozerTransport transport = new DozerTransport(spawnPosition, startRotation, vehicle, player, settings);
			TransportManager.RegisterTransport(transport);
			return transport;
		}

		public DozerTransport(Vector3 startPosition, Quaternion startRotation, MeshedVehicleDescription description,  Players.Player player, DozerSettings settings)
		{
			IsValid = true;
			Position = new Pipliz.Vector3Int(startPosition);
			nextPosition = Position;
			VehicleDescription = description;
			Settings = settings;
			NextUpdate = ServerTimeStamp.Now;
			Player = player;
			Rotation = startRotation;
		}

		public static MeshedVehicleDescription CreateVehicleDescription(MeshedObjectID ID)
		{
			return new MeshedVehicleDescription(new ClientMeshedObject(Dozer.DozerType, ID), new Vector3(0.0f, 1.25f, 0.0f), false);
		}

		JObject TransportManager.ITransportVehicle.Save()
		{
			//No need to save/load
			return null;
		}

		//		public override TransportManager.ETransportUpdateResult Update()
		//		{
		//					this.VehicleDescription.Object.SendMoveToInterpolatedRenderDistance(this.Position, this.Rotation, (float)150 * (1f / 1000f), (MeshedObjectTypeSettings)null);
		//		}
		public void ForceSendUpdate()
		{
			VehicleDescription.Object.SendMoveToInterpolatedRenderDistance(Position.Vector, Rotation, Settings.MeshedSettings, UpdateDelayMS);
			LastSendRealTime = Pipliz.Time.SecondsSinceStartDoubleThisFrame;
			NextUpdate = NextUpdate.Add(UpdateDelayMS);
		}

		bool TransportManager.ITransportVehicle.Update()
		{
			if (!IsValid)
			{
				return false;
			}
			if (Player.ConnectionState != Players.EConnectionState.Connected)
			{
				return true;
			}
			//if(Position == nextPosition)
			//{
			//	return true;
			//}
			if (!MeshedObjectManager.TryGetVehicle(Player, out var playerVehicle) || playerVehicle.Object.ObjectID.ID != VehicleDescription.Object.ObjectID.ID)
			{
				OnRemove();
				return false;
			}
			//VehicleDescription.Object.SendMoveToInterpolatedRenderDistance(Position.Vector, Rotation, Settings.MeshedSettings, UpdateDelayMS);
			//return true;
			int maxPerUpdate = 10 * 1;
			while (NextUpdate.IsPassed)
			{
				if (maxPerUpdate-- == 0)
				{
					NextUpdate = ServerTimeStamp.Now.Add(UPDATE_DELAY);
					break;
				}
				Position = nextPosition;

				LastSendRealTime = Pipliz.Time.SecondsSinceStartDoubleThisFrame;
				NextUpdate = NextUpdate.Add(UpdateDelayMS * 4);
				continue;

			}
			if (Pipliz.Time.SecondsSinceStartDoubleThisFrame - (double)Settings.BackupSendingTimeoutSeconds > LastSendRealTime)
			{
				VehicleDescription.Object.SendMoveToInterpolatedRenderDistance(Position.Vector, Rotation, Settings.MeshedSettings, UpdateDelayMS);
				LastSendRealTime = Pipliz.Time.SecondsSinceStartDoubleThisFrame;
			}
			return true;
		}

		public int samePosition = UPDATE_DELAY;
		public static int UPDATE_DELAY = 5;

		void TransportManager.ITransportVehicle.ProcessInputs(Players.Player player, Pipliz.Collections.SortedList<EInputKey, float> keyTimes, float deltaTime)
		{
			if (keyTimes.Count == 0)
			{
				if (samePosition < UPDATE_DELAY)
				{
					samePosition++;
					return;
				}
				if (samePosition == UPDATE_DELAY)
				{
					samePosition++;
					ConstructionPlacer.Show(player); //TODO null check
					return;
				}

				return;
			}

			string input = "";

			Pipliz.Vector3Int offset = Pipliz.Vector3Int.zero;

			foreach (KeyValuePair<EInputKey, float> Key in keyTimes)
			{
				input += Key.Key.ToString() + "_" + Key.Value + ";";

				switch (Key.Key)
				{
					case EInputKey.MoveForward:
						offset.x += 1;
						break;
					case EInputKey.MoveBack:
						offset.x -= 1;
						break;
					case EInputKey.MoveLeft:
						offset.z -= 1;
						break;
					case EInputKey.MoveRight:
						offset.z += 1;
						break;

					case EInputKey.FlyUp:
					case EInputKey.Jump:
						offset.y += 1;
						break;

					case EInputKey.FlyDown:
					case EInputKey.Crouch:
						offset.y -= 1;
						break;
				}
				UnityEngine.Quaternion rotation = player.Rotation;

				offset = rotateInput(offset, rotation);
			}



			if (samePosition >= UPDATE_DELAY) //We have already drawn the shape and need to clear it
			{
				ConstructionPlacer.ClearChunk(player);
			}

			move(offset);
			samePosition = -1;
			//SetPlayerInputSpeed(Settings.SpeedDefault, deltaTime); //TODO - Not sure what this is...
		}


		private void move(Pipliz.Vector3Int offset)
		{
			//((DozerMover)Mover).CoreTransform.position += offset.Vector;

			nextPosition = Position + offset;

			if (selection != null)
			{
				selection.Move(offset);
				AreaJobTracker.SendData(Player);
			}
		}

		private Pipliz.Vector3Int rotateInput(Pipliz.Vector3Int input, UnityEngine.Quaternion rotation)
		{


			float angle = (rotation.eulerAngles.y + 45) % 365;

			if (angle < 0)
				angle = 360 + angle; //Reset back to 360 base.


			if (angle < 90)
			{
				//(90 Clock - Facing positive Z
				return new Pipliz.Vector3Int(input.z, input.y, input.x);
			}
			if (angle < 180)
				return new Pipliz.Vector3Int(input.x, input.y, -input.z);

			if (angle < 270)
				return new Pipliz.Vector3Int(-input.z, input.y, -input.x);

			if (angle < 360)
				return new Pipliz.Vector3Int(-input.x, input.y, input.z);

			return input;
		}

		SelectedArea selection = null;
		public void setArea(SelectedArea area)
		{
			selection = area;
		}
		ConstructionPlacer placer = null;
		private MeshedVehicleDescription VehicleDescription;
		private DozerSettings Settings;
		private double LastSendRealTime;
		private ServerTimeStamp NextUpdate;
		private Players.Player Player;
		private bool IsValid;
		private Pipliz.Vector3Int Position;
		private Pipliz.Vector3Int nextPosition;
		private int UpdateDelayMS = 200;
		private Quaternion Rotation;

		public void setPlacer(ConstructionPlacer placer)
		{
			this.placer = placer;
		}

		void TransportManager.ITransportVehicle.OnClicked(Players.Player sender, PlayerClickedData click)
		{
			MeshedVehicleDescription description;
			bool attached = MeshedObjectManager.TryGetVehicle(sender, out description);

			if (click.ClickType == PlayerClickedData.EClickType.Right)
			{
				if (!attached)
				{
					MeshedObjectManager.Attach(sender, this.VehicleDescription);
					ConstructionPlacer.ClearChunk(sender);
				}
				else
				{
					MeshedObjectManager.Detach(sender);
					ConstructionPlacer.Show(sender);

					//OnRemove(); //Remove Dozer when exiting???
				}
			}

		}

		private void OnRemove()
		{
			IsValid = false;
			if (MeshedObjectManager.TryGetVehicle(Player, out var playerVehicle) && playerVehicle.Object.ObjectID.ID == VehicleDescription.Object.ObjectID.ID)
			{
				MeshedObjectManager.Detach(Player);
			}
			VehicleDescription.Object.SendRemoval(Position.Vector);
		}

		bool TransportManager.ITransportVehicle.MatchesMeshID(int id)
		{
			return IsValid && id == VehicleDescription.Object.ObjectID.ID;
		}

		public void OnPlayerAttached(Players.Player player)
		{
			//throw new NotImplementedException();
		}

		public void OnPlayerDetached(Players.Player player)
		{
			//throw new NotImplementedException();
		}
	}
}
