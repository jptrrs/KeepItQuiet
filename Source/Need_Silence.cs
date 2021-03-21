using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace KeepItQuiet
{
	public class Need_Silence : Need
	{
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

		private float EffectMultiplier
        {
            get
            {
				float basic = KeepQuietSettings.noiseDecaySpeed;
				if (pawn.story.traits.HasTrait(QuietDefOf.NoiseSensitive)) return basic * sensitiveFactor;
				if (pawn.story.traits.HasTrait(QuietDefOf.NoiseTolerant)) return basic * tolerantFactor;
				return basic;
            }
        }

		public Need_Silence(Pawn pawn) : base(pawn)
		{
			threshPercents = new List<float>();
			threshPercents.Add(0.85f);
			threshPercents.Add(0.3f);
			threshPercents.Add(0.15f);
		}


		public override void NeedInterval()
		{
			if (Disabled)
			{
				CurLevel = 1f;
				return;
			}
			if (IsFrozen || !pawn.Spawned)
			{
				return;
			}
			int noiseLevel = pawn.Map.GetComponent<MapComp_Noise>()?.noiseGrid[pawn.Map.cellIndices.CellToIndex(pawn.Position)] ?? 0;
			float num;
			if (noiseLevel > 0) num = noiseLevel * noiseLevelFactor * -1;
			else num = silenceGainFactor;
			num *= EffectMultiplier;
			num /= 200/*400*/;
			float curLevel = CurLevel;
			CurLevel += num;
			lastEffectiveDelta = CurLevel - curLevel;
			//Log.Warning($"Delta for {pawn}: {lastEffectiveDelta} (noiseLevel{noiseLevel} => {curLevel} + {num} = {CurLevel})");
		}

		private float lastEffectiveDelta;
		private const float
			noiseLevelFactor = 0.1f,
			silenceGainFactor = 0.2f, // 25% the speed of a 8-level noise (stonecutting), but in the other direction.
			sensitiveFactor = 1.5f,
			tolerantFactor = 0.5f;
	}
}
