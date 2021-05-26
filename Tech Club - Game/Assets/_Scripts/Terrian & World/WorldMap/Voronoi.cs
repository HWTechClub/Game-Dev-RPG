using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Voronoi
{
    public static float[,] VoronoiMap(int cellCount, int width, int seed) {
        float[,] map = new float[width,width];

        Vector2[] cells = new Vector2[cellCount];

        System.Random prng = new System.Random(seed);

        for (int i = 0; i < cellCount; i++)
        {
            cells[i] = new Vector2 (prng.Next(0,width), prng.Next(0, width));
        }

        for (int x = 0; x < width; x++) {
            for (int z = 0; z < width; z++) {

                map[x, z] = 0;

                foreach (Vector2 point in cells)
                {
                    if (x == point.x && z == point.y) {
                        map[x, z] = 1;
                        break;
                    }
                }
            }
        }
        



        return map;
    } 


}
