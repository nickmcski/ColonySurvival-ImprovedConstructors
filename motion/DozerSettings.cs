using MeshedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Improved_Construction.motion
{
	public class DozerSettings
	{
		public string ID;

		public MeshedObjectType MeshedObjectType;

		public MeshedObjectTypeSettings MeshedSettings;

		public Vector3 PlayerOffset;

		public bool AllowPlayerEditingBlocks;

		public ushort VehicleHeight;

		public float Acceleration;

		public float SpeedMin;

		public float SpeedMax;

		public float SpeedDefault;

		public float BackupSendingTimeoutSeconds = 1f;

		public int UpdateDelayMinimumMilliseconds = 166;
	}
}
