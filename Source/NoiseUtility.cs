using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace KeepItQuiet
{
    class NoiseUtility
    {
        private static Dictionary<SoundDef, float> cachedLevels = new Dictionary<SoundDef, float>();

        public static float GetSoundLevel(SoundDef soundDef)
        {
            float result = 1f;
            if (cachedLevels.ContainsKey(soundDef)) result = cachedLevels[soundDef];
            else
            {
                if (soundDef.subSounds.Any(x => x.volumeRange != null))
                {
                    result = soundDef.subSounds.Select(x => x.volumeRange.Average).Aggregate((a, b) => a > b ? a : b);
                }
                cachedLevels.Add(soundDef, result);
            }
            return result;
        }
    }
}
