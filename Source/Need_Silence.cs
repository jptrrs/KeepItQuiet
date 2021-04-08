using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace KeepItQuiet
{
	using static KeepQuietSettings;
	public class Need_Silence : Need
	{
        private const float
            noiseLevelFactor = 0.1f,
            silenceGainBaseOffset = 0.8f, // The speed of a 8-level noise (stonecutting), but in the other direction.
            sensitiveFactor = 2f,
            tolerantFactor = 0.5f;

        private float lastEffectiveDelta;
        
        private MapComp_Noise MapComp => pawn.Map?.GetComponent<MapComp_Noise>();

        public Need_Silence(Pawn pawn) : base(pawn)
        {
            threshPercents = new List<float>();
            threshPercents.Add(0.85f);
            threshPercents.Add(0.3f);
            threshPercents.Add(0.15f);
        }

        public QuietCategory CurCategory
        {
            get
            {
                if (CurLevel < 0.15f)
                {
                    return QuietCategory.DeeplyDisturbed;
                }
                if (CurLevel < 0.3f)
                {
                    return QuietCategory.Disturbed;
                }
                if (CurLevel < 0.85)
                {
                    return QuietCategory.Neutral;
                }
                return QuietCategory.Serene;
            }
        }

        public override int GUIChangeArrow
        {
            get
            {
                if (IsFrozen)
                {
                    return 0;
                }
                return Math.Sign(lastEffectiveDelta);
            }
        }
        public override bool ShowOnNeedList
		{
			get
			{
				return !Disabled;
			}
		}

		private bool Disabled
		{
			get
			{
				return false;//pawn.story.traits.HasTrait(TraitDefOf.Undergrounder);
			}
		}

		private float noiseEffectMultiplier
        {
            get
            {
				float basic = silenceDecaySpeed;
				if (pawn.story.traits.HasTrait(QuietDefOf.NoiseSensitive)) return basic * sensitiveFactor;
				if (pawn.story.traits.HasTrait(QuietDefOf.NoiseTolerant)) return basic * tolerantFactor;
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
            if (MapComp == null || IsFrozen || !pawn.Spawned) return;

            //read noise level from map
			int noiseLevel = MapComp.noiseGrid[pawn.Map.cellIndices.CellToIndex(pawn.Position)];

            //offset noise from current job, if necessary
            if (noiseLevel > 0 && !selfAnnoy && MapComp.currentJobNoise.ContainsKey(pawn))
            {
                noiseLevel -= MapComp.currentJobNoise[pawn];
                //abort if floored
                if (noiseLevel == 0)
                {
                    lastEffectiveDelta = 0;
                    return;
                }
            }

            //estabilish adequate response
            float num = noiseLevel * noiseLevelFactor;

            //if noise is to increase, apply custom setting
            if (noiseLevel > 0) num *= noiseEffectMultiplier;

            //if silent, setup basic gain and apply custom setting.
            else
            {
                num -= silenceGainBaseOffset;
                num *= silenceGainSpeed;
            }

            //reduce for this frame
			num /= 200;

            //apply to bar, expecting to subtract noise and add gains.
			float curLevel = CurLevel;
			CurLevel -= num;
			lastEffectiveDelta = CurLevel - curLevel;
		}

        private bool CheckAroundForSilence()
        {
            foreach (IntVec3 cell in GenAdjFast.AdjacentCellsCardinal(pawn.Position))
            {
                if (MapComp.noiseGrid[pawn.Map.cellIndices.CellToIndex(cell)] > 0)
                {
                    return false;
                }
            }
            return true;
        }
	}
}
