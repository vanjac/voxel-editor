using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct VoxelFace
{
    public Material material;
    public Material overlay;
    public byte orientation;
    public bool addSelected, storedSelected;

    public bool IsEmpty()
    {
        return material == null && overlay == null;
    }

    public void Clear()
    {
        material = null;
        overlay = null;
        orientation = 0;
        addSelected = false;
        storedSelected = false;
    }

    public static int GetOrientationRotation(byte orientation)
    {
        return orientation & 3;
    }

    public static bool GetOrientationMirror(byte orientation)
    {
        return (orientation & 4) != 0;
    }

    public static byte Orientation(int rotation, bool mirror)
    {
        while (rotation < 0)
            rotation += 4;
        rotation %= 4;
        return (byte)(rotation + (mirror ? 4 : 0));
    }
}

public struct VoxelEdge
{
    public enum BevelType : byte
    {
        NONE, FLAT, CURVE, STAIR_1_2, STAIR_1_4, STAIR_1_8
    }
    public enum BevelSize : byte
    {
        FULL, HALF, QUARTER
    }

    private byte bevel;
    public bool addSelected, storedSelected;

    public BevelType bevelType
    {
        get
        {
            return (BevelType)(bevel & 0x0F);
        }
        set
        {
            bevel = (byte)((bevel & 0xF0) | (byte)value);
        }
    }

    public BevelSize bevelSize
    {
        get
        {
            return (BevelSize)(bevel >> 4);
        }
        set
        {
            bevel = (byte)((bevel & 0x0f) | ((byte)value << 4));
        }
    }

    public float bevelSizeFloat
    {
        get
        {
            switch (bevelSize)
            {
                case BevelSize.FULL:
                    return 1.0f;
                case BevelSize.HALF:
                    return 0.5f;
                case BevelSize.QUARTER:
                    return 0.25f;
            }
            return 0.0f;
        }
    }

    public bool hasBevel
    {
        get
        {
            return bevelType != BevelType.NONE;
        }
    }
}


public class Voxel : MonoBehaviour
{
    public static VoxelFace EMPTY_FACE = new VoxelFace();

    public static Material selectedMaterial; // set by VoxelArray instance
    public static Material xRayMaterial;
    public static Material highlightMaterial;

    // constants for generating mesh
    private readonly static Vector2[] SQUARE_LOOP = new Vector2[]
    {
        Vector2.zero, Vector2.right, Vector2.one, Vector2.up
    };

    private readonly static int[] SQUARE_LOOP_COORD_INDEX = new int[] { 0, 1, 3, 2 };

    private static readonly Vector3[] POSITIVE_S_XYZ = new Vector3[]
    {
        new Vector3(0, 0, -1), new Vector3(0, 0, 1),
        new Vector3(-1, 0, 0), new Vector3(1, 0, 0),
        new Vector3(1, 0, 0), new Vector3(-1, 0, 0)
    };

    private static readonly Vector3[] POSITIVE_T_XYZ = new Vector3[]
    {
        Vector3.up, Vector3.up,
        Vector3.forward, Vector3.forward,
        Vector3.up, Vector3.up
    };

    private static readonly Vector2[] POSITIVE_U_ST = new Vector2[]
    {
        Vector2.right, Vector2.down, Vector2.left, Vector2.up
    };

    public static Vector3 DirectionForFaceI(int faceI)
    {
        switch (faceI)
        {
            case 0:
                return Vector3.left;
            case 1:
                return Vector3.right;
            case 2:
                return Vector3.down;
            case 3:
                return Vector3.up;
            case 4:
                return Vector3.back;
            case 5:
                return Vector3.forward;
            default:
                return Vector3.zero;
        }
    }

    public static Vector3 OppositeDirectionForFaceI(int faceI)
    {
        return DirectionForFaceI(OppositeFaceI(faceI));
    }

    public static int FaceIForDirection(Vector3 direction)
    {
        if (direction == Vector3.left)
            return 0;
        else if (direction == Vector3.right)
            return 1;
        else if (direction == Vector3.down)
            return 2;
        else if (direction == Vector3.up)
            return 3;
        else if (direction == Vector3.back)
            return 4;
        else if (direction == Vector3.forward)
            return 5;
        else
            return -1;
    }

