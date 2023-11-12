using System.Collections.Generic;
using UnityEngine;

public struct VoxelFace
{
    public Material material;
    public Material overlay;
    public byte orientation;
    public bool addSelected, storedSelected;

    public bool IsEmpty() => material == null && overlay == null;

    public void Clear()
    {
        material = null;
        overlay = null;
        orientation = 0;
        addSelected = false;
        storedSelected = false;
    }

    public VoxelFace PaintOnly()
    {
        VoxelFace paintOnly = this;
        paintOnly.addSelected = false;
        paintOnly.storedSelected = false;
        return paintOnly;
    }

    public override bool Equals(object obj) => obj is VoxelFace && this == (VoxelFace)obj;

    public static bool operator ==(VoxelFace s1, VoxelFace s2) =>
        s1.material == s2.material && s1.overlay == s2.overlay && s1.orientation == s2.orientation;
    public static bool operator !=(VoxelFace s1, VoxelFace s2) => !(s1 == s2);

    public override int GetHashCode()
    {
        int result = material.GetHashCode();
        result = 37 * result + overlay.GetHashCode();
        result = 37 * result + (int)orientation;
        return result;
    }

    public static int GetOrientationRotation(byte orientation) => orientation & 3;

    public static bool GetOrientationMirror(byte orientation) => (orientation & 4) != 0;

    public static byte Orientation(int rotation, bool mirror)
    {
        while (rotation < 0)
            rotation += 4;
        rotation %= 4;
        return (byte)(rotation + (mirror ? 4 : 0));
    }

    public MaterialSound GetSound()
    {
        MaterialSound matSound = ResourcesDirectory.GetMaterialSound(material);
        MaterialSound overSound = ResourcesDirectory.GetMaterialSound(overlay);
        if (overSound == MaterialSound.GENERIC)
            return matSound;
        else
            return overSound;
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

    // 0x08 and 0x80 used to be used for caps and may still be used in old world files
    public byte bevel;
    public bool addSelected, storedSelected;

    public void Clear()
    {
        bevel = 0;
        addSelected = storedSelected = false;
    }

    public BevelType bevelType
    {
        get => (BevelType)(bevel & 0x07);
        set => bevel = (byte)((bevel & 0xF8) | (byte)value);
    }

    public BevelSize bevelSize
    {
        get => (BevelSize)((bevel >> 4) & 0x07);
        set => bevel = (byte)((bevel & 0x8f) | ((byte)value << 4));
    }

    public Vector2[] bevelTypeArray
    {
        get
        {
            return bevelType switch
            {
                BevelType.SQUARE => SHAPE_SQUARE,
                BevelType.FLAT => SHAPE_FLAT,
                BevelType.CURVE => SHAPE_CURVE,
                BevelType.STAIR_2 => SHAPE_STAIR_2,
                BevelType.STAIR_4 => SHAPE_STAIR_4,
                _ => null,
            };
        }
    }

    public Vector2[] bevelTypeNormalArray
    {
        get
        {
            return bevelType switch
            {
                BevelType.SQUARE => NORMALS_SQUARE,
                BevelType.FLAT => NORMALS_FLAT,
                BevelType.CURVE => NORMALS_CURVE,
                BevelType.STAIR_2 => NORMALS_STAIR_2,
                BevelType.STAIR_4 => NORMALS_STAIR_4,
                _ => null,
            };
        }
    }

    public float bevelSizeFloat
    {
        get
        {
            return bevelSize switch
            {
                BevelSize.QUARTER => 0.25f,
                BevelSize.HALF => 0.5f,
                BevelSize.FULL => 1.0f,
                _ => 0.0f,
            };
        }
    }

    public bool hasBevel => bevelType != BevelType.NONE;

    public static bool BevelsMatch(VoxelEdge e1, VoxelEdge e2)
    {
        if (!e1.hasBevel && !e2.hasBevel)
            return true;
        return e1.bevelType == e2.bevelType && e1.bevelSize == e2.bevelSize;
    }
}


public class Voxel
{
    public static VoxelFace EMPTY_FACE = new VoxelFace();

