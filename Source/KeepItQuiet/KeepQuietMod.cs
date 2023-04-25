using UnityEngine;
using Verse;

namespace KeepItQuiet;

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
}