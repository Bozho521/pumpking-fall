using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FisheyeBend : MonoBehaviour
{
    [Range(-1f, 1f)] public float bendAmount = 0.5f;
    // Negative = convex (bubble out), Positive = concave (fisheye in)
    public float radius = 2f; // Controls curvature radius
    public bool invert = false; // Flip bend direction

    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] deformedVertices;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        originalVertices = mesh.vertices;
        deformedVertices = new Vector3[originalVertices.Length];

        ApplyBend();
    }

    void ApplyBend()
    {
        Vector3 meshCenter = mesh.bounds.center;
        float maxDist = mesh.bounds.extents.magnitude;

        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 v = originalVertices[i];
            Vector3 localPos = v - meshCenter;

            // Get normalized distance from center (0 at center, 1 at corner)
            float dist = localPos.magnitude / maxDist;

            // Calculate curvature based on distance
            float curve = Mathf.Sin(dist * Mathf.PI * 0.5f) * bendAmount;

            // Move vertices along their normal direction
            Vector3 bent = localPos + (localPos.normalized * curve * radius * (invert ? -1f : 1f));

            deformedVertices[i] = bent + meshCenter;
        }

        mesh.vertices = deformedVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

#if UNITY_EDITOR
    // Optional: update live in editor when parameters change
    void OnValidate()
    {
        if (mesh != null && originalVertices != null)
            ApplyBend();
    }
#endif
}
