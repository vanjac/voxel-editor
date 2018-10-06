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

        int numFilledFaces = 0;
        foreach (VoxelFace f in faces)
            if (!f.IsEmpty())
                numFilledFaces++;

        var vertices = new Vector3[numFilledFaces * 4];
        var uvs = new Vector2[numFilledFaces * 4];
        var normals = new Vector3[numFilledFaces * 4];
        var tangents = new Vector4[numFilledFaces * 4];

        float[] vertexPos = new float[3]; // reusable
        numFilledFaces = 0;
        int numMaterials = 0;
        for (int faceNum = 0; faceNum < 6; faceNum++)
        {
            VoxelFace face = faces[faceNum];
            if (face.IsEmpty())
                continue;
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

            // example for faceNum = 5 (z min)
            // 0 bottom left
            // 1 bottom right
            // 2 top right
            // 3 top left
            for (int i = 0; i < 4; i++)
            {
                int vertexI = numFilledFaces * 4 + i;
                vertexPos[axis] = faceNum % 2;
                vertexPos[(axis + 1) % 3] = SQUARE_LOOP[i].x;
                vertexPos[(axis + 2) % 3] = SQUARE_LOOP[i].y;
                Vector3 vertex = new Vector3(vertexPos[0], vertexPos[1], vertexPos[2]);
                vertices[vertexI] = vertex;

                normals[vertexI] = normal;
                tangents[vertexI] = tangent;

                vertex += transform.position;
                Vector2 uv = new Vector2(
                    vertex.x * positiveU_xyz.x + vertex.y * positiveU_xyz.y + vertex.z * positiveU_xyz.z,
                    vertex.x * positiveV_xyz.x + vertex.y * positiveV_xyz.y + vertex.z * positiveV_xyz.z);
                uvs[vertexI] = uv;
            }

            numFilledFaces++;
            if (xRay)
                numMaterials++;
            else
            {
                if (face.material != null)
                    numMaterials++;
                if (face.overlay != null)
                    numMaterials++;
            }
            if (coloredHighlightMaterial != null)
                numMaterials++;
            if (face.addSelected || face.storedSelected)
                numMaterials++;
        }
        
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.name = "Voxel Mesh";
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.tangents = tangents;
        mesh.subMeshCount = numMaterials;

        Material[] materials = new Material[numMaterials];

        numFilledFaces = 0;
        numMaterials = 0;
        for (int faceI = 0; faceI < 6; faceI++)
        {
            VoxelFace face = faces[faceI];
            if (face.IsEmpty())
                continue;

            var triangles = new int[6];

            if (faceI % 2 == 1)
            {
                triangles[0] = numFilledFaces * 4 + 0;
                triangles[1] = numFilledFaces * 4 + 1;
                triangles[2] = numFilledFaces * 4 + 2;
                triangles[3] = numFilledFaces * 4 + 0;
                triangles[4] = numFilledFaces * 4 + 2;
                triangles[5] = numFilledFaces * 4 + 3;
            }
            else
            {
                triangles[0] = numFilledFaces * 4 + 0;
                triangles[1] = numFilledFaces * 4 + 2;
                triangles[2] = numFilledFaces * 4 + 1;
                triangles[3] = numFilledFaces * 4 + 0;
                triangles[4] = numFilledFaces * 4 + 3;
                triangles[5] = numFilledFaces * 4 + 2;
            }

            if (xRay)
            {
                materials[numMaterials] = xRayMaterial;
                mesh.SetTriangles(triangles, numMaterials);
                numMaterials++;
            }
            else
            {
                if (face.material != null)
                {
                    materials[numMaterials] = face.material;
                    mesh.SetTriangles(triangles, numMaterials);
                    numMaterials++;
                }
                if (face.overlay != null)
                {
                    materials[numMaterials] = face.overlay;
                    mesh.SetTriangles(triangles, numMaterials);
                    numMaterials++;
                }
            }
            if (coloredHighlightMaterial != null)
            {
                materials[numMaterials] = coloredHighlightMaterial;
                mesh.SetTriangles(triangles, numMaterials);
                numMaterials++;
            }
            if (face.addSelected || face.storedSelected)
            {
                materials[numMaterials] = selectedMaterial;
                mesh.SetTriangles(triangles, numMaterials);
                numMaterials++;
            }

            numFilledFaces++;
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
}
