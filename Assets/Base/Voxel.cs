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
    private static readonly Vector2[] SHAPE_SQUARE = new Vector2[] {
        new Vector2(1.0f, 0.0f), new Vector2(0.0f, 0.0f) };
    private static readonly Vector2[] SHAPE_FLAT = new Vector2[] {
        new Vector2(1.0f, 0.0f), new Vector2(0.5f, 0.5f) };
    private static readonly Vector2[] SHAPE_CURVE = new Vector2[] {
        new Vector2(1.0f, 0.0f), new Vector2(0.96592f, 0.25881f), new Vector2(0.86602f, 0.5f), new Vector2(0.70710f, 0.70710f) };
    private static readonly Vector2[] SHAPE_STAIR_2 = new Vector2[] {
        new Vector2(1.0f, 0.0f), new Vector2(0.5f, 0.0f), new Vector2(0.5f, 0.5f) };
    private static readonly Vector2[] SHAPE_STAIR_4 = new Vector2[] {
        new Vector2(1.0f, 0.0f), new Vector2(0.75f, 0.0f), new Vector2(0.75f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f) };

    // a pair of normals for each line segment connecting 2 vertices
    private static readonly Vector2[] NORMALS_SQUARE = new Vector2[] {
        new Vector2(0.0f, 1.0f), new Vector2(0.0f, 1.0f) };
    private static readonly Vector2[] NORMALS_FLAT = new Vector2[] {
        new Vector2(0.70710f, 0.70710f), new Vector2(0.70710f, 0.70710f) };
    private static readonly Vector2[] NORMALS_CURVE = new Vector2[] {
        new Vector2(1.0f, 0.0f), new Vector2(0.96592f, 0.25881f), new Vector2(0.96592f, 0.25881f),
        new Vector2(0.86602f, 0.5f), new Vector2(0.86602f, 0.5f), new Vector2(0.70710f, 0.70710f) };
    private static readonly Vector2[] NORMALS_STAIR_2 = new Vector2[] {
        new Vector2(0.0f, 1.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 0.0f) };
    private static readonly Vector2[] NORMALS_STAIR_4 = new Vector2[] {
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


public class Voxel
{
    public static VoxelFace EMPTY_FACE = new VoxelFace();

    public readonly static int[] SQUARE_LOOP_COORD_INDEX = new int[] { 0, 1, 3, 2 };


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


    public Vector3Int position;
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
            {
                _substance.AddVoxel(this);
                if (voxelComponent != null)
                {
                    // TODO this seems pretty ugly
                    voxelComponent.VoxelDeleted(this);
                    GameObject singleBlockGO = new GameObject();
                    singleBlockGO.transform.position = GetBounds().center;
                    singleBlockGO.transform.parent = voxelComponent.transform.parent;
                    voxelComponent = singleBlockGO.AddComponent<VoxelComponent>();
                    voxelComponent.AddVoxel(this);
                }
            }
        }
    }
    public ObjectEntity objectEntity;

    public VoxelArray.OctreeNode octreeNode;
    public VoxelComponent voxelComponent;

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
        bounds.center += position;
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
        return new Bounds(center + position, size);
    }

    public Bounds GetBounds()
    {
        return new Bounds(position + new Vector3(0.5f, 0.5f, 0.5f), Vector3.one);
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

    public void UpdateVoxel()
    {
        voxelComponent.UpdateVoxel();
    }

    public void VoxelDeleted()
    {
        voxelComponent.VoxelDeleted(this);
        voxelComponent.UpdateVoxel();
    }
}


public class VoxelComponent : MonoBehaviour
{
    public static Material selectedMaterial; // set by VoxelArrayEditor instance
    public static Material xRayMaterial;
    public static Material[] highlightMaterials;

    // constants for generating mesh
    private readonly static Vector2[] SQUARE_LOOP = new Vector2[]
    {
        Vector2.zero, Vector2.right, Vector2.one, Vector2.up
    };

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

    private List<Voxel> voxels = new List<Voxel>();
    private FaceVertexIndex[] faceVertexIndices;
    private bool updateFlag = false;
    public bool isDestroyed = false;

    void Awake()
    {
        gameObject.tag = "Voxel";
        gameObject.AddComponent<MeshFilter>();
        var render = gameObject.AddComponent<MeshRenderer>();
        render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
    }

    public bool IsSingleBlock()
    {
        return voxels.Count == 1;
    }

    void OnBecameVisible()
    {
        if (IsSingleBlock())
            // don't use substance.component (doesn't work for clones)
            transform.parent.SendMessage("OnBecameVisible", options: SendMessageOptions.DontRequireReceiver); // for InCameraComponent
    }

