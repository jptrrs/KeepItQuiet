using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace KeepItQuiet;

internal class NoiseUtility
{
    private static readonly Dictionary<SoundDef, float> cachedLevels = new Dictionary<SoundDef, float>();
    public static Dictionary<Toil, int> noiseByToil = new Dictionary<Toil, int>();

    public static float GetSoundLevel(SoundDef soundDef, bool maxLevel = false)
    {
        if (cachedLevels.TryGetValue(soundDef, out var level))
        {
            return level;
        }

        var result = 1f;
        if (soundDef is NoisySoundDef noisyDef)
        {
            result = noisyDef.overrideNoiseVolume;
        }
        else if (soundDef.subSounds.Any(_ => true))
        {
            result = soundDef.subSounds.Select(x => maxLevel ? x.volumeRange.TrueMax : x.volumeRange.Average)
                .Aggregate((a, b) => a > b ? a : b);
        }

        cachedLevels.Add(soundDef, result);
        return result;
    }

    public static bool IsSilentEnough(Region region, int admitted = 0)
    {
        var mapComp = region.Map.GetComponent<MapComp_Noise>();
        if (mapComp == null)
        {
            return false;
        }

        bool BadCellFinder(IntVec3 c)
        {
            return mapComp.noiseGrid[region.Map.cellIndices.CellToIndex(c)] > admitted &&
                   !c.GetTerrain(region.Map).avoidWander;
        }

        return !region.TryFindRandomCellInRegion(BadCellFinder, out _);
    }
}