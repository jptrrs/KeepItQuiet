using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;

namespace KeepItQuiet
{
    //Adds noise when the pawn talks
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "TryInteractWith")]
    class Pawn_InteractionsTracker_TryInteractWith
    {
        public static void Postfix(Pawn ___pawn)
        {
            ___pawn.Map.GetComponent<MapComp_Noise>().AddBang(___pawn.Position, KeepQuietSettings.voiceLevel);
        } 
    }
}
