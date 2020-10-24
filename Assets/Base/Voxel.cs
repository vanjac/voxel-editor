using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VoxelFace
{
    public static VoxelFace EMPTY_FACE = new VoxelFace();

    public bool cap;  // shouldn't be set if face is empty
    public Material material;
    public Material overlay;
    public byte orientation;
    public bool addSelected, storedSelected;

    public bool IsEmpty()
    {
        return material == null && overlay == null;
    }

    public bool IsReal()
    {
        return !IsEmpty() && !cap;
    }

    public void Clear()
    {
        cap = false;
        material = null;
        overlay = null;
        orientation = 0;
        addSelected = false;
        storedSelected = false;
    }

    public VoxelFace PaintOnly()
    {
        VoxelFace paintOnly = this;
        paintOnly.cap = false;
        paintOnly.addSelected = false;
        paintOnly.storedSelected = false;
        return paintOnly;
    }

    public override bool Equals(object obj)
    {
        return obj is VoxelFace && this == (VoxelFace)obj;
    }

    public static bool operator ==(VoxelFace s1, VoxelFace s2)
    {
        return s1.material == s2.material && s1.overlay == s2.overlay
            && s1.orientation == s2.orientation;
    }
    public static bool operator !=(VoxelFace s1, VoxelFace s2)
    {
        return !(s1 == s2);
    }

    public override int GetHashCode()
    {
        int result = material.GetHashCode();
        result = 37 * result + overlay.GetHashCode();
        result = 37 * result + (int)orientation;
        return result;
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
    public bool hasBevel;
    public bool addSelected, storedSelected;

    public void Clear()
    {
        hasBevel = false;
        addSelected = storedSelected = false;
    }
}


public class Voxel
{
    public readonly static int[] SQUARE_LOOP_COORD_INDEX = new int[] { 0, 1, 3, 2 };

    public enum BevelType : byte
    {
        NONE, FLAT, CURVE, SQUARE, STAIR_2, STAIR_4
    }

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

    public static int FaceSurroundingEdge(int faceNum, int faceEdgeNum)
    {
        int axis = FaceIAxis(faceNum);
        switch(faceEdgeNum)
        {
            case 0:
                return ((axis + 1) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) * 2]; // 0 - 1
            case 1:
                return ((axis + 2) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) + 2]; // 1 - 2
            case 2:
                return ((axis + 1) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) * 2 + 1]; // 2 - 3
            case 3:
                return ((axis + 2) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2)]; // 3 - 0
            default:
                return -1;
        }
    }

    public static int OrthogonalEdge(int faceNum, int cornerI)
    {
        return Voxel.FaceIAxis(faceNum) * 4 + cornerI;
    }


    // describes the part of the face coplanar with the side of the voxel
    public struct FaceProfile
    {
        public bool isFlat;  // if false, face has no profile
        public BevelType bevelType;  // NONE if not beveled
        public bool concaveBevel;
        public int beveledEdge;  // -1 if not beveled
        public int beveledCorner;  // same but as a corner 0-3 of the face

        public bool Matches(FaceProfile other)
        {
            if (isFlat != other.isFlat)
                return false;
            if (!isFlat)
                return true;
            if (bevelType != other.bevelType)
                return false;
            if (bevelType == BevelType.NONE)
                return true;
            return beveledEdge == other.beveledEdge && concaveBevel == other.concaveBevel;
        }
    }


    public Vector3Int position;
    // see "Voxel Diagram.skp" for a diagram of face/edge numbers
    public VoxelFace[] faces = new VoxelFace[6]; // xMin, xMax, yMin, yMax, zMin, zMax
    // Edges: 0-3: x, 4-7: y, 8-11: z
    // Each group of four follows the pattern (0,0), (1,0), (1,1), (0,1)
    // for the Y/Z axes (0-3), Z/X axes (4-7, note order), or X/Y axes (8-11)
    public VoxelEdge[] edges = new VoxelEdge[12];
    public BevelType bevelType = BevelType.NONE;
    public bool concaveBevel = false;
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
        return !faces[faceA].IsReal() && !faces[faceB].IsReal();
    }

    public bool EdgeIsConvex(int edgeI)
    {
        int faceA, faceB;
        EdgeFaces(edgeI, out faceA, out faceB);
        return faces[faceA].IsReal() && faces[faceB].IsReal();
    }

    public int FaceTransformedEdgeNum(int faceNum, int faceEdgeNum)
    {
        int n = faceEdgeNum + 1;
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

    // doesn't check if face is empty, only checks bevels
    public FaceProfile GetFaceProfile(int faceI)
    {
        // default quad
        FaceProfile profile = new FaceProfile {
            isFlat = true, bevelType = BevelType.NONE, concaveBevel = false,
            beveledEdge = -1, beveledCorner = -1 };

        if (bevelType == BevelType.NONE)
            return profile;

        if (concaveBevel)
        {
            int oppositeFaceI = Voxel.OppositeFaceI(faceI);
            for (int i = 0; i < 4; i++)
            {
                if (edges[Voxel.FaceSurroundingEdge(oppositeFaceI, i)].hasBevel)
                    return profile;  // default quad
            }
        }

        for (int i = 0; i < 4; i++)  // iterate both corners and edges
        {
            if (edges[Voxel.FaceSurroundingEdge(faceI, i)].hasBevel)
            {
                // no profile
                profile = new FaceProfile {
                    isFlat = false, bevelType = BevelType.NONE, concaveBevel = false,
                    beveledEdge = -1, beveledCorner = -1 };
                if (!concaveBevel)
                    return profile;  // faces on concave voxels can have both bevels and bevel profiles
            }
            int edgeA = Voxel.OrthogonalEdge(faceI, i);
            if (edges[edgeA].hasBevel)
            {
                // bevel profile
                profile = new FaceProfile {
                    isFlat = true, bevelType = bevelType, concaveBevel = concaveBevel,
                    beveledEdge = edgeA, beveledCorner = i };
                if (concaveBevel)
                    return profile;
            }
        }

        return profile;
    }

    public int FirstRealFace()
    {
        for (int i = 0; i < 6; i++)
        {
            if (faces[i].IsReal())
                return i;
        }
        return -1;
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
        bevelType = BevelType.NONE;
        concaveBevel = false;
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

    public Voxel GetSingleBlock()
    {
        return voxels[0];
    }

    public Substance GetSubstance()
    {
        if (IsSingleBlock())
            return GetSingleBlock().substance;
        else
            return null;
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
                var faceVertices = GetFaceVertices(voxel, faceNum, numVertices);
                info.faces[faceNum] = faceVertices;
                numVertices += faceVertices.count;
                try
                {
                    info.faceTriangles[faceNum] = GenerateFaceTriangles(voxel, faceNum, faceVertices);
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
                    GenerateFaceVertices(info.voxel, faceNum, info.faces[faceNum],
                        vertices, uvs, normals, tangents);
                }
                catch (System.IndexOutOfRangeException ex)
                {
                    Debug.LogError(ex);
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
            foreach (VoxelEdge edge in GetSingleBlock().edges)
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
            Bounds bounds = GetSingleBlock().GetBounds();
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
        public FaceVertices[] faces;
        public int[][] faceTriangles;

        public VoxelMeshInfo(Voxel voxel)
        {
            this.voxel = voxel;
            faces = new FaceVertices[6];
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

    private struct FaceVertices
    {
        public int count;

        // triangle fan on the plane of the face
        // facePlane_i is always the lowest-numbered vertex of the face
        public int facePlane_i, facePlane_count;
        // if one of the corners edgeA is beveled, otherwise -1
        public int bevelProfileCorner;

        // index is faceEdgeNum (see FaceSurroundingEdge)
        public FaceEdgeVertices[] edges;

        public FaceVertices(int ignored)
        {
            count = facePlane_count = 0;
            facePlane_i = bevelProfileCorner = -1;
            edges = new FaceEdgeVertices[4];
            for (int i = 0; i < 4; i++)
                edges[i] = new FaceEdgeVertices(0);
        }
    }

    private struct FaceEdgeVertices
    {
        // bevel vertices alternate sides of the edge (interleaved)
        // Additionally he middle bevel vertices (not including first/last) are doubled up
        // -- same position, (potentially) different normals.
        // See GetBevelVertices
        public int bevel_i, bevel_count;

        public FaceEdgeVertices(int ignored)
        {
            bevel_count = 0;
            bevel_i = -1;
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


    public static Vector2[] BevelTypeArray(Voxel voxel)
    {
        switch (voxel.bevelType)
        {
            case Voxel.BevelType.SQUARE:
                return SHAPE_SQUARE;
            case Voxel.BevelType.FLAT:
                return SHAPE_FLAT;
            case Voxel.BevelType.CURVE:
                return SHAPE_CURVE;
            case Voxel.BevelType.STAIR_2:
                return SHAPE_STAIR_2;
            case Voxel.BevelType.STAIR_4:
                return SHAPE_STAIR_4;
            default:
                return System.Array.Empty<Vector2>();
        }
    }

    public static Vector2[] BevelTypeNormalArray(Voxel voxel)
    {
        switch (voxel.bevelType)
        {
            case Voxel.BevelType.SQUARE:
                return NORMALS_SQUARE;
            case Voxel.BevelType.FLAT:
                return NORMALS_FLAT;
            case Voxel.BevelType.CURVE:
                return NORMALS_CURVE;
            case Voxel.BevelType.STAIR_2:
                return NORMALS_STAIR_2;
            case Voxel.BevelType.STAIR_4:
                return NORMALS_STAIR_4;
            default:
                return System.Array.Empty<Vector2>();
        }
    }


    private static FaceVertices GetFaceVertices(Voxel voxel, int faceNum, int vertexI)
    {
        FaceVertices vertices = new FaceVertices(0);
        if (voxel.faces[faceNum].IsEmpty())
            return vertices;

        // determine face plane
        vertices.facePlane_i = vertexI;
        Voxel.FaceProfile profile = voxel.GetFaceProfile(faceNum);
        if (profile.isFlat)
        {
            if (profile.bevelType == Voxel.BevelType.NONE)
            {
                vertices.facePlane_count = 4;  // default quad
            }
            else
            {
                vertices.facePlane_count = BevelTypeArray(voxel).Length * 2;
                vertices.bevelProfileCorner = profile.beveledCorner;
            }
        }
        vertices.count += vertices.facePlane_count;
        vertexI += vertices.facePlane_count;

        // determine bevel vertices
        for (int faceEdgeNum = 0; faceEdgeNum < 4; faceEdgeNum++)
        {
            int edgeI = Voxel.FaceSurroundingEdge(faceNum, faceEdgeNum);
            if (voxel.edges[edgeI].hasBevel)
                vertices.count += GetBevelVertices(ref vertexI, voxel, ref vertices.edges[faceEdgeNum]);
        }
        return vertices;
    }

    // return number of vertices added
    private static int GetBevelVertices(ref int vertexI, Voxel voxel,
        ref FaceEdgeVertices hEdge)
    {
        hEdge.bevel_i = vertexI;
        // for each side
        hEdge.bevel_count = 2 * (BevelTypeArray(voxel).Length * 2 - 2);
        vertexI += hEdge.bevel_count;
        return hEdge.bevel_count;
    }


    // TODO this might actually be bad
    // https://stackoverflow.com/q/30205997
    private static float[] vertexPos = new float[3]; // reusable
    private static float[] vertexUVPos = new float[3];
    private void GenerateFaceVertices(Voxel voxel, int faceNum, FaceVertices faceVerts,
        Vector3[] vertices, Vector2[] uvs, Vector3[] normals, Vector4[] tangents)
    {
        VoxelFace face = voxel.faces[faceNum];
        if (face.IsEmpty())
            return;

        Vector3 positionOffset = voxel.position - transform.position;
        int axis = Voxel.FaceIAxis(faceNum);
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
        
        // generate face plane
        if (faceVerts.facePlane_count != 0)
        {
            Vector3 normal = Voxel.DirectionForFaceI(faceNum);

            vertexPos[axis] = faceNum % 2; // will stay for all planar vertices
            if (faceVerts.bevelProfileCorner == -1)
            {
                // default quad
                if (faceVerts.facePlane_count != 4)
                    Debug.LogError("Invalid facePlane state!");
                for (int i = 0; i < 4; i++)
                {
                    Vector2 squarePos = SQUARE_LOOP[i];
                    vertexPos[(axis + 1) % 3] = squarePos.x;
                    vertexPos[(axis + 2) % 3] = squarePos.y;
                    vertices[faceVerts.facePlane_i + i] = Vector3FromArray(vertexPos) + positionOffset;
                }
            }
            else
            {
                // bevel profile
                // start with triangle fan origin (opposite corner)
                Vector2 squarePos;
                squarePos = SQUARE_LOOP[(faceVerts.bevelProfileCorner + 2) % 4];
                vertexPos[(axis + 1) % 3] = squarePos.x;
                vertexPos[(axis + 2) % 3] = squarePos.y;
                vertices[faceVerts.facePlane_i] = Vector3FromArray(vertexPos) + positionOffset;

                // then bevel vertices
                squarePos = SQUARE_LOOP[faceVerts.bevelProfileCorner];
                Vector2[] bevelArray = BevelTypeArray(voxel);
                for (int bevelI = 0; bevelI < bevelArray.Length; bevelI++)
                {
                    Vector2 bevelVector = bevelArray[bevelI];
                    if (voxel.concaveBevel)
                        bevelVector = new Vector2(1.0f - bevelVector.y, 1.0f - bevelVector.x);
                    if (faceVerts.bevelProfileCorner % 2 == 1) // reverse direction
                        bevelVector = new Vector2(bevelVector.y, bevelVector.x);
                    vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x, bevelVector.x);
                    vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y, bevelVector.y);
                    vertices[faceVerts.facePlane_i + bevelI + 1] = Vector3FromArray(vertexPos) + positionOffset;

                    vertexPos[(axis + 1) % 3] = ApplyBevel(squarePos.x, bevelVector.y); // x/y are swapped
                    vertexPos[(axis + 2) % 3] = ApplyBevel(squarePos.y, bevelVector.x);
                    // last iteration vertices will overlap
                    vertices[faceVerts.facePlane_i + faceVerts.facePlane_count - 1 - bevelI]
                        = Vector3FromArray(vertexPos) + positionOffset;
                }
            }

            for (int i = 0; i < faceVerts.facePlane_count; i++)
            {
                int vIndex = faceVerts.facePlane_i + i;
                // undo positionOffset
                uvs[vIndex] = CalcUV(voxel, vertices[vIndex] + transform.position, positiveU_xyz, positiveV_xyz);
                normals[vIndex] = normal;
                tangents[vIndex] = tangent;
            }
        }  // end if face plane

        for (int faceEdgeNum = 0; faceEdgeNum < 4; faceEdgeNum++)
        {
            if (faceVerts.edges[faceEdgeNum].bevel_i != -1)
            {
                GenerateBevelVertices(voxel, faceNum, faceEdgeNum, faceVerts.edges[faceEdgeNum],
                    positionOffset, tangent, positiveU_xyz, positiveV_xyz,
                    vertices, uvs, normals, tangents);
            }
        }
    }

    private static void GenerateBevelVertices(Voxel voxel, int faceNum, int faceEdgeNum, FaceEdgeVertices edgeVerts,
        Vector3 positionOffset, Vector4 tangent, Vector3 positiveU_xyz, Vector3 positiveV_xyz,
        Vector3[] vertices, Vector2[] uvs, Vector3[] normals, Vector4[] tangents)
    {
        int edgeI = Voxel.FaceSurroundingEdge(faceNum, faceEdgeNum);
        int[] adjEdgeI = new int[] { Voxel.FaceSurroundingEdge(faceNum, (faceEdgeNum + 3) % 4),
            Voxel.FaceSurroundingEdge(faceNum, (faceEdgeNum + 1) % 4) };
        int[] edgeA = new int[] { Voxel.OrthogonalEdge(faceNum, faceEdgeNum),
            Voxel.OrthogonalEdge(faceNum, (faceEdgeNum + 1) % 4) };

        // orthogonal to face
        int faceAxis = Voxel.FaceIAxis(faceNum);
        int edgeAxis = Voxel.EdgeIAxis(edgeI);  // parallel to edge
        // orthogonal to edge
        int orthAxis = (faceAxis + 1) % 3 == edgeAxis ? (faceAxis + 2) % 3 : (faceAxis + 1) % 3;

        Vector2[] squarePos = new Vector2[] {
            SQUARE_LOOP[faceEdgeNum], SQUARE_LOOP[(faceEdgeNum + 1) % 4] };

        Vector2[] bevelArray = BevelTypeArray(voxel);
        int bevelVertex = edgeVerts.bevel_i;
        Vector3? sphereOrigin = null;
        for (int bevelI = 0; bevelI < bevelArray.Length; bevelI++)
        {
            float uvCoord = (float)bevelI / (float)(bevelArray.Length - 1);
            for (int j = 0; j < 2; j++)
            {
                vertexPos[faceAxis] = faceNum % 2;
                vertexPos[(faceAxis + 1) % 3] = squarePos[j].x;
                vertexPos[(faceAxis + 2) % 3] = squarePos[j].y;

                Vector2 bevelVector = bevelArray[bevelI];
                if (voxel.bevelType == Voxel.BevelType.CURVE && voxel.edges[edgeI].hasBevel
                    && voxel.edges[adjEdgeI[j]].hasBevel && voxel.edges[edgeA[j]].hasBevel)
                {
                    // 3 curved bevels joined at corner
                    float maxExtent = bevelArray[bevelArray.Length - 1].x;  // x and y should be equal
                    // jank sphere
                    // why tf is this the Euler–Mascheroni constant
                    float targetExtent = 0.57721f;  // 1/3 works for flat bevels
                    bevelVector.y *= targetExtent / maxExtent;
                    bevelVector.x = (bevelVector.x - 1) * (1 - targetExtent) / (1 - maxExtent) + 1;
                    sphereOrigin = Vector3FromArray(vertexPos);
                    if (!voxel.concaveBevel)
                        sphereOrigin = Vector3.one - sphereOrigin;
                    sphereOrigin += positionOffset;
                }

                vertexPos[faceAxis] = ApplyBevel(vertexPos[faceAxis], bevelVector.x);
                System.Array.Copy(vertexPos, vertexUVPos, 3);

                if (voxel.edges[edgeI].hasBevel)
                {
                    vertexPos[orthAxis] = ApplyBevel(vertexPos[orthAxis], bevelVector.y);
                    vertexUVPos[orthAxis] = ApplyBevel(vertexUVPos[orthAxis], uvCoord);
                }
                else if (voxel.edges[edgeA[j]].hasBevel)
                {
                    vertexPos[orthAxis] = ApplyBevel(vertexPos[orthAxis], bevelVector.x);
                    vertexUVPos[orthAxis] = vertexPos[orthAxis];
                }
                if (voxel.edges[adjEdgeI[j]].hasBevel)
                {
                    vertexPos[edgeAxis] = ApplyBevel(vertexPos[edgeAxis], bevelVector.y);
                    vertexUVPos[edgeAxis] = ApplyBevel(vertexUVPos[edgeAxis], uvCoord);
                }
                else if (voxel.edges[edgeA[j]].hasBevel)
                {
                    vertexPos[edgeAxis] = ApplyBevel(vertexPos[edgeAxis], bevelVector.x);
                    vertexUVPos[edgeAxis] = vertexPos[edgeAxis];
                }
                Vector3 vert = Vector3FromArray(vertexPos);
                if (voxel.concaveBevel)
                    vert = Vector3.one - vert;
                vertices[bevelVertex] = vert + positionOffset;
                uvs[bevelVertex] = CalcUV(voxel, Vector3FromArray(vertexUVPos) + voxel.position, positiveU_xyz, positiveV_xyz);
                tangents[bevelVertex] = tangent; // TODO

                bevelVertex++;
            }

            if (bevelI != 0 && bevelI != bevelArray.Length - 1)
            {
                for (int i = 0; i < 2; i++)
                {
                    vertices[bevelVertex] = vertices[bevelVertex - 2];
                    tangents[bevelVertex] = tangents[bevelVertex - 2];
                    uvs[bevelVertex] = uvs[bevelVertex - 2];
                    bevelVertex++;
                }
            }
        }

        // add normals for each bevel vertex
        Vector2[] bevelNormalArray = BevelTypeNormalArray(voxel);
        if (sphereOrigin == null)
        {
            for (int bevelI = 0; bevelI < bevelNormalArray.Length; bevelI++)
            {
                Vector2 normalVector = bevelNormalArray[bevelI];
                vertexPos[faceAxis] = normalVector.x * ((faceNum % 2) * 2 - 1);
                // shouldn't matter which square pos we use
                // along only the edge axis they will be identical
                vertexPos[(faceAxis + 1) % 3] = squarePos[0].x;
                vertexPos[(faceAxis + 2) % 3] = squarePos[0].y;
                vertexPos[orthAxis] = normalVector.y * (vertexPos[orthAxis] * 2 - 1);
                vertexPos[edgeAxis] = 0;
                Vector3 normal = Vector3FromArray(vertexPos);
                normals[edgeVerts.bevel_i + bevelI * 2] = normal;
                normals[edgeVerts.bevel_i + bevelI * 2 + 1] = normal;
            }
        }
        else  // sphere
        {
            for (int i = edgeVerts.bevel_i; i < bevelVertex; i++)
            {
                normals[i] = vertices[i] - sphereOrigin.Value;
                if (voxel.concaveBevel)
                    normals[i] = -normals[i];
            }
        }
    }


    private static Vector3 Vector3FromArray(float[] vector)
    {
        return new Vector3(vector[0], vector[1], vector[2]);
    }

    private static void Vector3ToArray(Vector3 vector, float[] array)
    {
        array[0] = vector.x;
        array[1] = vector.y;
        array[2] = vector.z;
    }

    private static Vector2 CalcUV(Voxel voxel, Vector3 vector, Vector3 positiveU_xyz, Vector3 positiveV_xyz)
    {
        return new Vector2(
            vector.x * positiveU_xyz.x + vector.y * positiveU_xyz.y + vector.z * positiveU_xyz.z,
            vector.x * positiveV_xyz.x + vector.y * positiveV_xyz.y + vector.z * positiveV_xyz.z);
    }


    private static int[] GenerateFaceTriangles(Voxel voxel, int faceNum, FaceVertices vertices)
    {
        if (voxel.faces[faceNum].IsEmpty())
            return new int[0];

        int triangleCount = 0;
        if (vertices.facePlane_count != 0)
            triangleCount += 3 * (vertices.facePlane_count - 2); // triangle fan
        for (int faceEdgeNum = 0; faceEdgeNum < 4; faceEdgeNum++)
        {
            if (vertices.edges[faceEdgeNum].bevel_count != 0)
            {
                triangleCount += 6 * (BevelTypeArray(voxel).Length - 1);
            }
        }

        var triangles = new int[triangleCount];
        triangleCount = 0;
        bool faceCCW = faceNum % 2 == 1;

        if (vertices.facePlane_count != 0)
        {
            for (int fanI = 1; fanI < vertices.facePlane_count - 1; fanI++)
            {
                AddTriangle(triangles, triangleCount, !faceCCW,
                    vertices.facePlane_i,
                    vertices.facePlane_i + fanI + 1,
                    vertices.facePlane_i + fanI);
                triangleCount += 3;
            }
        }

        for (int faceEdgeNum = 0; faceEdgeNum < 4; faceEdgeNum++)
        {
            FaceEdgeVertices edgeVerts = vertices.edges[faceEdgeNum];
            if (edgeVerts.bevel_count == 0)
                continue;
            for (int bevelI = 0; bevelI < edgeVerts.bevel_count / 4; bevelI++)
            {
                QuadTriangles(triangles, triangleCount, faceCCW,
                    edgeVerts.bevel_i + bevelI * 4,
                    edgeVerts.bevel_i + bevelI * 4 + 2,
                    edgeVerts.bevel_i + bevelI * 4 + 3,
                    edgeVerts.bevel_i + bevelI * 4 + 1);
                triangleCount += 6;
            }
        }

        return triangles;
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
            for (int faceEdgeNum = 0; faceEdgeNum < 4; faceEdgeNum++)
            {
                int edgeI = Voxel.FaceSurroundingEdge(faceNum, faceEdgeNum);
                var e = voxel.edges[edgeI];
                if (e.addSelected || e.storedSelected)
                {
                    int n = voxel.FaceTransformedEdgeNum(faceNum, faceEdgeNum);
                    highlightNum |= 1 << n;
                }
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
            // for debugging caps
            //bool[] capFaces = new bool[6];
            //for (int i = 0; i < 6; i++)
            //    capFaces[i] = voxel.faces[i].cap;
            //yield return new VoxelMaterialInfo(highlightMaterials[15], capFaces);
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


    private static float ApplyBevel(float coord, float bevelCoord = 0.0f)
    {
        return (coord - 0.5f) * (1 - 2 * (1 - bevelCoord)) + 0.5f;
    }
}