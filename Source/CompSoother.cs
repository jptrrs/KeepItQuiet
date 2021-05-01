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
            UpdateSoother();
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            UpdateSoother();
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "PowerTurnedOn" || signal == "PowerTurnedOff" || signal == "FlickedOn" || signal == "FlickedOff" || signal == "Refueled" || signal == "RanOutOfFuel" || signal == "ScheduledOn" || signal == "ScheduledOff" || signal == "MechClusterDefeated")
            {
                UpdateSoother();
            }
        }

        public void UpdateSoother()
        {
            MapComp_Noise noiseMap = parent.Map?.GetComponent<MapComp_Noise>();
            if (noiseMap == null) return;
            bool shouldBeLitNow = ShouldBeOnNow;
            if (effectOn == shouldBeLitNow)
            {
                return;
            }
            effectOn = shouldBeLitNow;
            if (!effectOn)
            {
                noiseMap.ClearSoother(parent);
                return;
            }
            noiseMap.AddSoother(parent, parent.Position, Props.power, Props.maxLevel);
        }


        public CompProperties_Soother Props
        {
            get
            {
                return (CompProperties_Soother)props;
            }
        }

        private bool effectOn;

        private bool ShouldBeOnNow
        {
            get
            {
                if (!parent.Spawned)
                {
                    return false;
                }
                if (!FlickUtility.WantsToBeOn(parent))
                {
                    return false;
                }
                CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
                if (compPowerTrader != null && !compPowerTrader.PowerOn)
                {
                    return false;
                }
                return true;
            }
        }

    }
}