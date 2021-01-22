using BlockTypes;
using Jobs;
using Jobs.Implementations.Construction;
using NPC;
using Pipliz;
using Shared;
using System.Collections.Generic;
using static Improved_Construction.ReplacerSpecialLoader;

namespace Improved_Construction
{
    public class ReplacerSpecial : IConstructionType
    {
        private static List<ItemTypes.ItemTypeDrops> GatherResults = new List<ItemTypes.ItemTypeDrops>();
        protected ItemTypes.ItemType digType;
        protected ItemTypes.ItemType buildType;

        public int MaxGatheredPerRun { get; set; } = 5;

        public int OnStockpileNewItemCount
        {
            get
            {
                return 5;
            }
        }

        public ReplacerSpecial(ItemTypes.ItemType digType, ItemTypes.ItemType placeType)
        {
            this.digType = digType;
            this.buildType = placeType;
        }
        public ReplacerSpecial(ReplaceType type)
        {
            this.digType = type.dig;
            this.buildType = type.place;
        }


        public void DoJob(
          IIterationType iterationType,
          IAreaJob areaJob,
          ConstructionJobInstance job,
          ref NPCBase.NPCState state)
        {
            if (iterationType == null)
            {
                AreaJobTracker.RemoveJob(areaJob);
            }
            else
            {
                Stockpile stockpile = areaJob.Owner.Stockpile;
                int num = 4096;
                while (num-- > 0)
                {
                    Vector3Int currentPosition = iterationType.CurrentPosition;
                    if (!currentPosition.IsValid)
                    {
                        state.SetIndicator(new IndicatorState(5f, BuiltinBlocks.Indices.erroridle, false, true), true);
                        AreaJobTracker.RemoveJob(areaJob);
                        return;
                    }
                    ushort val;
                    if (World.TryGetTypeAt(currentPosition, out val))
                    {
                        iterationType.MoveNext();
                        if (val != (ushort)0)
                        {
                            ItemTypes.ItemType type = ItemTypes.GetType(val);
                            if (type.IsDestructible)
                            {
                                if (type != this.digType)
                                {
                                    bool flag = false;
                                    for (ItemTypes.ItemType parentItemType = type.ParentItemType; parentItemType != (ItemTypes.ItemType)null; parentItemType = parentItemType.ParentItemType)
                                    {
                                        if (parentItemType == this.digType)
                                        {
                                            flag = true;
                                            break;
                                        }
                                    }
                                    if (!flag)
                                        continue;
                                }
                                if (!stockpile.Contains(this.buildType.ItemIndex, 1))
                                {
                                    float num2 = Random.NextFloat(5f, 8f);
                                    job.Owner.Stats.RecordNPCIdleSeconds(job.NPCType, num2);
                                    state.SetIndicator(new IndicatorState(num2, this.buildType.ItemIndex, true, false), true);
                                    return;
                                }

                                if (ServerManager.TryChangeBlock(currentPosition, this.buildType, (BlockChangeRequestOrigin)areaJob.Owner, ESetBlockFlags.DefaultAudio) == EServerChangeBlockResult.Success)
                                {
                                    stockpile.TryRemove(this.buildType.ItemIndex, 1, true);
                                    float cooldown = ReplacerSpecial.GetCooldown((float)type.DestructionTime * (1f / 1000f));
                                    ReplacerSpecial.GatherResults.Clear();
                                    List<ItemTypes.ItemTypeDrops> onRemoveItems = type.OnRemoveItems;
                                    for (int index = 0; index < onRemoveItems.Count; ++index)
                                        ReplacerSpecial.GatherResults.Add(onRemoveItems[index]);
                                    ModLoader.Callbacks.OnNPCGathered.Invoke((IJob)job, currentPosition, ReplacerSpecial.GatherResults);
                                    InventoryItem weightedRandom = ItemTypes.ItemTypeDrops.GetWeightedRandom(ReplacerSpecial.GatherResults);
                                    if (weightedRandom.Amount > 0)
                                        state.SetIndicator(new IndicatorState(cooldown, weightedRandom.Type, false, true), true);
                                    else
                                        state.SetCooldown((double)cooldown);
                                    state.Inventory.Add((IList<ItemTypes.ItemTypeDrops>)ReplacerSpecial.GatherResults);
                                    ++job.StoredItemCount;
                                    if (job.StoredItemCount < this.MaxGatheredPerRun)
                                        return;
                                    job.ShouldTakeItems = true;
                                    state.JobIsDone = true;
                                    return;
                                }
                                state.SetIndicator(new IndicatorState(5f, BuiltinBlocks.Indices.missingerror, true, false), true);
                                return;
                            }
                        }
                    }
                    else
                    {
                        state.SetIndicator(new IndicatorState(5f, BuiltinBlocks.Indices.missingerror, true, false), true);
                        return;
                    }
                }
                state.SetCooldown(0.8, 1.2);
            }
        }

        public static float GetCooldown(float blockDestructionTime)
        {
            return Math.Clamp(Random.NextFloat(0.8f, 1.2f) * blockDestructionTime * ServerManager.ServerSettings.NPCs.DiggerCooldownMultiplierSeconds, 0.05f, 15f);
        }
    }
}
