using HarmonyLib;
using Verse;

namespace KeepItQuiet;

//Remove soothing effect when source object de-spawns
[HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.DeSpawn))]
public static class ThingWithComps_DeSpawn
{
    public static void Prefix(ThingWithComps __instance)
    {
        var comp = __instance.TryGetComp<CompSoother>();
        if (comp == null)
        {
            return;
        }

        var noiseMap = __instance.Map.GetComponent<MapComp_Noise>();
        noiseMap?.ClearSoother(__instance);
    }
}