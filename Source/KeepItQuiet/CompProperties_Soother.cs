using RimWorld;

namespace KeepItQuiet;

public class CompProperties_Soother : CompProperties_Flickable
{
    public int maxLevel;
    public int power;

    public CompProperties_Soother()
    {
        compClass = typeof(CompSoother);
    }
}