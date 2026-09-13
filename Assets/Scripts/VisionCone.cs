using UnityEngine;

public class VisionCone : MonoBehaviour {
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private int _FOV;
    [SerializeField] private int _rayCount = 50;
    private MeshFilter _meshFilter;

    private Mesh _mesh;

    private Vector3 _originPos;
    private float _viewDistance;
    private float _startingAngle;

    void Awake() {
        this._mesh = new();
        this._meshFilter = this.GetComponent<MeshFilter>();
        this._meshFilter.mesh = this._mesh;
    }

    void LateUpdate() {
        if (!PlayerManager.instance.Player) return;

        // angle used to get the direction to beam a ray at from the origin to viewDistance
        var angle = this._startingAngle;
        // icncreases the angle by this value to change the direction to beam a ray at
        var angleIncrements = this._FOV / (float) this._rayCount;
          // + 1 for origin, + 1 for ray at startingAngle?
        Vector3[] vertices = new Vector3[this._rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[this._rayCount * 3]; // 3 rayCounts/(vertices?) for 1 triangle (3 sides)

        // Vertex uses local space so this is positioned at (0, 0) from Henchman's center
        vertices[0] = Vector3.zero;

        var vertexIndex = 1; // index 0 already accounted for above
        var triangleIndex = 0;
        for (var i = 0; i <= this._rayCount; ++i) {
            Vector3 vertex;
            RaycastHit2D raycastHit2D;

            Vector3 direction = GetDirFromAngle(angle);
            raycastHit2D = Physics2D.Raycast(this._originPos, direction, this._viewDistance, this._layerMask);
            if (raycastHit2D.collider == null) { // No hit
                // vertex/part of the ray still beams normally
                vertex = direction * this._viewDistance; // local-space offset
            } else { // Hit "Platform"
                // vertex/part of the ray stops at the point it collided
                // *Since raycast uses world-space and vertex uses local space, this converts
                // raycast's world space point to the local space relative to this gameObject*
                vertex = this.transform.InverseTransformPoint(raycastHit2D.point);
            }
            vertices[vertexIndex] = vertex;

            if (i > 0) {
                // A triangle forms a connection from the origin to prev vertex to current vertex
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1; // prev
                triangles[triangleIndex + 2] = vertexIndex; // curr

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrements;
        }

        this._mesh.vertices = vertices;
        this._mesh.uv = uv;
        this._mesh.triangles = triangles;
    }

    private static Vector3 GetDirFromAngle(float angle) {
        var angleRad = angle * Mathf.Deg2Rad; // Mathf.Cos/Sin's param must be in radians
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public void SetOriginPos(Vector3 origin) {
        this._originPos = origin;
        this.transform.position = this._originPos;
    }

    public void SetViewDistance(float viewDist) {
        this._viewDistance = viewDist;
    }

    public void SetStartingAngle(float angle) {
        // Assuming Henchman is facing at dir Vector2.left (-1, 0),
        // angle = 180, therefore startingAngle = 180 + (90/2) = 225
        // endAngle would then be startingAngle (225) + FOV (90) = 315
        this._startingAngle = angle + this._FOV / 2f;
    }
}
