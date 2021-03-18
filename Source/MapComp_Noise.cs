using System;
using System.Collections.Generic;
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
        public int[] noiseGrid;
        public Dictionary<Sustainer, List<Vector2Int>> polluters;
        protected static CellBoolDrawer drawer;
        protected float defaultOpacity;
        private Map lastSeenMap;
        private int nextUpdateTick;
        private int updateDelay = 60;

        public MapComp_Noise(Map map) : base(map)
        {
            int numGridCells = map.cellIndices.NumGridCells;
            noiseGrid = new int[numGridCells];
            for (int i = 0; i < numGridCells; i++)
            {
                noiseGrid[i] = 0;
            }
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z);
            //drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 2610, 0.5f);
            polluters = new Dictionary<Sustainer, List<Vector2Int>>();
            bangs = new Dictionary<int, List<Vector2Int>>();
        }

        public Color Color
        {
            get
            {
                return Color.red;
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
            Log.Message($"adding bang level {level} @ {center}");
            int exp = (int)(Find.TickManager.TicksGame + (level * 5)); //60 ticks = 1 second
            if (!bangs.ContainsKey(exp)) bangs.Add(exp, new List<Vector2Int>());
            List<Vector2Int> pollution;
            MakeNoise(center, level, out pollution);
            bangs[exp].AddRange(pollution);
        }

        public void AddPolluter(Sustainer source, IntVec3 center, float level = 1)
        {
            if (!polluters.ContainsKey(source))
            {
                polluters.Add(source, new List<Vector2Int>());
            }
            Log.Message($"adding sustainer level {level} @ {center}");
            List<Vector2Int> pollution;
            MakeNoise(center, level, out pollution);
            polluters[source].AddRange(pollution);
        }

        public bool GetCellBool(int index)
        {
            return noiseGrid[index] > 0;
            //return !Find.CurrentMap.fogGrid.IsFogged(index) && ShowCell(index);
        }

        public Color GetCellExtraColor(int index)
        {
            Color output = Color;
            output.a = Mathf.Min(noiseGrid[index] * 0.1f, 1f);
            return output;
        }

        public void MakeDrawer()
        {
            drawer = new CellBoolDrawer(this, Find.CurrentMap.Size.x, Find.CurrentMap.Size.z);
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

        void ClearNoise(List<Vector2Int> area)
        {
            foreach (Vector2Int value in area)
            {
                var level = noiseGrid[value.x] - value.y;
                noiseGrid[value.x] = Math.Max(0, level);
            }
        }

        private void MakeNoise(IntVec3 center, float level, out List<Vector2Int> log)
        {
            log = new List<Vector2Int>();
            foreach (IntVec3 tile in GenRadial.RadialCellsAround(center, Mathf.Min(level,56), true))
            {
                int str = Mathf.RoundToInt(level - tile.DistanceToSquared(center));
                if (str > 0)
                {
                    int idx = map.cellIndices.CellToIndex(tile);
                    log.Add(new Vector2Int(idx, str));
                    noiseGrid[idx] += str;
                }
            }
            drawer.SetDirty();
        }
        //public bool ShowCell(int index)
        //{
        //    return Find.CurrentMap.GetComponent<MapComp_Noise>().noiseGrid.ContainsKey(Find.CurrentMap.cellIndices.IndexToCell(index));
        //}

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