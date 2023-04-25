using HarmonyLib;
using RimWorld;
using Verse;

namespace KeepItQuiet;

//Adds noise when the pawn talks
[HarmonyPatch(typeof(Pawn_InteractionsTracker), "TryInteractWith")]
internal class Pawn_InteractionsTracker_TryInteractWith
{
    public static void Postfix(Pawn ___pawn)
    {
        ___pawn.Map.GetComponent<MapComp_Noise>().AddBang(___pawn.Position, KeepQuietSettings.voiceLevel);
    }
}