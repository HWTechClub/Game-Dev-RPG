using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    //This creates a terrain height at a given location using Perlin noise.
    public static float GetTerrainHeight(int x, int y, int z, Vector3[] octaveOffsets, int octaves, float persistance, float lacunarity, float regionalHeights, float heightPowered)
    {

        float perlinValue = 0;

        float amplitude = 1;
        float frequency = 1;

        //This loops over each octave
        for (int i = 0; i < octaves; i++)
        {
            perlinValue += Mathf.PerlinNoise((float)(x * frequency + octaveOffsets[0].x) / 128, (float)(z * frequency + octaveOffsets[0].y) / 128) * amplitude;

            amplitude *= persistance;   //This modifies how effective the next octave will be
            frequency *= lacunarity;    //This modifies the scale of the next octave
        }

        //Multiply it by regional heights and power it by heightsPowered for more interesting terrain
        perlinValue *= regionalHeights;

        perlinValue = Mathf.Pow(perlinValue, heightPowered);

        //TO DO: Make interesting terrain using 3d noise, currently its too messy and slow to run.
        //perlinValue *= 1 - Perlin3D((float)(x * frequency + octaveOffsets[0].x) / 256, (float)(y * frequency + octaveOffsets[0].y) / 256, (float)(z * frequency + octaveOffsets[0].z) / 256);

        //TO DO: Inverse lerp the perlin value so its between 0 and 1

        return (float)MarchingCubesData.heightRange * perlinValue + MarchingCubesData.seaLevel;
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