    void OnBecameInvisible()
    {
        if (IsSingleBlock())
            transform.parent.SendMessage("OnBecameInvisible", options: SendMessageOptions.DontRequireReceiver); // for InCameraComponent
    }

    public void GetVoxelFaceForVertex(int vertex, out Voxel voxel, out int faceNum)
    {
        FaceVertexIndex prevFVI = new FaceVertexIndex();
        foreach (FaceVertexIndex fvi in faceVertexIndices)
        {
            if (fvi.index > vertex)
            {
                voxel = prevFVI.voxel;
                faceNum = prevFVI.faceNum;
                return;
            }
            prevFVI = fvi;
        }
        voxel = prevFVI.voxel;
        faceNum = prevFVI.faceNum;
    }

    public void AddVoxel(Voxel voxel)
    {
        voxels.Add(voxel);
    }

    public void VoxelDeleted(Voxel voxel)
    {
        voxels.Remove(voxel);
        if (voxels.Count == 0)
            // this will cause voxel to be destroyed in the next frame
            UpdateVoxel();
    }

    public VoxelComponent Clone()
    {
        VoxelComponent vClone = Component.Instantiate<VoxelComponent>(this);
        vClone.voxels.AddRange(voxels);
        return vClone;
    }

    void Start()
    {
        // important, otherwise the game breaks :/
        if (updateFlag)
            UpdateVoxelImmediate();
    }

    void Update()
    {
        if (updateFlag)
            UpdateVoxelImmediate();
    }

    public void UpdateVoxel()
    {
        updateFlag = true;
    }

