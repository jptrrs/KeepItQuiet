using HarmonyLib;
using RimWorld;
using Verse;

namespace KeepItQuiet
{
    [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.DeSpawn))]
    public static class ThingWithComps_DeSpawn
    {
        public static void Prefix(ThingWithComps __instance)
        {
            CompSoother comp = __instance.TryGetComp<CompSoother>();
            if (comp != null)
            {
                MapComp_Noise noiseMap = __instance.Map.GetComponent<MapComp_Noise>();
                if (noiseMap != null)
                {
                    noiseMap.ClearSoother(__instance);
                }
            }
            //if (__instance is Plant)
            //{
            //    MapComp_Noise noiseMap = __instance.Map.GetComponent<MapComp_Noise>();
            //    if (noiseMap != null)
            //    {
            //        noiseMap.ClearSoother(__instance);
            //    }
            //}
        }
    }
}