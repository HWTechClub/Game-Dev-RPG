using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TerrainGenData {
    public Vector3Int position;
    public Vector2[] offset;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public float scale;
    public TerrainGenerator tg;

    public TerrainGenData(Vector3Int position, Vector2[] offset, int octaves, float persistance, float lacunarity, float scale, TerrainGenerator tg)
    {
        this.position = position;
        this.offset = offset;
        this.octaves = octaves;
        this.persistance = persistance;
        this.lacunarity = lacunarity;
        this.scale = scale;
        this.tg = tg;
    }
}