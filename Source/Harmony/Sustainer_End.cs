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
    //[HarmonyPatch(typeof(Sustainer), nameof(Sustainer.End))]
    //class Sustainer_End
    //{
    //    public static void Prefix(Sustainer __instance)
    //    {
    //        TargetInfo maker = info.Maker;
    //        if (maker != null)
    //        {
    //            StackTrace st = new StackTrace();
    //            Log.Message($"playoneshot at {maker.Cell} / stackTrace: {st.GetFrame(2).GetMethod().DeclaringType}");
    //            maker.Map.GetComponent<MapComp_Noise>().MakeNoise(maker.Cell);
    //        }
    //    } 
    //}
}
