using System.Collections.Generic;
using UnityEngine;

public struct VoxelFace {
    public Material material;
    public Material overlay;
    public byte orientation;

    public bool IsEmpty() => material == null && overlay == null;

    public VoxelFace(VoxelFace other) {
        this = other;
    }

    public override bool Equals(object obj) => obj is VoxelFace && this == (VoxelFace)obj;

    public static bool operator ==(VoxelFace s1, VoxelFace s2) =>
        s1.material == s2.material && s1.overlay == s2.overlay && s1.orientation == s2.orientation;
    public static bool operator !=(VoxelFace s1, VoxelFace s2) => !(s1 == s2);

    public override int GetHashCode() {
        int result = material.GetHashCode();
        result = 37 * result + overlay.GetHashCode();
        result = 37 * result + (int)orientation;
        return result;
    }

    public static int GetOrientationRotation(byte orientation) => orientation & 3;

    public static bool GetOrientationMirror(byte orientation) => (orientation & 4) != 0;

    public static byte Orientation(int rotation, bool mirror) {
        while (rotation < 0) {
            rotation += 4;
        }
        rotation %= 4;
        return (byte)(rotation + (mirror ? 4 : 0));
    }

    public MaterialSound GetSound() {
        MaterialSound matSound = ResourcesDirectory.GetMaterialSound(material);
        MaterialSound overSound = ResourcesDirectory.GetMaterialSound(overlay);
        if (overSound == MaterialSound.GENERIC) {
            return matSound;
        } else {
            return overSound;
        }
    }
}

public struct VoxelEdge {
    public enum BevelType : byte {
        NONE, FLAT, CURVE, SQUARE, STAIR_2, STAIR_4
    }
    public enum BevelSize : byte {
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

    public VoxelEdge(VoxelEdge other) {
        this = other;
    }

    public BevelType bevelType {
        get => (BevelType)(bevel & 0x07);
        set => bevel = (byte)((bevel & 0xF8) | (byte)value);
    }

    public BevelSize bevelSize {
        get => (BevelSize)((bevel >> 4) & 0x07);
        set => bevel = (byte)((bevel & 0x8f) | ((byte)value << 4));
    }

    public Vector2[] bevelTypeArray => bevelType switch {
        BevelType.SQUARE => SHAPE_SQUARE,
        BevelType.FLAT => SHAPE_FLAT,
        BevelType.CURVE => SHAPE_CURVE,
        BevelType.STAIR_2 => SHAPE_STAIR_2,
        BevelType.STAIR_4 => SHAPE_STAIR_4,
        _ => null,
    };

    public Vector2[] bevelTypeNormalArray => bevelType switch {
        BevelType.SQUARE => NORMALS_SQUARE,
        BevelType.FLAT => NORMALS_FLAT,
        BevelType.CURVE => NORMALS_CURVE,
        BevelType.STAIR_2 => NORMALS_STAIR_2,
        BevelType.STAIR_4 => NORMALS_STAIR_4,
        _ => null,
    };

    public float bevelSizeFloat => bevelSize switch {
        BevelSize.QUARTER => 0.25f,
        BevelSize.HALF => 0.5f,
        BevelSize.FULL => 1.0f,
        _ => 0.0f,
    };

    public bool hasBevel => bevelType != BevelType.NONE;

    public static bool BevelsMatch(VoxelEdge e1, VoxelEdge e2) {
        if (!e1.hasBevel && !e2.hasBevel) {
            return true;
        }
        return e1.bevelType == e2.bevelType && e1.bevelSize == e2.bevelSize;
    }
}


public class Voxel {
    public const int NUM_FACES = 6;
    public const int NUM_EDGES = 12;

    public readonly static int[] SQUARE_LOOP_COORD_INDEX = new int[] { 0, 1, 3, 2 };


    public static Vector3 DirectionForFaceI(int faceI) => faceI switch {
        0 => Vector3.left,
        1 => Vector3.right,
        2 => Vector3.down,
        3 => Vector3.up,
        4 => Vector3.back,
        5 => Vector3.forward,
        _ => Vector3.zero,
    };

