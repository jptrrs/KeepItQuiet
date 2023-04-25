using HarmonyLib;
using Verse;
using Verse.Sound;

namespace KeepItQuiet;

//Adds noise for sounds played one time.
[HarmonyPatch(typeof(SoundStarter), nameof(SoundStarter.PlayOneShot))]
internal class SoundStarter_PlayOneShot
{
    public static void Postfix(SoundDef soundDef, SoundInfo info)
    {
        var maker = info.Maker;
        if (maker == null)
        {
            return;
        }

        maker.Map.GetComponent<MapComp_Noise>().AddBang(maker.Cell, NoiseUtility.GetSoundLevel(soundDef));
    }
}