using RimWorld;

namespace KeepItQuiet
{
    public class CompProperties_Soother : CompProperties_Flickable
    {
        public CompProperties_Soother()
        {
            compClass = typeof(CompSoother);
        }
        public int power;
        public int maxLevel;
    }
}