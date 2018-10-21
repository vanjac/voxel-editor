﻿using System.Collections;
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
        NONE, SQUARE, FLAT, CURVE, STAIR_2, STAIR_4
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

    public static int ClosestFaceI(Vector3 point)
    {
        float dist = 2.0f;
        int closestFaceI = -1;
        if (Mathf.Abs(point.x) < dist)
        {
            dist = Mathf.Abs(point.x);
            closestFaceI = 0;
        }
        if (Mathf.Abs(point.x - 1) < dist)
        {
            dist = Mathf.Abs(point.x - 1);
            closestFaceI = 1;
        }
        if (Mathf.Abs(point.y) < dist)
        {
            dist = Mathf.Abs(point.y);
            closestFaceI = 2;
        }
        if (Mathf.Abs(point.y - 1) < dist)
        {
            dist = Mathf.Abs(point.y - 1);
            closestFaceI = 3;
        }
        if (Mathf.Abs(point.z) < dist)
        {
            dist = Mathf.Abs(point.z);
            closestFaceI = 4;
        }
        if (Mathf.Abs(point.z - 1) < dist)
        {
            dist = Mathf.Abs(point.z - 1);
            closestFaceI = 5;
        }
        return closestFaceI;
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
            GenerateFaceVertices(faceNum, faceCorners[faceNum], vertices, uvs, normals, tangents);
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
        public int count;
        // all part of the flat surface of the face:
        public int innerRect_i, edgeB_i, edgeC_i, bevelProfile_i, bevelProfile_count;
        public int bevel_i, bevel_count;
        public FaceCornerVertices(int ignored)
        {
            count = bevelProfile_count = bevel_count = 0;
            innerRect_i = edgeB_i = edgeC_i = bevelProfile_i = bevel_i = -1;
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
                int bevelCount = (edges[edgeB].hasBevel ? edges[edgeB] : edges[edgeC])
                    .bevelTypeArray.Length * 2 - 2;
                corners[i].bevel_i = vertexI;
                corners[i].bevel_count = bevelCount;
                corners[i].count += bevelCount;
                vertexI += bevelCount;
            }
            if (edges[edgeA].hasBevel && !edges[edgeB].hasBevel && !edges[edgeC].hasBevel)
            { // cutout/profile for bevel
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

            vertexPos[axis] = faceNum % 2; // will stay until bevel

            vertexPos[(axis + 1) % 3] = ApplyBevel(SQUARE_LOOP[i].x,
                corner.edgeC_i != -1 ? edges[edgeA] : edges[edgeC]);
            vertexPos[(axis + 2) % 3] = ApplyBevel(SQUARE_LOOP[i].y,
                corner.edgeB_i != -1 ? edges[edgeA] : edges[edgeB]);
            vertices[corner.innerRect_i] = Vector3FromArray(vertexPos);
            uvs[corner.innerRect_i] = CalcUV(vertexPos, positiveU_xyz, positiveV_xyz);
            normals[corner.innerRect_i] = normal;
            tangents[corner.innerRect_i] = tangent;

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
            if (corner.bevel_i != -1)
            {
                VoxelEdge beveledEdge = edges[edgeB].hasBevel ? edges[edgeB] : edges[edgeC];
                Vector2[] bevelArray = beveledEdge.bevelTypeArray;
                int bevelVertex = corner.bevel_i;
                for (int bevelI = 0; bevelI < bevelArray.Length; bevelI++)
                {
                    Vector2 bevelVector = bevelArray[bevelI];
                    vertexPos[axis] = ApplyBevel(faceNum % 2, beveledEdge, bevelVector.x);
                    vertexPos[(axis + 1) % 3] = ApplyBevel(SQUARE_LOOP[i].x, edges[edgeC].hasBevel ? edges[edgeC] : edges[edgeA],
                        edges[edgeC].hasBevel ? bevelVector.y : bevelVector.x);
                    vertexPos[(axis + 2) % 3] = ApplyBevel(SQUARE_LOOP[i].y, edges[edgeB].hasBevel ? edges[edgeB] : edges[edgeA],
                        edges[edgeB].hasBevel ? bevelVector.y : bevelVector.x);
                    vertices[bevelVertex] = Vector3FromArray(vertexPos);
                    tangents[bevelVertex] = tangent;

                    // calc uv
                    float uvCoord = ((float)bevelI + 1) / (float)bevelArray.Length;
                    vertexPos[(axis + 1) % 3] = ApplyBevel(SQUARE_LOOP[i].x, edges[edgeC].hasBevel ? edges[edgeC] : edges[edgeA],
                        edges[edgeC].hasBevel ? uvCoord : bevelVector.x);
                    vertexPos[(axis + 2) % 3] = ApplyBevel(SQUARE_LOOP[i].y, edges[edgeB].hasBevel ? edges[edgeB] : edges[edgeA],
                        edges[edgeB].hasBevel ? uvCoord : bevelVector.x);
                    uvs[bevelVertex] = CalcUV(vertexPos, positiveU_xyz, positiveV_xyz);

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
                    normals[corner.bevel_i + bevelI] = Vector3FromArray(vertexPos);
                }
            }
        }
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
                triangleCount += 6 * (edges[edgeI].bevelTypeArray.Length - 1);
        for (int i = 0; i < 4; i++)
        {
            if (vertices[i].bevelProfile_count != 0)
                triangleCount += 3 * (vertices[i].bevelProfile_count + 1);
            if (i % 2 == 0)
            {
                if (vertices[i].edgeB_i != -1)
                    triangleCount += 6;
            }
            else
            {
                if (vertices[i].edgeC_i != -1)
                    triangleCount += 6;
            }
        }

        var triangles = new int[triangleCount];
        triangleCount = 0;

        QuadTriangles(triangles, triangleCount, faceNum % 2 == 1,
            vertices[0].innerRect_i,
            vertices[1].innerRect_i,
            vertices[2].innerRect_i,
            vertices[3].innerRect_i);
        triangleCount += 6;

        // for each pair of edge vertices
        for (int i = 0; i < 4; i++)
        {
            int j = (i + 1) % 4;
            if (edges[surroundingEdges[i]].hasBevel)
            {
                for (int bevelI = 0; bevelI < vertices[i].bevel_count / 2; bevelI++)
                {
                    QuadTriangles(triangles, triangleCount, faceNum % 2 == 1,
                        vertices[i].bevel_i + bevelI * 2,
                        vertices[i].bevel_i + bevelI * 2 + 1,
                        vertices[j].bevel_i + bevelI * 2 + 1,
                        vertices[j].bevel_i + bevelI * 2);
                    triangleCount += 6;
                }
            }
            if (vertices[i].bevelProfile_count != 0)
            {
                bool profileCCW = (faceNum % 2 == 1) ^ (i % 2 == 0);
                // first
                AddTriangle(triangles, triangleCount, profileCCW,
                    vertices[i].innerRect_i,
                    vertices[i].bevelProfile_i,
                    vertices[i].edgeC_i);
                triangleCount += 3;
                // last
                AddTriangle(triangles, triangleCount, profileCCW,
                    vertices[i].innerRect_i,
                    vertices[i].edgeB_i,
                    vertices[i].bevelProfile_i + vertices[i].bevelProfile_count - 1);
                triangleCount += 3;
                // middle
                for (int profileI = 0; profileI < vertices[i].bevelProfile_count - 1; profileI++)
                {
                    AddTriangle(triangles, triangleCount, profileCCW,
                        vertices[i].innerRect_i,
                        vertices[i].bevelProfile_i + profileI + 1,
                        vertices[i].bevelProfile_i + profileI);
                    triangleCount += 3;
                }
            }
            if (i % 2 == 0 && vertices[i].edgeB_i != -1)
            {
                QuadTriangles(triangles, triangleCount, faceNum % 2 == 1,
                    vertices[i].innerRect_i,
                    vertices[i].edgeB_i,
                    vertices[j].edgeB_i,
                    vertices[j].innerRect_i);
                triangleCount += 6;
            }
            if (i % 2 == 1 && vertices[i].edgeC_i != -1)
            {
                QuadTriangles(triangles, triangleCount, faceNum % 2 == 1,
                    vertices[i].innerRect_i,
                    vertices[i].edgeC_i,
                    vertices[j].edgeC_i,
                    vertices[j].innerRect_i);
                triangleCount += 6;
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

    private float ApplyBevel(float coord, VoxelEdge edge, float bevelCoord = 0.0f)
    {
        if (edge.hasBevel)
            return (coord - 0.5f) * (1 - edge.bevelSizeFloat * 2 * (1 - bevelCoord)) + 0.5f;
        else
            return coord;
    }
}
