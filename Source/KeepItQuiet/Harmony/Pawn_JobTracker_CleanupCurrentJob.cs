using HarmonyLib;
using Verse;
using Verse.AI;

namespace KeepItQuiet;

//Clean up job noise when it's finished
[HarmonyPatch(typeof(Pawn_JobTracker), "CleanupCurrentJob")]
internal class Pawn_JobTracker_CleanupCurrentJob
{
    public static void Prefix(Pawn ___pawn)
    {
        if (KeepQuietSettings.selfAnnoy || !___pawn.RaceProps.Humanlike)
        {
            return;
        }

        var noiseMap = ___pawn.Map?.GetComponent<MapComp_Noise>();
        if (noiseMap != null && noiseMap.currentJobNoise.ContainsKey(___pawn))
        {
            noiseMap.currentJobNoise.Remove(___pawn);
        }
    }
}