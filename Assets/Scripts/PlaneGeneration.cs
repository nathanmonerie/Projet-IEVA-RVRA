using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
using System;
using System.Collections;

/// <summary>
/// Generates a 1x1 plane (XZ, facing up) subdivided into N x N quads,
/// ready for procedural vertex deformation.
/// Attach to an empty GameObject. MeshFilter/MeshRenderer are added automatically.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

[ExecuteInEditMode]
public class PlaneGeneration : MonoBehaviour
{
    [HideInInspector] public int subdivisions = 256;   // quads per side
    private float planeSize = 2f;     // plane width/length

    [HideInInspector] public float amplitude = 1f;
    [HideInInspector] public float frequency = 1f;

    [Space]

    [HideInInspector] public float warpAmplitude = 1f;
    [HideInInspector] public float warpFrequency = 1f;

    [Space]

    [HideInInspector] public float lacunarity = 2f;
    [HideInInspector] public float persistance = 2f;

    [Space]

    [HideInInspector] public int nbOctaves = 5;

    [Space]

    Mesh mesh;
    Vector3[] vertices;

    private MeshFilter meshFilter;

    Vector2 center = Vector2.zero;

    private float rand1;
    private float rand2;
    private float rand3;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();

        Generate();
        RNG();

        Deform();
    }

    [ContextMenu("RNG")]
    public void RNG()
    {
        rand1 = UnityEngine.Random.Range(0f, 10f);
        rand2 = UnityEngine.Random.Range(0f, 100f);
        rand3 = UnityEngine.Random.Range(0f, 10000f);
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        int n = subdivisions;
        int vertsPerSide = n + 1;
        int vertCount = vertsPerSide * vertsPerSide;

        vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[n * n * 6];

        float step = planeSize / n;
        float half = planeSize * 0.5f;

        // Vertices + UVs (centered on the origin)
        for (int z = 0, i = 0; z < vertsPerSide; z++)
        {
            for (int x = 0; x < vertsPerSide; x++, i++)
            {
                vertices[i] = new Vector3(x * step - half, 0f, z * step - half);
                uvs[i] = new Vector2((float)x / n, (float)z / n);
            }
        }

        // Triangles (clockwise winding = front face in Unity, normal pointing +Y)
        for (int z = 0, t = 0; z < n; z++)
        {
            for (int x = 0; x < n; x++)
            {
                int a = z * vertsPerSide + x;   // bottom-left
                int b = a + vertsPerSide;       // top-left
                int c = a + 1;                  // bottom-right
                int d = b + 1;                  // top-right

                triangles[t++] = a; triangles[t++] = b; triangles[t++] = c;
                triangles[t++] = c; triangles[t++] = b; triangles[t++] = d;
            }
        }

        if (mesh == null)
        {
            mesh = new Mesh
            {
                name = "Moutains Plane",
                indexFormat = IndexFormat.UInt32 // safe for higher subdivision counts
            };
            mesh.MarkDynamic();                    // optimised for frequent vertex updates
        }
        else mesh.Clear();

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
    }

    /// <summary>Example procedural deformation using Perlin noise.</summary>
    public void Deform()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];

            v.y = 0;

            float f = frequency;
            float a = amplitude;

            float wf = warpFrequency;
            float wa = warpAmplitude;

            float xWarp = ValueNoise(wf * v.x, wf * v.z) * wa;
            float zWarp = ValueNoise(wf * v.z, wf * v.x) * wa;

            for (int j = 0; j < nbOctaves; j++)
            {
                v.y += ValueNoise(f * (v.x + xWarp), f * (v.z + zWarp) ) * a;
                v.y *= Mathf.Log(1 + Vector2.Distance(new Vector2(v.x, v.z), center) / (planeSize / 2));

                f *= lacunarity;
                a /= persistance;
            }

           vertices[i] = v;
        }
        ApplyVertices();
    }

    /// <summary>Call after modifying the 'vertices' array.</summary>
    public void ApplyVertices()
    {
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public float WhiteNoise(float x, float z)
    {
        return math.frac(Mathf.Sin(x * rand1 + z * rand2) * rand3);
    }

    public float ValueNoise(float x, float z)
    {
        float i = math.floor(x);
        float j = math.floor(z);

        float y0 = WhiteNoise(i, j);
        float y1 = WhiteNoise(i + 1, j);
        float y2 = WhiteNoise(i, j + 1);
        float y3 = WhiteNoise(i + 1, j + 1);

        float tx = math.frac(x);
        float tz = math.frac(z);

        float txs = -2 * math.pow(tx, 3) + 3 * math.pow(tx, 2);
        float tzs = -2 * math.pow(tz, 3) + 3 * math.pow(tz, 2);

        float y01 = math.lerp(y0, y1, txs);
        float y23 = math.lerp(y2, y3, txs);

        float y = math.lerp(y01, y23, tzs);

        return y;
    }
    public Vector3[] Vertices => vertices;

    public void SetAmplitude(float val)
    {
        amplitude = val;
        Deform();
    }

    public void SetFrequency(float val)
    {
        frequency = val;
        Deform();
    }

    public void SetWarpAmplitude(float val)
    {
        warpAmplitude = val;
        Deform();
    }

    public void SetWarpFrequency(float val)
    {
        warpFrequency = val;
        Deform();
    }

    public void SetLacunarity(float val)
    {
        lacunarity = val;
        Deform();
    }

    public void SetPersistance(float val)
    {
        persistance = val;
        Deform();
    }

    public void SetNbOctaves(float val)
    {
        nbOctaves = Mathf.RoundToInt(val);
        Deform();
    }
}