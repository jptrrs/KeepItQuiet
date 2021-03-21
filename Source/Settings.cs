using UnityEngine;
using Verse;

namespace KeepItQuiet
{
    public class KeepQuietMod : Mod
    {
        private KeepQuietSettings settings;

        public KeepQuietMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<KeepQuietSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            KeepQuietSettings.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "KeepItQuiet".Translate();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }
    }

    public class KeepQuietSettings : ModSettings
    {
        private const float noiseDecaySpeedDefault = 1f; //indoors accelerated degradation when not under windows

        public static float noiseDecaySpeed = noiseDecaySpeedDefault;

        //public const float beautySensitivityReductionDefault = 0.25f; // zero for vanilla

        //public static float BeautySensitivityReduction = beautySensitivityReductionDefault;

        //public static bool IsBeautyOn = false;

        //public static bool LinkWindows = true;

        //public static bool LinkVents = true;

        public static void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            string label = "noiseDecaySpeed".Translate() + ": " + noiseDecaySpeed.ToStringDecimalIfSmall() + "x";
            string desc = ("noiseDecaySpeedDesc").Translate();
            listing.Label(label, -1f, desc);
            noiseDecaySpeed = listing.Slider(noiseDecaySpeed, -10f, 10f);
            //if (IsBeautyOn)
            //{
            //    listing.Gap(12f);
            //    string label2 = "BeautySensitivityReduction".Translate() + ": " + BeautySensitivityReduction.ToStringPercent();
            //    string desc2 = ("BeautySensitivityReductionDesc").Translate();
            //    listing.Label(label2, -1f, desc2);
            //    BeautySensitivityReduction = listing.Slider(BeautySensitivityReduction, 0f, 1f);
            //}
            //listing.Gap(12f);
            //listing.Label(("LinkOptionsLabel").Translate() + " (" + ("RequiresRestart").Translate() + "):");
            //listing.GapLine();
            //listing.CheckboxLabeled(("LinkWindowsAndWalls").Translate(), ref LinkWindows);
            //if (LoadedModManager.RunningModsListForReading.Any(x => x.Name.Contains("RimFridge")))

            //{
            //    listing.CheckboxLabeled(("LinkFridgesAndWalls").Translate(), ref LinkVents);
            //}
            //else
            //{
            //    listing.CheckboxLabeled(("LinkVentsAndWalls").Translate(), ref LinkVents);
            //}
            listing.Gap(12f);
            if (listing.ButtonText("Reset", null))
            {
                //BeautySensitivityReduction = 0.25f;
                noiseDecaySpeed = noiseDecaySpeedDefault;
                //LinkWindows = true;
                //LinkVents = true;
            }
            listing.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref noiseDecaySpeed, "noiseDecaySpeed", noiseDecaySpeedDefault);
            //Scribe_Values.Look(ref BeautySensitivityReduction, "ModifiedBeautyImpactFactor", beautySensitivityReductionDefault);
            //Scribe_Values.Look(ref LinkWindows, "LinkWindows", true);
            //Scribe_Values.Look(ref LinkVents, "LinkVents", true);
            base.ExposeData();
        }
    }
}