    public static Vector3Int IntDirectionForFaceI(int faceI) => faceI switch {
        0 => Vector3Int.left,
        1 => Vector3Int.right,
        2 => Vector3Int.down,
        3 => Vector3Int.up,
        4 => Vector3Int.back,
        5 => Vector3Int.forward,
        _ => Vector3Int.zero,
    };

    public static Vector3 OppositeDirectionForFaceI(int faceI) =>
        DirectionForFaceI(OppositeFaceI(faceI));

    public static int FaceIForDirection(Vector3 direction) {
        if (direction == Vector3.left) {
            return 0;
        } else if (direction == Vector3.right) {
            return 1;
        } else if (direction == Vector3.down) {
            return 2;
        } else if (direction == Vector3.up) {
            return 3;
        } else if (direction == Vector3.back) {
            return 4;
        } else if (direction == Vector3.forward) {
            return 5;
        } else {
            return -1;
        }
    }

    public static int OppositeFaceI(int faceI) => (faceI / 2) * 2 + (faceI % 2 == 0 ? 1 : 0);

    public static int SideFaceI(int faceI, int sideNum) {
        sideNum %= 4;
        faceI = (faceI / 2) * 2 + 2 + sideNum;
        faceI %= 6;
        return faceI;
    }

    public static void EdgeFaces(int edgeI, out int faceA, out int faceB) {
        int axis = EdgeIAxis(edgeI);
        faceA = ((axis + 1) % 3) * 2;
        faceB = ((axis + 2) % 3) * 2;
        edgeI %= 4;
        if (edgeI == 1 || edgeI == 2) {
            faceA += 1;
        }
        if (edgeI >= 2) {
            faceB += 1;
        }
    }

    public static IEnumerable<int> ConnectedEdges(int edgeI) {
        int axis = EdgeIAxis(edgeI);
        edgeI %= 4;
        if (edgeI >= 2) {
            yield return ((axis + 1) % 3) * 4 + 1;
            yield return ((axis + 1) % 3) * 4 + 2;
        } else {
            yield return ((axis + 1) % 3) * 4 + 0;
            yield return ((axis + 1) % 3) * 4 + 3;
        }
        if (edgeI == 1 || edgeI == 2) {
            yield return ((axis + 2) % 3) * 4 + 2;
            yield return ((axis + 2) % 3) * 4 + 3;
        } else {
            yield return ((axis + 2) % 3) * 4 + 0;
            yield return ((axis + 2) % 3) * 4 + 1;
        }
    }

    public static IEnumerable<int> UnconnectedEdges(int edgeI) {
        int axis = EdgeIAxis(edgeI);
        for (int i = 1; i < 4; i++) {
            yield return axis * 4 + ((edgeI + i) % 4);
        }
        edgeI %= 4;
        if (edgeI >= 2) {
            yield return ((axis + 1) % 3) * 4 + 0;
            yield return ((axis + 1) % 3) * 4 + 3;
        } else {
            yield return ((axis + 1) % 3) * 4 + 1;
            yield return ((axis + 1) % 3) * 4 + 2;
        }
        if (edgeI == 1 || edgeI == 2) {
            yield return ((axis + 2) % 3) * 4 + 0;
            yield return ((axis + 2) % 3) * 4 + 1;
        } else {
            yield return ((axis + 2) % 3) * 4 + 2;
            yield return ((axis + 2) % 3) * 4 + 3;
        }
    }

    public static int FaceIAxis(int faceI) => faceI / 2;

    public static int EdgeIAxis(int edgeI) => edgeI / 4;

