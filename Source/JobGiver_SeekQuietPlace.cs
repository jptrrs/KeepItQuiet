using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace KeepItQuiet
{
	public class JobGiver_SeekQuietPlace : ThinkNode_JobGiver
	{
		public override float GetPriority(Pawn pawn)
		{
			float zero = 0f;
			Need_Silence quietNeed = pawn.needs.TryGetNeed<Need_Silence>();
			if (quietNeed == null)
			{
				return zero;
			}
			if (FoodUtility.ShouldBeFedBySomeone(pawn))
			{
				return zero;
			}
			float num = 0.3f;
			if (pawn.mindState.IsIdle)
			{
				num = 0.5f;
			}
			Need_Rest restNeed = pawn.needs.TryGetNeed<Need_Rest>();
			if (restNeed != null && restNeed.CurLevel > 0.95f)
			{
				num = 0.6f;
			}
			if (quietNeed.CurLevel < num)
			{
				return 8f; // Bad Hygiene's use toilet is 9.6f;
			}
			return zero;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (GetPriority(pawn) == 0f) return null;
			Need_Silence quietNeed = pawn.needs.TryGetNeed<Need_Silence>();
			Region region = ClosestRegionWithinTemperatureRange(pawn.Position, pawn.Map, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), RegionType.Set_Passable);
			if (quietNeed != null && quietNeed.CurLevel == 0f && region != null)
			{
				return JobMaker.MakeJob(QuietDefOf.GotoQuietPlace, region.RandomCell);
			}
			return null;
		}

		private static Region ClosestRegionWithinTemperatureRange(IntVec3 root, Map map, TraverseParms traverseParms, RegionType traversableRegionTypes = RegionType.Set_Passable)
		{
			Region region = root.GetRegion(map, traversableRegionTypes);
			MapComp_Noise mapComp = map.GetComponent<MapComp_Noise>();
			if (region == null || mapComp == null) return null;
			RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParms, false);
			Region foundReg = null;
			RegionProcessor regionProcessor = delegate (Region r)
			{
				if (r.IsDoorway) return false;
				if (NoiseUtility.IsSilentEnough(region))
				{
					foundReg = r;
					return true;
				}
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 9999, traversableRegionTypes);
			return foundReg;
		}
	}
}
