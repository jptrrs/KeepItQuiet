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
            //silenceGainFactor = 0.2f, // 25% the speed of a 8-level noise (stonecutting), but in the other direction.
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
			if (Disabled)
			{
				CurLevel = 1f;
				return;
			}
            if (MapComp == null || IsFrozen || !pawn.Spawned) return;
			int noiseLevel = MapComp.noiseGrid[pawn.Map.cellIndices.CellToIndex(pawn.Position)];
            if (noiseLevel < 1 && !CheckAroundForSilence()) return;
			float num;
			if (noiseLevel > 0) num = noiseLevel * noiseLevelFactor * noiseEffectMultiplier * -1;
			else num = silenceGainSpeed;
			num /= 200/*400*/;
			float curLevel = CurLevel;
			CurLevel += num;
			lastEffectiveDelta = CurLevel - curLevel;
			//Log.Warning($"Delta for {pawn}: {lastEffectiveDelta} (noiseLevel{noiseLevel} => {curLevel} + {num} = {CurLevel})");
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
