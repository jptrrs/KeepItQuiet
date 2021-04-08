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
            silenceGainSpeedDefault = 1f;

        public static float 
            silenceDecaySpeed = silenceDecaySpeedDefault,
            silenceGainSpeed = silenceGainSpeedDefault;

        private const int voiceLevelDefault = 10;
        
        public static int voiceLevel = voiceLevelDefault;

        public static bool selfAnnoy = false;

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
            Scribe_Values.Look(ref selfAnnoy, "selfAnnoy", false);
            base.ExposeData();
        }
    }
}