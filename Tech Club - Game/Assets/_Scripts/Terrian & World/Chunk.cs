﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk 
{
    public GameObject chunkObject; //The gameObject of the chunk
    Mesh mesh;                     //The 3D mesh
    MeshFilter meshFilter;         //MeshFilter Component
    MeshCollider meshCollider;     //Collider
    MeshRenderer meshRenderer;     //MeshRenderer Component

    Vector3Int chunkPosition;      //Position of this chunk

    List<Vector3> vertices = new List<Vector3>();   //The vertices of the mesh
    List<int> triangles = new List<int>();          //The triangles of the mesh

    bool smoothTerrain = true;  //Turn this on for marching cubes interpolation
    float[,,] terrainMap;       //This is the 3D height map

    //Chunk constructor
    public Chunk (Vector3Int _position, Vector3[] offset, int octaves, float persistance, float lacunarity)
    {
        //Create the object and set it up.
        chunkObject = new GameObject();
        chunkObject.name = string.Format("Chunk {0}, {1}", _position.x, _position.z);
        chunkPosition = _position;
        chunkObject.transform.position = chunkPosition;

        //Add the necessary componenets to it. THESE MIGHT BE INEFFICIENT.
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load<Material>("Materials/Terrain");

        chunkObject.layer = 8; //Set the layer to ground.

        //Create a terrain map and populate it 
        terrainMap = new float[MarchingCubesData.chunkWidth + 1, MarchingCubesData.chunkHeight + 1, MarchingCubesData.chunkWidth + 1];
        PopulateTerrainMap(offset, octaves, persistance, lacunarity);

        CreateMeshData();
    }
    
    void CreateMeshData() {
        ClearMesh();

        for (int x = 0; x < MarchingCubesData.chunkWidth; x++)
        {
            for (int y = 0; y < MarchingCubesData.chunkHeight; y++)
            {
                for (int z = 0; z < MarchingCubesData.chunkWidth; z++)
                {
                    MarchCube(new Vector3Int(x, y, z));
                }
            }
        }

        UpdateMesh();
    }

    //Using noise, this function populates the terrain map. 
    public void PopulateTerrainMap (Vector3[] offset, int octaves, float persistance, float lacunarity)
    {
        for (int x = 0; x < MarchingCubesData.chunkWidth + 1; x++)
        {
            for (int z = 0; z < MarchingCubesData.chunkWidth + 1; z++)
            {
                //These two floats make the terrain generation more interesting by creating a difference between mountains and plains.
                float regionalHeights = Mathf.PerlinNoise((x + chunkPosition.x + offset[0].x) / 1280, (z + chunkPosition.z + offset[0].z) / 1280);
                float heightPowered = Mathf.PerlinNoise((x + chunkPosition.x + offset[0].x) / 160, (z + chunkPosition.z + offset[0].z) / 160) * 3;
                

                for (int y = 0; y < MarchingCubesData.chunkHeight + 1; y++)
                {
                    float thisHeight = Noise.GetTerrainHeight(x + chunkPosition.x, y + chunkPosition.y, z + chunkPosition.z, offset, octaves, persistance, lacunarity, regionalHeights, heightPowered);

                    terrainMap[x, y, z] = (float)y - thisHeight;
                }
            }
        }
    }

    //Gets the configuration of the marching cubes combination
    int GetCubeConfiguration (float[] cube)
    {
        int configurationIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cube[i] > MarchingCubesData.terrainSurface)
                configurationIndex |= 1 << i;
        }

        return configurationIndex;
    }

    //This fills the vertices and triangles to be used in creating the mesh
    void MarchCube(Vector3Int position)
    {
        float[] cube = new float[8];
        for (int i = 0; i < 8; i++)
        {
            cube[i] = SampleTerrain(position + MarchingCubesData.corners[i]);
        }

        int configIndex = GetCubeConfiguration (cube);

        //Both cases where the block is either just air or in the terrain
        if (configIndex == 0 || configIndex == 255)
            return;
        
        //Loop throught the triangles
        int edgeIndex = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int p = 0; p < 3; p++)
            {
                int index = MarchingCubesData.trianglulationTable[configIndex, edgeIndex];

                //-1 marks the end of the edge index.
                if (index == -1)
                    return;

                //idk what this does its black magic to me
                Vector3 vertex1 = position + MarchingCubesData.corners[MarchingCubesData.edges[index, 0]];
                Vector3 vertex2 = position + MarchingCubesData.corners[MarchingCubesData.edges[index, 1]];

                Vector3 vertPosition;

                //This does the interpolation, AKA smoothening the terrain.
                if (smoothTerrain)
                {
                    float ver1 = cube[MarchingCubesData.edges[index, 0]];
                    float ver2 = cube[MarchingCubesData.edges[index, 1]];

                    float difference = ver2 - ver1;

                    if (difference == 0)
                        difference = MarchingCubesData.terrainSurface;
                    else
                        difference = (MarchingCubesData.terrainSurface - ver1) / difference;

                    vertPosition = vertex1 + ((vertex2 - vertex1) * difference);
                }
                else
                    vertPosition = (vertex1 + vertex2) / 2f;

                //Add the vertices and triangles we got to the list. Then, increment the edge index.
                vertices.Add(vertPosition);
                triangles.Add(vertices.Count - 1);
                edgeIndex++;
            }
        }
    }

    //This just samples from the Terrain map
    float SampleTerrain (Vector3Int point)
    {
        return terrainMap[point.x, point.y, point.z];
    }

    void ClearMesh()
    {
        vertices.Clear();
        triangles.Clear();
    }

    //Creates the mesh and applies it to the mesh filter and the collider
    void UpdateMesh ()
    {
        mesh = new Mesh();

        mesh.vertices = vertices.ToArray ();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }


}