    private void UpdateVoxelImmediate()
    {
        if (voxels.Count == 0)
        {
            Destroy(gameObject);
            isDestroyed = true;
            return;
        }

        updateFlag = false;
        bool inEditor = VoxelArrayEditor.instance != null;
        Substance substance = null;
        if (IsSingleBlock())
            substance = voxels[0].substance;

        if (substance != null && substance.xRay)
            gameObject.layer = 8; // XRay layer
        else
            gameObject.layer = 0; // default
        
        int numFaces = 0;
        foreach (Voxel v in voxels)
            foreach (VoxelFace f in v.faces)
                if (!f.IsEmpty())
                    numFaces++;

        VoxelMeshInfo[] voxelInfos = new VoxelMeshInfo[voxels.Count];
        faceVertexIndices = new FaceVertexIndex[numFaces];
        numFaces = 0;
        int numVertices = 0;

        for (int i = 0; i < voxels.Count; i++)
        {
            Voxel voxel = voxels[i];
            VoxelMeshInfo info = new VoxelMeshInfo(voxel);
            for (int faceNum = 0; faceNum < 6; faceNum++)
            {
                if (!voxel.faces[faceNum].IsEmpty())
                    faceVertexIndices[numFaces++] = new FaceVertexIndex(voxel, faceNum, numVertices);
                var corners = GetFaceVertices(voxel, faceNum, numVertices);
                info.faceCorners[faceNum] = corners;
                int faceVertices = corners[0].count + corners[1].count + corners[2].count + corners[3].count;
                numVertices += faceVertices;
                try
                {
                    info.faceTriangles[faceNum] = GenerateFaceTriangles(voxel, faceNum, corners);
                }
                catch (System.IndexOutOfRangeException)
                {
                    Debug.LogError("Vertex indices don't match!");
                    return;
                }
            }
            voxelInfos[i] = info;
        }

        var vertices = new Vector3[numVertices];
        var uvs = new Vector2[numVertices];
        var normals = new Vector3[numVertices];
        var tangents = new Vector4[numVertices];

        foreach (VoxelMeshInfo info in voxelInfos)
        {
            for (int faceNum = 0; faceNum < 6; faceNum++)
            {
                try
                {
                    GenerateFaceVertices(info.voxel, faceNum, info.faceCorners[faceNum],
                        vertices, uvs, normals, tangents);
                }
                catch (System.IndexOutOfRangeException)
                {
                    Debug.LogError("Vertex indices don't match!");
                    return;
                }
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
        mesh.subMeshCount = 0;

        List<Material> matList = new List<Material>();
        foreach (VoxelMeshInfo info in voxelInfos)
        {
            foreach (var matInfo in IterateVoxelMaterials(info.voxel, inEditor))
            {
                if (matInfo.material == null || matInfo.NoFaces())
                    continue;
                int triangleCount = 0;
                for (int i = 0; i < 6; i++)
                {
                    if (matInfo.faces[i])
                        triangleCount += info.faceTriangles[i].Length;
                }
                int[] triangles = new int[triangleCount];
                triangleCount = 0;
                for (int i = 0; i < 6; i++)
                {
                    if (matInfo.faces[i])
                    {
                        System.Array.Copy(info.faceTriangles[i], 0, triangles, triangleCount,
                            info.faceTriangles[i].Length);
                        triangleCount += info.faceTriangles[i].Length;
                    }
                }
                mesh.subMeshCount++;
                mesh.SetTriangles(triangles, matList.Count);
                matList.Add(matInfo.material);
            }
        }

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.materials = matList.ToArray(); // TODO

        bool useMeshCollider = true;
        if (!inEditor && substance != null)
        {
            useMeshCollider = false;
            foreach (VoxelEdge edge in voxels[0].edges)
            {
                if (edge.hasBevel)
                {
                    useMeshCollider = true;
                    break;
                }
            }
        }

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        Collider theCollider;

        if (useMeshCollider)
        {
            if (boxCollider != null)
                Destroy(boxCollider);
            if (meshCollider == null)
                meshCollider = gameObject.AddComponent<MeshCollider>();
            // force the collider to update. It otherwise might not since we're using the same mesh object
            // this fixes a bug where rays would pass through a voxel that used to be empty
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = !inEditor && substance != null;
            theCollider = meshCollider;
        }
        else
        {
            if (meshCollider != null)
                Destroy(meshCollider);
            if (boxCollider == null)
                boxCollider = gameObject.AddComponent<BoxCollider>();
            Bounds bounds = voxels[0].GetBounds();
            boxCollider.size = bounds.size;
            boxCollider.center = bounds.center - transform.position;
            theCollider = boxCollider;
        }

        if (!inEditor && substance != null)
        {
            renderer.enabled = false;
            theCollider.isTrigger = true;
        }
        else
        {
            renderer.enabled = true;
            theCollider.isTrigger = false;
        }
    } // end UpdateVoxel()

    // Utility functions for UpdateVoxel() ...

    private struct VoxelMeshInfo
    {
        public Voxel voxel;
        public FaceCornerVertices[][] faceCorners;
        public int[][] faceTriangles;

        public VoxelMeshInfo(Voxel voxel)
        {
            this.voxel = voxel;
            faceCorners = new FaceCornerVertices[6][];
            faceTriangles = new int[6][];
        }
    }

    private struct FaceVertexIndex
    {
        public Voxel voxel;
        public int faceNum;
        public int index;

        public FaceVertexIndex(Voxel voxel, int faceNum, int index)
        {
            this.voxel = voxel;
            this.faceNum = faceNum;
            this.index = index;
        }
    }

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
        // The profile of a bevel orthogonal to the face.
        // edgeC and edgeB are the first and last vertices of the profile (in that order)
        // So only the middle vertices are part of bevelProfile.
        public int bevelProfile_i, bevelProfile_count;

        public FaceHalfEdgeVertices hEdgeB, hEdgeC;

        public FaceCornerVertices(int ignored)
        {
            count = bevelProfile_count = 0;
            innerQuad_i = bevelProfile_i = -1;
            hEdgeB = hEdgeC = new FaceHalfEdgeVertices(0);
        }
    }

    private struct FaceHalfEdgeVertices
    {
        public int count;

        /* on face plane */

        // "Tabs" on the edge of the quad to leave a rectangular cutout for the bevel profile
        public int tab_i;

        /* not on face plane */

        // The first bevel vertex will be identical to innerRect but with a different normal.
        // The middle bevel vertices (not including first/last) are doubled up -- same position,
        // (potentially) different normals.
        public int bevel_i, bevel_count;

        public int cap_i, cap_count;

        public FaceHalfEdgeVertices(int ignored)
        {
            count = bevel_count = cap_count = 0;
            tab_i = bevel_i = cap_i = -1;
        }
    }

    private struct VoxelMaterialInfo
    {
        public Material material;
        public bool[] faces;

        public VoxelMaterialInfo(Material material, bool[] faces)
        {
            this.material = material;
            this.faces = faces;
        }

        public bool NoFaces()
        {
            foreach (bool face in faces)
                if (face)
                    return false;
            return true;
        }
    }

    private static FaceCornerVertices[] GetFaceVertices(Voxel voxel, int faceNum, int vertexI)
    {
        var corners = new FaceCornerVertices[4] {
            new FaceCornerVertices(0), new FaceCornerVertices(0), new FaceCornerVertices(0), new FaceCornerVertices(0)};
        if (voxel.faces[faceNum].IsEmpty())
            return corners;
        for (int i = 0; i < 4; i++)
        {
            corners[i].innerQuad_i = vertexI++;
            corners[i].count++;
            int edgeA, edgeB, edgeC;
            VertexEdges(faceNum, i, out edgeA, out edgeB, out edgeC);
            if (voxel.edges[edgeB].hasBevel)
            {
                GetBevelVertices(ref vertexI, voxel.edges[edgeB], ref corners[i].hEdgeB);
                corners[i].count += corners[i].hEdgeB.count;
            }
            if (voxel.edges[edgeC].hasBevel)
            {
                GetBevelVertices(ref vertexI, voxel.edges[edgeC], ref corners[i].hEdgeC);
                corners[i].count += corners[i].hEdgeC.count;
            }
            if (voxel.edges[edgeA].hasBevel && !voxel.edges[edgeB].hasBevel && !voxel.edges[edgeC].hasBevel)
            {
                bool concaveB = voxel.faces[EdgeBOtherFace(faceNum, i)].IsEmpty();
                bool concaveC = voxel.faces[EdgeCOtherFace(faceNum, i)].IsEmpty();
                if (!concaveB && !concaveC)
                {
                    corners[i].bevelProfile_i = vertexI;
                    corners[i].bevelProfile_count = voxel.edges[edgeA].bevelTypeArray.Length * 2 - 3;
                    corners[i].count += corners[i].bevelProfile_count;
                    vertexI += corners[i].bevelProfile_count;

                    if (corners[i].hEdgeB.tab_i == -1)
                    {
                        corners[i].hEdgeB.tab_i = vertexI++;
                        corners[i].count++;
                    }
                    if (corners[i].hEdgeC.tab_i == -1)
                    {
                        corners[i].hEdgeC.tab_i = vertexI++;
                        corners[i].count++;
                    }
                    int nextI = (i + 1) % 4;
                    int prevI = (i + 3) % 4;
                    int otherEdgeBI = i % 2 == 0 ? nextI : prevI;
                    int otherEdgeCI = i % 2 == 0 ? prevI : nextI;
                    if (corners[otherEdgeBI].hEdgeB.tab_i == -1)
                    {
                        corners[otherEdgeBI].hEdgeB.tab_i = vertexI++;
                        corners[otherEdgeBI].count++;
                    }
                    if (corners[otherEdgeCI].hEdgeC.tab_i == -1)
                    {
                        corners[otherEdgeCI].hEdgeC.tab_i = vertexI++;
                        corners[otherEdgeCI].count++;
                    }
                }
            }
        } // end for each corner
        return corners;
    }

    private static void GetBevelVertices(ref int vertexI, VoxelEdge bevelEdge, ref FaceHalfEdgeVertices hEdge)
    {
        hEdge.bevel_i = vertexI;
        hEdge.bevel_count = bevelEdge.bevelTypeArray.Length * 2 - 2;
        hEdge.count += hEdge.bevel_count;
        vertexI += hEdge.bevel_count;

        // TODO determine if cap!!
        if (false)
        {
            hEdge.cap_i = vertexI;
            hEdge.cap_count = bevelEdge.bevelTypeArray.Length + 1;
            hEdge.count += hEdge.cap_count;
            vertexI += hEdge.cap_count;
        }
    }


    private static float[] vertexPos = new float[3]; // reusable
    private void GenerateFaceVertices(Voxel voxel, int faceNum, FaceCornerVertices[] corners,
        Vector3[] vertices, Vector2[] uvs, Vector3[] normals, Vector4[] tangents)
    {
        VoxelFace face = voxel.faces[faceNum];
        if (face.IsEmpty())
            return;

        Vector3 positionOffset = voxel.position - transform.position;
        int axis = Voxel.FaceIAxis(faceNum);
        Vector3 normal = Voxel.DirectionForFaceI(faceNum);
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
            bool concaveB = voxel.faces[EdgeBOtherFace(faceNum, i)].IsEmpty();
            bool concaveC = voxel.faces[EdgeCOtherFace(faceNum, i)].IsEmpty();
            bool concaveA = concaveB || concaveC;

            vertexPos[axis] = faceNum % 2; // will stay for all planar vertices
            Vector2 squarePos = SQUARE_LOOP[i];

            // set the innerQuad vertex
            vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x,
                corner.hEdgeC.tab_i != -1 && !concaveA ? voxel.edges[edgeA] : voxel.edges[edgeC]);
            vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y,
                corner.hEdgeB.tab_i != -1 && !concaveA ? voxel.edges[edgeA] : voxel.edges[edgeB]);
            vertices[corner.innerQuad_i] = Vector3FromArray(vertexPos) + positionOffset;
            uvs[corner.innerQuad_i] = CalcUV(voxel, vertexPos, positiveU_xyz, positiveV_xyz);
            normals[corner.innerQuad_i] = normal;
            tangents[corner.innerQuad_i] = tangent;

