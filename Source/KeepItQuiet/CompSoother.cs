using RimWorld;
using Verse;

namespace KeepItQuiet;

public class CompSoother : ThingComp
{
    private bool effectOn;


    public CompProperties_Soother Props => (CompProperties_Soother)props;

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

            var compPowerTrader = parent.TryGetComp<CompPowerTrader>();
            return compPowerTrader is not { PowerOn: false };
        }
    }

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
        if (signal is "PowerTurnedOn" or "PowerTurnedOff" or "FlickedOn" or "FlickedOff" or "Refueled" or "RanOutOfFuel"
            or "ScheduledOn" or "ScheduledOff" or "MechClusterDefeated")
        {
            UpdateSoother();
        }
    }

    public void UpdateSoother()
    {
        var noiseMap = parent.Map?.GetComponent<MapComp_Noise>();
        if (noiseMap == null)
        {
            return;
        }

        var shouldBeLitNow = ShouldBeOnNow;
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
}