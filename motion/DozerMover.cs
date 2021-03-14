using Shared;
using System;
using System.Collections.Generic;
using Transport;
using UnityEngine;
using Pipliz.Collections;
using Pipliz;

namespace Improved_Construction.motion
{
	public class DozerMover : TransportManager.ITransportMovement
	{
		private static System.Type[] RigidBodyList = new System.Type[1]
{
								typeof (Rigidbody)
};

		public Transform CoreTransform;
		private static readonly IComparer<EInputKey> KeyComparer = (IComparer<EInputKey>)new Pipliz.Collections.SortedList<EInputKey, float>.Comparer((Func<EInputKey, EInputKey, int>)((a, b) => ((uint)a).CompareTo((uint)b)));
		private Pipliz.Collections.SortedList<EInputKey, float> LastInputs = new Pipliz.Collections.SortedList<EInputKey, float>(5, DozerMover.KeyComparer);
		public Players.Player LastInputPlayer { get; private set; }
		private float LastInputsDeltaTime;
		//private double LastSend;
		private DozerTransport ParentTransport;

		public DozerMover(Vector3 startPosition,
						Quaternion startRotation,
						Players.Player playerInside)
		{
			this.LastInputPlayer = playerInside;
			if ((UnityEngine.Object)TransportManager.TransportRootTransform == (UnityEngine.Object)null)
				TransportManager.TransportRootTransform = new GameObject("transport_root").transform;
			this.CoreTransform = SpawnRootBox();
			this.CoreTransform.position = startPosition;
			this.CoreTransform.rotation = startRotation;
		}

		private static Transform SpawnRootBox()
		{
			Transform transform = new GameObject("Test123").transform;
			transform.SetParent(TransportManager.TransportRootTransform);
			return transform;
		}

		public Vector3 Position
		{
			get
			{
				return this.CoreTransform.position;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return this.CoreTransform.rotation;
			}
		}

		public int GetDelayMillisecondsToNextUpdate()
		{
			return 200;
		}

		public void OnRemove()
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)this.CoreTransform.gameObject);
			this.CoreTransform = (Transform)null;
			this.LastInputPlayer = (Players.Player)null;
			this.LastInputs = (Pipliz.Collections.SortedList<EInputKey, float>)null;
		}

		public void ProcessInputs(
				Players.Player player,
				Pipliz.Collections.SortedList<EInputKey, float> keyTimes,
				float deltaTime)
		{
			Log.Write("GOT INPUTS!!!!!!");
			this.LastInputs.Clear();
			keyTimes.CopyTo(this.LastInputs);
			this.LastInputsDeltaTime = deltaTime;
			this.LastInputPlayer = player;
			this.ParentTransport = (DozerTransport)null;
			//TODO Hook the inputs!
		}

		public void SetParent(DozerTransport vehicle)
		{
			this.ParentTransport = vehicle;
		}

		public TransportManager.ETransportUpdateResult UpdateTransport()
		{
			if ((UnityEngine.Object)this.CoreTransform == (UnityEngine.Object)null)
			{
				Log.WriteWarning("ERROR! Dozer Mover is NULL!");
				return TransportManager.ETransportUpdateResult.Remove;
			}
			else
			{
				return TransportManager.ETransportUpdateResult.KeepUpdating;
			}
		}
	}
}
