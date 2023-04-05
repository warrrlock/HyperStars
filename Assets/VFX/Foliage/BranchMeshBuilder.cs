using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Branch
{
    public Matrix4x4 transform;
    public float length;
    public float bRadius;
    public float tRadius;
    public int numRadialSegments;
    public int numHeightSegments;

    public Branch(Matrix4x4 transform, float length, float bRadius, float tRadius, int numRadialSegments = 6, int numHeightSegments = 1)
    {
        this.transform = transform;
        this.length = length;
        this.bRadius = bRadius;
        this.tRadius = tRadius;
        this.numRadialSegments = numRadialSegments;
        this.numHeightSegments = numHeightSegments;
    }
};

public static class BranchMeshBuilder
{
    public static CombineInstance CreateBranch(Branch branchData)
    {
        int slices = branchData.numRadialSegments;
        int stacks = branchData.numHeightSegments;
        float length = branchData.length;
        float bottomRadius = branchData.bRadius;
        float topRadius = branchData.tRadius;

        float sliceStep = (Mathf.PI * 2) / slices;
        float heightStep = length / stacks;
        float radiusStep = (topRadius - bottomRadius) / stacks;
        float currentHeight = -length / 2;
        int vertexCount = (stacks + 1) * slices + 2;
        int triangleCount = (stacks + 1) * slices * 2;
        int indexCount = triangleCount * 3;
        float currentRadius = bottomRadius;

        Vector3[] cylinderVertices = new Vector3[vertexCount];
        Vector3[] cylinderNormals = new Vector3[vertexCount];
        Vector2[] cylinderUVs = new Vector2[vertexCount];

        // Start at the bottom of the cylinder            
        int currentVertex = 0;
        cylinderVertices[currentVertex] = new Vector3(0, currentHeight - bottomRadius, 0);
        cylinderNormals[currentVertex] = Vector3.down;
        currentVertex++;

        // Build vertex stacks
        for (int i = 0; i <= stacks; i++)
        {
            float sliceAngle = 0;
            for (int j = 0; j < slices; j++)
            {
                float x = currentRadius * Mathf.Cos(sliceAngle);
                float y = currentHeight;
                float z = currentRadius * Mathf.Sin(sliceAngle);

                Vector3 position = new Vector3(x, y, z);
                Vector3 nPos = position.normalized;
                cylinderVertices[currentVertex] = position;
                cylinderNormals[currentVertex] = new Vector3(x, 0, z).normalized;
                cylinderUVs[currentVertex] = new Vector2(
                    Mathf.Sin(sliceAngle) * 0.5f + 1, currentHeight);

                currentVertex++;

                sliceAngle += sliceStep;
            }

            currentHeight += heightStep;
            currentRadius += radiusStep;
        }

        cylinderVertices[currentVertex] = new Vector3(0, (length / 2) + topRadius, 0);
        cylinderNormals[currentVertex] = Vector3.up;
        currentVertex++;

        int[] indices = new int[indexCount];
        int currentIndex = 0;

        // Bottom circle
        for (int i = 1; i <= slices; i++)
        {
            indices[currentIndex++] = i;
            indices[currentIndex++] = 0;
            if (i - 1 == 0)
                indices[currentIndex++] = i + slices - 1;
            else
                indices[currentIndex++] = i - 1;
        }

        // Middle sides of shape
        for (int i = 1; i < vertexCount - slices - 1; i++)
        {
            indices[currentIndex++] = i + slices;
            indices[currentIndex++] = i;
            if ((i - 1) % slices == 0)
                indices[currentIndex++] = i + slices + slices - 1;
            else
                indices[currentIndex++] = i + slices - 1;

            indices[currentIndex++] = i;
            if ((i - 1) % slices == 0)
                indices[currentIndex++] = i + slices - 1;
            else
                indices[currentIndex++] = i - 1;
            if ((i - 1) % slices == 0)
                indices[currentIndex++] = i + slices + slices - 1;
            else
                indices[currentIndex++] = i + slices - 1;
        }

        // Top circle
        for (int i = vertexCount - slices - 1; i < vertexCount - 1; i++)
        {
            indices[currentIndex++] = i;
            if ((i - 1) % slices == 0)
                indices[currentIndex++] = i + slices - 1;
            else
                indices[currentIndex++] = i - 1;
            indices[currentIndex++] = vertexCount - 1;
        }

        Mesh branchMesh = new Mesh();
        branchMesh.SetVertices(cylinderVertices);
        branchMesh.SetNormals(cylinderNormals);
        branchMesh.SetUVs(0, cylinderUVs);
        branchMesh.SetTriangles(indices, 0);

        // Move origin to bottom
        Matrix4x4 t = branchData.transform;
        t = t * Matrix4x4.Translate(Vector3.down * branchData.length * 0.5f);

        // Return the info required to combine this branch into a single mesh
        CombineInstance branchInstance = new CombineInstance();
        branchInstance.mesh = branchMesh;
        branchInstance.transform = t;
        return branchInstance;
    }
}
