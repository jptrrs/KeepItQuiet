﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace KeepItQuiet;

[StaticConstructorOnStartup]
public class MapComp_Noise : MapComponent, ICellBoolGiver
{
    private const float roomContainment = 0.33f;
    private const int updateDelay = 60; //60 ticks = 1 second

    public static readonly SimpleCurve noiseDecayPerLevel = new SimpleCurve
    {
        new CurvePoint(1f, 20f),
        new CurvePoint(10f, 10f),
        new CurvePoint(50f, 5f)
    };

    public static bool toggleShow = false;
    protected static CellBoolDrawer drawer;

    private static readonly Color
        noisyColor = Color.red;

    private static readonly Color
        silentColor = Color.cyan;

    private readonly Func<int, Color> noiseColor = value => value > 0 ? noisyColor : silentColor;

    public Dictionary<int, List<Vector2Int>> bangs;
    public Dictionary<Pawn, int> currentJobNoise;
    protected float defaultOpacity;
    private Map lastSeenMap;
    public int[] noiseGrid;
    public Dictionary<Sustainer, List<Vector2Int>> polluters;
    public Dictionary<Thing, List<Vector2Int>> soothers;

    public MapComp_Noise(Map map) : base(map)
    {
        var numGridCells = map.cellIndices.NumGridCells;
        noiseGrid = new int[numGridCells];
        for (var i = 0; i < numGridCells; i++)
        {
            noiseGrid[i] = 0;
        }

        drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z);
        polluters = new Dictionary<Sustainer, List<Vector2Int>>();
        bangs = new Dictionary<int, List<Vector2Int>>();
        soothers = new Dictionary<Thing, List<Vector2Int>>();
        currentJobNoise = new Dictionary<Pawn, int>();
    }

    public Color Color => Color.white;

    public bool GetCellBool(int index)
    {
        return !Find.CurrentMap.fogGrid.IsFogged(index) && noiseGrid[index] != 0;
    }

    public Color GetCellExtraColor(int index)
    {
        var output = noiseColor(noiseGrid[index]);
        output.a = Mathf.Min(Math.Abs(noiseGrid[index]) * 0.1f, 1f);
        return output;
    }

    public static Texture2D Icon()
    {
        return ContentFinder<Texture2D>.Get("NoiseMap");
    }

    public static string IconTip()
    {
        return "NoiseMap".Translate();
    }

    public void AddBang(IntVec3 center, float level = 1)
    {
        if (Prefs.LogVerbose)
        {
            Log.Message($"[KeepItQuiet] Adding bang level {level} @ {center}");
        }

        var exp = (int)(Find.TickManager.TicksGame + (level * noiseDecayPerLevel.Evaluate(level)));
        if (!bangs.ContainsKey(exp))
        {
            bangs.Add(exp, new List<Vector2Int>());
        }

        bangs[exp].AddRange(MakeNoise(center, level));
    }

    public void AddPolluter(Sustainer source, IntVec3 center, int level = 1, Pawn actor = null)
    {
        if (Prefs.LogVerbose)
        {
            var sourceName = actor != null ? actor.ToString() : source.def.defName;
            Log.Message($"[KeepItQuiet] Adding sustainer level {level} for {sourceName} @ {center}");
        }

        if (!polluters.ContainsKey(source))
        {
            polluters.Add(source, new List<Vector2Int>());
        }

        var persistent = actor == null;
        polluters[source].AddRange(MakeNoise(center, level, 0, persistent, persistent));
        if (KeepQuietSettings.selfAnnoy || actor == null)
        {
            return;
        }

        currentJobNoise[actor] = level;
    }

    public void AddSoother(Thing source, IntVec3 center, float level = 1, int maxLevel = 1)
    {
        if (!soothers.ContainsKey(source))
        {
            soothers.Add(source, new List<Vector2Int>());
        }

        if (Prefs.LogVerbose)
        {
            Log.Message($"[KeepItQuiet] Adding soothers level {level} for {source} @ {center}");
        }

        soothers[source].AddRange(MakeNoise(center, level * -1, maxLevel, true));
    }

    public void ClearSoother(Thing source)
    {
        if (!soothers.ContainsKey(source))
        {
            return;
        }

        ClearNoise(soothers[source]);
        soothers[source].Clear();
        drawer.SetDirty();
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        var ticksGame = Find.TickManager.TicksGame;
        if (ticksGame % updateDelay != 0 && Find.CurrentMap == lastSeenMap)
        {
            return;
        }

        Silence(ticksGame);
        lastSeenMap = Find.CurrentMap;
    }

    public override void MapComponentUpdate()
    {
        base.MapComponentUpdate();
        //if (toggleShow)
        //{
        //    drawer.MarkForDraw();
        //}
        //drawer.CellBoolDrawerUpdate();
        if (!toggleShow)
        {
            return;
        }

        if (Find.CurrentMap != lastSeenMap)
        {
            MakeDrawer();
        }

        drawer.MarkForDraw();
        drawer.CellBoolDrawerUpdate();
    }

    public void MakeDrawer()
    {
        drawer = new CellBoolDrawer(this, Find.CurrentMap.Size.x, Find.CurrentMap.Size.z);
    }

    public void Silence(int tick)
    {
        foreach (var sust in polluters.Keys.Where(x => x.Ended))
        {
            ClearNoise(polluters[sust]);
            polluters[sust].Clear();
        }

        polluters.RemoveAll(x => x.Value.NullOrEmpty());
        foreach (var age in bangs.Keys.Where(x => x < tick))
        {
            ClearNoise(bangs[age]);
            bangs[age].Clear();
        }

        bangs.RemoveAll(x => x.Key < tick);
        drawer.SetDirty();
    }

    private void ClearNoise(List<Vector2Int> area)
    {
        foreach (var value in area)
        {
            noiseGrid[value.x] -= value.y;
        }
    }

    private List<Vector2Int> MakeNoise(IntVec3 center, float level, int maxLevel = 0, bool spread = false,
        bool attenuate = false)
    {
        var maxRadius = Mathf.RoundToInt(GenRadial.MaxRadialPatternRadius);
        if (level > 2 * maxRadius)
        {
            level = 2 * maxRadius;
            spread = false;
        }

        var levelMod = Mathf.Abs(level); // because there is such a thing as "negative noise".
        var radius = Mathf.Min(levelMod, maxRadius) / (spread ? 1 : 2);
        var room = center.GetRoom(map);
        var result = new List<Vector2Int>();
        foreach (var tile in GenRadial.RadialCellsAround(center, radius, true).Where(c => c.InBounds(map)))
        {
            var dist = tile.DistanceTo(center);
            var str = NoiseSpread(levelMod, dist, spread, attenuate);
            if (maxLevel > 0 && str > maxLevel)
            {
                str = maxLevel;
            }

            if (!attenuate && room != null && !room.ContainsCell(tile))
            {
                str *= 1 - roomContainment;
            }

            if (!(str > 0))
            {
                continue;
            }

            if (level < 0)
            {
                str *= -1;
            }

            var idx = map.cellIndices.CellToIndex(tile);
            try
            {
                var strInt = (int)str;
                noiseGrid[idx] += strInt;
                result.Add(new Vector2Int(idx, strInt));
            }
            catch (Exception e)
            {
                Log.Warning($"Error at MakeNoise: {e.Message}");
            }
        }

        drawer.SetDirty();
        return result;
    }

    private float NoiseSpread(float level, float dist, bool spread, bool attenuate)
    {
        //base cosine function: f(x) = (x/yFactor)*(1+cos(dist*pi/(x/xFactor)))
        var xFactor =
            spread ? 1 : 2; // determines how far the noise reaches: if spread, reach = level, else reach = level/2
        var yFactor = attenuate ? 4 : 2; //determines the peak level: if attenuated, peak = level, else peak = level/2;
        return level / yFactor * (1 + Mathf.Cos(dist * Mathf.PI / (level / xFactor)));
    }
}