using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapRenderer : MonoBehaviour
{
    [SerializeField] RawImage map;

    [SerializeField] Gradient heightColor;
    [SerializeField] Gradient humidityColor;
    [SerializeField] Gradient temperatureColor;

    [SerializeField] Gradient[] biomeGradient;

    [SerializeField] TypeOfMap mapType;

    [SerializeField] bool[] biomeHighlight;

    Biome[,] BiomeTable = new Biome[6, 6]
    {
        {Biome.Ice, Biome.Tundra, Biome.Plains,       Biome.Desert,              Biome.Desert,             Biome.Desert},
        {Biome.Ice, Biome.Tundra, Biome.Plains,       Biome.Plains,              Biome.Desert,             Biome.Desert},
        {Biome.Ice, Biome.Tundra, Biome.Forest,       Biome.Forest,              Biome.Savanna,            Biome.Savanna},
        {Biome.Ice, Biome.Tundra, Biome.Taiga,        Biome.Forest,              Biome.Savanna,            Biome.Savanna},
        {Biome.Ice, Biome.Tundra, Biome.Taiga,        Biome.SeasonalForest,      Biome.TropicalRainforest, Biome.TropicalRainforest},
        {Biome.Ice, Biome.Tundra, Biome.Taiga,        Biome.TemperateRainforest, Biome.TropicalRainforest, Biome.TropicalRainforest}
    };

    public void DrawNoiseMap (float[,] heightMap, float[,] humidMap, float [,] tempMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);
        

        Color[] colorMap = new Color[width * height];

        switch (mapType)
        {
            case TypeOfMap.Height:

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        colorMap[y * width + x] = heightColor.Evaluate(heightMap[x, y]);
                    }
                }

                break;
            case TypeOfMap.Humidity:

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (heightMap[x, y] > 0.5f)
                            colorMap[y * width + x] = humidityColor.Evaluate(humidMap[x, y]);
                        else
                            colorMap[y * width + x] = Color.grey;
                    }
                }


                break;
            case TypeOfMap.Temperature:

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (heightMap[x, y] > 0.5f)
                            colorMap[y * width + x] = temperatureColor.Evaluate(tempMap[x, y]);
                        else
                            colorMap[y * width + x] = Color.grey;
                    }
                }


                break;
            case TypeOfMap.Biome:

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (heightMap[x, y] < 0.51f)
                            colorMap[y * width + x] = heightColor.Evaluate(heightMap[x, y]);
                        else
                        {
                            Biome biome = GetBiome(humidMap[x, y], tempMap[x, y]);

                            if (biomeHighlight[(int)biome])
                            {
                                Color color = biomeGradient[(int)biome].Evaluate(0) * heightColor.Evaluate(heightMap[x, y]);

                                colorMap[y * width + x] = color;
                            }
                            else
                                colorMap[y * width + x] = heightColor.Evaluate(heightMap[x, y]);
                        }
                    }
                }

                break;
        } 


        texture.SetPixels(colorMap);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        

        if (map != null)
        {
            map.texture = texture;
        }
    }


    Biome GetBiome(float humidity, float temperature) {
        int hum = (int)(5 * humidity + 0.9f);
        int temp = (int)(5 * temperature + 0.9f);
        
        return BiomeTable[hum, temp];
    }
}

public enum TypeOfMap
{
    Height,
    Humidity,
    Temperature,
    Biome
}

public enum Biome
{
    Desert,
    Savanna,
    TropicalRainforest,
    Plains,
    Forest,
    SeasonalForest,
    TemperateRainforest,
    Taiga,
    Tundra,
    Ice
}