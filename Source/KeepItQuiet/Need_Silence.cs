using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace KeepItQuiet;

using static KeepQuietSettings;

public class Need_Silence : Need
{
    private const float
        noiseLevelFactor = 0.1f,
        silenceGainBaseOffset = 0.8f, // The speed of a 8-level noise (stonecutting), but in the other direction.
        sensitiveFactor = 2f,
        tolerantFactor = 0.5f;

    private float lastEffectiveDelta;

    public Need_Silence(Pawn pawn) : base(pawn)
    {
        threshPercents = new List<float>
        {
            0.85f,
            0.3f,
            0.15f
        };
    }

    private MapComp_Noise MapComp => pawn.Map?.GetComponent<MapComp_Noise>();

    public QuietCategory CurCategory
    {
        get
        {
            switch (CurLevel)
            {
                case < 0.15f:
                    return QuietCategory.DeeplyDisturbed;
                case < 0.3f:
                    return QuietCategory.Disturbed;
                case < 0.85f:
                    return QuietCategory.Neutral;
            }

            return QuietCategory.Serene;
        }
    }

    public override int GUIChangeArrow => IsFrozen ? 0 : Math.Sign(lastEffectiveDelta);

    public override bool ShowOnNeedList => !Disabled;

    private bool Disabled => false; //pawn.story.traits.HasTrait(TraitDefOf.Undergrounder);

    private float noiseEffectMultiplier
    {
        get
        {
            var basic = silenceDecaySpeed;
            if (pawn.story.traits.HasTrait(QuietDefOf.NoiseSensitive))
            {
                return basic * sensitiveFactor;
            }

            if (pawn.story.traits.HasTrait(QuietDefOf.NoiseTolerant))
            {
                return basic * tolerantFactor;
            }

            return basic;
        }
    }

    public override void NeedInterval()
    {
        //abort if unapplicable
        if (Disabled)
        {
            CurLevel = 1f;
            return;
        }

        if (MapComp == null || IsFrozen || !pawn.Spawned)
        {
            return;
        }

        //read noise level from map
        var noiseLevel = MapComp.noiseGrid[pawn.Map.cellIndices.CellToIndex(pawn.Position)];

        //offset noise from current job, if necessary
        if (noiseLevel > 0 && !selfAnnoy && MapComp.currentJobNoise.TryGetValue(pawn, out var value))
        {
            noiseLevel -= value;
            //abort if floored
            if (noiseLevel == 0)
            {
                lastEffectiveDelta = 0;
                return;
            }
        }

        //estabilish adequate response
        var num = noiseLevel * noiseLevelFactor;

        //if noise is to increase, apply custom setting
        if (noiseLevel > 0)
        {
            num *= noiseEffectMultiplier;
        }

        //if silent, setup basic gain and apply custom setting.
        else
        {
            num -= silenceGainBaseOffset;
            num *= silenceGainSpeed;
        }

        //reduce for this frame
        num /= 200;

        //apply to bar, expecting to subtract noise and add gains.
        var curLevel = CurLevel;
        CurLevel -= num;
        lastEffectiveDelta = CurLevel - curLevel;
    }

    private bool CheckAroundForSilence()
    {
        foreach (var cell in GenAdjFast.AdjacentCellsCardinal(pawn.Position))
        {
            if (MapComp.noiseGrid[pawn.Map.cellIndices.CellToIndex(cell)] > 0)
            {
                return false;
            }
        }

        return true;
    }
}