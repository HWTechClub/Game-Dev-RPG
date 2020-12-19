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
    [SerializeField] Gradient magicColor;

    [SerializeField] Gradient[] biomeGradient;
    [SerializeField] Gradient[] magicBiomeGradient;

    [SerializeField] TypeOfMap mapType;

    [SerializeField] bool[] biomeHighlight;

    Biome[,] BiomeTable = new Biome[6, 6]
    {
        {Biome.Ice, Biome.Tundra, Biome.Plains,       Biome.Desert,              Biome.Desert,             Biome.Desert},
        {Biome.Ice, Biome.Tundra, Biome.Plains,       Biome.Plains,              Biome.Desert,             Biome.Desert},
        {Biome.Ice, Biome.Tundra, Biome.Forest,       Biome.Forest,              Biome.Savanna,            Biome.Savanna},
        {Biome.Ice, Biome.Tundra, Biome.Taiga,        Biome.Forest,              Biome.Savanna,            Biome.Savanna},
        {Biome.Ice, Biome.Tundra, Biome.Taiga,        Biome.SeasonalForest,      Biome.Jungle,             Biome.Jungle},
        {Biome.Ice, Biome.Tundra, Biome.Taiga,        Biome.SeasonalForest,      Biome.Jungle,             Biome.Jungle}
    };

    MagicBiome[,] MagicBiomeTable = new MagicBiome[6, 6]
    {
        {MagicBiome.DarkIce, MagicBiome.DarkIce,   MagicBiome.FaePlains,    MagicBiome.Lavalands,    MagicBiome.Lavalands,  MagicBiome.Lavalands},
        {MagicBiome.DarkIce, MagicBiome.DarkIce,   MagicBiome.FaePlains,    MagicBiome.FaePlains,    MagicBiome.Lavalands,  MagicBiome.Lavalands},
        {MagicBiome.DarkIce, MagicBiome.FaeTundra, MagicBiome.SpiritForest, MagicBiome.SpiritForest, MagicBiome.FireCanyon, MagicBiome.FireCanyon},
        {MagicBiome.DarkIce, MagicBiome.FaeTundra, MagicBiome.SpiritForest, MagicBiome.FaeForest,    MagicBiome.FireCanyon, MagicBiome.FireCanyon},
        {MagicBiome.DarkIce, MagicBiome.FaeTundra, MagicBiome.SpiritForest, MagicBiome.FaeForest,    MagicBiome.FaeForest,  MagicBiome.FaeForest},
        {MagicBiome.DarkIce, MagicBiome.FaeTundra, MagicBiome.FaeForest,    MagicBiome.FaeForest,    MagicBiome.FaeForest,  MagicBiome.FaeForest}
    };

    public void DrawNoiseMap (float[,] heightMap, float[,] humidMap, float [,] tempMap, float [,] magicMap)
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
            case TypeOfMap.Magic:

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (heightMap[x, y] > 0.5f)
                            colorMap[y * width + x] = magicColor.Evaluate(magicMap[x, y]);
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
                            if (magicMap[x, y] < 0.6f)
                            {
                                Biome biome = GetBiome(humidMap[x, y], tempMap[x, y]);

                                if (biomeHighlight[(int)biome])
                                {
                                    Color color = biomeGradient[(int)biome].Evaluate(0) * heightColor.Evaluate(heightMap[x, y]);

                                    colorMap[y * width + x] = color;
                                }
                                else
                                    colorMap[y * width + x] = heightColor.Evaluate(heightMap[x, y]);
                            } else
                            {
                                MagicBiome biome = GetMagicBiome(humidMap[x, y], tempMap[x, y]);

                                if (biomeHighlight[(int)biome + 9])
                                {
                                    Color color = magicBiomeGradient[(int)biome].Evaluate(0) * heightColor.Evaluate(heightMap[x, y]);

                                    colorMap[y * width + x] = color;
                                }
                                else
                                    colorMap[y * width + x] = heightColor.Evaluate(heightMap[x, y]);
                            }
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

    public void DrawNoiseMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);


        Color[] colorMap = new Color[width * height];


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = heightColor.Evaluate(heightMap[x, y]);
            }
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
        int hum = Mathf.RoundToInt(5 * humidity);
        int temp = Mathf.RoundToInt(5 * temperature);
        
        return BiomeTable[hum, temp];
    }

    MagicBiome GetMagicBiome(float humidity, float temperature)
    {
        int hum = Mathf.RoundToInt(5 * humidity);
        int temp = Mathf.RoundToInt(5 * temperature);

        return MagicBiomeTable[hum, temp];
    }
}

public enum TypeOfMap
{
    Height,
    Humidity,
    Temperature,
    Magic,
    Biome
}

public enum Biome
{
    Desert,
    Savanna,
    Plains,
    Forest,
    SeasonalForest,
    Jungle,
    Taiga,
    Tundra,
    Ice
}

public enum MagicBiome
{
    Lavalands,
    FireCanyon,
    FaePlains,
    SpiritForest,
    FaeForest,
    FaeTundra,
    DarkIce
}