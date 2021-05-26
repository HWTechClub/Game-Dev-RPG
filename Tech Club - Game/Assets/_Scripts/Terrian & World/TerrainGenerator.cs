using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{

    [SerializeField] World world;

    [Space (20)]
    
    [SerializeField] int initialChunks = 10;

    [SerializeField] const float maxViewDst = 63;
    [SerializeField] Transform viewer;
    [SerializeField] int chunksVisibleInViewDst;

    [SerializeField] int maxChunkCount;
    
    [SerializeField] Vector3Int furthest = Vector3Int.zero;

    [Space (20)]

    //Dictionary that holds all chunks, both loaded and hidden
    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    [SerializeField, Range(0,500)] int seed; //The seed. This allows for some randomness.

    [SerializeField, Range(1,15)] int octaves; //Layers of noise for detail
    [SerializeField, Range(0, 1)] float persistance; //Persistance of each layer
    [SerializeField, Range(1, 10)] float lacunarity; //Lacunarity of each layer
    [SerializeField] float scale; //Lacunarity of each layer

    [Space(10)]

    //Temperature
    [SerializeField, Range(1, 10)] int tempOctaves;
    [SerializeField, Range(0, 1)] float tempPersistance;
    [SerializeField, Range(1, 10)] float tempLacunarity;
    [SerializeField, Range(1, 5)] float tempScale;

    [Space(10)]

    //Humidity
    [SerializeField, Range(1, 10)] int humOctaves;
    [SerializeField, Range(0, 1)] float humPersistance;
    [SerializeField, Range(1, 10)] float humLacunarity;
    [SerializeField, Range(1, 5)] float humScale;

    [Space(10)]


    //Offsets based on seed
    private Vector2[] heightOffsets;
    private Vector2[] humidOffsets;
    private Vector2[] tempOffsets;

    [SerializeField] MapRenderer mapRenderer;
    [SerializeField] int mapWidth;

    //This uses the seed to generate a "random" offest for the noise maps.
    void GenerateOffset()
    {
        System.Random prng = new System.Random(seed);
        heightOffsets = new Vector2[octaves];
        humidOffsets = new Vector2[humOctaves];
        tempOffsets = new Vector2[tempOctaves];

        //A unique offset for each map and each octave

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetZ = prng.Next(-100000, 100000);
            heightOffsets[i] = new Vector2(offsetX, offsetZ);
        }

        for (int i = 0; i < humOctaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetZ = prng.Next(-100000, 100000);
            humidOffsets[i] = new Vector2(offsetX, offsetZ);
        }

        for (int i = 0; i < tempOctaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetZ = prng.Next(-100000, 100000);
            tempOffsets[i] = new Vector2(offsetX, offsetZ);
        }
    }

    private void Start()
    {
        GenerateOffset();
        GenerateTerrain();
        //GenerateMap(Vector2.zero);
    }

    private void Update()
    {
        UpdateVisibleChunks();

        //GenerateMap(new Vector2((int)viewer.position.x, (int)viewer.position.z));
    }

    void UpdateVisibleChunks ()
    {
        maxChunkCount = (int) (Mathf.PI * Mathf.Pow(chunksVisibleInViewDst, 2));

        int currentChunkCoordX = Mathf.RoundToInt(viewer.position.x - viewer.position.x % MarchingCubesData.chunkWidth);
        int currentChunkCoordZ = Mathf.RoundToInt(viewer.position.z - viewer.position.z % MarchingCubesData.chunkWidth);

        bool createChunk = false;
        Vector3Int chunkToCreate = furthest;

        foreach (KeyValuePair<Vector3Int, Chunk> entry in chunks) {
            if ((Mathf.Pow(currentChunkCoordX - entry.Value.position.x, 2) + Mathf.Pow(currentChunkCoordZ - entry.Value.position.z, 2)) > (Mathf.Pow(currentChunkCoordX - furthest.x, 2) + Mathf.Pow(currentChunkCoordZ - furthest.z, 2)))
            {
                furthest = Vector3Int.CeilToInt(entry.Value.position);
            }
        }

        //BIG WIP
        for (int zOffset = -chunksVisibleInViewDst; zOffset <= chunksVisibleInViewDst; zOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector3Int viewedChunkCOord = new Vector3Int(currentChunkCoordX + xOffset * MarchingCubesData.chunkWidth, 0, currentChunkCoordZ + zOffset * MarchingCubesData.chunkWidth);
                
                if (chunks.ContainsKey(viewedChunkCOord))
                {
                    //Chunk already exists.
                }
                else
                {
                    //Create new chunk/
                    if ((Mathf.Pow(currentChunkCoordX - viewedChunkCOord.x, 2) + Mathf.Pow(currentChunkCoordZ - viewedChunkCOord.z, 2)) < (Mathf.Pow(currentChunkCoordX - chunkToCreate.x, 2) + Mathf.Pow(currentChunkCoordZ - chunkToCreate.z, 2)))
                    {
                        chunkToCreate = viewedChunkCOord;
                        createChunk = true;

                    }

                }

            }
        }


        if (createChunk) {

            if (chunks.Count >= maxChunkCount)
            {
                //Reuse old chunk
                Vector3Int oldPosition = furthest;
                
                ReuseChunk(oldPosition, chunkToCreate);

                furthest = new Vector3Int(currentChunkCoordX, 0, currentChunkCoordZ);

                return;
            }

            CreateNewChunks(chunkToCreate);
        }
    }

    //Generates the terrain
    public void GenerateTerrain ()
    {
        chunks.Clear();

        for (int x = 0; x < initialChunks; x++)
        {
            for (int z = 0; z < initialChunks; z++)
            {
                Vector3Int chunkPos = new Vector3Int(x * MarchingCubesData.chunkWidth, 0, z * MarchingCubesData.chunkWidth);
                chunks.Add(chunkPos, new Chunk(new TerrainGenData( chunkPos, heightOffsets, octaves, persistance, lacunarity, scale, this)));
                chunks[chunkPos].chunkObject.transform.SetParent(transform);
            }
        }

    }

    private void GenerateMap (Vector2 position)
    {
        float[,] heightMap = new float[mapWidth,mapWidth];

        for (int x = 0; x < mapWidth; x++) {
            for (int z = 0; z < mapWidth; z++)
            {
                float value = 0;
                //world.WorldHeightMap.TryGetValue(new Vector2((x - mapWidth / 2) * 10 + position.x, (z - mapWidth / 2) * 10 + position.y), out value);

                heightMap[x, z] = value;
            }
        }
        
        mapRenderer.DrawNoiseMap(heightMap);
    }

    public void CreateNewChunks(Vector3Int position) {
        Vector3Int chunkPos = new Vector3Int(position.x, 0,position.z);
        chunks.Add(chunkPos, new Chunk(new TerrainGenData(chunkPos, heightOffsets, octaves, persistance, lacunarity, scale, this)));
        chunks[chunkPos].chunkObject.transform.SetParent(transform);
    }

    public Vector2[] HeightOffest {
        get {
            return heightOffsets;
        }
    }

    private void ReuseChunk(Vector3Int oldPosition, Vector3Int newPosition) {

        Vector3Int chunkPos = newPosition;
        Chunk chunk = chunks[oldPosition];
        chunks.Remove(oldPosition);

        chunk.UpdateChunk(new TerrainGenData(chunkPos, heightOffsets, octaves, persistance, lacunarity, scale, this));

        chunks.Add(newPosition, chunk);
    }
}
