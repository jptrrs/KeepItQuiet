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

namespace KeepItQuiet
{
    [HarmonyPatch(typeof(SoundStarter), nameof(SoundStarter.TrySpawnSustainer))]
    class SoundStarter_TrySpawnSustainer
    {
        public static void Postfix(SoundDef soundDef, SoundInfo info, Sustainer __result)
        {
            TargetInfo maker = info.Maker;
            if (maker != null)
            {
                string sound = soundDef != null ? soundDef.defName : "none";
                //var recipeDef = ToilEffects_PlaySustainerOrSound.currentRecipe;
                //string recipe = recipeDef != null ? (recipeDef.label ?? recipeDef.defName) : "none";
                //Log.Message($"TrySpawnSustainer at {maker.Cell} / sound: {sound}, sustain {soundDef.sustain}");
                //Log.Message($"TrySpawnSustainer at {maker.Cell} / stackTrace: {new StackTrace().GetFrame(2).GetMethod().DeclaringType}");
                if (soundDef.sustain) maker.Map.GetComponent<MapComp_Noise>().AddPolluter(__result, maker.Cell, NoiseUtility.GetSoundLevel(soundDef));
            }
        }

    }
}
