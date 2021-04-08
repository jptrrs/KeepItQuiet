using HarmonyLib;
using RimWorld;
using Verse;

namespace KeepItQuiet
{
    //Remove soothing effect when source object de-spawns
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
        }
    }
}