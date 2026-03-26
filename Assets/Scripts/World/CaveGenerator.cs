using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class CaveGenerator : MonoBehaviour
{
    [Header("World Depth")]
    public int worldHeight = 120;

    [Header("Generation")]
    public int seed = 0;
    public bool randomSeed = true;
    public float caveScale = 0.12f;
    public float caveThreshold = 0.45f;
    public float stoneScale = 0.07f;

    [Header("Ore Settings")]
    public float coalScale = 0.15f;
    public float coalThreshold = 0.70f;
    public float ironScale = 0.18f;
    public float ironThreshold = 0.74f;
    public float copperScale = 0.20f;
    public float copperThreshold = 0.76f;
    public float goldScale = 0.22f;
    public float goldThreshold = 0.80f;
    public float emeraldScale = 0.25f;
    public float emeraldThreshold = 0.85f;

    [Header("Depth Layers")]
    public int dirtLayerDepth = 8;
    public int bedrockLayerDepth = 5;

    [Header("Tilemaps")]
    public Tilemap foregroundTilemap;
    public Tilemap backgroundTilemap;

    [Header("Tiles")]
    public TileBase stoneTile;
    public TileBase dirtTile;
    public TileBase coalTile;
    public TileBase ironTile;
    public TileBase copperTile;
    public TileBase goldTile;
    public TileBase emeraldTile;
    public TileBase bedrockTile;
    public TileBase backgroundTile;

    [Header("Chunking")]
    public int chunkLoadRadius = 20;
    public int verticalLoadRadius = 20;
    public bool loadFullVertical = false;
    public int columnEvictRadius = 60;

    [Header("Save")]
    public string worldName = "world1";
    public bool ignoreSave = false;

    [Header("Player Spawn")]
    public GameObject playerPrefab;

    private string SavePath => Path.Combine(Application.persistentDataPath, "worlds", worldName + ".dat");

    [BurstCompile]
    struct GenerateColumnJob : IJob
    {
        public int columnX;
        public int worldHeight;
        public int bedrockLayerDepth;
        public int dirtLayerDepth;
        public float ox;
        public float oy;
        public float stoneScale;
        public float caveScale; public float caveThreshold;
        public float coalScale; public float coalThreshold;
        public float ironScale; public float ironThreshold;
        public float copperScale; public float copperThreshold;
        public float goldScale; public float goldThreshold;
        public float emeraldScale; public float emeraldThreshold;

        public NativeArray<int> result;

        public void Execute()
        {
            float xox = columnX + ox;
            int surfaceY = (int)(worldHeight * 0.75f + noise(xox * stoneScale, oy * stoneScale) * 12f);

            for (int y = 0; y < worldHeight; y++)
            {
                if (y < bedrockLayerDepth) { result[y] = 8; continue; }
                if (y > surfaceY) { result[y] = 0; continue; }
                if (y > surfaceY - dirtLayerDepth) { result[y] = 2; continue; }

                float yoy = y + oy;
                if (noise(xox * caveScale, yoy * caveScale) < caveThreshold) { result[y] = 0; continue; }

                float depth01 = 1f - (float)y / worldHeight;

                if (depth01 > 0.6f)
                {
                    if (noise(xox * emeraldScale + 50f, yoy * emeraldScale + 50f) > emeraldThreshold) { result[y] = 7; continue; }
                    if (noise(xox * goldScale + 40f, yoy * goldScale + 40f) > goldThreshold) { result[y] = 6; continue; }
                }
                if (depth01 > 0.35f)
                {
                    if (noise(xox * copperScale + 30f, yoy * copperScale + 30f) > copperThreshold) { result[y] = 5; continue; }
                    if (noise(xox * ironScale + 20f, yoy * ironScale + 20f) > ironThreshold) { result[y] = 4; continue; }
                }
                result[y] = noise(xox * coalScale + 10f, yoy * coalScale + 10f) > coalThreshold ? 3 : 1;
            }

            for (int y = 1; y < worldHeight - 1; y++)
            {
                int t = result[y];
                if (t == 0 || t == 8) continue;
                int airCount = 0;
                if (result[y + 1] == 0) airCount++;
                if (result[y - 1] == 0) airCount++;
                if (airCount >= 2) result[y] = 0;
            }
        }

        static float noise(float x, float y)
        {
            int xi = (int)Unity.Mathematics.math.floor(x) & 255;
            int yi = (int)Unity.Mathematics.math.floor(y) & 255;
            float xf = x - Unity.Mathematics.math.floor(x);
            float yf = y - Unity.Mathematics.math.floor(y);
            float u = fade(xf);
            float v = fade(yf);
            int aa = p[p[xi] + yi]; int ab = p[p[xi] + yi + 1];
            int ba = p[p[xi + 1] + yi]; int bb = p[p[xi + 1] + yi + 1];
            float res = lerp(v,
                lerp(u, grad(aa, xf, yf), grad(ba, xf - 1f, yf)),
                lerp(u, grad(ab, xf, yf - 1f), grad(bb, xf - 1f, yf - 1f)));
            return (res + 1f) * 0.5f;
        }

        static float fade(float t) { return t * t * t * (t * (t * 6f - 15f) + 10f); }
        static float lerp(float t, float a, float b) { return a + t * (b - a); }
        static float grad(int hash, float x, float y)
        {
            int h = hash & 3;
            float u = h < 2 ? x : y;
            float v = h < 2 ? y : x;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        static readonly int[] p = {
            151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,
            8,99,37,240,21,10,23,190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,
            35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,74,165,71,
            134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,
            55,46,245,40,244,102,143,54,65,25,63,161,1,216,80,73,209,76,132,187,208,89,
            18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,186,3,64,52,217,226,
            250,124,123,5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,
            189,28,42,223,183,170,213,119,248,152,2,44,154,163,70,221,153,101,155,167,43,
            172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,185,112,104,218,246,97,
            228,251,34,242,193,238,210,144,12,191,179,162,241,81,51,145,235,249,14,239,
            107,49,192,214,31,181,199,106,157,184,84,204,176,115,121,50,45,127,4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
            151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,
            8,99,37,240,21,10,23,190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,
            35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,74,165,71,
            134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,
            55,46,245,40,244,102,143,54,65,25,63,161,1,216,80,73,209,76,132,187,208,89,
            18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,186,3,64,52,217,226,
            250,124,123,5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,
            189,28,42,223,183,170,213,119,248,152,2,44,154,163,70,221,153,101,155,167,43,
            172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,185,112,104,218,246,97,
            228,251,34,242,193,238,210,144,12,191,179,162,241,81,51,145,235,249,14,239,
            107,49,192,214,31,181,199,106,157,184,84,204,176,115,121,50,45,127,4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };
    }

    struct PendingJob
    {
        public JobHandle handle;
        public NativeArray<int> result;
        public int columnX;
    }

    private Dictionary<int, TileType[]> map = new Dictionary<int, TileType[]>();
    private ConcurrentDictionary<Vector2Int, TileType> modifications = new ConcurrentDictionary<Vector2Int, TileType>();
    private Dictionary<int, Vector2Int> loadedColumns = new Dictionary<int, Vector2Int>();
    private Dictionary<int, Vector2Int> pendingLoad = new Dictionary<int, Vector2Int>();
    private HashSet<int> pendingSet = new HashSet<int>();
    private List<PendingJob> activeJobs = new List<PendingJob>();

    private Vector3Int[] posBuffer;
    private TileBase[] fgBuffer;
    private TileBase[] bgBuffer;
    private TileBase[] tileAssets;

    private Transform player;
    private bool lastLoadFullVertical;
    private int lastPlayerCol;
    private int lastPlayerRow;

    void Start()
    {
        if (randomSeed) seed = Random.Range(0, 999999);
        lastLoadFullVertical = loadFullVertical;

        posBuffer = new Vector3Int[worldHeight];
        fgBuffer = new TileBase[worldHeight];
        bgBuffer = new TileBase[worldHeight];

        tileAssets = new TileBase[9];
        tileAssets[0] = null;
        tileAssets[1] = stoneTile;
        tileAssets[2] = dirtTile;
        tileAssets[3] = coalTile;
        tileAssets[4] = ironTile;
        tileAssets[5] = copperTile;
        tileAssets[6] = goldTile;
        tileAssets[7] = emeraldTile;
        tileAssets[8] = bedrockTile;

        if (!ignoreSave) LoadWorld();
        Invoke("SpawnPlayer", 0.05f);
    }

    void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }

        for (int i = activeJobs.Count - 1; i >= 0; i--)
        {
            PendingJob job = activeJobs[i];
            if (!job.handle.IsCompleted) continue;

            job.handle.Complete();

            TileType[] col = new TileType[worldHeight];
            for (int y = 0; y < worldHeight; y++)
                col[y] = (TileType)job.result[y];

            for (int y = 0; y < worldHeight; y++)
            {
                TileType mod;
                if (modifications.TryGetValue(new Vector2Int(job.columnX, y), out mod))
                    col[y] = mod;
            }

            job.result.Dispose();
            map[job.columnX] = col;

            TileType[] leftCol, rightCol;
            bool hasLeft = map.TryGetValue(job.columnX - 1, out leftCol);
            bool hasRight = map.TryGetValue(job.columnX + 1, out rightCol);
            for (int y = 1; y < worldHeight - 1; y++)
            {
                if (col[y] == TileType.Air || col[y] == TileType.Bedrock) continue;
                int airCount = 0;
                if (col[y + 1] == TileType.Air) airCount++;
                if (col[y - 1] == TileType.Air) airCount++;
                if (hasLeft && leftCol[y] == TileType.Air) airCount++;
                if (hasRight && rightCol[y] == TileType.Air) airCount++;
                if (airCount >= 3) col[y] = TileType.Air;
            }
            pendingSet.Remove(job.columnX);
            activeJobs.RemoveAt(i);

            Vector2Int range;
            if (pendingLoad.TryGetValue(job.columnX, out range))
            {
                pendingLoad.Remove(job.columnX);
                if (!loadedColumns.ContainsKey(job.columnX))
                {
                    LoadColumn(job.columnX, range.x, range.y);
                    loadedColumns[job.columnX] = range;
                }
            }
        }

        int playerCol = Mathf.FloorToInt(player.position.x);
        int playerRow = Mathf.FloorToInt(player.position.y);

        bool modeChanged = loadFullVertical != lastLoadFullVertical;
        bool movedEnough = Mathf.Abs(playerCol - lastPlayerCol) >= 1 ||
                           (!loadFullVertical && Mathf.Abs(playerRow - lastPlayerRow) >= 1);

        if (modeChanged)
        {
            UnloadAll();
            lastLoadFullVertical = loadFullVertical;
        }

        if (modeChanged || movedEnough)
        {
            UpdateChunks(playerCol, playerRow);
            lastPlayerCol = playerCol;
            lastPlayerRow = playerRow;
        }
    }

    void OnApplicationQuit()
    {
        foreach (var job in activeJobs)
        {
            job.handle.Complete();
            job.result.Dispose();
        }
        activeJobs.Clear();
        SaveWorld();
    }

    void RequestColumn(int x)
    {
        if (map.ContainsKey(x) || pendingSet.Contains(x)) return;
        pendingSet.Add(x);

        var resultArray = new NativeArray<int>(worldHeight, Allocator.Persistent);

        var job = new GenerateColumnJob
        {
            columnX = x,
            worldHeight = worldHeight,
            bedrockLayerDepth = bedrockLayerDepth,
            dirtLayerDepth = dirtLayerDepth,
            ox = seed * 0.1f,
            oy = seed * 0.1f,
            stoneScale = stoneScale,
            caveScale = caveScale,
            caveThreshold = caveThreshold,
            coalScale = coalScale,
            coalThreshold = coalThreshold,
            ironScale = ironScale,
            ironThreshold = ironThreshold,
            copperScale = copperScale,
            copperThreshold = copperThreshold,
            goldScale = goldScale,
            goldThreshold = goldThreshold,
            emeraldScale = emeraldScale,
            emeraldThreshold = emeraldThreshold,
            result = resultArray
        };

        JobHandle handle = job.Schedule();
        activeJobs.Add(new PendingJob { handle = handle, result = resultArray, columnX = x });
    }

    void UpdateChunks(int centerCol, int centerRow)
    {
        int minCol = centerCol - chunkLoadRadius;
        int maxCol = centerCol + chunkLoadRadius;
        int yMin = loadFullVertical ? 0 : Mathf.Clamp(centerRow - verticalLoadRadius, 0, worldHeight - 1);
        int yMax = loadFullVertical ? worldHeight : Mathf.Clamp(centerRow + verticalLoadRadius, 0, worldHeight - 1);

        for (int x = minCol; x <= maxCol; x++)
        {
            if (loadedColumns.ContainsKey(x))
            {
                Vector2Int loaded = loadedColumns[x];
                if (loaded.x == yMin && loaded.y == yMax) continue;
                UnloadColumn(x);
            }

            if (map.ContainsKey(x))
            {
                LoadColumn(x, yMin, yMax);
                loadedColumns[x] = new Vector2Int(yMin, yMax);
            }
            else
            {
                pendingLoad[x] = new Vector2Int(yMin, yMax);
                RequestColumn(x);
            }
        }

        for (int pre = 1; pre <= 5; pre++)
        {
            RequestColumn(maxCol + pre);
            RequestColumn(minCol - pre);
        }

        List<int> toUnload = new List<int>();
        foreach (int x in loadedColumns.Keys)
            if (x < minCol || x > maxCol) toUnload.Add(x);
        foreach (int x in toUnload)
        {
            UnloadColumn(x);
            loadedColumns.Remove(x);
        }

        List<int> toEvict = new List<int>();
        foreach (int x in map.Keys)
            if (x < centerCol - columnEvictRadius || x > centerCol + columnEvictRadius)
                toEvict.Add(x);
        foreach (int x in toEvict)
            map.Remove(x);
    }

    void LoadColumn(int x, int yMin, int yMax)
    {
        int count = yMax - yMin;
        if (count <= 0) return;

        TileType[] col;
        if (!map.TryGetValue(x, out col)) return;

        for (int i = 0; i < count; i++)
        {
            int y = yMin + i;
            posBuffer[i] = new Vector3Int(x, y, 0);
            TileType t = col[y];
            if (t == TileType.Air)
            {
                fgBuffer[i] = null;
                bgBuffer[i] = (backgroundTile != null && y >= bedrockLayerDepth) ? backgroundTile : null;
            }
            else
            {
                fgBuffer[i] = tileAssets[(int)t];
                bgBuffer[i] = null;
            }
        }

        Vector3Int[] pos = new Vector3Int[count];
        TileBase[] fgOut = new TileBase[count];
        TileBase[] bgOut = new TileBase[count];
        System.Array.Copy(posBuffer, pos, count);
        System.Array.Copy(fgBuffer, fgOut, count);
        System.Array.Copy(bgBuffer, bgOut, count);

        foregroundTilemap.SetTiles(pos, fgOut);
        if (backgroundTilemap != null) backgroundTilemap.SetTiles(pos, bgOut);
    }

    void UnloadColumn(int x)
    {
        if (!loadedColumns.ContainsKey(x)) return;
        Vector2Int range = loadedColumns[x];
        int count = range.y - range.x;
        if (count <= 0) return;

        for (int i = 0; i < count; i++)
            posBuffer[i] = new Vector3Int(x, range.x + i, 0);

        Vector3Int[] pos = new Vector3Int[count];
        TileBase[] nul = new TileBase[count];
        System.Array.Copy(posBuffer, pos, count);
        foregroundTilemap.SetTiles(pos, nul);
        if (backgroundTilemap != null) backgroundTilemap.SetTiles(pos, nul);
    }

    void UnloadAll()
    {
        foreach (int x in new List<int>(loadedColumns.Keys))
            UnloadColumn(x);
        loadedColumns.Clear();
        pendingLoad.Clear();
        foregroundTilemap.ClearAllTiles();
        if (backgroundTilemap != null) backgroundTilemap.ClearAllTiles();
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null) return;

        for (int offset = 0; offset < 1000; offset++)
        {
            foreach (int x in new int[] { offset, -offset })
            {
                TileType[] col = GenerateColumnSync(x);
                for (int y = worldHeight - 1; y > bedrockLayerDepth + 3; y--)
                {
                    if (col[y] != TileType.Air && col[y + 1] == TileType.Air &&
                        col[y + 2] == TileType.Air && col[y + 3] == TileType.Air)
                    {
                        GameObject p = Instantiate(playerPrefab, new Vector3(x + 0.5f, y + 1.5f, 0f), Quaternion.identity);
                        player = p.transform;
                        UpdateChunks(x, y);
                        return;
                    }
                }
            }
        }

        Debug.LogWarning("No safe spawn found.");
        GameObject fb = Instantiate(playerPrefab, new Vector3(0.5f, worldHeight * 0.8f, 0f), Quaternion.identity);
        player = fb.transform;
        UpdateChunks(0, worldHeight / 2);
    }

    TileType[] GenerateColumnSync(int x)
    {
        TileType[] existing;
        if (map.TryGetValue(x, out existing)) return existing;

        float ox = seed * 0.1f;
        float oy = seed * 0.1f;
        float xox = x + ox;
        int surfaceY = Mathf.FloorToInt(worldHeight * 0.75f + Mathf.PerlinNoise(xox * stoneScale, oy * stoneScale) * 12f);
        TileType[] col = new TileType[worldHeight];

        for (int y = 0; y < worldHeight; y++)
        {
            if (y < bedrockLayerDepth) { col[y] = TileType.Bedrock; continue; }
            if (y > surfaceY) { col[y] = TileType.Air; continue; }
            if (y > surfaceY - dirtLayerDepth) { col[y] = TileType.Dirt; continue; }

            float yoy = y + oy;
            if (Mathf.PerlinNoise(xox * caveScale, yoy * caveScale) < caveThreshold) { col[y] = TileType.Air; continue; }

            float d = 1f - (float)y / worldHeight;
            if (d > 0.6f)
            {
                if (Mathf.PerlinNoise(xox * emeraldScale + 50f, yoy * emeraldScale + 50f) > emeraldThreshold) { col[y] = TileType.EmeraldOre; continue; }
                if (Mathf.PerlinNoise(xox * goldScale + 40f, yoy * goldScale + 40f) > goldThreshold) { col[y] = TileType.GoldOre; continue; }
            }
            if (d > 0.35f)
            {
                if (Mathf.PerlinNoise(xox * copperScale + 30f, yoy * copperScale + 30f) > copperThreshold) { col[y] = TileType.CopperOre; continue; }
                if (Mathf.PerlinNoise(xox * ironScale + 20f, yoy * ironScale + 20f) > ironThreshold) { col[y] = TileType.IronOre; continue; }
            }
            col[y] = Mathf.PerlinNoise(xox * coalScale + 10f, yoy * coalScale + 10f) > coalThreshold ? TileType.CoalOre : TileType.Stone;
        }

        for (int y = 0; y < worldHeight; y++)
        {
            TileType mod;
            if (modifications.TryGetValue(new Vector2Int(x, y), out mod)) col[y] = mod;
        }

        map[x] = col;
        return col;
    }

    public void SaveWorld()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
        using (BinaryWriter w = new BinaryWriter(File.Open(SavePath, FileMode.Create)))
        {
            w.Write(seed);
            w.Write(modifications.Count);
            foreach (var kvp in modifications)
            {
                w.Write(kvp.Key.x);
                w.Write(kvp.Key.y);
                w.Write((int)kvp.Value);
            }
        }
        Debug.Log("Saved: " + modifications.Count + " modifications");
    }

    void LoadWorld()
    {
        if (!File.Exists(SavePath)) return;
        using (BinaryReader r = new BinaryReader(File.Open(SavePath, FileMode.Open)))
        {
            int savedSeed = r.ReadInt32();
            if (!randomSeed) seed = savedSeed;
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int mx = r.ReadInt32(); int my = r.ReadInt32();
                modifications[new Vector2Int(mx, my)] = (TileType)r.ReadInt32();
            }
        }
        Debug.Log("Loaded: " + modifications.Count + " modifications");
    }

    [ContextMenu("Clear Save")]
    public void ClearSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        Debug.Log("Save cleared.");
    }

    public static int GetHardness(TileType t)
    {
        switch (t)
        {
            case TileType.Dirt: return 1;
            case TileType.Stone: return 3;
            case TileType.CoalOre: return 3;
            case TileType.IronOre: return 4;
            case TileType.CopperOre: return 4;
            case TileType.GoldOre: return 5;
            case TileType.EmeraldOre: return 6;
            default: return 3;
        }
    }

    public void BreakTile(int x, int y)
    {
        TileType[] col;
        if (!map.TryGetValue(x, out col)) return;
        if (y < 0 || y >= worldHeight) return;
        if (col[y] == TileType.Bedrock || col[y] == TileType.Air) return;
        col[y] = TileType.Air;
        modifications[new Vector2Int(x, y)] = TileType.Air;
        Vector3Int pos = new Vector3Int(x, y, 0);
        foregroundTilemap.SetTile(pos, null);
        if (backgroundTilemap != null) backgroundTilemap.SetTile(pos, backgroundTile);
    }

    public TileType GetTile(int x, int y)
    {
        if (y < 0 || y >= worldHeight) return TileType.Bedrock;
        TileType[] col;
        return map.TryGetValue(x, out col) ? col[y] : TileType.Air;
    }
}