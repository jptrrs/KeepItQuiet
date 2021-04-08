using HarmonyLib;
using RimWorld;
using Verse;

namespace KeepItQuiet
{
    //Injects the toggle for the noise map
    [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
    public static class PlaySettings_DoPlaySettingsGlobalControls
    {
        public static void Postfix(WidgetRow row, bool worldView)
        {
            if (worldView)
            {
                return;
            }
            if (row == null || MapComp_Noise.Icon() == null)
            {
                return;
            }
            row.ToggleableIcon(ref MapComp_Noise.toggleShow, MapComp_Noise.Icon(), MapComp_Noise.IconTip(), null, null);
        }
    }
}