    public readonly static int[] SQUARE_LOOP_COORD_INDEX = new int[] { 0, 1, 3, 2 };


    public static Vector3 DirectionForFaceI(int faceI)
    {
        return faceI switch
        {
            0 => Vector3.left,
            1 => Vector3.right,
            2 => Vector3.down,
            3 => Vector3.up,
            4 => Vector3.back,
            5 => Vector3.forward,
            _ => Vector3.zero,
        };
    }

    public static Vector3 OppositeDirectionForFaceI(int faceI) =>
        DirectionForFaceI(OppositeFaceI(faceI));

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

    public static int OppositeFaceI(int faceI) => (faceI / 2) * 2 + (faceI % 2 == 0 ? 1 : 0);

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

    public static IEnumerable<int> ConnectedEdges(int edgeI)
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

    public static IEnumerable<int> UnconnectedEdges(int edgeI)
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

    public static int FaceIAxis(int faceI) => faceI / 2;

    public static int EdgeIAxis(int edgeI) => edgeI / 4;

    public static IEnumerable<int> FaceSurroundingEdges(int faceNum)
    {
        int axis = FaceIAxis(faceNum);
        yield return ((axis + 1) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) * 2]; // 0 - 1
        yield return ((axis + 2) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) + 2]; // 1 - 2
        yield return ((axis + 1) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) * 2 + 1]; // 2 - 3
        yield return ((axis + 2) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2)]; // 3 - 0
    }


    public Vector3Int position; // TODO remove
    // see "Voxel Diagram.skp" for a diagram of face/edge numbers
    public VoxelFace[] faces = new VoxelFace[6]; // xMin, xMax, yMin, yMax, zMin, zMax
    // Edges: 0-3: x, 4-7: y, 8-11: z
    // Each group of four follows the pattern (0,0), (1,0), (1,1), (0,1)
    // for the Y/Z axes (0-3), Z/X axes (4-7, note order), or X/Y axes (8-11)
    public VoxelEdge[] edges = new VoxelEdge[12];
    public Substance substance;

    public Bounds GetFaceBounds(int faceI)
    {
        var bounds = faceI switch
        {
            0 => new Bounds(new Vector3(0, 0.5f, 0.5f), new Vector3(0, 1, 1)),
            1 => new Bounds(new Vector3(1, 0.5f, 0.5f), new Vector3(0, 1, 1)),
            2 => new Bounds(new Vector3(0.5f, 0, 0.5f), new Vector3(1, 0, 1)),
            3 => new Bounds(new Vector3(0.5f, 1, 0.5f), new Vector3(1, 0, 1)),
            4 => new Bounds(new Vector3(0.5f, 0.5f, 0), new Vector3(1, 1, 0)),
            5 => new Bounds(new Vector3(0.5f, 0.5f, 1), new Vector3(1, 1, 0)),
            _ => new Bounds(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, 0, 0)),
        };
        bounds.center += position;
        return bounds;
    }

    public Bounds GetEdgeBounds(int edgeI)
    {
        Vector3 center = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 size = Vector3.zero;
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

    public Bounds GetBounds() => new Bounds(position + new Vector3(0.5f, 0.5f, 0.5f), Vector3.one);

    public bool EdgeIsEmpty(int edgeI)
    {
        EdgeFaces(edgeI, out int faceA, out int faceB);
        return faces[faceA].IsEmpty() && faces[faceB].IsEmpty();
    }

    public bool EdgeIsConvex(int edgeI)
    {
        EdgeFaces(edgeI, out int faceA, out int faceB);
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
        if (VoxelFace.GetOrientationMirror(faces[faceNum].orientation))
            n = 5 - (n % 4);
        n += VoxelFace.GetOrientationRotation(faces[faceNum].orientation);
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

    public void ClearFaces()
    {
        for (int faceI = 0; faceI < faces.Length; faceI++)
        {
            faces[faceI].Clear();
        }
        for (int edgeI = 0; edgeI < edges.Length; edgeI++)
        {
            edges[edgeI].Clear();
        }
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

    public VoxelArray voxelArray;
    public List<Voxel> voxels = new List<Voxel>(); // TODO: change to list of positions
    private FaceVertexIndex[] faceVertexIndices;
    private Collider[] voxelColliders; // for substance only
    private bool updateFlag = false;

    void Awake()
    {
        gameObject.tag = "Voxel";
        if (gameObject.GetComponent<MeshFilter>() != null)
            return; // this is a clone!
        gameObject.AddComponent<MeshFilter>();
        var render = gameObject.AddComponent<MeshRenderer>();
        render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
    }

    public Substance GetSubstance()
    {
        if (voxels.Count != 0)
            return voxels[0].substance;
        else
            return null;
    }

    void OnBecameVisible()
    {
        if (VoxelArrayEditor.instance == null && GetSubstance() != null)
            // don't use substance.component (doesn't work for clones)
            transform.parent.SendMessage("OnBecameVisible", options: SendMessageOptions.DontRequireReceiver); // for InCameraComponent
    }

    void OnBecameInvisible()
    {
        if (VoxelArrayEditor.instance == null && GetSubstance() != null)
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

    public Voxel GetVoxelForCollider(Collider collider)
    {
        int index = System.Array.IndexOf(voxelColliders, collider);
        return (index >= 0) ? voxels[index] : null;
    }

    public VoxelComponent Clone()
    {
        VoxelComponent vClone = Component.Instantiate<VoxelComponent>(this);
        vClone.voxelArray = voxelArray;
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
        updateFlag = false;
        bool inEditor = VoxelArrayEditor.instance != null;
        Substance substance = GetSubstance();

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
                info.faceCorners[faceNum] = GetFaceVertices(voxelArray, voxel, faceNum, numVertices);
                numVertices += info.FaceVertexCount(faceNum);
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
                    info.faceTriangles[faceNum] = GenerateFaceTriangles(info.voxel, faceNum,
                        info.faceCorners[faceNum]);
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

        // collision

        bool gameSubstance = !inEditor && substance != null;
        renderer.enabled = !gameSubstance;
        if (!gameSubstance)
        {
            MeshCollider collider = GetComponent<MeshCollider>();
            if (collider == null)
                collider = gameObject.AddComponent<MeshCollider>();
            collider.convex = false;
            // force the collider to update. It otherwise might not since we're using the same mesh object
            // this fixes a bug where rays would pass through a voxel that used to be empty
            collider.sharedMesh = null;
            collider.sharedMesh = mesh;
            collider.isTrigger = false;
        }
        else // gameSubstance
        {
            // this shouldn't be necessary
            foreach (MeshCollider c in GetComponents<MeshCollider>())
                Destroy(c);
            foreach (BoxCollider c in GetComponents<BoxCollider>())
                Destroy(c);

            voxelColliders = new Collider[voxels.Count];
            for (int v = 0; v < voxels.Count; v++)
            {
                Voxel voxel = voxels[v];
                bool hasBevel = false;
                foreach (VoxelEdge edge in voxel.edges)
                {
                    if (edge.hasBevel)
                    {
                        hasBevel = true;
                        break;
                    }
                }
                Collider theCollider;
                if (!hasBevel)
                {
                    BoxCollider collider = gameObject.AddComponent<BoxCollider>();
                    Bounds bounds = voxel.GetBounds();
                    collider.size = bounds.size;
                    collider.center = bounds.center - transform.position;
                    theCollider = collider;
                }
                else // hasBevel
                {
                    // TODO: copied from above

                    VoxelMeshInfo info = new VoxelMeshInfo(voxel);
                    int numVerticesSingle = 0;
                    for (int faceNum = 0; faceNum < 6; faceNum++)
                    {
                        info.faceCorners[faceNum] = GetFaceVertices(voxelArray, voxel, faceNum, numVerticesSingle);
                        numVerticesSingle += info.FaceVertexCount(faceNum);
                    }

                    var verticesSingle = new Vector3[numVerticesSingle];
                    // TODO: these are unnecessary for collision
                    var uvsSingle = new Vector2[numVerticesSingle];
                    var normalsSingle = new Vector3[numVerticesSingle];
                    var tangentsSingle = new Vector4[numVerticesSingle];
                    int numTriangles = 0;
                    for (int faceNum = 0; faceNum < 6; faceNum++)
                    {
                        try
                        {
                            GenerateFaceVertices(info.voxel, faceNum, info.faceCorners[faceNum],
                                verticesSingle, uvsSingle, normalsSingle, tangentsSingle);
                            info.faceTriangles[faceNum] = GenerateFaceTriangles(voxel, faceNum,
                                info.faceCorners[faceNum]);
                            numTriangles += info.faceTriangles[faceNum].Length;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            Debug.LogError("Vertex indices don't match!");
                            return;
                        }
                    }
                    int[] triangles = new int[numTriangles];
                    numTriangles = 0;
                    for (int i = 0; i < 6; i++)
                    {
                        System.Array.Copy(info.faceTriangles[i], 0, triangles, numTriangles,
                            info.faceTriangles[i].Length);
                        numTriangles += info.faceTriangles[i].Length;
                    }

                    Mesh meshSingle = new Mesh();
                    meshSingle.vertices = verticesSingle;
                    meshSingle.triangles = triangles;
                    MeshCollider collider = gameObject.AddComponent<MeshCollider>();
                    collider.convex = true; // TODO: broken for planar meshes!!
                    collider.sharedMesh = meshSingle;
                    theCollider = collider;
                }
                theCollider.isTrigger = true; // by default, changed by Solid behavior
                voxelColliders[v] = theCollider;
            }
        } // end if gameSubstance
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

        public int FaceVertexCount(int faceNum)
        {
            var corners = faceCorners[faceNum];
            return corners[0].count + corners[1].count + corners[2].count + corners[3].count;
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
        public VoxelEdge? reverseCap;

        public FaceHalfEdgeVertices(int ignored)
        {
            count = bevel_count = cap_count = 0;
            tab_i = bevel_i = cap_i = -1;
            reverseCap = null;
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

    private static FaceCornerVertices[] GetFaceVertices(VoxelArray voxelArray, Voxel voxel, int faceNum, int vertexI)
    {
        var corners = new FaceCornerVertices[4] {
            new FaceCornerVertices(0), new FaceCornerVertices(0), new FaceCornerVertices(0), new FaceCornerVertices(0)};
        if (voxel.faces[faceNum].IsEmpty())
            return corners;
        for (int i = 0; i < 4; i++)
        {
            corners[i].innerQuad_i = vertexI++;
            corners[i].count++;
            VertexEdges(faceNum, i, out int edgeA, out int edgeB, out int edgeC);
            if (voxel.edges[edgeB].hasBevel)
            {
                GetBevelVertices(voxelArray, voxel, ref vertexI, voxel.edges[edgeB], ref corners[i].hEdgeB, i, edgeB, false);
                corners[i].count += corners[i].hEdgeB.count;
            }
            if (voxel.edges[edgeC].hasBevel)
            {
                GetBevelVertices(voxelArray, voxel, ref vertexI, voxel.edges[edgeC], ref corners[i].hEdgeC, i, edgeC, true);
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

    private static void GetBevelVertices(VoxelArray voxelArray, Voxel voxel, ref int vertexI, VoxelEdge bevelEdge,
        ref FaceHalfEdgeVertices hEdge, int cornerI, int edgeI, bool isEdgeC)
    {
        hEdge.bevel_i = vertexI;
        hEdge.bevel_count = bevelEdge.bevelTypeArray.Length * 2 - 2;
        hEdge.count += hEdge.bevel_count;
        vertexI += hEdge.bevel_count;

        int capFaceI = Voxel.EdgeIAxis(edgeI) * 2;
        if (!isEdgeC && SQUARE_LOOP[cornerI].x == 1)  // TODO is this right??
            capFaceI++;
        else if (isEdgeC && SQUARE_LOOP[cornerI].y == 1)
            capFaceI++;
        Vector3Int capDir = Voxel.DirectionForFaceI(capFaceI).ToInt();
        Voxel adjacent = voxelArray.VoxelAt(voxel.position + capDir, false);

        if (BevelCap(voxel, adjacent, edgeI, capFaceI, out hEdge.reverseCap))
        {
            hEdge.cap_i = vertexI;
            hEdge.cap_count = bevelEdge.bevelTypeArray.Length + 1;
            if (hEdge.reverseCap != null)
                hEdge.cap_count++; // extra vertex to make square
            hEdge.count += hEdge.cap_count;
            vertexI += hEdge.cap_count;
        }
    }

    private static bool BevelCap(Voxel thisVoxel, Voxel otherVoxel, int edgeI, int capFaceI,
        out VoxelEdge? reverseCap)
    {
        reverseCap = null;

        // check for joined corner faces that don't match
        bool face = !thisVoxel.faces[capFaceI].IsEmpty();
        foreach (int connectedI in Voxel.ConnectedEdges(edgeI))
        {
            Voxel.EdgeFaces(connectedI, out int faceA, out int faceB);
            // only use faces on this half of the edge
            if (faceA != capFaceI && faceB != capFaceI)
                continue;
            VoxelEdge connectedEdge = thisVoxel.edges[connectedI];
            if (connectedEdge.hasBevel && !VoxelEdge.BevelsMatch(thisVoxel.edges[edgeI], connectedEdge))
            {
                if (thisVoxel.EdgeIsConvex(edgeI) && thisVoxel.EdgeIsConvex(connectedI))
                    reverseCap = connectedEdge;
                return true;
            }
        }

        bool otherEmpty = otherVoxel == null || otherVoxel.EdgeIsEmpty(edgeI);
        bool match = false;
        if (otherVoxel != null)
        {
            if (otherVoxel.substance != thisVoxel.substance)
                otherEmpty = true;
            else
                match = VoxelEdge.BevelsMatch(thisVoxel.edges[edgeI], otherVoxel.edges[edgeI]);
        }

        if (thisVoxel.EdgeIsConvex(edgeI))
        {
            if (!otherEmpty && otherVoxel.EdgeIsConvex(edgeI))
            {
                if (!match)
                    reverseCap = otherVoxel.edges[edgeI];
                return !match;
            }
            else
                return !face;
        }
        else // this voxel is concave
        {
            if (otherEmpty)
                return face;
            else if (otherVoxel.edges[edgeI].hasBevel && !otherVoxel.EdgeIsConvex(edgeI))
                return !match;  // other voxel edge is concave
            else
                return true;
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
        bool mirrored = VoxelFace.GetOrientationMirror(face.orientation);

        // ST space is always upright
        Vector3 positiveS_xyz = POSITIVE_S_XYZ[faceNum]; // positive S in XYZ space
        Vector3 positiveT_xyz = POSITIVE_T_XYZ[faceNum];

        int uRot = VoxelFace.GetOrientationRotation(face.orientation);
        if (mirrored)
            uRot = 5 - uRot;
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

        // use half bevels on all four edges to carve out a smaller block
        // move the face plane to fix the convex hull
        float collapsePlane = 0.0f;
        foreach (int edgeI in Voxel.FaceSurroundingEdges(faceNum))
        {
            var edge = voxel.edges[edgeI];
            var bevelType = edge.bevelType;
            if (edge.bevelSize == VoxelEdge.BevelSize.HALF
                && (bevelType == VoxelEdge.BevelType.SQUARE
                || bevelType == VoxelEdge.BevelType.STAIR_2
                || bevelType == VoxelEdge.BevelType.STAIR_4)
                && voxel.EdgeIsConvex(edgeI))
            {
                float bevelCollapse = edge.bevelTypeArray[1].x;
                if (bevelCollapse > collapsePlane)
                    collapsePlane = bevelCollapse;
            }
            else
            {
                collapsePlane = 1.0f;
                break;
            }
        }
        //if (collapsePlane != 1.0f)
        //    Debug.Log("collapse " + collapsePlane);

        // example for faceNum = 4 (z min)
        // 0 bottom left
        // 1 bottom right (+X)
        // 2 top right
        // 3 top left (+Y)
        for (int i = 0; i < 4; i++)
        {
            FaceCornerVertices corner = corners[i];
            VertexEdges(faceNum, i, out int edgeA, out int edgeB, out int edgeC);
            bool concaveB = voxel.faces[EdgeBOtherFace(faceNum, i)].IsEmpty();
            bool concaveC = voxel.faces[EdgeCOtherFace(faceNum, i)].IsEmpty();
            bool concaveA = concaveB || concaveC;

            // will stay for all planar vertices
            vertexPos[axis] = ((faceNum % 2) - 0.5f) * collapsePlane + 0.5f;
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
                    collapsePlane, edgeA, edgeB, edgeC,
                    vertices, uvs, normals, tangents);
            }
            if (corner.hEdgeC.bevel_i != -1)
            {
                GenerateBevelVertices(voxel, faceNum, voxel.edges[edgeC], corner.hEdgeC, true,
                    concaveC, axis, squarePos, positionOffset, tangent, positiveU_xyz, positiveV_xyz,
                    collapsePlane, edgeA, edgeB, edgeC,
                    vertices, uvs, normals, tangents);
            }
        } // end for each corner
    }

    private static void GenerateBevelVertices(Voxel voxel, int faceNum, VoxelEdge beveledEdge, FaceHalfEdgeVertices hEdge, bool isEdgeC,
        bool concave, int axis, Vector2 squarePos, Vector3 positionOffset, Vector4 tangent, Vector3 positiveU_xyz, Vector3 positiveV_xyz,
        float collapsePlane, int edgeA, int edgeB, int edgeC,
        Vector3[] vertices, Vector2[] uvs, Vector3[] normals, Vector4[] tangents)
    {
        bool isEdgeB = !isEdgeC;
        bool aHasBevel = voxel.edges[edgeA].hasBevel;
        bool bHasBevel = voxel.edges[edgeB].hasBevel;
        bool cHasBevel = voxel.edges[edgeC].hasBevel;

        Vector3 capNormal = Vector3.zero;
        if (hEdge.cap_i != -1)
        {
            vertexPos[axis] = faceNum % 2;
            vertexPos[(axis + 1) % 3] = squarePos.x;
            vertexPos[(axis + 2) % 3] = squarePos.y;
            if (hEdge.reverseCap != null)
            {
                // found all this with trial and error sorry in advance
                VoxelEdge reverseEdge = hEdge.reverseCap.Value;
                if (reverseEdge.hasBevel && reverseEdge.bevelSizeFloat < beveledEdge.bevelSizeFloat)
                    hEdge.reverseCap = beveledEdge;
                if (bHasBevel && cHasBevel)  // joined
                {
                    vertexPos[(axis + 1) % 3] = ApplyBevel(vertexPos[(axis + 1) % 3], voxel.edges[edgeC]);
                    vertexPos[(axis + 2) % 3] = ApplyBevel(vertexPos[(axis + 2) % 3], voxel.edges[edgeB]);
                }
                else if (isEdgeC)
                    vertexPos[(axis + 1) % 3] = ApplyBevel(vertexPos[(axis + 1) % 3], hEdge.reverseCap.Value);
                else // isEdgeB
                    vertexPos[(axis + 2) % 3] = ApplyBevel(vertexPos[(axis + 2) % 3], hEdge.reverseCap.Value);
            }
            vertices[hEdge.cap_i] = Vector3FromArray(vertexPos) + positionOffset;
            if (hEdge.reverseCap != null)
            {
                // extra vertex to make a square
                if (aHasBevel)
                {
                    vertexPos[axis] = ApplyBevel(faceNum % 2, beveledEdge);
                    if (isEdgeB)
                        vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x, cHasBevel ? voxel.edges[edgeC] : voxel.edges[edgeA]);
                    else
                        vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y, bHasBevel ? voxel.edges[edgeB] : voxel.edges[edgeA]);
                }
                else
                    vertexPos[axis] = ApplyBevel(faceNum % 2, hEdge.reverseCap.Value);
                vertices[hEdge.cap_i + hEdge.cap_count - 1] = Vector3FromArray(vertexPos) + positionOffset;
                vertexPos[axis] = faceNum % 2; // reset
            }

            tangents[hEdge.cap_i] = tangent; // TODO
            // uv TODO
            // 0.29289f = 1 - 1/sqrt(2) for some reason
            vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x, beveledEdge, 0.29289f);
            vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y, beveledEdge, 0.29289f);
            uvs[hEdge.cap_i] = CalcUV(voxel, vertexPos, positiveU_xyz, positiveV_xyz);
            if (hEdge.reverseCap != null)
            {
                tangents[hEdge.cap_i + hEdge.cap_count - 1] = tangent;
                vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x, hEdge.reverseCap.Value, 0);
                vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y, hEdge.reverseCap.Value, 0);
                uvs[hEdge.cap_i + hEdge.cap_count - 1] = CalcUV(voxel, vertexPos, positiveU_xyz, positiveV_xyz);
            }

            // normal (b and c are supposed to be swapped)
            vertexPos[axis] = aHasBevel ? (1 - (faceNum % 2) * 2) : 0;
            vertexPos[(axis + 1) % 3] = bHasBevel ? (1 - squarePos.x * 2) : 0;
            vertexPos[(axis + 2) % 3] = cHasBevel ? (1 - squarePos.y * 2) : 0;
            capNormal = Vector3FromArray(vertexPos);
            if (concave ^ (hEdge.reverseCap != null && hEdge.reverseCap.Value.hasBevel))
                capNormal = -capNormal;
            normals[hEdge.cap_i] = capNormal;
            if (hEdge.reverseCap != null)
                normals[hEdge.cap_i + hEdge.cap_count - 1] = capNormal;
        }

        // 3 curved bevels joined at corner
        bool isSphereCorner = !concave
                && voxel.edges[edgeA].bevelType == VoxelEdge.BevelType.CURVE
                && voxel.edges[edgeB].bevelType == VoxelEdge.BevelType.CURVE
                && voxel.edges[edgeC].bevelType == VoxelEdge.BevelType.CURVE;
        bool isFullSphereCorner = isSphereCorner
                && voxel.edges[edgeA].bevelSize == VoxelEdge.BevelSize.FULL
                && voxel.edges[edgeB].bevelSize == VoxelEdge.BevelSize.FULL
                && voxel.edges[edgeC].bevelSize == VoxelEdge.BevelSize.FULL;
        Vector3 sphereOrigin = Vector3.zero;
        if (isSphereCorner)
        {
            vertexPos[axis] = ApplyBevel(faceNum % 2, beveledEdge, 0);
            vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x, beveledEdge, 0);
            vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y, beveledEdge, 0);
            sphereOrigin = Vector3FromArray(vertexPos);
            sphereOrigin += positionOffset;
        }

        Vector2[] bevelArray = beveledEdge.bevelTypeArray;
        float lastY = bevelArray[bevelArray.Length - 1].y;
        if (isFullSphereCorner)
            lastY = FixSphereCornerBevel(new Vector2(0, lastY), bevelArray).y;
        int bevelVertex = hEdge.bevel_i;
        for (int bevelI = 0; bevelI < bevelArray.Length; bevelI++)
        {
            Vector2 bevelVector = bevelArray[bevelI];
            if (isFullSphereCorner)
                bevelVector = FixSphereCornerBevel(bevelVector, bevelArray);

            float xCoord = bevelVector.x;
            float yCoord;
            if (concave)
                xCoord = 2 - xCoord;
            vertexPos[axis] = ApplyBevel(faceNum % 2, beveledEdge, xCoord);
            if (bevelI == 0)
                vertexPos[axis] = (vertexPos[axis] - 0.5f) * collapsePlane + 0.5f;
            if (concave)
                xCoord = 1; // concave bevels aren't joined
            if (concave && isEdgeB)
                // fix joined concave and convex edges
                // special zero check for SHAPE_SQUARE
                yCoord = lastY != 0 ? bevelVector.y / lastY : ((float)bevelI / (bevelArray.Length - 1));
            else
                yCoord = bevelVector.y;
            vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x, cHasBevel ? voxel.edges[edgeC] : voxel.edges[edgeA],
                cHasBevel ? yCoord : xCoord);
            if (concave && !isEdgeB)
                yCoord = lastY != 0 ? bevelVector.y / lastY : ((float)bevelI / (bevelArray.Length - 1));
            else
                yCoord = bevelVector.y;
            vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y, bHasBevel ? voxel.edges[edgeB] : voxel.edges[edgeA],
                bHasBevel ? yCoord : xCoord);
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
            vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x, cHasBevel ? voxel.edges[edgeC] : voxel.edges[edgeA],
                cHasBevel ? uvCoord : xCoord);
            vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y, bHasBevel ? voxel.edges[edgeB] : voxel.edges[edgeA],
                bHasBevel ? uvCoord : xCoord);
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
        if (!isSphereCorner)
        {
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
        else  // sphere
        {
            for (int i = hEdge.bevel_i; i < bevelVertex; i++)
            {
                normals[i] = vertices[i] - sphereOrigin;
                if (concave)
                    normals[i] = -normals[i];
            }
        }
    }


    private static Vector3 Vector3FromArray(float[] vector) =>
        new Vector3(vector[0], vector[1], vector[2]);

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

        int triangleCount = 0;
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
            VertexEdges(faceNum, i, out int edgeA, out int edgeB, out int edgeC);
            if (voxel.edges[edgeA].hasBevel && voxel.edges[edgeA].bevelSize == VoxelEdge.BevelSize.FULL
                && voxel.EdgeIsConvex(edgeA))
            {
                if (!voxel.edges[edgeB].hasBevel && !voxel.edges[edgeC].hasBevel)
                    noInnerQuad = true; // quad would be convex which might cause problems
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

        if (!noInnerQuad)
            triangleCount += 6;

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

    private static Vector2 FixSphereCornerBevel(Vector2 bevelVector, Vector2[] bevelArray)
    {
        float maxExtent = bevelArray[bevelArray.Length - 1].x;  // x and y should be equal
        // jank sphere
        // why tf is this the Euler–Mascheroni constant
        float targetExtent = 0.57721f;  // 1/3 works for flat bevels
        bevelVector.y *= targetExtent / maxExtent;
        bevelVector.x = (bevelVector.x - 1) * (1 - targetExtent) / (1 - maxExtent) + 1;
        return bevelVector;
    }

    private static float ApplyBevel(float coord, VoxelEdge edge, float bevelCoord = 0.0f)
    {
        if (edge.hasBevel)
            return (coord - 0.5f) * (1 - edge.bevelSizeFloat * 2 * (1 - bevelCoord)) + 0.5f;
        else
            return coord;
    }
}