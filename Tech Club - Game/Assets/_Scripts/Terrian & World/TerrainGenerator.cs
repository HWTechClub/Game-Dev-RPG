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

    [SerializeField, Range(1,10)] int octaves; //Layers of noise for detail
    [SerializeField, Range(0, 1)] float persistance; //Persistance of each layer
    [SerializeField, Range(1, 10)] float lacunarity; //Lacunarity of each layer

    [SerializeField] Vector2 offset; //Offset gotten from seed.

    Vector3[] octaveOffsets;

    private void Start()
    {
        //chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / MarchingCubesData.chunkWidth);


        System.Random prng = new System.Random(seed);
        octaveOffsets = new Vector3[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetZ = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);
        }

        GenerateMap();
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

                    CreateNewChunks(viewedChunkCOord);
                    return;
                }

            }
        }

    }

    //Generates the map
    public void GenerateMap ()
    {
        chunks.Clear();
        

        for (int x = 0; x < worldChunks; x++)
        {
            for (int z = 0; z < worldChunks; z++)
            {
                Vector3Int chunkPos = new Vector3Int(x * MarchingCubesData.chunkWidth, 0, z * MarchingCubesData.chunkWidth);
                chunks.Add(chunkPos, new Chunk(chunkPos, octaveOffsets, octaves, persistance, lacunarity));
                chunks[chunkPos].chunkObject.transform.SetParent(transform);
            }
        }

    }

    public void CreateNewChunks(Vector3Int position) {
        Vector3Int chunkPos = new Vector3Int(position.x, 0,position.z);
        chunks.Add(chunkPos, new Chunk(chunkPos, octaveOffsets, octaves, persistance, lacunarity));
        chunks[chunkPos].chunkObject.transform.SetParent(transform);
    }
}
