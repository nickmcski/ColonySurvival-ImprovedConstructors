using ModLoaderInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Improved_Construction.CopyTool
{
	public class CopyManager : IAfterSelectedWorld
	{
		public void AfterSelectedWorld()
		{
			AreaJobTracker.RegisterAreaJobDefinition(new CopyJobDefinition());
		}


	}
}
