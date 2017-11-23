using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VoxelFace
{
    public Material material;
    public Material overlay;
    public byte orientation;
    public bool selected;

    public bool IsEmpty()
    {
        return material == null && overlay == null;
    }

    public void Clear()
    {
        material = null;
        overlay = null;
        orientation = 0;
        selected = false;
    }
}

public struct VoxelFaceReference
{
    public Voxel voxel;
    public int faceI;

    public VoxelFaceReference(Voxel voxel, int faceI)
    {
        this.voxel = voxel;
        this.faceI = faceI;
    }

    public VoxelFace face
    {
        get
        {
            return voxel.faces[faceI];
        }
    }
}

public class Voxel : MonoBehaviour
{

    public static VoxelFace EMPTY_FACE = new VoxelFace();
    public static Vector2[] SQUARE_LOOP = new Vector2[]
    {
        Vector2.zero,
        Vector2.right,
        Vector2.one,
        Vector2.up
    };

    public static Material selectedMaterial; // set by VoxelArray instance

    public static Vector3 NormalForFaceI(int faceI)
    {
        switch (faceI)
        {
            case 0:
                return Vector3.right;
            case 1:
                return Vector3.left;
            case 2:
                return Vector3.up;
            case 3:
                return Vector3.down;
            case 4:
                return Vector3.forward;
            case 5:
                return Vector3.back;
            default:
                return Vector3.zero;
        }
    }

    public static int FaceIForNormal(Vector3 normal)
    {
        if (normal == Vector3.right)
            return 0;
        else if (normal == Vector3.left)
            return 1;
        else if (normal == Vector3.up)
            return 2;
        else if (normal == Vector3.down)
            return 3;
        else if (normal == Vector3.forward)
            return 4;
        else if (normal == Vector3.back)
            return 5;
        else
            return -1;
    }

    public static int OppositeFaceI(int faceI)
    {
        return (faceI / 2) * 2 + (faceI % 2 == 0 ? 1 : 0);
    }

    public VoxelFace[] faces = new VoxelFace[6]; // xMin, xMax, yMin, yMax, zMin, zMax

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

    public bool IsEmpty()
    {
        foreach (VoxelFace face in faces)
        {
            if (!face.IsEmpty())
                return false;
        }
        return true;
    }

    public void Clear()
    {
        for (int faceI = 0; faceI < faces.Length; faceI++)
        {
            faces[faceI].Clear();
        }
    }

    void OnDestroy()
    {
        VoxelArray array = transform.parent.GetComponent<VoxelArray>();
        array.VoxelDestroyed(this);
    }

    public void UpdateVoxel()
    {
        int numFilledFaces = 0;
        foreach (VoxelFace f in faces)
            if (!f.IsEmpty())
                numFilledFaces++;

        var vertices = new Vector3[numFilledFaces * 4];
        var uv = new Vector2[numFilledFaces * 4];

        float[] transformPos = new float[]
        {
            transform.position.x, transform.position.y, transform.position.z
        };
        numFilledFaces = 0;
        int numMaterials = 0;
        for (int faceNum = 0; faceNum < 6; faceNum++)
        {
            VoxelFace face = faces[faceNum];
            if (face.IsEmpty())
                continue;
            int axis = faceNum / 2;

            // example for faceNum = 5 (z min)
            // 0 bottom left
            // 1 bottom right
            // 2 top right
            // 3 top left
            for (int i = 0; i < 4; i++)
            {
                int vertexI = numFilledFaces * 4 + i;
                float[] vertexPos = new float[3];
                vertexPos[axis] = faceNum % 2;
                vertexPos[(axis + 1) % 3] = SQUARE_LOOP[i].x;
                vertexPos[(axis + 2) % 3] = SQUARE_LOOP[i].y;
                vertices[vertexI] = new Vector3(vertexPos[0], vertexPos[1], vertexPos[2]);
                int uvNum = face.orientation & 3;
                if (faceNum == 1 || faceNum == 2 || faceNum == 4)
                    uvNum += 1;
                if ((((face.orientation & 4) != 0) ^ (faceNum % 2) == 1))
                    uvNum += i;
                else
                    uvNum += 4 - i;
                uvNum %= 4;
                Vector2 uvOrigin; // materials can span multiple voxels
                if ((face.orientation & 3) % 2 == 1 ^ (faceNum == 0 || faceNum == 1))
                    uvOrigin = new Vector2(transformPos[(axis + 2) % 3], transformPos[(axis + 1) % 3]);
                else
                    uvOrigin = new Vector2(transformPos[(axis + 1) % 3], transformPos[(axis + 2) % 3]);
                uv[vertexI] =  uvOrigin + SQUARE_LOOP[uvNum];
            }

            numFilledFaces++;
            if (face.material != null)
                numMaterials++;
            if (face.overlay != null)
                numMaterials++;
            if (face.selected)
                numMaterials++;
        }
        
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.name = "Voxel Mesh";
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.subMeshCount = numMaterials;

        Material[] materials = new Material[numMaterials];

        numFilledFaces = 0;
        numMaterials = 0;
        for (int faceNum = 0; faceNum < 6; faceNum++)
        {
            VoxelFace face = faces[faceNum];
            if (face.IsEmpty())
                continue;

            var triangles = new int[6];

            if (faceNum % 2 == 0)
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

            if (face.material != null) {
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
            if (face.selected)
            {
                materials[numMaterials] = selectedMaterial;
                mesh.SetTriangles(triangles, numMaterials);
                numMaterials++;
            }

            numFilledFaces++;
        }
        
        mesh.RecalculateNormals();

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.materials = materials;
        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider != null)
            collider.sharedMesh = mesh;
    }
}