    public static IEnumerable<int> FaceSurroundingEdges(int faceNum) {
        int axis = FaceIAxis(faceNum);
        yield return ((axis + 1) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) * 2]; // 0 - 1
        yield return ((axis + 2) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) + 2]; // 1 - 2
        yield return ((axis + 1) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2) * 2 + 1]; // 2 - 3
        yield return ((axis + 2) % 3) * 4 + SQUARE_LOOP_COORD_INDEX[(faceNum % 2)]; // 3 - 0
    }

    // see "Voxel Diagram.skp" for a diagram of face/edge numbers
    public readonly VoxelFace[] faces = new VoxelFace[NUM_FACES]; // xMin, xMax, yMin, yMax, zMin, zMax
    // Edges: 0-3: x, 4-7: y, 8-11: z
    // Each group of four follows the pattern (0,0), (1,0), (1,1), (0,1)
    // for the Y/Z axes (0-3), Z/X axes (4-7, note order), or X/Y axes (8-11)
    public readonly VoxelEdge[] edges = new VoxelEdge[NUM_EDGES];
    public Substance substance;

    public static Bounds FaceBounds(VoxelFaceLoc loc) {
        var bounds = loc.faceI switch {
            0 => new Bounds(new Vector3(0, 0.5f, 0.5f), new Vector3(0, 1, 1)),
            1 => new Bounds(new Vector3(1, 0.5f, 0.5f), new Vector3(0, 1, 1)),
            2 => new Bounds(new Vector3(0.5f, 0, 0.5f), new Vector3(1, 0, 1)),
            3 => new Bounds(new Vector3(0.5f, 1, 0.5f), new Vector3(1, 0, 1)),
            4 => new Bounds(new Vector3(0.5f, 0.5f, 0), new Vector3(1, 1, 0)),
            5 => new Bounds(new Vector3(0.5f, 0.5f, 1), new Vector3(1, 1, 0)),
            _ => new Bounds(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, 0, 0)),
        };
        bounds.center += loc.position;
        return bounds;
    }

    public static Bounds EdgeBounds(VoxelEdgeLoc loc) {
        var edgeI = loc.edgeI;
        Vector3 center = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 size = Vector3.zero;
        if (edgeI >= 0 && edgeI < 4) {
            size = new Vector3(1, 0, 0);
        } else if (edgeI >= 4 && edgeI < 8) {
            size = new Vector3(0, 1, 0);
        } else if (edgeI >= 8 && edgeI < 12) {
            size = new Vector3(0, 0, 1);
        }
        if (edgeI == 4 || edgeI == 5 || edgeI == 8 || edgeI == 11) {
            center.x = 0;
        } else if (edgeI == 6 || edgeI == 7 || edgeI == 9 || edgeI == 10) {
            center.x = 1;
        }
        if (edgeI == 0 || edgeI == 3 || edgeI == 8 || edgeI == 9) {
            center.y = 0;
        } else if (edgeI == 1 || edgeI == 2 || edgeI == 10 || edgeI == 11) {
            center.y = 1;
        }
        if (edgeI == 0 || edgeI == 1 || edgeI == 4 || edgeI == 7) {
            center.z = 0;
        } else if (edgeI == 2 || edgeI == 3 || edgeI == 5 || edgeI == 6) {
            center.z = 1;
        }
        return new Bounds(center + loc.position, size);
    }

    public static Bounds Bounds(Vector3Int position) =>
        new Bounds(position + new Vector3(0.5f, 0.5f, 0.5f), Vector3.one);

    public bool EdgeIsEmpty(int edgeI) {
        EdgeFaces(edgeI, out int faceA, out int faceB);
        return faces[faceA].IsEmpty() && faces[faceB].IsEmpty();
    }

    public bool EdgeIsConvex(int edgeI) {
        EdgeFaces(edgeI, out int faceA, out int faceB);
        return !faces[faceA].IsEmpty() && !faces[faceB].IsEmpty();
    }

    public int FaceTransformedEdgeNum(int faceNum, int edgeI) {
        int n = edgeI + 1;
        if (faceNum % 2 == 1) {
            n = 4 - (n % 4);
        }
        if (faceNum == 4) {
            n += 3;
        }
        if (faceNum == 5) {
            n += 1;
        }
        if (VoxelFace.GetOrientationMirror(faces[faceNum].orientation)) {
            n = 5 - (n % 4);
        }
        n += VoxelFace.GetOrientationRotation(faces[faceNum].orientation);
        return n % 4;
    }

    public bool IsEmpty() {
        if (substance != null) {
            return false;
        }
        foreach (VoxelFace face in faces) {
            if (!face.IsEmpty()) {
                return false;
            }
        }
        return true;
    }

    public void ClearFaces() {
        for (int faceI = 0; faceI < NUM_FACES; faceI++) {
            faces[faceI] = default;
        }
        for (int edgeI = 0; edgeI < NUM_EDGES; edgeI++) {
            edges[edgeI] = default;
        }
    }
}
