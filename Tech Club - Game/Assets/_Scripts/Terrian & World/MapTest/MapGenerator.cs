using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] int width;
    [SerializeField, Range (1,300)] float scale;

    [Space(10)]

    //Height
    [SerializeField, Range(1,10)] int heightOctaves;
    [SerializeField, Range(0, 1)] float heightPersistance;
    [SerializeField, Range(1, 10)] float heightLacunarity;
    [SerializeField, Range(1, 5)] float heightScale;

    [Space(10)]

    [SerializeField, Range(0, 10000)] int seed; 

    //Offsets based on seed
    private Vector2[] heightOffsets;

    //Maps
    private float[,] heightMap;


    [Space(20)]
    [SerializeField] MapRenderer mapRenderer;
    public bool autoUpdate;

    //This uses the seed to generate a "random" offest for the noise maps.
    void GenerateOffset ()
    {
        System.Random prng = new System.Random(seed);
        heightOffsets = new Vector2[heightOctaves];

        //A unique offset for each map and each octave

        for (int i = 0; i < heightOctaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetZ = prng.Next(-100000, 100000);
            heightOffsets[i] = new Vector2(offsetX, offsetZ);
        }
    }

    //Generates a noise map based on given parameters
    float[,] GenerateNoiseMap(int octaves, float persistance, float lacunarity, float scale, Vector2[] offset)
    {
        //Create a map that will hold our values
        float[,] noiseMap = new float[width, width];

        //If scale is less than 0, set it to a really low number. Can't divide by zero.
        if (scale < 0f)
            scale = 0.01f;

        //Loop through both axes.
        for (int x = 0; x < width; x++) {
            for (int z = 0; z < width; z++) {

                noiseMap[x, z] = Height(x / (scale * 0.1f), z / (scale * 0.1f), octaves, offset, persistance, lacunarity);
            }
        }


        //Then we inverse lerp to keep all values between 0 to 1.
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                noiseMap[x,z] = Mathf.InverseLerp(-1.2f, 1, noiseMap[x,z]);
            }
        }

        return noiseMap;
    }

    //Generates a noise map based on given parameters
    float[,] GenerateNoiseMapPattern(int octaves, float persistance, float lacunarity, float scale, Vector2[] offset)
    {
        //Create a map that will hold our values
        float[,] noiseMap = new float[width, width];

        //If scale is less than 0, set it to a really low number. Can't divide by zero.
        if (scale < 0f)
            scale = 0.01f;

        //Loop through both axes.
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {

                noiseMap[x, z] = Pattern(x, z, offset, octaves, persistance, lacunarity) * Height(x / (10), z / (10), octaves, offset, persistance, lacunarity);
            }
        }


        //Then we inverse lerp to keep all values between 0 to 1.
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                noiseMap[x, z] = Mathf.InverseLerp(-1.2f, 1, noiseMap[x, z]);
            }
        }

        return noiseMap;
    }


    public void GenerateMap ()
    {
        //GenerateOffset();
        //heightMap = GenerateNoiseMapPattern(heightOctaves, heightPersistance, heightLacunarity, scale * heightScale, heightOffsets);


        //mapRenderer.DrawNoiseMap(heightMap);

        mapRenderer.DrawNoiseMap(Voronoi.VoronoiMap(75, width, seed));

    }


    float Height(float x, float z, int octaves, Vector2[] offset, float persistance, float lacunarity)
    {
        float noise = 0;
        float amplitude = 1;
        float frequency = 1;

        //For each octave, add to the noise height.
        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x / scale + offset[i].x) * frequency;
            float sampleZ = (z / scale + offset[i].y) * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;

            noise += perlinValue * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }

        return noise;
    }

    float pv = 80;

    float Pattern(int x, int z, Vector2[] offset, int octaves, float persistance, float lacunarity)
    {
        Vector2 q = new Vector2(Height(x, z, octaves, offset, persistance, lacunarity),
                                Height(x + 5.2f, z + 1.3f, octaves, offset, persistance, lacunarity));

        Vector2 r = new Vector2(Height(x + pv * q.x + 1.7f, z + pv * q.y + 9.2f, octaves, offset, persistance, lacunarity),
                                Height(x + pv * q.x + 8.3f, z + pv * q.y + 2.8f, octaves, offset, persistance, lacunarity));

        return Height(x + pv * r.x, z + pv * r.y, octaves, offset, persistance, lacunarity);
    }

}
