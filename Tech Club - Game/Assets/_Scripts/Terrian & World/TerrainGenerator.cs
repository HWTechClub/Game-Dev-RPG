using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] int worldChunks = 10;

    [SerializeField] const float maxViewDst = 63;
    [SerializeField] Transform viewer;
    [SerializeField] int chunksVisibleInViewDst;

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
        GenerateMap(Vector2.one * 50);
    }

    private void Update()
    {
        
        UpdateVisibleChunks();

    }

    void UpdateVisibleChunks ()
    {
        int currentChunkCoordX = Mathf.RoundToInt(viewer.position.x - viewer.position.x % MarchingCubesData.chunkWidth);
        int currentChunkCoordZ = Mathf.RoundToInt(viewer.position.z - viewer.position.z % MarchingCubesData.chunkWidth);
        
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
                    //Create new chunk/ TO DO: instead of creating a new chunk, take the furthest chunk and change it.
                    GenerateMap (new Vector2 (currentChunkCoordX, currentChunkCoordZ));

                    CreateNewChunks(viewedChunkCOord);
                    return;
                }

            }
        }

    }

    //Generates the terrain
    public void GenerateTerrain ()
    {
        chunks.Clear();
        

        for (int x = 0; x < worldChunks; x++)
        {
            for (int z = 0; z < worldChunks; z++)
            {
                Vector3Int chunkPos = new Vector3Int(x * MarchingCubesData.chunkWidth, 0, z * MarchingCubesData.chunkWidth);
                chunks.Add(chunkPos, new Chunk(chunkPos, heightOffsets, octaves, persistance, lacunarity, scale));
                chunks[chunkPos].chunkObject.transform.SetParent(transform);
            }
        }

    }

    public void GenerateMap (Vector2 location) {
        float[,] heightMap;
        float[,] humidityMap;
        float[,] temperatureMap;

        heightMap = Noise.GenerateNoiseMap(mapWidth, octaves, persistance, lacunarity, scale, heightOffsets, location);
        humidityMap = Noise.GenerateNoiseMap(mapWidth, humOctaves, humPersistance, humLacunarity, scale * humScale, humidOffsets, location);
        temperatureMap = Noise.GenerateNoiseMap(mapWidth, tempOctaves, tempPersistance, tempLacunarity, scale * tempScale, tempOffsets, location);

        mapRenderer.DrawNoiseMap(heightMap, humidityMap, temperatureMap);
    }

    public void CreateNewChunks(Vector3Int position) {
        Vector3Int chunkPos = new Vector3Int(position.x, 0,position.z);
        chunks.Add(chunkPos, new Chunk(chunkPos, heightOffsets, octaves, persistance, lacunarity, scale));
        chunks[chunkPos].chunkObject.transform.SetParent(transform);
    }
}
