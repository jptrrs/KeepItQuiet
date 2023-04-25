using System;
using RimWorld;
using Verse;

namespace KeepItQuiet;

internal class ThoughtWorker_Quiet : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (p.needs.TryGetNeed<Need_Silence>() == null)
        {
            return ThoughtState.Inactive;
        }

        switch (p.needs.TryGetNeed<Need_Silence>().CurCategory)
        {
            case QuietCategory.DeeplyDisturbed:
                return ThoughtState.ActiveAtStage(0);
            case QuietCategory.Disturbed:
                return ThoughtState.ActiveAtStage(1);
            case QuietCategory.Neutral:
                return ThoughtState.Inactive;
            case QuietCategory.Serene:
                return ThoughtState.ActiveAtStage(2);
            default:
                throw new NotImplementedException();
        }
    }
}