using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Chunk
{
    public GameObject chunkObject; //The gameObject of the chunk
    Mesh mesh;                     //The 3D mesh
    MeshFilter meshFilter;         //MeshFilter Component
    MeshCollider meshCollider;     //Collider
    MeshRenderer meshRenderer;     //MeshRenderer Component

    Vector3Int chunkPosition;      //Position of this chunk

    List<Vector3> vertices = new List<Vector3>();   //The vertices of the mesh including borders
    List<int> triangles = new List<int>();          //The triangles of the mesh
    List<int> bTriangles = new List<int>();          //The triangles of the mesh with borders

    bool smoothTerrain = true;  //Turn this on for marching cubes interpolation
    bool flatShaded = true;    //This is for flat shading terrain
    float[,] terrainMap;       //This is the 2D height map
    float humidity;
    float temperature;

    //Chunk constructor
    public Chunk(TerrainGenData tdg)
    {
        //Create the object and set it up.
        chunkObject = new GameObject();
        chunkObject.name = string.Format("Chunk {0}, {1}", tdg.position.x, tdg.position.z);
        chunkPosition = tdg.position;
        chunkObject.transform.position = chunkPosition;

        //Add the necessary componenets to it.
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = Resources.Load<Material>("Materials/Terrain");

        chunkObject.layer = 8; //Set the layer to ground.


        //Create a terrain map and populate it 
        terrainMap = new float[MarchingCubesData.chunkBorderWidth + 1, MarchingCubesData.chunkBorderWidth + 1];
        PopulateTerrainMap(tdg.offset, tdg.octaves, tdg.persistance, tdg.lacunarity, tdg.scale, tdg.tg);

        CreateMeshData();
    }

    public void UpdateChunk(TerrainGenData tdg) {

        //Change the object properties
        chunkObject.name = string.Format("Chunk {0}, {1}", tdg.position.x, tdg.position.z);
        chunkPosition = tdg.position;
        chunkObject.transform.position = chunkPosition;

        PopulateTerrainMap(tdg.offset, tdg.octaves, tdg.persistance, tdg.lacunarity, tdg.scale, tdg.tg);

        CreateMeshData();
    }

    void CreateMeshData() {
        ClearMesh();

        //for (int x = 0; x < MarchingCubesData.chunkWidth; x++)
        //{
        //    for (int z = 0; z < MarchingCubesData.chunkWidth; z++)
        //    {
        //        for (int y = 0; y < MarchingCubesData.chunkHeight; y++)
        //        {
        //            MarchCube(new Vector3Int(x, y, z));
        //        }
        //    }
        //}

        for (int x = 0; x < MarchingCubesData.chunkBorderWidth + 1; x++)
        {
            for (int z = 0; z < MarchingCubesData.chunkBorderWidth + 1; z++)
            {
                //if (!(x == 0 || x == MarchingCubesData.chunkBorderWidth || z == 0 || z == MarchingCubesData.chunkBorderWidth)) {
                //    vertices.Add(new Vector3(x, terrainMap[x, z], z));
                //}

                vertices.Add(new Vector3(x, terrainMap[x, z], z));
            }
        }

        int bVerts = 0;
        for (int x = 0; x < MarchingCubesData.chunkBorderWidth; x++)
        {
            for (int z = 0; z < MarchingCubesData.chunkBorderWidth; z++)
            {
                if (!(x == 0 || x == MarchingCubesData.chunkBorderWidth - 1|| z == 0 || z == MarchingCubesData.chunkBorderWidth -1))
                {
                    triangles.Add(bVerts + 2 + MarchingCubesData.chunkBorderWidth);
                    triangles.Add(bVerts + 1 + MarchingCubesData.chunkBorderWidth);
                    triangles.Add(bVerts + 1);
                    triangles.Add(bVerts + 1);
                    triangles.Add(bVerts + 1 + MarchingCubesData.chunkBorderWidth);
                    triangles.Add(bVerts);
                }

                bTriangles.Add(bVerts + 2 + MarchingCubesData.chunkBorderWidth);
                bTriangles.Add(bVerts + 1 + MarchingCubesData.chunkBorderWidth);
                bTriangles.Add(bVerts + 1);
                bTriangles.Add(bVerts + 1);
                bTriangles.Add(bVerts + 1 + MarchingCubesData.chunkBorderWidth);
                bTriangles.Add(bVerts);

                bVerts++;
            }
            bVerts++;
        }

        UpdateMesh();
    }

    //Using noise, this function populates the terrain map. 
    private void PopulateTerrainMap(Vector2[] offset, int octaves, float persistance, float lacunarity, float scale, TerrainGenerator tg)
    {
        if (scale < 0f)
            scale = 0.01f;

        for (int x = 0; x < MarchingCubesData.chunkBorderWidth + 1; x++)
        {
            for (int z = 0; z < MarchingCubesData.chunkBorderWidth + 1; z++)
            {
                //float thisHeight = Noise.GetTerrainHeight(x + chunkPosition.x, z + chunkPosition.z, tg.HeightOffest, octaves, persistance, lacunarity, scale);
                //float h1 = Noise.GetTerrainHeight(x + chunkPosition.x, z + chunkPosition.z, offset, octaves, persistance, lacunarity, scale);
                //float h2 = Noise.GetTerrainHeight((x + chunkPosition.x) * h1, (z + chunkPosition.z) * h1, offset, octaves, persistance, lacunarity, scale);

                //float thisHeight = Noise.GetTerrainHeight((x + chunkPosition.x) * h1, (z + chunkPosition.z) * h1, offset, octaves, persistance, lacunarity, scale);
                float thisHeight = Pattern((x + chunkPosition.x), (z + chunkPosition.z), offset, octaves, persistance, lacunarity, scale);
                float rivers = 0;

                terrainMap[x, z] = MarchingCubesData.heightRange - (MarchingCubesData.heightRange * thisHeight) * Noise.GetTerrainHeight(x + chunkPosition.x, z + chunkPosition.z, offset, octaves, persistance, lacunarity, scale * 5);
            }
        }

    }

    //Add details like trees, rocks, and stuff
    private void AddDetails() {

    }

    //Gets the configuration of the marching cubes combination
    int GetCubeConfiguration(float[] cube)
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
            //cube[i] = SampleTerrain(position + MarchingCubesData.corners[i]);
            cube[i] = (position + MarchingCubesData.corners[i]).y - terrainMap[(position + MarchingCubesData.corners[i]).x, (position + MarchingCubesData.corners[i]).z];
        }

        int configIndex = GetCubeConfiguration(cube);

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
                if (flatShaded)
                {
                    vertices.Add(vertPosition);
                    triangles.Add(vertices.Count - 1);
                }
                else
                    triangles.Add(VertForIndic(vertPosition));



                edgeIndex++;
            }
        }


    }

    int VertForIndic(Vector3 vert) {
        for (int i = 0; i < vertices.Count; i++) {
            if (vertices[i] == vert)
                return i;
        }

        vertices.Add(vert);
        return vertices.Count - 1;
    }

    //This just samples from the Terrain map
    float SampleTerrain(Vector3Int point)
    {
        return point.y - terrainMap[point.x, point.z];
    }

    void ClearMesh()
    {
        vertices.Clear();
        triangles.Clear();
        bTriangles.Clear();
    }


    Vector3[] CalculateNormals() {
        Vector3[] vertexNormals = new Vector3[vertices.Count];
        int triangleCount = bTriangles.Count / 3;

        for (int  i = 0; i < triangleCount; i++) {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = bTriangles[normalTriangleIndex];
            int vertexIndexB = bTriangles[normalTriangleIndex + 1];
            int vertexIndexC = bTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        for (int i = 0; i < vertexNormals.Length; i++) {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC) {
        Vector3 pointA = vertices[indexA];
        Vector3 pointB = vertices[indexB];
        Vector3 pointC = vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }


    //Creates the mesh and applies it to the mesh filter and the collider
    void UpdateMesh()
    {
        mesh = new Mesh();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.normals = CalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
    
    float pMult = 1200;

    float Pattern(int x, int z, Vector2[] octaveOffsets, int octaves, float persistance, float lacunarity, float scale) {
        Vector2 q = new Vector2(Noise.GetTerrainHeight(x, z, octaveOffsets, octaves, persistance, lacunarity, scale),
                                Noise.GetTerrainHeight(x + 5.2f, z + 1.3f, octaveOffsets, octaves, persistance, lacunarity, scale));

        Vector2 r = new Vector2(Noise.GetTerrainHeight(x + (int)(pMult * q.x) + 1.7f, z + (int)(pMult * q.y) + 9.2f, octaveOffsets, octaves, persistance, lacunarity, scale),
                                Noise.GetTerrainHeight(x + (int)(pMult * q.x) + 8.3f, z + (int)(pMult * q.y) + 2.8f, octaveOffsets, octaves, persistance, lacunarity, scale));

        return Noise.GetTerrainHeight(x + (int)(pMult * r.x), z + (int)(pMult * r.y), octaveOffsets, octaves, persistance, lacunarity, scale);
    }

    public Vector3 position {
        get {
            return chunkPosition;
        }
    }

}