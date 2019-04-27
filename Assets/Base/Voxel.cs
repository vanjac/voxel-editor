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
        NONE, FLAT, CURVE, SQUARE, STAIR_2, STAIR_4
    }
    public enum BevelSize : byte
    {
        QUARTER, HALF, FULL
    }

    // each bevel shape starts at (1, 0) and goes to the y=x diagonal
    public static readonly Vector2[] SHAPE_SQUARE = new Vector2[] {
        new Vector2(1.0f, 0.0f), new Vector2(0.0f, 0.0f) };
    public static readonly Vector2[] SHAPE_FLAT = new Vector2[] {
        new Vector2(1.0f, 0.0f), new Vector2(0.5f, 0.5f) };
    public static readonly Vector2[] SHAPE_CURVE = new Vector2[] {
        new Vector2(1.0f, 0.0f), new Vector2(0.96592f, 0.25881f), new Vector2(0.86602f, 0.5f), new Vector2(0.70710f, 0.70710f) };
    public static readonly Vector2[] SHAPE_STAIR_2 = new Vector2[] {
        new Vector2(1.0f, 0.0f), new Vector2(0.5f, 0.0f), new Vector2(0.5f, 0.5f) };
    public static readonly Vector2[] SHAPE_STAIR_4 = new Vector2[] {
        new Vector2(1.0f, 0.0f), new Vector2(0.75f, 0.0f), new Vector2(0.75f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f) };

    // a pair of normals for each line segment connecting 2 vertices
    public static readonly Vector2[] NORMALS_SQUARE = new Vector2[] {
        new Vector2(0.0f, 1.0f), new Vector2(0.0f, 1.0f) };
    public static readonly Vector2[] NORMALS_FLAT = new Vector2[] {
        new Vector2(0.70710f, 0.70710f), new Vector2(0.70710f, 0.70710f) };
    public static readonly Vector2[] NORMALS_CURVE = new Vector2[] {
        new Vector2(1.0f, 0.0f), new Vector2(0.96592f, 0.25881f), new Vector2(0.96592f, 0.25881f),
        new Vector2(0.86602f, 0.5f), new Vector2(0.86602f, 0.5f), new Vector2(0.70710f, 0.70710f) };
    public static readonly Vector2[] NORMALS_STAIR_2 = new Vector2[] {
        new Vector2(0.0f, 1.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 0.0f) };
    public static readonly Vector2[] NORMALS_STAIR_4 = new Vector2[] {
        new Vector2(0.0f, 1.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 0.0f),
        new Vector2(0.0f, 1.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 0.0f) };

    public byte bevel;
    public bool addSelected, storedSelected;

    public void Clear()
    {
        bevel = 0;
        addSelected = storedSelected = false;
    }

    public BevelType bevelType
    {
        get
        {
            return (BevelType)(bevel & 0x07);
        }
        set
        {
            bevel = (byte)((bevel & 0xF8) | (byte)value);
        }
    }

    public BevelSize bevelSize
    {
        get
        {
            return (BevelSize)((bevel >> 4) & 0x07);
        }
        set
        {
            bevel = (byte)((bevel & 0x8f) | ((byte)value << 4));
        }
    }

    public bool capMin
    {
        get
        {
            return (bevel & 0x08) != 0;
        }
        set
        {
            bevel = (byte)((bevel & 0xF7) | (value ? 0x08 : 0));
        }
    }

    public bool capMax
    {
        get
        {
            return (bevel & 0x80) != 0;
        }
        set
        {
            bevel = (byte)((bevel & 0x7F) | (value ? 0x80 : 0));
        }
    }

    public Vector2[] bevelTypeArray
    {
        get
        {
            switch (bevelType)
            {
                case BevelType.SQUARE:
                    return SHAPE_SQUARE;
                case BevelType.FLAT:
                    return SHAPE_FLAT;
                case BevelType.CURVE:
                    return SHAPE_CURVE;
                case BevelType.STAIR_2:
                    return SHAPE_STAIR_2;
                case BevelType.STAIR_4:
                    return SHAPE_STAIR_4;
            }
            return null;
        }
    }

    public Vector2[] bevelTypeNormalArray
    {
        get
        {
            switch (bevelType)
            {
                case BevelType.SQUARE:
                    return NORMALS_SQUARE;
                case BevelType.FLAT:
                    return NORMALS_FLAT;
                case BevelType.CURVE:
                    return NORMALS_CURVE;
                case BevelType.STAIR_2:
                    return NORMALS_STAIR_2;
                case BevelType.STAIR_4:
                    return NORMALS_STAIR_4;
            }
            return null;
        }
    }

    public float bevelSizeFloat
    {
        get
        {
            switch (bevelSize)
            {
                case BevelSize.QUARTER:
                    return 0.25f;
                case BevelSize.HALF:
                    return 0.5f;
                case BevelSize.FULL:
                    return 1.0f;
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

    public static Material selectedMaterial; // set by VoxelArrayEditor instance
    public static Material xRayMaterial;
    public static Material[] highlightMaterials;

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

    public static void EdgeFaces(int edgeI, out int faceA, out int faceB)
    {
        int axis = EdgeIAxis(edgeI);
        faceA = ((axis + 1) % 3) * 2;
        faceB = ((axis + 2) % 3) * 2;
        edgeI %= 4;
        if (edgeI == 1 || edgeI == 2)
            faceA += 1;
        if (edgeI >= 2)
            faceB += 1;
    }

    public static System.Collections.Generic.IEnumerable<int> ConnectedEdges(int edgeI)
    {
        int axis = EdgeIAxis(edgeI);
        edgeI %= 4;
        if (edgeI >= 2)
        {
            yield return ((axis + 1) % 3) * 4 + 1;
            yield return ((axis + 1) % 3) * 4 + 2;
        }
        else
        {
            yield return ((axis + 1) % 3) * 4 + 0;
            yield return ((axis + 1) % 3) * 4 + 3;
        }
        if (edgeI == 1 || edgeI == 2)
        {
            yield return ((axis + 2) % 3) * 4 + 2;
            yield return ((axis + 2) % 3) * 4 + 3;
        }
        else
        {
            yield return ((axis + 2) % 3) * 4 + 0;
            yield return ((axis + 2) % 3) * 4 + 1;
        }
    }

    public static System.Collections.Generic.IEnumerable<int> UnconnectedEdges(int edgeI)
    {
        int axis = EdgeIAxis(edgeI);
        for (int i = 1; i < 4; i++)
            yield return axis * 4 + ((edgeI + i) % 4);
        edgeI %= 4;
        if (edgeI >= 2)
        {
            yield return ((axis + 1) % 3) * 4 + 0;
            yield return ((axis + 1) % 3) * 4 + 3;
        }
        else
        {
            yield return ((axis + 1) % 3) * 4 + 1;
            yield return ((axis + 1) % 3) * 4 + 2;
        }
        if (edgeI == 1 || edgeI == 2)
        {
            yield return ((axis + 2) % 3) * 4 + 0;
            yield return ((axis + 2) % 3) * 4 + 1;
        }
        else
        {
            yield return ((axis + 2) % 3) * 4 + 2;
            yield return ((axis + 2) % 3) * 4 + 3;
        }
    }

    public static int FaceIAxis(int faceI)
    {
        return faceI / 2;
    }

    public static int EdgeIAxis(int edgeI)
    {
        return edgeI / 4;
    }

    public static System.Collections.Generic.IEnumerable<int> FaceSurroundingEdges(int faceNum)
    {
        int axis = FaceIAxis(faceNum);
        yield return ((axis + 1) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) * 2]; // 0 - 1
        yield return ((axis + 2) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) + 2]; // 1 - 2
        yield return ((axis + 1) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) * 2 + 1]; // 2 - 3
        yield return ((axis + 2) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2)]; // 3 - 0
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
    public byte[] faceSubMeshes = new byte[6];

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

    public Bounds GetEdgeBounds(int edgeI)
    {
        Vector3 center = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 size = new Vector3(0, 0, 0);
        if (edgeI >= 0 && edgeI < 4)
            size = new Vector3(1, 0, 0);
        else if (edgeI >= 4 && edgeI < 8)
            size = new Vector3(0, 1, 0);
        else if (edgeI >= 8 && edgeI < 12)
            size = new Vector3(0, 0, 1);
        if (edgeI == 4 || edgeI == 5 || edgeI == 8 || edgeI == 11)
            center.x = 0;
        else if (edgeI == 6 || edgeI == 7 || edgeI == 9 || edgeI == 10)
            center.x = 1;
        if (edgeI == 0 || edgeI == 3 || edgeI == 8 || edgeI == 9)
            center.y = 0;
        else if (edgeI == 1 || edgeI == 2 || edgeI == 10 || edgeI == 11)
            center.y = 1;
        if (edgeI == 0 || edgeI == 1 || edgeI == 4 || edgeI == 7)
            center.z = 0;
        else if (edgeI == 2 || edgeI == 3 || edgeI == 5 || edgeI == 6)
            center.z = 1;
        return new Bounds(center + transform.position, size);
    }

    public Bounds GetBounds()
    {
        return new Bounds(transform.position + new Vector3(0.5f, 0.5f, 0.5f), Vector3.one);
    }

    public bool EdgeIsEmpty(int edgeI)
    {
        int faceA, faceB;
        EdgeFaces(edgeI, out faceA, out faceB);
        return faces[faceA].IsEmpty() && faces[faceB].IsEmpty();
    }

    public bool EdgeIsConvex(int edgeI)
    {
        int faceA, faceB;
        EdgeFaces(edgeI, out faceA, out faceB);
        return !faces[faceA].IsEmpty() && !faces[faceB].IsEmpty();
    }

    public int FaceTransformedEdgeNum(int faceNum, int edgeI)
    {
        int n = edgeI + 1;
        if (faceNum % 2 == 1)
            n = 4 - (n % 4);
        if (faceNum == 4)
            n += 3;
        if (faceNum == 5)
            n += 1;
        n += VoxelFace.GetOrientationRotation(faces[faceNum].orientation);
        if (VoxelFace.GetOrientationMirror(faces[faceNum].orientation))
            n = 3 - (n % 4);
        return n % 4;
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
        for (int edgeI = 0; edgeI < edges.Length; edgeI++)
        {
            edges[edgeI].Clear();
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
        bool inEditor = VoxelArrayEditor.instance != null;

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
            faceSubMeshes[faceNum] = (byte)numMaterials;
            numMaterials += FaceMaterialCount(faceNum, xRay, coloredHighlightMaterial);
        }

        var vertices = new Vector3[numVertices];
        var uvs = new Vector2[numVertices];
        var normals = new Vector3[numVertices];
        var tangents = new Vector4[numVertices];
        for (int faceNum = 0; faceNum < 6; faceNum++)
        {
            try
            {
                GenerateFaceVertices(faceNum, faceCorners[faceNum], vertices, uvs, normals, tangents);
            }
            catch (System.IndexOutOfRangeException)
            {
                Debug.LogError("Vertex indices don't match!");
                return;
            }
        }

        // according to Mesh documentation, vertices must be assigned before triangles
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.name = "v";
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
            int[] triangles;
            try
            {
                triangles = GenerateFaceTriangles(faceNum, faceCorners[faceNum]);
            }
            catch (System.IndexOutOfRangeException)
            {
                Debug.LogError("Vertex indices don't match!");
                return;
            }
            if (triangles == null)
                continue;

            foreach (Material faceMaterial in IterateFaceMaterials(faceNum, xRay, coloredHighlightMaterial))
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

        bool useMeshCollider; // if false, use box collider instead
        if (inEditor)
            useMeshCollider = true;
        else
        {
            useMeshCollider = false;
            foreach (VoxelEdge edge in edges)
                if (edge.hasBevel)
                    useMeshCollider = true;
        }
        Collider theCollider;
        if (useMeshCollider)
        {
            theCollider = meshCollider;
            meshCollider.enabled = true;
            // force the collider to update. It otherwise might not since we're using the same mesh object
            // this fixes a bug where rays would pass through a voxel that used to be empty
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
            boxCollider.enabled = false;

            meshCollider.convex = (!inEditor && substance != null);
        }
        else
        {
            theCollider = boxCollider;
            boxCollider.enabled = true;
            meshCollider.sharedMesh = null;
            meshCollider.enabled = false;
        }

        if (inEditor)
        {
            renderer.enabled = true;
        }
        else
        {
            if (substance != null)
            {
                renderer.enabled = false;
                theCollider.isTrigger = true;
            }
            else if (!IsEmpty()) // a wall
            {
                renderer.enabled = true;
                theCollider.isTrigger = false;
            }
            else // probably an object
            {
                renderer.enabled = false;
                theCollider.enabled = false;
            }
        }
    } // end UpdateVoxel()

    // Utility functions for UpdateVoxel() ...

    // stores indices of the mesh vertices of one corner of a face
    // 4 corners per face, 6 faces per voxel
    // see "Voxel Diagram.skp" for a diagram of mesh vertices
    private struct FaceCornerVertices
    {
        public int count; // total number of vertices

        /* on face plane */

        // All faces have an inner quad; other structures build off of it.
        // innerQuad_i of the zeroth corner is always the lowest-numbered vertex of the face.
        public int innerQuad_i;
        // "Tabs" on the edge of the quad to leave a rectangular cutout for the bevel profile
        public int edgeB_i, edgeC_i;
        // The profile of a bevel orthogonal to the face.
        // edgeC and edgeB are the first and last vertices of the profile (in that order)
        // So only the middle vertices are part of bevelProfile.
        public int bevelProfile_i, bevelProfile_count;

        /* not on face plane */

        // The first bevel vertex will be identical to innerRect but with a different normal.
        // The middle bevel vertices (not including first/last) are doubled up -- same position,
        // (potentially) different normals.
        public int bevel_i, bevel_count;

        public int cap_i, cap_count;

        public FaceCornerVertices(int ignored)
        {
            count = bevelProfile_count = bevel_count = cap_count = 0;
            innerQuad_i = edgeB_i = edgeC_i = bevelProfile_i = bevel_i = cap_i = -1;
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
            corners[i].innerQuad_i = vertexI++;
            corners[i].count++;
            int edgeA, edgeB, edgeC;
            VertexEdges(faceNum, i, out edgeA, out edgeB, out edgeC);
            if (edges[edgeB].hasBevel || edges[edgeC].hasBevel)
            {
                VoxelEdge bevelEdge = edges[edgeB].hasBevel ? edges[edgeB] : edges[edgeC];
                corners[i].bevel_i = vertexI;
                corners[i].bevel_count = bevelEdge.bevelTypeArray.Length * 2 - 2;
                corners[i].count += corners[i].bevel_count;
                vertexI += corners[i].bevel_count;

                if ((SQUARE_LOOP[i].x == 0 && edges[edgeB].capMin) || (SQUARE_LOOP[i].x == 1 && edges[edgeB].capMax)
                 || (SQUARE_LOOP[i].y == 0 && edges[edgeC].capMin) || (SQUARE_LOOP[i].y == 1 && edges[edgeC].capMax))
                {
                    corners[i].cap_i = vertexI;
                    corners[i].cap_count = bevelEdge.bevelTypeArray.Length + 1;
                    corners[i].count += corners[i].cap_count;
                    vertexI += corners[i].cap_count;
                }
            }
            if (edges[edgeA].hasBevel && !edges[edgeB].hasBevel && !edges[edgeC].hasBevel)
            {
                bool concaveB = faces[EdgeBOtherFace(faceNum, i)].IsEmpty();
                bool concaveC = faces[EdgeCOtherFace(faceNum, i)].IsEmpty();
                if (!concaveB && !concaveC)
                {
                    corners[i].bevelProfile_i = vertexI;
                    corners[i].bevelProfile_count = edges[edgeA].bevelTypeArray.Length * 2 - 3;
                    corners[i].count += corners[i].bevelProfile_count;
                    vertexI += corners[i].bevelProfile_count;

                    if (corners[i].edgeB_i == -1)
                    {
                        corners[i].edgeB_i = vertexI++;
                        corners[i].count++;
                    }
                    if (corners[i].edgeC_i == -1)
                    {
                        corners[i].edgeC_i = vertexI++;
                        corners[i].count++;
                    }
                    int nextI = (i + 1) % 4;
                    int prevI = i == 0 ? 3 : i - 1;
                    int otherEdgeBI = i % 2 == 0 ? nextI : prevI;
                    int otherEdgeCI = i % 2 == 0 ? prevI : nextI;
                    if (corners[otherEdgeBI].edgeB_i == -1)
                    {
                        corners[otherEdgeBI].edgeB_i = vertexI++;
                        corners[otherEdgeBI].count++;
                    }
                    if (corners[otherEdgeCI].edgeC_i == -1)
                    {
                        corners[otherEdgeCI].edgeC_i = vertexI++;
                        corners[otherEdgeCI].count++;
                    }
                }
            }
        } // end for each corner
        return corners;
    }


    private static float[] vertexPos = new float[3]; // reusable
    private void GenerateFaceVertices(int faceNum, FaceCornerVertices[] corners,
        Vector3[] vertices, Vector2[] uvs, Vector3[] normals, Vector4[] tangents)
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
            FaceCornerVertices corner = corners[i];
            int edgeA, edgeB, edgeC;
            VertexEdges(faceNum, i, out edgeA, out edgeB, out edgeC);
            bool concaveB = faces[EdgeBOtherFace(faceNum, i)].IsEmpty();
            bool concaveC = faces[EdgeCOtherFace(faceNum, i)].IsEmpty();
            bool concaveA = concaveB || concaveC;

            vertexPos[axis] = faceNum % 2; // will stay for all planar vertices

            vertexPos[(axis + 1) % 3] = ApplyBevel(SQUARE_LOOP[i].x,
                corner.edgeC_i != -1 && !concaveA ? edges[edgeA] : edges[edgeC]);
            vertexPos[(axis + 2) % 3] = ApplyBevel(SQUARE_LOOP[i].y,
                corner.edgeB_i != -1 && !concaveA ? edges[edgeA] : edges[edgeB]);
            vertices[corner.innerQuad_i] = Vector3FromArray(vertexPos);
            uvs[corner.innerQuad_i] = CalcUV(vertexPos, positiveU_xyz, positiveV_xyz);
            normals[corner.innerQuad_i] = normal;
            tangents[corner.innerQuad_i] = tangent;

            if (corner.edgeB_i != -1)
            {
                vertexPos[(axis + 1) % 3] = ApplyBevel(SQUARE_LOOP[i].x, edges[edgeC].hasBevel ? edges[edgeC] : edges[edgeA]);
                vertexPos[(axis + 2) % 3] = SQUARE_LOOP[i].y; // will never have both edgeB and a bevel
                vertices[corner.edgeB_i] = Vector3FromArray(vertexPos);
                uvs[corner.edgeB_i] = CalcUV(vertexPos, positiveU_xyz, positiveV_xyz);
                normals[corner.edgeB_i] = normal;
                tangents[corner.edgeB_i] = tangent;
            }
            if (corner.edgeC_i != -1)
            {
                vertexPos[(axis + 1) % 3] = SQUARE_LOOP[i].x;
                vertexPos[(axis + 2) % 3] = ApplyBevel(SQUARE_LOOP[i].y, edges[edgeB].hasBevel ? edges[edgeB] : edges[edgeA]);
                vertices[corner.edgeC_i] = Vector3FromArray(vertexPos);
                uvs[corner.edgeC_i] = CalcUV(vertexPos, positiveU_xyz, positiveV_xyz);
                normals[corner.edgeC_i] = normal;
                tangents[corner.edgeC_i] = tangent;
            }
            if (corner.bevelProfile_i != -1)
            {
                Vector2[] bevelArray = edges[edgeA].bevelTypeArray;
                for (int bevelI = 0; bevelI < bevelArray.Length - 1; bevelI++)
                {
                    Vector2 bevelVector = bevelArray[bevelI + 1];
                    vertexPos[(axis + 1) % 3] = ApplyBevel(SQUARE_LOOP[i].x, edges[edgeA], bevelVector.x);
                    vertexPos[(axis + 2) % 3] = ApplyBevel(SQUARE_LOOP[i].y, edges[edgeA], bevelVector.y);
                    vertices[corner.bevelProfile_i + bevelI] = Vector3FromArray(vertexPos);
                    uvs[corner.bevelProfile_i + bevelI] = CalcUV(vertexPos, positiveU_xyz, positiveV_xyz);
                    normals[corner.bevelProfile_i + bevelI] = normal;
                    tangents[corner.bevelProfile_i + bevelI] = tangent;

                    vertexPos[(axis + 1) % 3] = ApplyBevel(SQUARE_LOOP[i].x, edges[edgeA], bevelVector.y); // x/y are swapped
                    vertexPos[(axis + 2) % 3] = ApplyBevel(SQUARE_LOOP[i].y, edges[edgeA], bevelVector.x);
                    // last iteration vertices will overlap
                    vertices[corner.bevelProfile_i + corner.bevelProfile_count - 1 - bevelI] = Vector3FromArray(vertexPos);
                    uvs[corner.bevelProfile_i + corner.bevelProfile_count - 1 - bevelI] = CalcUV(vertexPos, positiveU_xyz, positiveV_xyz);
                    normals[corner.bevelProfile_i + corner.bevelProfile_count - 1 - bevelI] = normal;
                    tangents[corner.bevelProfile_i + corner.bevelProfile_count - 1 - bevelI] = tangent;
                }
            }

            // END PLANAR VERTICES

            if (corner.bevel_i != -1)
            {
                VoxelEdge beveledEdge = edges[edgeB].hasBevel ? edges[edgeB] : edges[edgeC];
                bool concave = edges[edgeB].hasBevel ? concaveB : concaveC;

                Vector3 capNormal = Vector3.zero;
                if (corner.cap_i != -1)
                {
                    vertexPos[axis] = faceNum % 2;
                    vertexPos[(axis + 1) % 3] = SQUARE_LOOP[i].x;
                    vertexPos[(axis + 2) % 3] = SQUARE_LOOP[i].y;
                    vertices[corner.cap_i] = Vector3FromArray(vertexPos);
                    tangents[corner.cap_i] = tangent; // TODO
                    // uv TODO
                    // 0.29289f = 1 - 1/sqrt(2) for some reason
                    vertexPos[(axis + 1) % 3] = ApplyBevel(SQUARE_LOOP[i].x, beveledEdge, 0.29289f);
                    vertexPos[(axis + 2) % 3] = ApplyBevel(SQUARE_LOOP[i].y, beveledEdge, 0.29289f);
                    uvs[corner.cap_i] = CalcUV(vertexPos, positiveU_xyz, positiveV_xyz);

                    // normal (b and c are supposed to be swapped)
                    vertexPos[axis] = 0;
                    vertexPos[(axis + 1) % 3] = edges[edgeB].hasBevel ? (1 - SQUARE_LOOP[i].x * 2) : 0;
                    vertexPos[(axis + 2) % 3] = edges[edgeC].hasBevel ? (1 - SQUARE_LOOP[i].y * 2) : 0;
                    capNormal = Vector3FromArray(vertexPos);
                    if (concave)
                        capNormal = -capNormal;
                    normals[corner.cap_i] = capNormal;
                }

                Vector2[] bevelArray = beveledEdge.bevelTypeArray;
                int bevelVertex = corner.bevel_i;
                for (int bevelI = 0; bevelI < bevelArray.Length; bevelI++)
                {
                    Vector2 bevelVector = bevelArray[bevelI];
                    float xCoord = bevelVector.x;
                    if (concave)
                        xCoord = 2 - xCoord;
                    vertexPos[axis] = ApplyBevel(faceNum % 2, beveledEdge, xCoord);
                    if (concave)
                        xCoord = 1; // concave bevels aren't joined
                    vertexPos[(axis + 1) % 3] = ApplyBevel(SQUARE_LOOP[i].x, edges[edgeC].hasBevel ? edges[edgeC] : edges[edgeA],
                        edges[edgeC].hasBevel ? bevelVector.y : xCoord);
                    vertexPos[(axis + 2) % 3] = ApplyBevel(SQUARE_LOOP[i].y, edges[edgeB].hasBevel ? edges[edgeB] : edges[edgeA],
                        edges[edgeB].hasBevel ? bevelVector.y : xCoord);
                    vertices[bevelVertex] = Vector3FromArray(vertexPos);
                    tangents[bevelVertex] = tangent; // TODO

                    if (corner.cap_i != -1)
                    {
                        vertices[corner.cap_i + bevelI + 1] = Vector3FromArray(vertexPos);
                        tangents[corner.cap_i + bevelI + 1] = tangent; // TODO
                        normals[corner.cap_i + bevelI + 1] = capNormal;
                    }

                    // calc uv (this is partially copy/pasted from vertex pos above, which is bad)
                    float uvCoord = (float)bevelI / (float)(bevelArray.Length - 1);
                    vertexPos[(axis + 1) % 3] = ApplyBevel(SQUARE_LOOP[i].x, edges[edgeC].hasBevel ? edges[edgeC] : edges[edgeA],
                        edges[edgeC].hasBevel ? uvCoord : xCoord);
                    vertexPos[(axis + 2) % 3] = ApplyBevel(SQUARE_LOOP[i].y, edges[edgeB].hasBevel ? edges[edgeB] : edges[edgeA],
                        edges[edgeB].hasBevel ? uvCoord : xCoord);
                    uvs[bevelVertex] = CalcUV(vertexPos, positiveU_xyz, positiveV_xyz);
                    if (corner.cap_i != -1)
                        uvs[corner.cap_i + bevelI + 1] = uvs[bevelVertex];

                    if (bevelI != 0 && bevelI != bevelArray.Length - 1)
                    {
                        vertices[bevelVertex + 1] = vertices[bevelVertex];
                        tangents[bevelVertex + 1] = tangents[bevelVertex];
                        uvs[bevelVertex + 1] = uvs[bevelVertex];
                        bevelVertex++;
                    }
                    bevelVertex++;
                }

                // add normals for each bevel vertex
                Vector2[] bevelNormalArray = beveledEdge.bevelTypeNormalArray;
                for (int bevelI = 0; bevelI < bevelNormalArray.Length; bevelI++)
                {
                    Vector2 normalVector = bevelNormalArray[bevelI];
                    vertexPos[axis] = normalVector.x * ((faceNum % 2) * 2 - 1);
                    vertexPos[(axis + 1) % 3] = edges[edgeC].hasBevel ? normalVector.y * (SQUARE_LOOP[i].x * 2 - 1) : 0;
                    vertexPos[(axis + 2) % 3] = edges[edgeB].hasBevel ? normalVector.y * (SQUARE_LOOP[i].y * 2 - 1) : 0;
                    if (concave)
                    {
                        vertexPos[(axis + 1) % 3] *= -1;
                        vertexPos[(axis + 2) % 3] *= -1;
                    }
                    normals[corner.bevel_i + bevelI] = Vector3FromArray(vertexPos);
                }
            } // end if bevel
        } // end for each corner
    }


    private Vector3 Vector3FromArray(float[] vector)
    {
        return new Vector3(vector[0], vector[1], vector[2]);
    }

    private Vector2 CalcUV(float[] vertex, Vector3 positiveU_xyz, Vector3 positiveV_xyz)
    {
        Vector3 vector = Vector3FromArray(vertex) + transform.position;
        return new Vector2(
            vector.x * positiveU_xyz.x + vector.y * positiveU_xyz.y + vector.z * positiveU_xyz.z,
            vector.x * positiveV_xyz.x + vector.y * positiveV_xyz.y + vector.z * positiveV_xyz.z);
    }


    private int[] GenerateFaceTriangles(int faceNum, FaceCornerVertices[] vertices)
    {
        if (faces[faceNum].IsEmpty())
            return null;

        int[] surroundingEdges = new int[4];
        int surroundingEdgeI = 0;

        int triangleCount = 6;
        bool noInnerQuad = false;
        // for each pair of edge vertices
        foreach (int edgeI in FaceSurroundingEdges(faceNum))
        {
            if (edges[edgeI].hasBevel)
                triangleCount += 6 * (edges[edgeI].bevelTypeArray.Length - 1);
            surroundingEdges[surroundingEdgeI++] = edgeI;
        }
        for (int i = 0; i < 4; i++)
        {
            int edgeA, edgeB, edgeC;
            VertexEdges(faceNum, i, out edgeA, out edgeB, out edgeC);
            if (edges[edgeA].hasBevel && edges[edgeA].bevelSize == VoxelEdge.BevelSize.FULL
                && EdgeIsConvex(edgeA))
            {
                noInnerQuad = true; // quad would be convex which might cause problems
                triangleCount -= 6;
            }

            if (vertices[i].bevelProfile_count != 0)
                triangleCount += 3 * (vertices[i].bevelProfile_count + 1);
            if (i % 2 == 0) // make sure each edge only counts once
            {
                if (vertices[i].edgeB_i != -1)
                    triangleCount += 6;
            }
            else
            {
                if (vertices[i].edgeC_i != -1)
                    triangleCount += 6;
            }
            if (vertices[i].cap_i != -1)
                triangleCount += 3 * (vertices[i].cap_count - 2);
        }

        var triangles = new int[triangleCount];
        triangleCount = 0;
        bool faceCCW = faceNum % 2 == 1;

        if (!noInnerQuad)
        {
            QuadTriangles(triangles, triangleCount, faceCCW,
                vertices[0].innerQuad_i,
                vertices[1].innerQuad_i,
                vertices[2].innerQuad_i,
                vertices[3].innerQuad_i);
            triangleCount += 6;
        }

        // for each pair of edge vertices
        for (int i = 0; i < 4; i++)
        {
            int j = (i + 1) % 4;
            if (edges[surroundingEdges[i]].hasBevel)
            {
                for (int bevelI = 0; bevelI < vertices[i].bevel_count / 2; bevelI++)
                {
                    QuadTriangles(triangles, triangleCount, faceCCW,
                        vertices[i].bevel_i + bevelI * 2,
                        vertices[i].bevel_i + bevelI * 2 + 1,
                        vertices[j].bevel_i + bevelI * 2 + 1,
                        vertices[j].bevel_i + bevelI * 2);
                    triangleCount += 6;
                }
            }
            if (vertices[i].bevelProfile_count != 0)
            {
                bool profileCCW = faceCCW ^ (i % 2 == 0);
                // first
                AddTriangle(triangles, triangleCount, profileCCW,
                    vertices[i].innerQuad_i,
                    vertices[i].bevelProfile_i,
                    vertices[i].edgeC_i);
                triangleCount += 3;
                // last
                AddTriangle(triangles, triangleCount, profileCCW,
                    vertices[i].innerQuad_i,
                    vertices[i].edgeB_i,
                    vertices[i].bevelProfile_i + vertices[i].bevelProfile_count - 1);
                triangleCount += 3;
                // middle
                for (int profileI = 0; profileI < vertices[i].bevelProfile_count - 1; profileI++)
                {
                    AddTriangle(triangles, triangleCount, profileCCW,
                        vertices[i].innerQuad_i,
                        vertices[i].bevelProfile_i + profileI + 1,
                        vertices[i].bevelProfile_i + profileI);
                    triangleCount += 3;
                }
            }
            if (i % 2 == 0 && vertices[i].edgeB_i != -1)
            {
                QuadTriangles(triangles, triangleCount, faceCCW,
                    vertices[i].innerQuad_i,
                    vertices[i].edgeB_i,
                    vertices[j].edgeB_i,
                    vertices[j].innerQuad_i);
                triangleCount += 6;
            }
            if (i % 2 == 1 && vertices[i].edgeC_i != -1)
            {
                QuadTriangles(triangles, triangleCount, faceCCW,
                    vertices[i].innerQuad_i,
                    vertices[i].edgeC_i,
                    vertices[j].edgeC_i,
                    vertices[j].innerQuad_i);
                triangleCount += 6;
            }
            if (vertices[i].cap_i != -1)
            {
                int edgeA, edgeB, edgeC;
                VertexEdges(faceNum, i, out edgeA, out edgeB, out edgeC);
                bool capCCW = faceCCW ^ (edges[edgeC].hasBevel) ^ (i % 2 == 0);
                for (int capI = 1; capI < vertices[i].cap_count - 1; capI++)
                {
                    AddTriangle(triangles, triangleCount, capCCW,
                        vertices[i].cap_i,
                        vertices[i].cap_i + capI,
                        vertices[i].cap_i + capI + 1);
                    triangleCount += 3;
                }
            }
        }

        return triangles;
    }

    // specify vertices 0-3 of a quad in counter-clockwise order
    // ccw = counter-clockwise?
    private void QuadTriangles(int[] triangleArray, int i, bool ccw,
        int vertex0, int vertex1, int vertex2, int vertex3)
    {
        triangleArray[i + 0] = vertex0;
        triangleArray[i + 1] = ccw ? vertex1 : vertex2;
        triangleArray[i + 2] = ccw ? vertex2 : vertex1;
        triangleArray[i + 3] = vertex0;
        triangleArray[i + 4] = ccw ? vertex2 : vertex3;
        triangleArray[i + 5] = ccw ? vertex3 : vertex2;
    }

    // specify vertices 0-2 of a triangle in counter-clockwise order
    // ccw = counter-clockwise?
    private void AddTriangle(int[] triangleArray, int i, bool ccw,
        int vertex0, int vertex1, int vertex2)
    {
        triangleArray[i + 0] = vertex0;
        triangleArray[i + 1] = ccw ? vertex1 : vertex2;
        triangleArray[i + 2] = ccw ? vertex2 : vertex1;
    }

    private int FaceMaterialCount(int faceNum, bool xRay, Material coloredHighlightMaterial)
    {
        VoxelFace face = faces[faceNum];
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

        foreach (int edgeI in FaceSurroundingEdges(faceNum))
        {
            if (edges[edgeI].addSelected || edges[edgeI].storedSelected)
            {
                count++;
                break;
            }
        }

        return count;
    }


    private IEnumerable<Material> IterateFaceMaterials(int faceNum, bool xRay, Material coloredHighlightMaterial)
    {
        VoxelFace face = faces[faceNum];
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

        int highlightNum = 0;
        int surroundingEdgeI = 0;
        foreach (int edgeI in FaceSurroundingEdges(faceNum))
        {
            var e = edges[edgeI];
            if (e.addSelected || e.storedSelected)
            {
                int n = FaceTransformedEdgeNum(faceNum, surroundingEdgeI);
                highlightNum |= 1 << n;
            }
            surroundingEdgeI++;
        }
        if (highlightNum != 0)
            yield return highlightMaterials[highlightNum];
    }


    private static void VertexEdges(int faceNum, int vertexI, out int edgeA, out int edgeB, out int edgeC)
    {
        int axis = FaceIAxis(faceNum);
        edgeA = axis * 4 + vertexI;
        edgeB = ((axis + 1) % 3) * 4
            + SQUARE_LOOP_COORD_INDEX[(vertexI >= 2 ? 1 : 0) + (faceNum % 2) * 2];
        edgeC = ((axis + 2) % 3) * 4
            + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) + (vertexI == 1 || vertexI == 2 ? 2 : 0)];
    }

    private static int EdgeBOtherFace(int faceNum, int vertexI)
    {
        int axis = FaceIAxis(faceNum);
        int other = ((axis + 2) % 3) * 2;
        if (vertexI >= 2)
            return other + 1;
        else
            return other;
    }

    private static int EdgeCOtherFace(int faceNum, int vertexI)
    {
        int axis = FaceIAxis(faceNum);
        int other = ((axis + 1) % 3 * 2);
        if (vertexI == 1 || vertexI == 2)
            return other + 1;
        else
            return other;
    }

    private static float ApplyBevel(float coord, VoxelEdge edge, float bevelCoord = 0.0f)
    {
        if (edge.hasBevel)
            return (coord - 0.5f) * (1 - edge.bevelSizeFloat * 2 * (1 - bevelCoord)) + 0.5f;
        else
            return coord;
    }
}
