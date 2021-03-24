using HarmonyLib;
using RimWorld;
using Verse;

namespace KeepItQuiet
{
    [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.SpawnSetup))]
    public static class ThingWithComps_SpawnSetup
    {
        public static void Postfix(ThingWithComps __instance, Map map)
        {
            CompSoother comp = __instance.TryGetComp<CompSoother>();
            if (comp != null)
            {
                Log.Message($"adding soother for {__instance}");

                MapComp_Noise noiseMap = map.GetComponent<MapComp_Noise>();
                if (noiseMap != null)
                {
                    noiseMap.AddSoother(__instance, __instance.Position, comp.Props.volume, comp.Props.maxLevel);
                }
            }

            //if (__instance is Plant)
            //{
            //    MapComp_Noise noiseMap = map.GetComponent<MapComp_Noise>();
            //    if (noiseMap != null)
            //    {
            //        noiseMap.AddSoother(__instance, __instance.Position, -3);
            //    }
            //}
        }
    }
}