    public static int OppositeFaceI(int faceI)
    {
        return (faceI / 2) * 2 + (faceI % 2 == 0 ? 1 : 0);
    }

    public static int SideFaceI(int faceI, int sideNum)
    {
        sideNum %= 4;
        faceI = (faceI / 2) * 2 + 2 + sideNum;
        faceI %= 6;
        return faceI;
    }

    public static int FaceIAxis(int faceI)
    {
        return faceI / 2;
    }

    public static bool InEditor()
    {
        // TODO: better way to check this?
        return SceneManager.GetActiveScene().name == "editScene";
    }

    // see "Voxel Diagram.skp" for a diagram of face/edge numbers
    public VoxelFace[] faces = new VoxelFace[6]; // xMin, xMax, yMin, yMax, zMin, zMax
    // Edges: 0-3: x, 4-7: y, 8-11: z
    // Each group of four follows the pattern (0,0), (1,0), (1,1), (0,1)
    // for the Y/Z axes (0-3), Z/X axes (4-7, note order), or X/Y axes (8-11)
    public VoxelEdge[] edges = new VoxelEdge[12];
    private Substance _substance = null;
    public Substance substance
    {
        get
        {
            return _substance;
        }
        set
        {
            if (_substance != null)
                _substance.RemoveVoxel(this);
            _substance = value;
            if (_substance != null)
                _substance.AddVoxel(this);
        }
    }
    public ObjectEntity objectEntity;

    void Start ()
    {
        UpdateVoxel();
    }

    void OnBecameVisible()
    {
        if (substance != null)
            // don't use substance.component (doesn't work for clones)
            transform.parent.SendMessage("OnBecameVisible", options: SendMessageOptions.DontRequireReceiver); // for InCameraComponent
    }

    void OnBecameInvisible()
    {
        if (substance != null)
            transform.parent.SendMessage("OnBecameInvisible", options: SendMessageOptions.DontRequireReceiver); // for InCameraComponent
    }

