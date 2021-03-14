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

namespace Improved_Construction.motion
{
	public class DozerTransport : TransportManager.GenericTransport
	{
		public DozerTransport(
						DozerMover mover,
						MeshedVehicleDescription description,
						InventoryItem refundItems)
						: base((TransportManager.ITransportMovement)mover, description, refundItems)
		{ }

		public override JObject Save()
		{
			//No need to save/load
			return null;
		}

		public override TransportManager.ETransportUpdateResult Update()
		{
			if (this.Mover == null)
				return TransportManager.ETransportUpdateResult.Remove;
			TransportManager.ETransportUpdateResult etransportUpdateResult = this.Mover.UpdateTransport();
			switch (etransportUpdateResult)
			{
				case TransportManager.ETransportUpdateResult.KeepUpdating:
					this.VehicleDescription.Object.SendMoveToInterpolatedRenderDistance(this.Mover.Position, this.Mover.Rotation, (float)150 * (1f / 1000f), (MeshedObjectTypeSettings)null);
					return etransportUpdateResult;
				case TransportManager.ETransportUpdateResult.KeepUpdatingNoSend:
					return etransportUpdateResult;
				default:
					return TransportManager.ETransportUpdateResult.Remove;
			}
		}

		public int samePosition = UPDATE_DELAY;
		public static int UPDATE_DELAY = 5;

		public override void ProcessInputs(Players.Player player, Pipliz.Collections.SortedList<EInputKey, float> keyTimes, float deltaTime)
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
		}

		private void move(Pipliz.Vector3Int offset)
		{
			((DozerMover)Mover).CoreTransform.position += offset.Vector;
			if (selection != null)
			{
				selection.Move(offset);
				AreaJobTracker.SendData(((DozerMover)Mover).LastInputPlayer);
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
		public void setPlacer(ConstructionPlacer placer)
		{
			this.placer = placer;
		}

		public override void OnClicked(Players.Player sender, PlayerClickedData click)
		{
			if (this.Mover == null)
				return;
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
				}
			}

		}
	}
}
