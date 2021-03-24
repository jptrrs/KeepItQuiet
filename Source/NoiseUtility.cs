using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace KeepItQuiet
{
    class NoiseUtility
    {
        private static Dictionary<SoundDef, float> cachedLevels = new Dictionary<SoundDef, float>();
        public static Dictionary<Toil, int> noiseByToil = new Dictionary<Toil, int>();

        public static float GetSoundLevel(SoundDef soundDef)
        {
            if (cachedLevels.ContainsKey(soundDef)) return cachedLevels[soundDef];
            float result = 1f;
            if (soundDef is NoisySoundDef noisyDef)
            {
                result = noisyDef.overrideNoiseVolume;
            }
            else if (soundDef.subSounds.Any(x => x.volumeRange != null))
            {
                result = soundDef.subSounds.Select(x => x.volumeRange.Average).Aggregate((a, b) => a > b ? a : b);
            }
            cachedLevels.Add(soundDef, result);
            return result;
        }

        public static bool IsSilentEnough(Region region, int admitted = 0)
        {
            MapComp_Noise mapComp = region.Map.GetComponent<MapComp_Noise>();
            if (mapComp != null)
            {
                IntVec3 badCell;
                Predicate<IntVec3> badCellFinder = (IntVec3 c) => mapComp.noiseGrid[region.Map.cellIndices.CellToIndex(c)] > admitted && !c.GetTerrain(region.Map).avoidWander;
                return !region.TryFindRandomCellInRegion(badCellFinder, out badCell);
            }
            return false;
        }

    }
}