    public Bounds GetFaceBounds(int faceI)
    {
        Bounds bounds;
        switch (faceI)
        {
            case 0:
                bounds = new Bounds(new Vector3(0, 0.5f, 0.5f), new Vector3(0, 1, 1));
                break;
            case 1:
                bounds = new Bounds(new Vector3(1, 0.5f, 0.5f), new Vector3(0, 1, 1));
                break;
            case 2:
                bounds = new Bounds(new Vector3(0.5f, 0, 0.5f), new Vector3(1, 0, 1));
                break;
            case 3:
                bounds = new Bounds(new Vector3(0.5f, 1, 0.5f), new Vector3(1, 0, 1));
                break;
            case 4:
                bounds = new Bounds(new Vector3(0.5f, 0.5f, 0), new Vector3(1, 1, 0));
                break;
            case 5:
                bounds = new Bounds(new Vector3(0.5f, 0.5f, 1), new Vector3(1, 1, 0));
                break;
            default:
                bounds = new Bounds(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, 0, 0));
                break;
        }
        bounds.center += transform.position;
        return bounds;
    }

    public Bounds GetBounds()
    {
        return new Bounds(transform.position + new Vector3(0.5f,0.5f,0.5f), Vector3.one);
    }

    public bool IsEmpty()
    {
        if (substance != null)
            return false;
        foreach (VoxelFace face in faces)
        {
            if (!face.IsEmpty())
                return false;
        }
        return true;
    }

    public bool CanBeDeleted()
    {
        return IsEmpty() && objectEntity == null;
    }

    public void Clear()
    {
        for (int faceI = 0; faceI < faces.Length; faceI++)
        {
            faces[faceI].Clear();
        }
        substance = null;
        // does NOT clear objectEntity!
    }

    public Voxel Clone()
    {
        Voxel vClone = Instantiate<Voxel>(this);
        for (int i = 0; i < 6; i++)
            vClone.faces[i] = faces[i];
        // don't add to substance
        vClone._substance = _substance;
        // don't copy objectEntity
        return vClone;
    }

    public void UpdateVoxel()
    {
        bool inEditor = InEditor();

        Material coloredHighlightMaterial = null;
        if (substance != null && substance.highlight != Color.clear)
            coloredHighlightMaterial = substance.highlightMaterial;

        bool xRay = false;
        if (substance != null && inEditor)
            xRay = substance.xRay;
        if (xRay)
            gameObject.layer = 8; // XRay layer
        else
            gameObject.layer = 0; // default

        var faceCorners = new FaceCornerVertices[6][];
        int numVertices = 0;
        int numMaterials = 0;
        for (int faceNum = 0; faceNum < 6; faceNum++)
        {
            var corners = GetFaceVertices(faceNum, numVertices);
            faceCorners[faceNum] = corners;
            numVertices += corners[0].count + corners[1].count + corners[2].count + corners[3].count;
            numMaterials += FaceMaterialCount(faces[faceNum], xRay, coloredHighlightMaterial);
        }

        var vertices = new Vector3[numVertices];
        var uvs = new Vector2[numVertices];
        var normals = new Vector3[numVertices];
        var tangents = new Vector4[numVertices];
        for (int faceNum = 0; faceNum < 6; faceNum++)
        {
            GenerateFaceVertices(faceNum, faceCorners[faceNum][0].innerRect_i, vertices, uvs, normals, tangents);
        }
        
        // according to Mesh documentation, vertices must be assigned before triangles
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.name = "Voxel Mesh";
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.tangents = tangents;
        mesh.subMeshCount = numMaterials;

        Material[] materials = new Material[numMaterials];
        numMaterials = 0;
        for (int faceNum = 0; faceNum < 6; faceNum++)
        {
            var triangles = GenerateFaceTriangles(faceNum, faceCorners[faceNum]);
            if (triangles == null)
                continue;

            foreach (Material faceMaterial in IterateFaceMaterials(faces[faceNum], xRay, coloredHighlightMaterial))
            {
                materials[numMaterials] = faceMaterial;
                mesh.SetTriangles(triangles, numMaterials);
                numMaterials++;
            }
        }

        Renderer renderer = GetComponent<Renderer>();
        renderer.materials = materials;
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (inEditor)
        {
            renderer.enabled = true;
            meshCollider.enabled = true;
            // force the collider to update. It otherwise might not since we're using the same mesh object
            // this fixes a bug where rays would pass through a voxel that used to be empty
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
            boxCollider.enabled = false;
        }
        else
        {
            boxCollider.enabled = true;
            if (substance == null && !IsEmpty()) // a wall
            {
                renderer.enabled = true;
                boxCollider.isTrigger = false;
            }
            else if (substance != null)
            {
                renderer.enabled = false;
                boxCollider.isTrigger = true;
            }
            else // probably an object
            {
                renderer.enabled = false;
                boxCollider.enabled = false;
            }
            meshCollider.sharedMesh = null;
            meshCollider.enabled = false;
        }
    } // end UpdateVoxel()

    // Utility functions for UpdateVoxel() ...

    private struct FaceCornerVertices
    {
        public int count, innerRect_i, edgeB_i, edgeC_i, bevel_i, bevel_count;
        public FaceCornerVertices(int ignored)
        {
            count = bevel_count = 0;
            innerRect_i = edgeB_i = edgeC_i = bevel_i = -1;
        }
    }

    private FaceCornerVertices[] GetFaceVertices(int faceNum, int vertexI)
    {
        var corners = new FaceCornerVertices[4] {
            new FaceCornerVertices(0), new FaceCornerVertices(0), new FaceCornerVertices(0), new FaceCornerVertices(0)};
        if (faces[faceNum].IsEmpty())
            return corners;
        for (int i = 0; i < 4; i++)
        {
            corners[i].innerRect_i = vertexI++;
            corners[i].count++;
            int edgeA, edgeB, edgeC;
            VertexEdges(faceNum, i, out edgeA, out edgeB, out edgeC);
            if (edges[edgeB].hasBevel || edges[edgeC].hasBevel)
            {
                corners[i].bevel_i = vertexI++;
                corners[i].bevel_count = 1;
                corners[i].count++;
            }
        }
        return corners;
    }


    private static float[] vertexPos = new float[3]; // reusable
    private void GenerateFaceVertices(int faceNum, int vertexI, Vector3[] vertices, Vector2[] uvs, Vector3[] normals, Vector4[] tangents)
    {
        VoxelFace face = faces[faceNum];
        if (face.IsEmpty())
            return;

        int axis = FaceIAxis(faceNum);
        Vector3 normal = DirectionForFaceI(faceNum);
        int rotation = VoxelFace.GetOrientationRotation(face.orientation);
        bool mirrored = VoxelFace.GetOrientationMirror(face.orientation);

        // ST space is always upright
        Vector3 positiveS_xyz = POSITIVE_S_XYZ[faceNum]; // positive S in XYZ space
        Vector3 positiveT_xyz = POSITIVE_T_XYZ[faceNum];

        int uRot = rotation;
        if (mirrored)
            uRot += 3;
        int vRot;
        if (!mirrored)
            vRot = uRot + 3;
        else
            vRot = uRot + 1;
        Vector2 positiveU_st = POSITIVE_U_ST[uRot % 4];
        Vector2 positiveV_st = POSITIVE_U_ST[vRot % 4];

        Vector3 positiveU_xyz = positiveS_xyz * positiveU_st.x
            + positiveT_xyz * positiveU_st.y;
        Vector3 positiveV_xyz = positiveS_xyz * positiveV_st.x
            + positiveT_xyz * positiveV_st.y;

        Vector4 tangent = new Vector4(positiveU_xyz.x, positiveU_xyz.y, positiveU_xyz.z,
            mirrored ? 1 : -1);

        // example for faceNum = 4 (z min)
        // 0 bottom left
        // 1 bottom right (+X)
        // 2 top right
        // 3 top left (+Y)
        for (int i = 0; i < 4; i++)
        {
            int edgeA, edgeB, edgeC;
            VertexEdges(faceNum, i, out edgeA, out edgeB, out edgeC);

            vertexPos[axis] = faceNum % 2;
            vertexPos[(axis + 1) % 3] = ApplyBevel(SQUARE_LOOP[i].x, edges[edgeC]);
            vertexPos[(axis + 2) % 3] = ApplyBevel(SQUARE_LOOP[i].y, edges[edgeB]);
            Vector3 vertex = new Vector3(vertexPos[0], vertexPos[1], vertexPos[2]);
            vertices[vertexI] = vertex;

            normals[vertexI] = normal;
            tangents[vertexI] = tangent;

            vertex += transform.position;
            Vector2 uv = new Vector2(
                vertex.x * positiveU_xyz.x + vertex.y * positiveU_xyz.y + vertex.z * positiveU_xyz.z,
                vertex.x * positiveV_xyz.x + vertex.y * positiveV_xyz.y + vertex.z * positiveV_xyz.z);
            uvs[vertexI] = uv;

            vertexI++;

            if (edges[edgeB].hasBevel || edges[edgeC].hasBevel)
            {
                vertexPos[axis] = ApplyBevel(faceNum % 2, edges[edgeB].hasBevel ? edges[edgeB] : edges[edgeC]);
                vertexPos[(axis + 1) % 3] = ApplyBevel(SQUARE_LOOP[i].x, edges[edgeC].hasBevel ? edges[edgeC] : edges[edgeA]);
                vertexPos[(axis + 2) % 3] = ApplyBevel(SQUARE_LOOP[i].y, edges[edgeB].hasBevel ? edges[edgeB] : edges[edgeA]);
                vertices[vertexI] = new Vector3(vertexPos[0], vertexPos[1], vertexPos[2]);
                vertexI++;
            }
        }
    }


    private int[] GenerateFaceTriangles(int faceNum, FaceCornerVertices[] vertices)
    {
        if (faces[faceNum].IsEmpty())
            return null;

        int axis = FaceIAxis(faceNum);
        int[] surroundingEdges = new int[] {
            ((axis + 1) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) * 2], // 0 - 1
            ((axis + 2) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) + 2], // 1 - 2
            ((axis + 1) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) * 2 + 1], // 2 - 3
            ((axis + 2) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2)] // 3 - 0
        };

        int triangleCount = 6;
        // for each pair of edge vertices
        foreach (int edgeI in surroundingEdges)
            if (edges[edgeI].hasBevel)
                triangleCount += 6;

        var triangles = new int[triangleCount];

        if (faceNum % 2 == 1)
        {
            triangles[0] = vertices[0].innerRect_i;
            triangles[1] = vertices[1].innerRect_i;
            triangles[2] = vertices[2].innerRect_i;
            triangles[3] = vertices[0].innerRect_i;
            triangles[4] = vertices[2].innerRect_i;
            triangles[5] = vertices[3].innerRect_i;
        }
        else
        {
            triangles[0] = vertices[0].innerRect_i;
            triangles[1] = vertices[2].innerRect_i;
            triangles[2] = vertices[1].innerRect_i;
            triangles[3] = vertices[0].innerRect_i;
            triangles[4] = vertices[3].innerRect_i;
            triangles[5] = vertices[2].innerRect_i;
        }

        triangleCount = 6;

        // for each pair of edge vertices
        for (int i = 0; i < 4; i++)
        {
            int j = (i + 1) % 4;
            if (!edges[surroundingEdges[i]].hasBevel)
                continue;
            if (faceNum % 2 == 1)
            {
                triangles[triangleCount + 0] = vertices[i].innerRect_i;
                triangles[triangleCount + 1] = vertices[i].bevel_i;
                triangles[triangleCount + 2] = vertices[j].innerRect_i;
                triangles[triangleCount + 3] = vertices[i].bevel_i;
                triangles[triangleCount + 4] = vertices[j].bevel_i;
                triangles[triangleCount + 5] = vertices[j].innerRect_i;
            }
            else
            {
                triangles[triangleCount + 0] = vertices[i].innerRect_i;
                triangles[triangleCount + 1] = vertices[j].innerRect_i;
                triangles[triangleCount + 2] = vertices[i].bevel_i;
                triangles[triangleCount + 3] = vertices[i].bevel_i;
                triangles[triangleCount + 4] = vertices[j].innerRect_i;
                triangles[triangleCount + 5] = vertices[j].bevel_i;
            }
            triangleCount += 6;
        }

        return triangles;
    }


    private int FaceMaterialCount(VoxelFace face, bool xRay, Material coloredHighlightMaterial)
    {
        if (face.IsEmpty())
            return 0;
        int count = 0;
        if (xRay)
            count++;
        else
        {
            if (face.material != null)
                count++;
            if (face.overlay != null)
                count++;
        }
        if (coloredHighlightMaterial != null)
            count++;
        if (face.addSelected || face.storedSelected)
            count++;
        return count;
    }


    private IEnumerable<Material> IterateFaceMaterials(VoxelFace face, bool xRay, Material coloredHighlightMaterial)
    {
        if (face.IsEmpty())
            yield break;
        if (xRay)
            yield return xRayMaterial;
        else
        {
            if (face.material != null)
                yield return face.material;
            if (face.overlay != null)
                yield return face.overlay;
        }
        if (coloredHighlightMaterial != null)
            yield return coloredHighlightMaterial;
        if (face.addSelected || face.storedSelected)
            yield return selectedMaterial;
    }


    private void VertexEdges(int faceNum, int vertexI, out int edgeA, out int edgeB, out int edgeC)
    {
        int axis = FaceIAxis(faceNum);
        edgeA = axis * 4 + vertexI;
        edgeB = ((axis + 1) % 3) * 4
            + SQUARE_LOOP_COORD_INDEX[(vertexI >= 2 ? 1 : 0) + (faceNum % 2) * 2];
        edgeC = ((axis + 2) % 3) * 4
            + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) + (vertexI == 1 || vertexI == 2 ? 2 : 0)];
    }

    private float ApplyBevel(float coord, VoxelEdge edge)
    {
        if (edge.hasBevel)
            return (coord - 0.5f) * (1 - edge.bevelSizeFloat * 2) + 0.5f;
        else
            return coord;
    }
}
