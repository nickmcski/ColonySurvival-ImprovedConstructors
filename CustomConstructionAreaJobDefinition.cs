using Jobs;
using Jobs.Implementations.Construction;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Improved_Construction
{
    public class CustomConstructionAreaJobDefinition : AbstractAreaJobDefinition, IAreaJobSubArguments
    {
        ConstructionArea areaJob;
        public CustomConstructionAreaJobDefinition()
        {
            Identifier = "wingdings.customconstruction";
        }
        public override IAreaJob CreateAreaJob(Colony owner, Vector3Int min, Vector3Int max, bool isLoaded, int npcID = 0)
        {
            areaJob = new CustomConstructionArea(this, owner, min, max);

            
            return (IAreaJob)areaJob;
        }

        public void SetArgument(JSONNode args)
        {
            areaJob.SetArgument(args);
        }
    }
}
