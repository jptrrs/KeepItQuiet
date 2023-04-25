using HarmonyLib;
using RimWorld;
using Verse;

namespace KeepItQuiet;

//Add soothing effect when source object spawns
[HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.SpawnSetup))]
public static class ThingWithComps_SpawnSetup
{
    public static void Postfix(ThingWithComps __instance, Map map)
    {
        if (__instance is not Plant)
        {
            return;
        }

        var noiseMap = map.GetComponent<MapComp_Noise>();
        noiseMap?.AddSoother(__instance, __instance.Position, -3);
    }
}