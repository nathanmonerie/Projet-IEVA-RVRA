using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider), typeof(PlaneGeneration))]
public class ColliderGeneration : MonoBehaviour
{

    public MeshFilter sourceMeshFilter;
    public Transform playerTransform;
    private MeshCollider localCollider;
    private Vector3 lastBakePosition;

    void Start()
    {
        localCollider = GetComponent<MeshCollider>();
        UpdatePlayerCollision();
    }

    void Update()
    {
        //// Re-slice only if player moved more than 5 meters to save performance
        //if (Vector3.Distance(playerTransform.position, lastBakePosition) > 5f)
        //{
        //    UpdatePlayerCollision();
        //}
        UpdatePlayerCollision();

        Debug.Log(playerTransform.position);
    }

    void UpdatePlayerCollision()
    {
        lastBakePosition = playerTransform.position;

        // Extract a 15-meter bubble of mesh data around the player
        Mesh isolatedMesh = CopyMeshPortion(sourceMeshFilter.sharedMesh, lastBakePosition, 15f);

        // Assign to physics
        localCollider.sharedMesh = isolatedMesh;
        localCollider.convex = true; // Safe! The sub-mesh will easily be under 256 polys
    }

    /// <summary>
    /// Copies a spherical portion of a source mesh into a destination mesh.
    /// </summary>
    /// <param name="sourceMesh">The massive master mesh.</param>
    /// <param name="center">The center of the copy zone (e.g., Player position).</param>
    /// <param name="radius">How far around the center to copy.</param>
    /// <returns>A new, isolated lightweight Mesh.</returns>
    public Mesh CopyMeshPortion(Mesh sourceMesh, Vector3 center, float radius)
    {
        // 1. Get all original data
        Vector3[] srcVertices = sourceMesh.vertices;
        int[] srcTriangles = sourceMesh.triangles;
        Vector2[] srcUvs = sourceMesh.uv;

        // Tracks: [Original Vertex Index] -> [New Extracted Vertex Index]
        Dictionary<int, int> indexMap = new();

        List<Vector3> newVertices = new();
        List<int> newTriangles = new();
        List<Vector2> newUvs = new();

        // 2. Loop through triangles in steps of 3 (each triangle has 3 corners)
        for (int i = 0; i < srcTriangles.Length; i += 3)
        {
            int idx0 = srcTriangles[i];
            int idx1 = srcTriangles[i + 1];
            int idx2 = srcTriangles[i + 2];

            Vector3 v0 = srcVertices[idx0];
            Vector3 v1 = srcVertices[idx1];
            Vector3 v2 = srcVertices[idx2];

            // Transform local mesh positions to World Position if this script 
            // runs on a moved GameObject. (Skip transform if sourceMesh is already in world space)
            Vector3 w0 = transform.TransformPoint(v0);
            Vector3 w1 = transform.TransformPoint(v1);
            Vector3 w2 = transform.TransformPoint(v2);

            // 3. Condition: Keep the triangle if AT LEAST ONE corner is near the center
            if (Vector3.Distance(w0, center) <= radius ||
                Vector3.Distance(w1, center) <= radius ||
                Vector3.Distance(w2, center) <= radius)
            {
                // Process each of the 3 corners and map them to their new positions
                newTriangles.Add(GetOrAddVertex(idx0, srcVertices, srcUvs, indexMap, newVertices, newUvs));
                newTriangles.Add(GetOrAddVertex(idx1, srcVertices, srcUvs, indexMap, newVertices, newUvs));
                newTriangles.Add(GetOrAddVertex(idx2, srcVertices, srcUvs, indexMap, newVertices, newUvs));
            }
        }

        // 4. Create the new extracted mesh
        Mesh partialMesh = new Mesh();
        partialMesh.name = "Extracted_Local_Mesh";
        partialMesh.vertices = newVertices.ToArray();
        partialMesh.triangles = newTriangles.ToArray();

        if (srcUvs.Length > 0)
            partialMesh.uv = newUvs.ToArray();

        // Automatically regenerate physics and lighting directions
        partialMesh.RecalculateNormals();
        partialMesh.RecalculateBounds();

        return partialMesh;
    }

    // Helper method to ensure vertices aren't duplicated and indexes match up perfectly
    private int GetOrAddVertex(int oldIndex, Vector3[] srcVerts, Vector2[] srcUvs,
                               Dictionary<int, int> map, List<Vector3> newVerts, List<Vector2> newUvs)
    {
        if (map.TryGetValue(oldIndex, out int newIndex))
        {
            return newIndex; // Vertex already copied, return its new index
        }

        // Vertex not copied yet. Add it to our new lists!
        int assignedIndex = newVerts.Count;
        newVerts.Add(srcVerts[oldIndex]);

        if (srcUvs.Length > 0)
            newUvs.Add(srcUvs[oldIndex]);

        map.Add(oldIndex, assignedIndex);
        return assignedIndex;
    }
}
