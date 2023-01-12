using Jobs;
using Jobs.Implementations.Construction;
using NPC;
using Pipliz;
using System.Collections.Generic;
using System.Linq;
using BlockTypes;
using ModLoaderInterfaces;
using static ItemTypes;
using System;
using Shared;

namespace ExtendedBuilder.Jobs
{
	[ModLoader.ModManager]
	public class StructureBuilder : IConstructionType, IOnShouldKeepChunkLoaded
	{
		private static List<StructureIterator> _needsChunkLoaded = new List<StructureIterator>();

		public void OnShouldKeepChunkLoaded(ChunkUpdating.KeepChunkLoadedData data)
		{
			foreach (var iterator in _needsChunkLoaded)
			{
				if (iterator != null &&
						iterator.CurrentPosition != Vector3Int.invalidPos
						// TODO add back to restore chunk loading  //&& iterator.CurrentPosition.IsWithinBounds(data.CheckedChunk.Position, data.CheckedChunk.Bounds)
						)
					data.Result = true;
			}
		}

		public int OnStockpileNewItemCount => 5;

		public void DoJob(IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ref NPCBase.NPCState state)
		{
			int i = 0;
			StructureIterator bpi = (StructureIterator)iterationType;

			if (bpi == null)
			{
				state.SetIndicator(IndicatorState.NewIdleIndicator(5f));
				AreaJobTracker.RemoveJob(areaJob);
				return;
			}

			if (bpi.BuilderSchematic == null)
			{
				Log.Write("<color=red>SchematicIterator.BuilderSchematic == NULL</color>");
				state.SetIndicator(IndicatorState.NewMissingItemIndicator(5f, BuiltinBlocks.Indices.erroridle));
				AreaJobTracker.RemoveJob(areaJob);
				return;
			}

			while (true) // This is to move past air.
			{
				if (i > 4000)
				{
					break;
				}

				var adjX = bpi.CurrentPosition.x - bpi.location.x;
				var adjY = bpi.CurrentPosition.y - bpi.location.y;
				var adjZ = bpi.CurrentPosition.z - bpi.location.z;
				ushort block;
				try
				{
					block = bpi.BuilderSchematic.GetBlock(adjX, adjY, adjZ);
				}
				catch (Exception ex)
				{
					Log.WriteException("Error at block " + adjX + "," + adjY + "," + adjZ, ex);
					iterationType.MoveNext();
					continue;
				}
				var buildType = ItemTypes.GetType(block);

				if (buildType == null)
				{
					state.SetIndicator(IndicatorState.NewItemIndicator(5f, BuiltinBlocks.Indices.erroridle));
					AreaJobTracker.RemoveJob(areaJob);
					return;
				}

				if (World.TryGetTypeAt(iterationType.CurrentPosition, out ushort foundTypeIndex))
				{
					i++;
					ItemType founditemId = ItemTypes.GetType(foundTypeIndex);

					if (foundTypeIndex == buildType.ItemIndex || buildType.Name.Contains("bedend") || (founditemId.Name.Contains("bedend") && buildType.ItemIndex == BuiltinBlocks.Indices.air)) // check if the blocks are the same, if they are, move past. Most of the time this will be air.
					{
						if (iterationType.MoveNext())
							continue;
						else
						{
							if (_needsChunkLoaded.Contains(bpi))
								_needsChunkLoaded.Remove(bpi);

							state.SetIndicator(IndicatorState.NewMissingItemIndicator(5f, BuiltinBlocks.Indices.erroridle));
							AreaJobTracker.RemoveJob(areaJob);
							return;
						}
					}

					Stockpile ownerStockPile = areaJob.Owner.ColonyGroup.Stockpile;

					bool stockpileContainsBuildItem = buildType.ItemIndex == BuiltinBlocks.Indices.air;

					if (!stockpileContainsBuildItem && ownerStockPile.Contains(buildType.ItemIndex))
						stockpileContainsBuildItem = true;

					if (!stockpileContainsBuildItem && buildType.Name.Contains("bed") && ownerStockPile.Contains(ItemTypes.IndexLookup.GetIndex("bed")))
						stockpileContainsBuildItem = true;

					if (!stockpileContainsBuildItem &&
							!string.IsNullOrWhiteSpace(buildType.ParentType) &&
							ownerStockPile.Contains(buildType.ParentItemType.ItemIndex))
					{
						stockpileContainsBuildItem = true;
					}

					//TODO Check if buildType .isPlaceable() 

					if (stockpileContainsBuildItem)
					{
						if (foundTypeIndex != BuiltinBlocks.Indices.air && foundTypeIndex != BuiltinBlocks.Types.water.ItemIndex)
						{
							var foundItem = ItemTypes.GetType(foundTypeIndex);

							if (foundItem != null && foundItem.ItemIndex != BlockTypes.BuiltinBlocks.Indices.air && foundItem.OnRemoveItems != null && foundItem.OnRemoveItems.Count > 0)
								ownerStockPile.Add(foundItem.OnRemoveItems.Select(itm => itm.item).ToList()); //Don't add until we know change result was successful
						}

						var changeResult = ServerManager.TryChangeBlock(iterationType.CurrentPosition, buildType.ItemIndex, new BlockChangeRequestOrigin(job.Owner), ESetBlockFlags.DefaultAudio);

						if (changeResult == EServerChangeBlockResult.Success)
						{
							if (buildType.ItemIndex != BuiltinBlocks.Indices.air)
							{
								if (--job.StoredItemCount <= 0)
								{
									job.ShouldTakeItems = true;
									state.JobIsDone = true;
								}

								ownerStockPile.TryRemove(buildType.ItemIndex);

								if (buildType.Name.Contains("bed"))
									ownerStockPile.TryRemove(ItemTypes.IndexLookup.GetIndex("bed"));
							}
						}
						else if (changeResult != EServerChangeBlockResult.CancelledByCallback)
						{
							if (!_needsChunkLoaded.Contains(bpi))
								_needsChunkLoaded.Add(bpi);

							state.SetIndicator(IndicatorState.NewItemIndicator(5f, buildType.ItemIndex));
							//ChunkQueue.QueuePlayerSurrounding(iterationType.CurrentPosition.ToChunk()); //TODO See if chunk needs to be queued manually
							return;
						}
					}
					else
					{
						state.SetIndicator(IndicatorState.NewItemIndicator(5f, buildType.ItemIndex));
						return;
					}
				}
				else
				{
					if (!_needsChunkLoaded.Contains(bpi))
						_needsChunkLoaded.Add(bpi);

					//ChunkQueue.QueuePlayerSurrounding(iterationType.CurrentPosition.ToChunk()); //TODO See if chunk needs to be queued manually
					state.SetIndicator(IndicatorState.NewIdleIndicator(5f));
					return;
				}

				if (iterationType.MoveNext())
				{
					if (buildType.ItemIndex != BlockTypes.BuiltinBlocks.Indices.air)
						state.SetIndicator(IndicatorState.NewItemIndicator(GetCooldown(), buildType.ItemIndex));
					else
						state.SetIndicator(IndicatorState.NewItemIndicator(GetCooldown(), foundTypeIndex));

					return;
				}
				else
				{
					if (_needsChunkLoaded.Contains(bpi))
						_needsChunkLoaded.Remove(bpi);

					// failed to find next position to do job at, self-destruct
					state.SetIndicator(IndicatorState.NewMissingItemIndicator(5f, BuiltinBlocks.Indices.erroridle));
					AreaJobTracker.RemoveJob(areaJob);
					return;
				}
			}

			if (iterationType.MoveNext())
			{
				state.SetIndicator(IndicatorState.NewMissingItemIndicator(5f, BuiltinBlocks.Indices.erroridle));
				return;
			}
			else
			{
				if (_needsChunkLoaded.Contains(bpi))
					_needsChunkLoaded.Remove(bpi);

				// failed to find next position to do job at, self-destruct
				state.SetIndicator(IndicatorState.NewMissingItemIndicator(5f, BuiltinBlocks.Indices.erroridle));
				AreaJobTracker.RemoveJob(areaJob);
				return;
			}
		}

		public static float GetCooldown()
		{
			return Pipliz.Random.NextFloat(1.5f, 2.5f);
		}
	}
}
