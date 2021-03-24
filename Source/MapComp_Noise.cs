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
        protected static CellBoolDrawer drawer;
        protected float defaultOpacity;
        private static Color
            noisyColor = Color.red,
            silentColor = Color.cyan;
        private Map lastSeenMap;
        private int 
            nextUpdateTick,
            updateDelay = 60,
            noiseDecayPerLevel = 5; //60 ticks = 1 second
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
            int exp = (int)(Find.TickManager.TicksGame + (level * noiseDecayPerLevel));
            if (!bangs.ContainsKey(exp)) bangs.Add(exp, new List<Vector2Int>());
            bangs[exp].AddRange(MakeNoise(center, level));
        }

        public void AddPolluter(Sustainer source, IntVec3 center, float level = 1)
        {
            if (!polluters.ContainsKey(source))
            {
                polluters.Add(source, new List<Vector2Int>());
            }
            if (Prefs.LogVerbose) Log.Message($"[KeepItQuiet] Adding sustainer level {level} for {source} @ {center}");
            polluters[source].AddRange(MakeNoise(center, level));
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
            if (nextUpdateTick == 0 || ticksGame >= nextUpdateTick || Find.CurrentMap != lastSeenMap)
            {
                Silence(ticksGame);
                nextUpdateTick = ticksGame + updateDelay;
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
            foreach (Sustainer sust in polluters.Keys)
            {
                if (sust.Ended)
                {
                    ClearNoise(polluters[sust]);
                    polluters[sust].Clear();
                }
            }
            polluters.RemoveAll(x => x.Value.NullOrEmpty());
            foreach (int age in bangs.Keys)
            {
                if (age < tick)
                {
                    ClearNoise(bangs[age]);
                    bangs[age].Clear();
                }
            }
            bangs.RemoveAll(x => x.Key < tick);
            drawer.SetDirty();
        }

        private void ClearNoise(List<Vector2Int> area)
        {
            foreach (Vector2Int value in area)
            {
                //var level = noiseGrid[value.x] - value.y;
                //noiseGrid[value.x] = Math.Max(0, level);
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

        private List<Vector2Int> MakeNoise(IntVec3 center, float level, int maxLevel = 0)
        {
            var result = new List<Vector2Int>();
            int levelMod = (int)Mathf.Abs(level);
            foreach (IntVec3 tile in GenRadial.RadialCellsAround(center, Mathf.Min(levelMod,56), KeepQuietSettings.selfAnnoy))
            {
                int str = Mathf.RoundToInt(levelMod - tile.DistanceToSquared(center));
                if (maxLevel > 0 && str > maxLevel) str = maxLevel;
                if (str > 0)
                {
                    if (level < 0) str *= -1;
                    int idx = map.cellIndices.CellToIndex(tile);
                    try
                    {
                        noiseGrid[idx] += str;
                        result.Add(new Vector2Int(idx, str));
                    }
                    catch { };
                }
            }
            drawer.SetDirty();
            return result;
        }

        //public void Update()
        //{
        //    if (toggleShow)
        //    {
        //        if (drawer == null)
        //        {
        //            MakeDrawer();
        //        }
        //        drawer.MarkForDraw();
        //        //from heatmap
        //        int ticksGame = Find.TickManager.TicksGame;
        //        if (nextUpdateTick == 0 || ticksGame >= nextUpdateTick || Find.CurrentMap != lastSeenMap)
        //        {
        //            drawer.SetDirty();
        //            nextUpdateTick = ticksGame + updateDelay;
        //            lastSeenMap = Find.CurrentMap;
        //        }//
        //        drawer.CellBoolDrawerUpdate();
        //        //dirty = false;
        //        return;
        //    }
        //    drawer = null;
        //}


    }
}