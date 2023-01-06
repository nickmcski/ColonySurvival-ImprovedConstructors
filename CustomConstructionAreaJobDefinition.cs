using Jobs;
using Jobs.Implementations.Construction;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Improved_Construction
{
	public class CustomConstructionAreaJobDefinition : ConstructionAreaDefinition
	{
		public CustomConstructionAreaJobDefinition()
		{
			Identifier = "wingdings.customconstruction";
		}
	}
}
