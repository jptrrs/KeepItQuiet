using RimWorld;
using Verse;
using Verse.AI;

namespace KeepItQuiet;

public class JobGiver_SeekQuietPlace : ThinkNode_JobGiver
{
    public override float GetPriority(Pawn pawn)
    {
        var zero = 0f;
        var quietNeed = pawn.needs.TryGetNeed<Need_Silence>();
        if (quietNeed == null)
        {
            return zero;
        }

        if (FoodUtility.ShouldBeFedBySomeone(pawn))
        {
            return zero;
        }

        var num = 0.3f;
        if (pawn.mindState.IsIdle)
        {
            num = 0.5f;
        }

        var restNeed = pawn.needs.TryGetNeed<Need_Rest>();
        if (restNeed is { CurLevel: > 0.95f })
        {
            num = 0.6f;
        }

        return quietNeed.CurLevel < num
            ? 8f
            : // Bad Hygiene's use toilet is 9.6f;
            zero;
    }

    protected override Job TryGiveJob(Pawn pawn)
    {
        if (GetPriority(pawn) == 0f)
        {
            return null;
        }

        var quietNeed = pawn.needs.TryGetNeed<Need_Silence>();
        var region = ClosestRegionWithinTemperatureRange(pawn.Position, pawn.Map, TraverseParms.For(pawn));
        if (quietNeed is { CurLevel: 0f } && region != null)
        {
            return JobMaker.MakeJob(QuietDefOf.GotoQuietPlace, region.RandomCell);
        }

        return null;
    }

    private static Region ClosestRegionWithinTemperatureRange(IntVec3 root, Map map, TraverseParms traverseParms,
        RegionType traversableRegionTypes = RegionType.Set_Passable)
    {
        var region = root.GetRegion(map, traversableRegionTypes);
        var mapComp = map.GetComponent<MapComp_Noise>();
        if (region == null || mapComp == null)
        {
            return null;
        }

        bool EntryCondition(Region from, Region r)
        {
            return r.Allows(traverseParms, false);
        }

        Region foundReg = null;

        bool RegionProcessor(Region r)
        {
            if (r.IsDoorway)
            {
                return false;
            }

            if (!NoiseUtility.IsSilentEnough(region))
            {
                return false;
            }

            foundReg = r;
            return true;
        }

        RegionTraverser.BreadthFirstTraverse(region, EntryCondition, RegionProcessor, 9999, traversableRegionTypes);
        return foundReg;
    }
}