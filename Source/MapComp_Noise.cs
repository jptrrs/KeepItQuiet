using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace KeepItQuiet
{
    [StaticConstructorOnStartup]
    public class MapComp_Noise : MapComponent, ICellBoolGiver
    {
        public static bool toggleShow = false;
        public Dictionary<int, List<Vector2Int>> bangs;
        public Dictionary<Thing, List<Vector2Int>> soothers;
        public int[] noiseGrid;
        public Dictionary<Sustainer, List<Vector2Int>> polluters;
        public Dictionary<Pawn, int> currentJobNoise;
        protected static CellBoolDrawer drawer;
        protected float defaultOpacity;
        private static Color
            noisyColor = Color.red,
            silentColor = Color.cyan;
        private Map lastSeenMap;
        private const int updateDelay = 60; //60 ticks = 1 second
        private Func<int, Color> noiseColor = (value) => value > 0 ? noisyColor : silentColor;

        public MapComp_Noise(Map map) : base(map)
        {
            int numGridCells = map.cellIndices.NumGridCells;
            noiseGrid = new int[numGridCells];
            for (int i = 0; i < numGridCells; i++)
            {
                noiseGrid[i] = 0;
            }
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z);
            polluters = new Dictionary<Sustainer, List<Vector2Int>>();
            bangs = new Dictionary<int, List<Vector2Int>>();
            soothers = new Dictionary<Thing, List<Vector2Int>>();
            currentJobNoise = new Dictionary<Pawn, int>();
        }

        public Color Color
        {
            get
            {
                return Color.white;
            }
        }

        public static Texture2D Icon()
        {
            return ContentFinder<Texture2D>.Get("NoiseMap", true);
        }

        public static string IconTip()
        {
            return "NoiseMap".Translate();
        }

        public void AddBang(IntVec3 center, float level = 1)
        {
            if (Prefs.LogVerbose) Log.Message($"[KeepItQuiet] Adding bang level {level} @ {center}");
            int exp = (int)(Find.TickManager.TicksGame + (level * noiseDecayPerLevel.Evaluate(level)));
            if (!bangs.ContainsKey(exp)) bangs.Add(exp, new List<Vector2Int>());
            bangs[exp].AddRange(MakeNoise(center, level));
        }

        public void AddPolluter(Sustainer source, IntVec3 center, int level = 1, Pawn actor = null)
        {
            if (Prefs.LogVerbose)
            {
                string sourceName = actor != null ? actor.ToString() : source.def.defName;
                Log.Message($"[KeepItQuiet] Adding sustainer level {level} for {sourceName} @ {center}");
            }
            if (!polluters.ContainsKey(source))
            {
                polluters.Add(source, new List<Vector2Int>());
            }
            polluters[source].AddRange(MakeNoise(center, level, 0, actor == null));
            if (!KeepQuietSettings.selfAnnoy && actor != null)
            {
                if (!currentJobNoise.ContainsKey(actor)) currentJobNoise.Add(actor, level);
                else currentJobNoise[actor] = level;
            }
        }

        public void AddSoother(Thing source, IntVec3 center, float level = 1, int maxLevel = 1)
        {
            if (!soothers.ContainsKey(source))
            {
                soothers.Add(source, new List<Vector2Int>());
            }
            if (Prefs.LogVerbose) Log.Message($"[KeepItQuiet] Adding soothers level {level} for {source} @ {center}");
            soothers[source].AddRange(MakeNoise(center, level * -1, maxLevel));
        }

        public bool GetCellBool(int index)
        {
            return !Find.CurrentMap.fogGrid.IsFogged(index) && noiseGrid[index] != 0;
        }

        public Color GetCellExtraColor(int index)
        {
            Color output = noiseColor(noiseGrid[index]);
            output.a = Mathf.Min(Math.Abs(noiseGrid[index]) * 0.1f, 1f);
            return output;
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            int ticksGame = Find.TickManager.TicksGame;
            if (ticksGame % updateDelay == 0 || Find.CurrentMap != lastSeenMap)
            {
                Silence(ticksGame);
                lastSeenMap = Find.CurrentMap;
            }
        }

        public override void MapComponentUpdate()
        {
            if (toggleShow)
            {
                drawer.MarkForDraw();
            }
            drawer.CellBoolDrawerUpdate();
        }

        public void Silence(int tick)
        {
            foreach (Sustainer sust in polluters.Keys.Where(x => x.Ended))
            {
                ClearNoise(polluters[sust]);
                polluters[sust].Clear();
            }
            polluters.RemoveAll(x => x.Value.NullOrEmpty());
            foreach (int age in bangs.Keys.Where(x => x < tick))
            {
                ClearNoise(bangs[age]);
                bangs[age].Clear();
            }
            bangs.RemoveAll(x => x.Key < tick);
            drawer.SetDirty();
        }

        private void ClearNoise(List<Vector2Int> area)
        {
            foreach (Vector2Int value in area)
            {
                noiseGrid[value.x] -= value.y;
            }
        }

        public void ClearSoother(Thing source)
        {
            if (soothers.ContainsKey(source))
            {
                ClearNoise(soothers[source]);
                soothers[source].Clear();
                drawer.SetDirty();
            }
        }

        private List<Vector2Int> MakeNoise(IntVec3 center, float level, int maxLevel = 0, bool spread = false)
        {
            var result = new List<Vector2Int>();
            var levelMod = Mathf.Abs(level); // because there is such a thing as "negative noise".
            float radius = Mathf.Min(levelMod, 56) / (spread ? 1 : 2);
            foreach (IntVec3 tile in GenRadial.RadialCellsAround(center, radius, true).Where(c => c.InBounds(map)))
            {
                //float decay = /*spread ? tile.DistanceTo(center) :*/ tile.DistanceToSquared(center) / levelMod;
                //int str = Mathf.RoundToInt(levelMod - decay);
                var dist = tile.DistanceTo(center);
                float str = NoiseSpread(levelMod, dist, spread);
                if (maxLevel > 0 && str > maxLevel) str = maxLevel;
                if (str > 0)
                {
                    if (level < 0) str *= -1;
                    int idx = map.cellIndices.CellToIndex(tile);
                    try
                    {
                        var strInt = (int)str;
                        noiseGrid[idx] += strInt;
                        result.Add(new Vector2Int(idx, strInt));
                    }
                    catch (Exception e)
                    {
                        Log.Warning("Error at MakeNoise: " + e.Message);
                    }
                }
            }
            drawer.SetDirty();
            return result;
        }

        private float NoiseSpread(float level, float dist, bool spread)
        {
            //spread:       f(x) = (x/4)*(1+cos(dist*pi/x))      peak is half the level, reach equals level
            //no spread:    f(x) = (x/2)*(1+cos(dist*pi/(x/2)))  peak equals level, reaches half the level
            int xFactor = spread ? 1 : 2;
            int yFactor = spread ? 4 : 2;
            return (level / yFactor) * (1 + Mathf.Cos(dist * Mathf.PI / (level / xFactor)));
        }

        public static readonly SimpleCurve noiseDecayPerLevel = new SimpleCurve
        {
            {
                new CurvePoint(1f, 20f),
                true
            },
            {
                new CurvePoint(10f, 10f),
                true
            },
            {
                new CurvePoint(50f, 5f),
                true
            }
        };
    }
}