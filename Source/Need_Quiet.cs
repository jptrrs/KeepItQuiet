using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace KeepItQuiet
{
	public class Need_Quiet : Need
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

        //public OutdoorsCategory CurCategory
        //{
        //	get
        //	{
        //		if (CurLevel > 0.8f)
        //		{
        //			return OutdoorsCategory.Free;
        //		}
        //		if (CurLevel > 0.6f)
        //		{
        //			return OutdoorsCategory.NeedFreshAir;
        //		}
        //		if (CurLevel > 0.4f)
        //		{
        //			return OutdoorsCategory.CabinFeverLight;
        //		}
        //		if (CurLevel > 0.2f)
        //		{
        //			return OutdoorsCategory.CabinFeverSevere;
        //		}
        //		if (CurLevel > 0.05f)
        //		{
        //			return OutdoorsCategory.Trapped;
        //		}
        //		return OutdoorsCategory.Entombed;
        //	}
        //}

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

		public Need_Quiet(Pawn pawn) : base(pawn)
		{
			threshPercents = new List<float>();
			threshPercents.Add(0.8f);
			threshPercents.Add(0.6f);
			threshPercents.Add(0.4f);
			threshPercents.Add(0.2f);
			threshPercents.Add(0.05f);
		}

		public override void SetInitialLevel()
		{
			CurLevel = /*1f*/0.5f;
		}

		public override void NeedInterval()
		{
			if (Disabled)
			{
				CurLevel = 1f;
				return;
			}
			if (IsFrozen)
			{
				return;
			}
			int noiseLevel = pawn.Map.GetComponent<MapComp_Noise>().noiseGrid[pawn.Map.cellIndices.CellToIndex(pawn.Position)];
			float num = noiseLevel * noiseLevelFactor;

			//float b = 0.2f;
			//bool flag = !pawn.Spawned || pawn.Position.UsesOutdoorTemperature(pawn.Map);
			//RoofDef roofDef = pawn.Spawned ? pawn.Position.GetRoof(pawn.Map) : null;
			//float num;
			//if (!flag)
			//{
			//	if (roofDef == null)
			//	{
			//		num = 5f;
			//	}
			//	else if (!roofDef.isThickRoof)
			//	{
			//		num = -0.32f;
			//	}
			//	else
			//	{
			//		num = -0.45f;
			//		b = 0f;
			//	}
			//}
			//else if (roofDef == null)
			//{
			//	num = 8f;
			//}
			//else if (roofDef.isThickRoof)
			//{
			//	num = -0.4f;
			//}
			//else
			//{
			//	num = 1f;
			//}
			//if (pawn.InBed() && num < 0f)
			//{
			//	num *= 0.2f;
			//}
			num /= 400;
			float curLevel = CurLevel;
			CurLevel -= num;
			//if (num < 0f)
			//{
			//	CurLevel = Mathf.Min(CurLevel, Mathf.Max(CurLevel + num, b));
			//}
			//else
			//{
			//	CurLevel = Mathf.Min(CurLevel + num, 1f);
			//}
			lastEffectiveDelta = CurLevel - curLevel;

            if (lastEffectiveDelta > 0)
            {
				Log.Warning($"positive delta for {pawn} (noiseLevel{noiseLevel} => {curLevel} + {num} = {CurLevel})");
			}

		}

		// Token: 0x04002A09 RID: 10761
		private const float Delta_IndoorsThickRoof = -0.45f;

		// Token: 0x04002A0A RID: 10762
		private const float Delta_OutdoorsThickRoof = -0.4f;

		// Token: 0x04002A0B RID: 10763
		private const float Delta_IndoorsThinRoof = -0.32f;

		// Token: 0x04002A0C RID: 10764
		private const float Minimum_IndoorsThinRoof = 0.2f;

		// Token: 0x04002A0D RID: 10765
		private const float Delta_OutdoorsThinRoof = 1f;

		// Token: 0x04002A0E RID: 10766
		private const float Delta_IndoorsNoRoof = 5f;

		// Token: 0x04002A0F RID: 10767
		private const float Delta_OutdoorsNoRoof = 8f;

		// Token: 0x04002A10 RID: 10768
		private const float DeltaFactor_InBed = 0.2f;

		private float lastEffectiveDelta;
		private const float noiseLevelFactor = 0.1f;

	}
}
