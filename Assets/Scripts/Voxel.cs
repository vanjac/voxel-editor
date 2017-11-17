using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VoxelFace
{
    public Material material;
    public bool selected;

    public bool IsEmpty()
    {
        return material == null;
    }

    public void Clear()
    {
        material = null;
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

    public void UpdateVoxel()
    {
        int numFilledFaces = 0;
        foreach (VoxelFace f in faces)
            if (!f.IsEmpty())
                numFilledFaces++;

        var vertices = new Vector3[numFilledFaces * 4];
        var uv = new Vector2[numFilledFaces * 4];

        numFilledFaces = 0;
        for (int faceNum = 0; faceNum < 6; faceNum++)
        {
            VoxelFace face = faces[faceNum];
            if (face.IsEmpty())
                continue;
            int axis = faceNum / 2;

            // example for faceNum = 5 (z min)
            // 0 bottom left
            // 1 bottom right
            // 2 top left
            // 3 top right
            for (int v = 0; v <= 1; v++)
            {
                for (int u = 0; u <= 1; u++)
                {
                    int vertexI = numFilledFaces * 4 + v * 2 + u;
                    float[] vertexPos = new float[3];
                    vertexPos[axis] = faceNum % 2;
                    vertexPos[(axis + 1) % 3] = u;
                    vertexPos[(axis + 2) % 3] = v;
                    vertices[vertexI] = new Vector3(vertexPos[0], vertexPos[1], vertexPos[2]);
                    uv[vertexI] = new Vector2(u, v);
                }
            }

            numFilledFaces++;
        }
        
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.name = "Voxel Mesh";
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.subMeshCount = numFilledFaces;

        Material[] materials = new Material[numFilledFaces];

        numFilledFaces = 0;
        for (int faceNum = 0; faceNum < 6; faceNum++)
        {
            VoxelFace face = faces[faceNum];
            if (face.IsEmpty())
                continue;
            if (face.selected)
                materials[numFilledFaces] = selectedMaterial;
            else
                materials[numFilledFaces] = face.material;
            var triangles = new int[6];

            if (faceNum % 2 == 0)
            {
                triangles[0] = numFilledFaces * 4 + 0;
                triangles[1] = numFilledFaces * 4 + 1;
                triangles[2] = numFilledFaces * 4 + 3;
                triangles[3] = numFilledFaces * 4 + 0;
                triangles[4] = numFilledFaces * 4 + 3;
                triangles[5] = numFilledFaces * 4 + 2;
            }
            else
            {
                triangles[0] = numFilledFaces * 4 + 0;
                triangles[1] = numFilledFaces * 4 + 3;
                triangles[2] = numFilledFaces * 4 + 1;
                triangles[3] = numFilledFaces * 4 + 0;
                triangles[4] = numFilledFaces * 4 + 2;
                triangles[5] = numFilledFaces * 4 + 3;
            }

            mesh.SetTriangles(triangles, numFilledFaces);

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
