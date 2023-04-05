using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GrassMeshBuilder
{
    public static void CreateGrass(Mesh mesh, int count, float radius, float width, float widthVariation, float heightVariation, int resolution)
    {
        CombineInstance[] grassInstances = new CombineInstance[count];
        for (int i = 0; i < count; ++i)
        {
            Vector2 r = Random.insideUnitCircle * radius;
            Matrix4x4 t = Matrix4x4.Translate(new Vector3(r.x, 0, r.y));
            t *= Matrix4x4.Scale(new Vector3(width * Random.Range(1, 1 + widthVariation), Random.Range(1, 1 + heightVariation), 1));
            t *= Matrix4x4.Rotate(Quaternion.Euler(0, Random.Range(0, 360), 0));

            CombineInstance grassInstance = new CombineInstance();
            grassInstance.mesh = GeneratePlane(resolution);
            grassInstance.transform = t;

            grassInstances[i] = grassInstance;
        }

        mesh.CombineMeshes(grassInstances);
    }

    private static Mesh GeneratePlane(int resolution)
    {
        // Calculate the offsets for each resolution step
        float xPerStep = 1f / resolution;
        float yPerStep = 1f / resolution;

        // A 2x2 grid of quads requires a 3x3 grid of vertices
        int numVerts = (resolution + 1) * (resolution + 1);

        // Use arrays instead of lists. Since we know the size of all of our data in advance, we allocate the exact amount of memory we need
        Vector3[] verts = new Vector3[numVerts];
        Vector3[] normals = new Vector3[numVerts];
        Vector2[] uvs = new Vector2[numVerts];
        Color32[] colors = new Color32[numVerts];

        for (int row = 0; row <= resolution; row++)
        {
            for (int col = 0; col <= resolution; col++)
            {
                int i = (row * resolution) + row + col;

                verts[i] = new Vector3(col * xPerStep, row * yPerStep, 0);
                uvs[i] = new Vector2(col, row) / resolution;
                normals[i] = Vector3.Lerp(Vector3.up, Vector3.back, ((float)row / (float)resolution) * 0.5f);
                colors[i] = new Color32(0, 0, 0, 255);
            }
        }

        int triIndex = 0;
        int[] tris = new int[resolution * resolution * 2 * 3];
        for (int row = 0; row < resolution; row++)
        {
            for (int col = 0; col < resolution; col++)
            {
                int i = (row * resolution) + row + col;

                tris[triIndex + 0] = i;
                tris[triIndex + 1] = i + resolution + 1;
                tris[triIndex + 2] = i + resolution + 2;

                tris[triIndex + 3] = i;
                tris[triIndex + 4] = i + resolution + 2;
                tris[triIndex + 5] = i + 1;

                triIndex += 6;
            }
        }

        Mesh quadMesh = new Mesh();
        quadMesh.SetVertices(verts);
        quadMesh.SetNormals(normals);
        quadMesh.SetUVs(0, uvs);
        quadMesh.SetColors(colors);
        quadMesh.SetTriangles(tris, 0);

        return quadMesh;
    }
}