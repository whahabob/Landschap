using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {

    

	public static MeshData GenerateTerrain(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail)
    {
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
        int levelOfDetailIncrement = (levelOfDetail == 0) ? 1: levelOfDetail* 2;
        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - (2* levelOfDetailIncrement);
        int meshSizedUnsimplified = borderedSize -2;
       
        
        int verticesPerLine = (meshSize - 1) / levelOfDetailIncrement + 1;
        float topLeftX = (meshSizedUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizedUnsimplified - 1) / 2f;
        MeshData meshData = new MeshData(verticesPerLine);
        //int vertexIndex = 0;

        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

         for(int y = 0; y < borderedSize; y+= levelOfDetailIncrement)
        {
            for(int x = 0; x < borderedSize; x+= levelOfDetailIncrement)
            {
                bool isBorderVertex = y == 0 || y == borderedSize -1 || x == 0 || x == borderedSize -1;

                if(isBorderVertex)
                {
                    vertexIndicesMap[x,y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x,y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for(int y = 0; y < borderedSize; y+= levelOfDetailIncrement)
        {
            for(int x = 0; x < borderedSize; x+= levelOfDetailIncrement)
            {
                int vertexIndex = vertexIndicesMap[x,y];
                
                Vector2 percent = new Vector2((x-levelOfDetailIncrement) / (float)meshSize, (y-levelOfDetailIncrement) / (float)meshSize);
                float height = heightCurve.Evaluate(heightMap[x,y]) * heightMultiplier;
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizedUnsimplified, height , topLeftZ - percent.y * meshSizedUnsimplified);
               
                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                if(x < borderedSize -1 && y < borderedSize -1)
                {
                    int a = vertexIndicesMap[x,y];
                    int b = vertexIndicesMap[x + levelOfDetailIncrement,y];
                    int c = vertexIndicesMap[x,y+ levelOfDetailIncrement];
                    int d = vertexIndicesMap[x+ levelOfDetailIncrement,y+ levelOfDetailIncrement];

                    meshData.AddTriangles(a,d,c);
                    meshData.AddTriangles(d,a,b);
                }
                vertexIndex++;
            }
        }
        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    Vector3[] borderVertices;
    int[] borderTriangles;

    int triangleIndex = 0;
    int borderTriangleIndex = 0;

    public MeshData(int verticesPerLine)
    {
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        uvs = new Vector2[verticesPerLine * verticesPerLine];
        triangles = new int[((verticesPerLine - 1) * (verticesPerLine - 1)) * 6];

        borderVertices = new Vector3[verticesPerLine*4+4];
        borderTriangles = new int[verticesPerLine*24];

    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if(vertexIndex < 0)
        {
            borderVertices[-vertexIndex -1] = vertexPosition;
        } else
        {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }
    public void AddTriangles(int a, int b, int c)
    {
        if(a < 0 || b < 0 || c < 0)
        {
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;
            borderTriangleIndex += 3;

        }else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;

            triangleIndex += 3;
        }
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length/3;
        for(int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int VertexIndexA = triangles[normalTriangleIndex];
            int VertexIndexB = triangles[normalTriangleIndex + 1];
            int VertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromTriangle(VertexIndexA,VertexIndexB,VertexIndexC);
            vertexNormals[VertexIndexA] += triangleNormal;
            vertexNormals[VertexIndexB] += triangleNormal;
            vertexNormals[VertexIndexC] += triangleNormal;
        }

         int borderTriangleCount = borderTriangles.Length/3;
        for(int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int VertexIndexA = borderTriangles[normalTriangleIndex];
            int VertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int VertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromTriangle(VertexIndexA,VertexIndexB,VertexIndexC);
            if(VertexIndexA >= 0)
            vertexNormals[VertexIndexA] += triangleNormal;
            if(VertexIndexB >= 0)
            vertexNormals[VertexIndexB] += triangleNormal;
            if(VertexIndexC >= 0)
            vertexNormals[VertexIndexC] += triangleNormal;
        }

        for(int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }      
        return vertexNormals;
    }

    Vector3 SurfaceNormalFromTriangle(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0)? borderVertices[-indexA -1] : vertices[indexA];
        Vector3 pointB = (indexB < 0)? borderVertices[-indexB -1] : vertices[indexB];
        Vector3 pointC = (indexC < 0)? borderVertices[-indexC -1] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB,sideAC).normalized;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = CalculateNormals();
        return mesh;
    }
}
