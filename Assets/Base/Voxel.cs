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

        int[] faceVertexCounts = new int[6];
        int numVertices = 0;
        int numMaterials = 0;
        for (int faceNum = 0; faceNum < 6; faceNum++)
        {
            int vertexCount = FaceVertexCount(faceNum);
            faceVertexCounts[faceNum] = vertexCount;
            numVertices += vertexCount;
            numMaterials += FaceMaterialCount(faces[faceNum], xRay, coloredHighlightMaterial);
        }

        var vertices = new Vector3[numVertices];
        var uvs = new Vector2[numVertices];
        var normals = new Vector3[numVertices];
        var tangents = new Vector4[numVertices];
        numVertices = 0;
        for (int faceNum = 0; faceNum < 6; faceNum++)
        {
            GenerateFaceVertices(faceNum, numVertices, vertices, uvs, normals, tangents);
            numVertices += faceVertexCounts[faceNum];
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
        numVertices = 0;
        numMaterials = 0;
        for (int faceNum = 0; faceNum < 6; faceNum++)
        {
            var triangles = GenerateFaceTriangles(faceNum, numVertices);
            if (triangles == null)
                continue;
            numVertices += faceVertexCounts[faceNum];

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

    private int FaceVertexCount(int faceNum)
    {
        if (faces[faceNum].IsEmpty())
            return 0;
        return 4;
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
            int edgeA = axis * 4 + i;
            int edgeB = ((axis + 1) % 3) * 4
                + SQUARE_LOOP_COORD_INDEX[(i >= 2 ? 1 : 0) + (faceNum % 2) * 2];
            int edgeC = ((axis + 2) % 3) * 4
                + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) + (i == 1 || i == 2 ? 2 : 0)];

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
        }
    }


    private int[] GenerateFaceTriangles(int faceNum, int vertexI)
    {
        if (faces[faceNum].IsEmpty())
            return null;
        var triangles = new int[6];
        if (faceNum % 2 == 1)
        {
            triangles[0] = vertexI + 0;
            triangles[1] = vertexI + 1;
            triangles[2] = vertexI + 2;
            triangles[3] = vertexI + 0;
            triangles[4] = vertexI + 2;
            triangles[5] = vertexI + 3;
        }
        else
        {
            triangles[0] = vertexI + 0;
            triangles[1] = vertexI + 2;
            triangles[2] = vertexI + 1;
            triangles[3] = vertexI + 0;
            triangles[4] = vertexI + 3;
            triangles[5] = vertexI + 2;
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


    private float ApplyBevel(float coord, VoxelEdge edge)
    {
        if (edge.bevelType == VoxelEdge.BevelType.NONE)
            return coord;
        else
            return (coord - 0.5f) * (1 - edge.bevelSizeFloat * 2) + 0.5f;
    }
}
