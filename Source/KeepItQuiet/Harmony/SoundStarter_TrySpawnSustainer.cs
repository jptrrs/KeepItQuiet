using HarmonyLib;
using Verse;
using Verse.Sound;

namespace KeepItQuiet;

//Adds the noise for continuing sounds.
[HarmonyPatch(typeof(SoundStarter), nameof(SoundStarter.TrySpawnSustainer))]
internal class SoundStarter_TrySpawnSustainer
{
    public static void Postfix(SoundDef soundDef, SoundInfo info, Sustainer __result)
    {
        var maker = info.Maker;
        if (maker == null || !soundDef.sustain)
        {
            return;
        }

        var level = (int)NoiseUtility.GetSoundLevel(soundDef);
        Pawn actor = null;
        if (!KeepQuietSettings.selfAnnoy && maker.Thing is Pawn pawn && pawn.RaceProps.Humanlike)
        {
            actor = pawn;
        }

        maker.Map.GetComponent<MapComp_Noise>().AddPolluter(__result, maker.Cell, level, actor);
    }
}