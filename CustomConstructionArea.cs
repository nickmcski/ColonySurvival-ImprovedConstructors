// Decompiled with JetBrains decompiler
// Type: Jobs.Implementations.Construction.ConstructionArea
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3FAB3D2F-86CC-423B-B640-408C96EAC726
// Assembly location: Q:\GAMES\Steam\steamapps\common\Colony Survival\colonyserver_Data\Managed\Assembly-CSharp.dll

using Jobs;
using Jobs.Implementations.Construction;
using Jobs.Implementations.Construction.Loaders;
using NPC;
using Pipliz;
using Pipliz.JSON;
using Shared;
using System.Collections.Generic;

namespace Improved_Construction
{
  public class CustomConstructionArea : IAreaJob, IAreaJobSubArguments
  {
    protected bool isValid = true;
    public static Dictionary<string, IConstructionLoader> constructionLoaders = new Dictionary<string, IConstructionLoader>();
    protected Vector3Int positionMin;
    protected Vector3Int positionMax;
    protected JSONNode arguments;
    protected CustomConstructionAreaJobDefinition definition;

    public IConstructionType ConstructionType { get; set; }

    public IIterationType IterationType { get; set; }

    public virtual Colony Owner { get; protected set; }

    public virtual Vector3Int Minimum
    {
      get
      {
        return this.positionMin;
      }
    }

    public virtual Vector3Int Maximum
    {
      get
      {
        return this.positionMax;
      }
    }

    public virtual NPCBase NPC
    {
      get
      {
        return (NPCBase) null;
      }
      set
      {
      }
    }

    public virtual EServerAreaType AreaType
    {
      get
      {
        return EServerAreaType.ConstructionArea;
      }
    }

    public virtual EAreaMeshType AreaTypeMesh
    {
      get
      {
        return EAreaMeshType.ThreeD;
      }
    }

    public virtual bool IsValid
    {
      get
      {
        return this.isValid && this.arguments != null && this.ConstructionType != null && this.IterationType != null;
      }
    }

    static CustomConstructionArea()
    {
      ConstructionArea.RegisterLoader((IConstructionLoader) new BuilderLoader());
      ConstructionArea.RegisterLoader((IConstructionLoader) new DiggerLoader());
      ConstructionArea.RegisterLoader((IConstructionLoader) new DiggerSpecialLoader());
    }

    public CustomConstructionArea(
      CustomConstructionAreaJobDefinition definition,
      Colony owner,
      Vector3Int min,
      Vector3Int max)
    {
      this.definition = definition;
      min.y = Math.Max(1, min.y);
      this.positionMin = min;
      this.positionMax = max;
      this.isValid = max != Vector3Int.invalidPos;
      this.Owner = owner;
    }

    public void SetArgument(JSONNode args)
    {
      if (args == null)
      {
        Log.WriteWarning("Unexpected construction area args; null");
      }
      else
      {
        this.arguments = args;
        string result;
        if (args.TryGetAs<string>("constructionType", out result))
        {
          IConstructionLoader constructionLoader;
                    if (ConstructionArea.constructionLoaders.TryGetValue(result, out constructionLoader))
                        //constructionLoader.ApplyTypes(this, args);
                        return;
                    else
                        Log.WriteWarning<string>("Unexpected construction type: {0}", result);
        }
        else
          Log.WriteWarning("Unexpected construction area args; no constructionType set");
      }
    }

    public static void RegisterLoader(IConstructionLoader type)
    {
        CustomConstructionArea.constructionLoaders[type.JobName] = type;
    }

    public virtual void OnRemove()
    {
      this.definition.OnRemove((IAreaJob) this);
      this.isValid = false;
    }

    public virtual void SaveAreaJob(JSONNode colonyRootNode)
    {
      if (this.arguments == null)
        return;
      JSONNode node1;
      if (!colonyRootNode.TryGetChild(this.definition.Identifier, out node1))
      {
        node1 = new JSONNode(NodeType.Array);
        colonyRootNode[this.definition.Identifier] = node1;
      }
      JSONNode node2 = new JSONNode(NodeType.Object).SetAs<int>("x-", this.positionMin.x).SetAs<int>("y-", this.positionMin.y).SetAs<int>("z-", this.positionMin.z).SetAs<int>("xd", this.positionMax.x - this.positionMin.x).SetAs<int>("yd", this.positionMax.y - this.positionMin.y).SetAs<int>("zd", this.positionMax.z - this.positionMin.z).SetAs<JSONNode>("args", this.arguments);
      string result;
      IConstructionLoader constructionLoader;
      if (this.arguments.TryGetAs<string>("constructionType", out result) && ConstructionArea.constructionLoaders.TryGetValue(result, out constructionLoader))
        constructionLoader.SaveTypes(this, node2);
      node1.AddToArray(node2);
    }

    public virtual void DoJob(ConstructionJobInstance job, ref NPCBase.NPCState state)
    {
      if (this.ConstructionType == null)
        return;
      this.ConstructionType.DoJob(this.IterationType, (IAreaJob) this, job, ref state);
    }
  }
}
