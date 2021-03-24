using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace KeepItQuiet
{
    public class CompSoother : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            MapComp_Noise noiseMap = parent.Map.GetComponent<MapComp_Noise>();
            if (noiseMap != null)
            {
                noiseMap.AddSoother(parent, parent.Position, Props.power, Props.maxLevel);
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            MapComp_Noise noiseMap = map.GetComponent<MapComp_Noise>();
            if (noiseMap != null)
            {
                noiseMap.ClearSoother(parent);
            }
        }

        public CompProperties_Soother Props
        {
            get
            {
                return (CompProperties_Soother)props;
            }
        }
    }
}