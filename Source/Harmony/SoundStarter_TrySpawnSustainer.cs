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
                if (soundDef.sustain) maker.Map.GetComponent<MapComp_Noise>().AddPolluter(__result, maker.Cell, NoiseUtility.GetSoundLevel(soundDef));
            }
        }

    }
}
