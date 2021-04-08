using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;

namespace KeepItQuiet
{
    //Clean up job noise when it's finished
    [HarmonyPatch(typeof(Pawn_JobTracker), "CleanupCurrentJob")]
    class Pawn_JobTracker_CleanupCurrentJob
    {
        public static void Prefix(Pawn ___pawn)
        {
            if (KeepQuietSettings.selfAnnoy  || !___pawn.RaceProps.Humanlike) return;
            MapComp_Noise noiseMap = ___pawn.Map.GetComponent<MapComp_Noise>();
            if (noiseMap != null && noiseMap.currentJobNoise.ContainsKey(___pawn))
            {
                noiseMap.currentJobNoise.Remove(___pawn);
            }
        }
    }
}
