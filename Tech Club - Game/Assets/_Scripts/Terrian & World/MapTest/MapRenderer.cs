using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapRenderer : MonoBehaviour
{
    [SerializeField] RawImage map;

    [SerializeField] Gradient heightColor;

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


}