            if (corner.hEdgeB.tab_i != -1)
            {
                vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x, voxel.edges[edgeC].hasBevel ? voxel.edges[edgeC] : voxel.edges[edgeA]);
                vertexPos[(axis + 2) % 3] = squarePos.y; // will never have both edgeB and a bevel
                vertices[corner.hEdgeB.tab_i] = Vector3FromArray(vertexPos) + positionOffset;
                uvs[corner.hEdgeB.tab_i] = CalcUV(voxel, vertexPos, positiveU_xyz, positiveV_xyz);
                normals[corner.hEdgeB.tab_i] = normal;
                tangents[corner.hEdgeB.tab_i] = tangent;
            }
            if (corner.hEdgeC.tab_i != -1)
            {
                vertexPos[(axis + 1) % 3] = squarePos.x;
                vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y, voxel.edges[edgeB].hasBevel ? voxel.edges[edgeB] : voxel.edges[edgeA]);
                vertices[corner.hEdgeC.tab_i] = Vector3FromArray(vertexPos) + positionOffset;
                uvs[corner.hEdgeC.tab_i] = CalcUV(voxel, vertexPos, positiveU_xyz, positiveV_xyz);
                normals[corner.hEdgeC.tab_i] = normal;
                tangents[corner.hEdgeC.tab_i] = tangent;
            }
            if (corner.bevelProfile_i != -1)
            {
                Vector2[] bevelArray = voxel.edges[edgeA].bevelTypeArray;
                for (int bevelI = 0; bevelI < bevelArray.Length - 1; bevelI++)
                {
                    Vector2 bevelVector = bevelArray[bevelI + 1];
                    vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x, voxel.edges[edgeA], bevelVector.x);
                    vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y, voxel.edges[edgeA], bevelVector.y);
                    vertices[corner.bevelProfile_i + bevelI] = Vector3FromArray(vertexPos) + positionOffset;
                    uvs[corner.bevelProfile_i + bevelI] = CalcUV(voxel, vertexPos, positiveU_xyz, positiveV_xyz);
                    normals[corner.bevelProfile_i + bevelI] = normal;
                    tangents[corner.bevelProfile_i + bevelI] = tangent;

                    vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x, voxel.edges[edgeA], bevelVector.y); // x/y are swapped
                    vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y, voxel.edges[edgeA], bevelVector.x);
                    // last iteration vertices will overlap
                    vertices[corner.bevelProfile_i + corner.bevelProfile_count - 1 - bevelI] = Vector3FromArray(vertexPos) + positionOffset;
                    uvs[corner.bevelProfile_i + corner.bevelProfile_count - 1 - bevelI] = CalcUV(voxel, vertexPos, positiveU_xyz, positiveV_xyz);
                    normals[corner.bevelProfile_i + corner.bevelProfile_count - 1 - bevelI] = normal;
                    tangents[corner.bevelProfile_i + corner.bevelProfile_count - 1 - bevelI] = tangent;
                }
            }

            // END PLANAR VERTICES

            if (corner.hEdgeB.bevel_i != -1)
            {
                GenerateBevelVertices(voxel, faceNum, voxel.edges[edgeB], corner.hEdgeB, false,
                    concaveB, axis, squarePos, positionOffset, tangent, positiveU_xyz, positiveV_xyz,
                    edgeA, edgeB, edgeC,
                    vertices, uvs, normals, tangents);
            }
            if (corner.hEdgeC.bevel_i != -1)
            {
                GenerateBevelVertices(voxel, faceNum, voxel.edges[edgeC], corner.hEdgeC, true,
                    concaveC, axis, squarePos, positionOffset, tangent, positiveU_xyz, positiveV_xyz,
                    edgeA, edgeB, edgeC,
                    vertices, uvs, normals, tangents);
            }
        } // end for each corner
    }

    private static void GenerateBevelVertices(Voxel voxel, int faceNum, VoxelEdge beveledEdge, FaceHalfEdgeVertices hEdge, bool isEdgeC,
        bool concave, int axis, Vector2 squarePos, Vector3 positionOffset, Vector4 tangent, Vector3 positiveU_xyz, Vector3 positiveV_xyz,
        int edgeA, int edgeB, int edgeC,
        Vector3[] vertices, Vector2[] uvs, Vector3[] normals, Vector4[] tangents)
    {
        bool isEdgeB = !isEdgeC;

        Vector3 capNormal = Vector3.zero;
        if (hEdge.cap_i != -1)
        {
            vertexPos[axis] = faceNum % 2;
            vertexPos[(axis + 1) % 3] = squarePos.x;
            vertexPos[(axis + 2) % 3] = squarePos.y;
            vertices[hEdge.cap_i] = Vector3FromArray(vertexPos) + positionOffset;
            tangents[hEdge.cap_i] = tangent; // TODO
            // uv TODO
            // 0.29289f = 1 - 1/sqrt(2) for some reason
            vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x, beveledEdge, 0.29289f);
            vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y, beveledEdge, 0.29289f);
            uvs[hEdge.cap_i] = CalcUV(voxel, vertexPos, positiveU_xyz, positiveV_xyz);

            // normal (b and c are supposed to be swapped)
            vertexPos[axis] = 0;
            vertexPos[(axis + 1) % 3] = isEdgeB ? (1 - squarePos.x * 2) : 0;
            vertexPos[(axis + 2) % 3] = isEdgeC ? (1 - squarePos.y * 2) : 0;
            capNormal = Vector3FromArray(vertexPos);
            if (concave)
                capNormal = -capNormal;
            normals[hEdge.cap_i] = capNormal;
        }

        Vector2[] bevelArray = beveledEdge.bevelTypeArray;
        int bevelVertex = hEdge.bevel_i;
        for (int bevelI = 0; bevelI < bevelArray.Length; bevelI++)
        {
            Vector2 bevelVector = bevelArray[bevelI];
            float xCoord = bevelVector.x;
            if (concave)
                xCoord = 2 - xCoord;
            vertexPos[axis] = ApplyBevel(faceNum % 2, beveledEdge, xCoord);
            if (concave)
                xCoord = 1; // concave bevels aren't joined
            vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x, voxel.edges[edgeC].hasBevel ? voxel.edges[edgeC] : voxel.edges[edgeA],
                voxel.edges[edgeC].hasBevel ? bevelVector.y : xCoord);
            vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y, voxel.edges[edgeB].hasBevel ? voxel.edges[edgeB] : voxel.edges[edgeA],
                voxel.edges[edgeB].hasBevel ? bevelVector.y : xCoord);
            vertices[bevelVertex] = Vector3FromArray(vertexPos) + positionOffset;
            tangents[bevelVertex] = tangent; // TODO

            if (hEdge.cap_i != -1)
            {
                vertices[hEdge.cap_i + bevelI + 1] = Vector3FromArray(vertexPos) + positionOffset;
                tangents[hEdge.cap_i + bevelI + 1] = tangent; // TODO
                normals[hEdge.cap_i + bevelI + 1] = capNormal;
            }

            // calc uv (this is partially copy/pasted from vertex pos above, which is bad)
            float uvCoord = (float)bevelI / (float)(bevelArray.Length - 1);
            vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x, voxel.edges[edgeC].hasBevel ? voxel.edges[edgeC] : voxel.edges[edgeA],
                voxel.edges[edgeC].hasBevel ? uvCoord : xCoord);
            vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y, voxel.edges[edgeB].hasBevel ? voxel.edges[edgeB] : voxel.edges[edgeA],
                voxel.edges[edgeB].hasBevel ? uvCoord : xCoord);
            uvs[bevelVertex] = CalcUV(voxel, vertexPos, positiveU_xyz, positiveV_xyz);
            if (hEdge.cap_i != -1)
                uvs[hEdge.cap_i + bevelI + 1] = uvs[bevelVertex];

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
            vertexPos[(axis + 1) % 3] = isEdgeC ? normalVector.y * (squarePos.x * 2 - 1) : 0;
            vertexPos[(axis + 2) % 3] = isEdgeB ? normalVector.y * (squarePos.y * 2 - 1) : 0;
            if (concave)
            {
                vertexPos[(axis + 1) % 3] *= -1;
                vertexPos[(axis + 2) % 3] *= -1;
            }
            normals[hEdge.bevel_i + bevelI] = Vector3FromArray(vertexPos);
        }
    }


    private static Vector3 Vector3FromArray(float[] vector)
    {
        return new Vector3(vector[0], vector[1], vector[2]);
    }

    private static Vector2 CalcUV(Voxel voxel, float[] vertex, Vector3 positiveU_xyz, Vector3 positiveV_xyz)
    {
        Vector3 vector = Vector3FromArray(vertex) + voxel.position;
        return new Vector2(
            vector.x * positiveU_xyz.x + vector.y * positiveU_xyz.y + vector.z * positiveU_xyz.z,
            vector.x * positiveV_xyz.x + vector.y * positiveV_xyz.y + vector.z * positiveV_xyz.z);
    }


    private static int[] GenerateFaceTriangles(Voxel voxel, int faceNum, FaceCornerVertices[] vertices)
    {
        if (voxel.faces[faceNum].IsEmpty())
            return new int[0];

        int[] surroundingEdges = new int[4];
        int surroundingEdgeI = 0;

        int triangleCount = 6;
        bool noInnerQuad = false;
        // for each pair of edge vertices
        foreach (int edgeI in Voxel.FaceSurroundingEdges(faceNum))
        {
            if (voxel.edges[edgeI].hasBevel)
                triangleCount += 6 * (voxel.edges[edgeI].bevelTypeArray.Length - 1);
            surroundingEdges[surroundingEdgeI++] = edgeI;
        }
        for (int i = 0; i < 4; i++)
        {
            int edgeA, edgeB, edgeC;
            VertexEdges(faceNum, i, out edgeA, out edgeB, out edgeC);
            if (voxel.edges[edgeA].hasBevel && voxel.edges[edgeA].bevelSize == VoxelEdge.BevelSize.FULL
                && voxel.EdgeIsConvex(edgeA))
            {
                noInnerQuad = true; // quad would be convex which might cause problems
                triangleCount -= 6;
            }

            if (vertices[i].bevelProfile_count != 0)
                triangleCount += 3 * (vertices[i].bevelProfile_count + 1);
            if (i % 2 == 0) // make sure each edge only counts once
            {
                if (vertices[i].hEdgeB.tab_i != -1)
                    triangleCount += 6;
            }
            else
            {
                if (vertices[i].hEdgeC.tab_i != -1)
                    triangleCount += 6;
            }
            if (vertices[i].hEdgeB.cap_i != -1)
                triangleCount += 3 * (vertices[i].hEdgeB.cap_count - 2);
            if (vertices[i].hEdgeC.cap_i != -1)
                triangleCount += 3 * (vertices[i].hEdgeC.cap_count - 2);
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
            if (voxel.edges[surroundingEdges[i]].hasBevel)
            {
                FaceHalfEdgeVertices vi_hEdge, vj_hEdge;
                if (i % 2 == 0)
                {
                    vi_hEdge = vertices[i].hEdgeB;
                    vj_hEdge = vertices[j].hEdgeB;
                }
                else
                {
                    vi_hEdge = vertices[i].hEdgeC;
                    vj_hEdge = vertices[j].hEdgeC;
                }
                for (int bevelI = 0; bevelI < vi_hEdge.bevel_count / 2; bevelI++)
                {
                    QuadTriangles(triangles, triangleCount, faceCCW,
                        vi_hEdge.bevel_i + bevelI * 2,
                        vi_hEdge.bevel_i + bevelI * 2 + 1,
                        vj_hEdge.bevel_i + bevelI * 2 + 1,
                        vj_hEdge.bevel_i + bevelI * 2);
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
                    vertices[i].hEdgeC.tab_i);
                triangleCount += 3;
                // last
                AddTriangle(triangles, triangleCount, profileCCW,
                    vertices[i].innerQuad_i,
                    vertices[i].hEdgeB.tab_i,
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
            if (i % 2 == 0 && vertices[i].hEdgeB.tab_i != -1)
            {
                QuadTriangles(triangles, triangleCount, faceCCW,
                    vertices[i].innerQuad_i,
                    vertices[i].hEdgeB.tab_i,
                    vertices[j].hEdgeB.tab_i,
                    vertices[j].innerQuad_i);
                triangleCount += 6;
            }
            if (i % 2 == 1 && vertices[i].hEdgeC.tab_i != -1)
            {
                QuadTriangles(triangles, triangleCount, faceCCW,
                    vertices[i].innerQuad_i,
                    vertices[i].hEdgeC.tab_i,
                    vertices[j].hEdgeC.tab_i,
                    vertices[j].innerQuad_i);
                triangleCount += 6;
            }
            if (vertices[i].hEdgeB.cap_i != -1)
            {
                GenerateCapTriangles(voxel, faceNum, vertices,
                    triangles, ref triangleCount, vertices[i].hEdgeB, i, faceCCW, false);
            }
            if (vertices[i].hEdgeC.cap_i != -1)
            {
                GenerateCapTriangles(voxel, faceNum, vertices,
                    triangles, ref triangleCount, vertices[i].hEdgeC, i, faceCCW, true);
            }
        }

        return triangles;
    }

    private static void GenerateCapTriangles(Voxel voxel, int faceNum, FaceCornerVertices[] vertices,
        int[] triangles, ref int triangleCount, FaceHalfEdgeVertices hEdge, int i, bool faceCCW, bool isEdgeC)
    {
        int edgeA, edgeB, edgeC;
        VertexEdges(faceNum, i, out edgeA, out edgeB, out edgeC);
        bool capCCW = faceCCW ^ (isEdgeC) ^ (i % 2 == 0);
        for (int capI = 1; capI < hEdge.cap_count - 1; capI++)
        {
            AddTriangle(triangles, triangleCount, capCCW,
                hEdge.cap_i,
                hEdge.cap_i + capI,
                hEdge.cap_i + capI + 1);
            triangleCount += 3;
        }
    }

    // specify vertices 0-3 of a quad in counter-clockwise order
    // ccw = counter-clockwise?
    private static void QuadTriangles(int[] triangleArray, int i, bool ccw,
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
    private static void AddTriangle(int[] triangleArray, int i, bool ccw,
        int vertex0, int vertex1, int vertex2)
    {
        triangleArray[i + 0] = vertex0;
        triangleArray[i + 1] = ccw ? vertex1 : vertex2;
        triangleArray[i + 2] = ccw ? vertex2 : vertex1;
    }

    private static IEnumerable<VoxelMaterialInfo> IterateVoxelMaterials(Voxel voxel, bool inEditor)
    {
        if (voxel.IsEmpty())
            yield break; // no materials

        bool[] facesEnabled = new bool[6];
        // apply following materials to all non-empty faces
        for (int i = 0; i < 6; i++)
            facesEnabled[i] = !voxel.faces[i].IsEmpty();

        bool xRay = false;
        if (voxel.substance != null)
        {
            if (voxel.substance.xRay && inEditor)
            {
                xRay = true;
                yield return new VoxelMaterialInfo(xRayMaterial, facesEnabled);
            }
            if (voxel.substance.highlight != Color.clear)
                yield return new VoxelMaterialInfo(voxel.substance.highlightMaterial, facesEnabled);
        }

        for (int i = 0; i < 6; i++) // apply to all selected faces
            facesEnabled[i] = voxel.faces[i].addSelected || voxel.faces[i].storedSelected;
        yield return new VoxelMaterialInfo(selectedMaterial, facesEnabled);

        // show selected edges
        for (int i = 0; i < 6; i++)
            facesEnabled[i] = false;
        for (int faceNum = 0; faceNum < 6; faceNum++)
        {
            if (voxel.faces[faceNum].IsEmpty())
                continue;
            int highlightNum = 0;
            int surroundingEdgeI = 0;
            foreach (int edgeI in Voxel.FaceSurroundingEdges(faceNum))
            {
                var e = voxel.edges[edgeI];
                if (e.addSelected || e.storedSelected)
                {
                    int n = voxel.FaceTransformedEdgeNum(faceNum, surroundingEdgeI);
                    highlightNum |= 1 << n;
                }
                surroundingEdgeI++;
            }
            facesEnabled[faceNum] = true;
            if (highlightNum != 0)
                yield return new VoxelMaterialInfo(highlightMaterials[highlightNum], facesEnabled);
            facesEnabled[faceNum] = false;
        }

        if (!xRay) // materials and overlays
        {
            // facesEnabled is already cleared from above
            foreach (var mat in IteratePaintMaterials(voxel, facesEnabled, false))
                yield return mat;
            foreach (var mat in IteratePaintMaterials(voxel, facesEnabled, true))
                yield return mat;
        }
    }

    // facesEnabled should be an array of 6 falses
    private static IEnumerable<VoxelMaterialInfo> IteratePaintMaterials(
        Voxel voxel, bool[] facesEnabled, bool overlay)
    {
        bool[] facesUsed = new bool[6];
        for (int i = 0; i < 6; i++)
        {
            if (facesUsed[i])
                continue;
            Material mat;
            if (overlay)
                mat = voxel.faces[i].overlay;
            else
                mat = voxel.faces[i].material;
            if (mat == null)
                continue;
            facesEnabled[i] = true;
            for (int j = i + 1; j < 6; j++)
            {
                Material mat2;
                if (overlay)
                    mat2 = voxel.faces[j].overlay;
                else
                    mat2 = voxel.faces[j].material;
                if (mat2 == mat)
                {
                    facesEnabled[j] = true;
                    facesUsed[j] = true;
                }
            }
            yield return new VoxelMaterialInfo(mat, facesEnabled);
            for (int j = i; j < 6; j++)
                facesEnabled[j] = false;
        }
    }


    private static void VertexEdges(int faceNum, int vertexI, out int edgeA, out int edgeB, out int edgeC)
    {
        int axis = Voxel.FaceIAxis(faceNum);
        edgeA = axis * 4 + vertexI;
        edgeB = ((axis + 1) % 3) * 4
            + Voxel.SQUARE_LOOP_COORD_INDEX[(vertexI >= 2 ? 1 : 0) + (faceNum % 2) * 2];
        edgeC = ((axis + 2) % 3) * 4
            + Voxel.SQUARE_LOOP_COORD_INDEX[(faceNum % 2) + (vertexI == 1 || vertexI == 2 ? 2 : 0)];
    }

    private static int EdgeBOtherFace(int faceNum, int vertexI)
    {
        int axis = Voxel.FaceIAxis(faceNum);
        int other = ((axis + 2) % 3) * 2;
        if (vertexI >= 2)
            return other + 1;
        else
            return other;
    }

    private static int EdgeCOtherFace(int faceNum, int vertexI)
    {
        int axis = Voxel.FaceIAxis(faceNum);
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