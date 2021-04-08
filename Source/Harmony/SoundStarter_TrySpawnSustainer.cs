using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;
using Verse.AI;

namespace KeepItQuiet
{
    //Adds the noise for continuing sounds.
    [HarmonyPatch(typeof(SoundStarter), nameof(SoundStarter.TrySpawnSustainer))]
    class SoundStarter_TrySpawnSustainer
    {
        public static void Postfix(SoundDef soundDef, SoundInfo info, Sustainer __result)
        {
            TargetInfo maker = info.Maker;
            if (maker != null && soundDef.sustain)
            {
                int level = (int)NoiseUtility.GetSoundLevel(soundDef);
                Pawn actor = null;
                if (!KeepQuietSettings.selfAnnoy && maker.Thing != null && maker.Thing is Pawn pawn && pawn.RaceProps.Humanlike)
                {
                    actor = pawn;
                }
                maker.Map.GetComponent<MapComp_Noise>().AddPolluter(__result, maker.Cell, level, actor);
            }
        }

    }
}
