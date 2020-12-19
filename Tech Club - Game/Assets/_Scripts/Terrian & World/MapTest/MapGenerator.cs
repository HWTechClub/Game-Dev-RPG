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

    //Magic
    [SerializeField, Range(1, 10)] int magicOctaves;
    [SerializeField, Range(0, 1)] float magicPersistance;
    [SerializeField, Range(1, 10)] float magicLacunarity;
    [SerializeField, Range(1, 5)] float magicScale;

    [Space(10)]

    [SerializeField, Range(0, 1000)] int seed; 

    //Offsets based on seed
    private Vector2[] heightOffsets;
    private Vector2[] humidOffsets;
    private Vector2[] tempOffsets;
    private Vector2[] magicOffsets;

    //Maps
    private float[,] heightMap;
    private float[,] humidityMap;
    private float[,] temperatureMap;
    private float[,] magicMap;


    [Space(20)]
    [SerializeField] MapRenderer mapRenderer;
    [SerializeField] public bool autoUpdate;

    //This uses the seed to generate a "random" offest for the noise maps.
    void GenerateOffset ()
    {
        System.Random prng = new System.Random(seed);
        heightOffsets = new Vector2[heightOctaves];
        humidOffsets = new Vector2[humOctaves];
        tempOffsets = new Vector2[tempOctaves];
        magicOffsets = new Vector2[magicOctaves];

        //A unique offset for each map and each octave

        for (int i = 0; i < heightOctaves; i++)
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

        for (int i = 0; i < magicOctaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetZ = prng.Next(-100000, 100000);
            magicOffsets[i] = new Vector2(offsetX, offsetZ);
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

                float noise = 0;
                float amplitude = 1;
                float frequency = 1;

                //For each octave, add to the noise height.
                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x / scale + offset[i].x) * frequency;
                    float sampleZ = (z / scale + offset[i].y) * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;

                    noise += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                noiseMap[x, z] = noise;
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
    

    public void GenerateMap ()
    {
        GenerateOffset();
        heightMap = GenerateNoiseMap(heightOctaves, heightPersistance, heightLacunarity, scale * heightScale, heightOffsets);
        humidityMap = GenerateNoiseMap(humOctaves, humPersistance, humLacunarity, scale * humScale, humidOffsets);
        temperatureMap = GenerateNoiseMap(tempOctaves, tempPersistance, tempLacunarity, scale * tempScale, tempOffsets);
        magicMap = GenerateNoiseMap(magicOctaves, magicPersistance, magicLacunarity, scale * magicScale, magicOffsets);


        mapRenderer.DrawNoiseMap(heightMap, humidityMap, temperatureMap, magicMap);
    }
    
}
