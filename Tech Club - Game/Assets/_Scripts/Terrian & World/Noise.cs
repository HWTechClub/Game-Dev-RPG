using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    //This creates a terrain height at a given location using Perlin noise.
    public static float GetTerrainHeight(int x, int z, Vector2[] octaveOffsets, int octaves, float persistance, float lacunarity, float scale)
    {
        float perlinValue = 0;

        float amplitude = 1;
        float frequency = 1;

        //This loops over each octave
        for (int i = 0; i < octaves; i++)
        {
            perlinValue += (Mathf.PerlinNoise((float)(x * frequency + octaveOffsets[i].x) / scale, (float)(z * frequency + octaveOffsets[i].y) / scale) * amplitude);

            amplitude *= persistance;   //This modifies how effective the next octave will be
            frequency *= lacunarity;    //This modifies the scale of the next octave
        }

        perlinValue = Mathf.InverseLerp(0, 1, perlinValue);
        perlinValue = Mathf.Clamp(perlinValue, 0.5f, 1);

        return (float)(perlinValue);
    }

    //Generates a noise map based on given parameters
    public static float[,] GenerateNoiseMap(int width, int octaves, float persistance, float lacunarity, float scale, Vector2[] offset, Vector2 location)
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

                float noise = 0;
                float amplitude = 1;
                float frequency = 1;

                //For each octave, add to the noise height.
                for (int i = 0; i < octaves; i++)
                {
                    noise += (Mathf.PerlinNoise((float)((x + location.x - width/2) * frequency + offset[0].x) / scale, (float)((z + location.y - width/2) * frequency + offset[0].y) / scale) * amplitude);

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
                noiseMap[x, z] = Mathf.InverseLerp(0, 1, noiseMap[x, z]);
            }
        }

        return noiseMap;
    }

    //This is a easy DIY 3d Perlin function I stole from stackoverflow or something
    public static float Perlin3D(float x, float y, float z)
    {
        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);

        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        float ABC = AB + BC + AC + BA + CB + CA;
        return ABC / 6f;

    }

}
