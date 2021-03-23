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
        private const float
            silenceDecaySpeedDefault = 1f,
            silenceGainSpeedDefault = 0.2f;

        public static float 
            silenceDecaySpeed = silenceDecaySpeedDefault,
            silenceGainSpeed = silenceGainSpeedDefault;

        //public const float beautySensitivityReductionDefault = 0.25f; // zero for vanilla

        //public static float BeautySensitivityReduction = beautySensitivityReductionDefault;

        public static bool selfAnnoy = false;

        //public static bool LinkWindows = true;

        //public static bool LinkVents = true;

        public static void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            string label = "silenceDecaySpeed".Translate() + $": {silenceDecaySpeed.ToStringDecimalIfSmall()}x";
            string desc = "silenceDecaySpeedDesc".Translate();
            listing.Label(label, -1f, desc);
            silenceDecaySpeed = listing.Slider(silenceDecaySpeed, 0f, 10f);
            string label2 = "silenceGainSpeed".Translate() + $": {silenceGainSpeed.ToStringDecimalIfSmall()}x";
            string desc2 = ("silenceGainSpeedDesc").Translate();
            listing.Label(label2, -1f, desc2);
            silenceGainSpeed = listing.Slider(silenceGainSpeed, 0f, 10f);
            listing.Gap(12f);
            listing.CheckboxLabeled("selfAnnoy".Translate(), ref selfAnnoy);


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
                silenceDecaySpeed = silenceDecaySpeedDefault;
                silenceGainSpeed = silenceGainSpeedDefault;
                selfAnnoy = false;
            }
            listing.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref silenceDecaySpeed, "silenceDecaySpeed", silenceDecaySpeedDefault);
            Scribe_Values.Look(ref silenceGainSpeed, "silenceGainSpeed", silenceGainSpeedDefault);
            //Scribe_Values.Look(ref BeautySensitivityReduction, "ModifiedBeautyImpactFactor", beautySensitivityReductionDefault);
            //Scribe_Values.Look(ref LinkWindows, "LinkWindows", true);
            Scribe_Values.Look(ref selfAnnoy, "selfAnnoy", false);
            base.ExposeData();
        }
    }
}