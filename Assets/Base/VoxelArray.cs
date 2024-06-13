using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct VoxelFaceLoc : IEquatable<VoxelFaceLoc>
{
    public static readonly VoxelFaceLoc NONE = new VoxelFaceLoc(VoxelArray.NONE, 0);
    public Vector3Int position;
    public int faceI;
    public VoxelFaceLoc(Vector3Int position, int faceI)
    {
        this.position = position;
        this.faceI = faceI;
    }

    public override bool Equals(object obj) => obj is VoxelFaceLoc other && Equals(other);
    public bool Equals(VoxelFaceLoc other) => position == other.position && faceI == other.faceI;
    public override int GetHashCode() => position.GetHashCode() * 37 + faceI.GetHashCode();
}

public struct VoxelEdgeLoc : IEquatable<VoxelEdgeLoc>
{
    public static readonly VoxelEdgeLoc NONE = new VoxelEdgeLoc(VoxelArray.NONE, 0);
    public Vector3Int position;
    public int edgeI;
    public VoxelEdgeLoc(Vector3Int position, int edgeI)
    {
        this.position = position;
        this.edgeI = edgeI;
    }

    public override bool Equals(object obj) => obj is VoxelEdgeLoc other && Equals(other);
    public bool Equals(VoxelEdgeLoc other) => position == other.position && edgeI == other.edgeI;
    public override int GetHashCode() => position.GetHashCode() * 37 + edgeI.GetHashCode();
}

public class VoxelArray : MonoBehaviour
{
    public static readonly Vector3Int NONE = new Vector3Int(9999, 9999, 9999);
    public enum WorldType
    {
        INDOOR, FLOATING, OUTDOOR
    }

    public WorldProperties world = new WorldProperties();
    public WorldType type = WorldType.INDOOR;
    public List<Material> customMaterials = new List<Material>();
    public List<Material> customOverlays = new List<Material>();
    private Dictionary<Vector3Int, Voxel> voxels = new Dictionary<Vector3Int, Voxel>();
    protected VoxelGroup worldGroup = new VoxelGroup();
    private Dictionary<Vector3Int, ObjectEntity> objects = new Dictionary<Vector3Int, ObjectEntity>();

    public Voxel VoxelAt(Vector3Int position, bool createIfMissing)
    {
        if (!voxels.TryGetValue(position, out Voxel voxel) && createIfMissing)
        {
            voxel = new Voxel();
            voxels[position] = voxel;
            worldGroup.AddVoxel(position, this);
        }
        return voxel;
    }

    public VoxelFace FaceAt(VoxelFaceLoc loc) =>
        VoxelAt(loc.position, false)?.faces?[loc.faceI] ?? default;
    
    public bool SetFace(VoxelFaceLoc loc, VoxelFace face)
    {
        var voxel = VoxelAt(loc.position, !face.IsEmpty());
        if (voxel != null)
            voxel.faces[loc.faceI] = face;
        return voxel != null;
    }

    public bool ChangeFace(VoxelFaceLoc loc, Func<VoxelFace, VoxelFace> fn, bool createIfMissing = true)
    {
        var voxel = VoxelAt(loc.position, createIfMissing);
        if (voxel != null)
            voxel.faces[loc.faceI] = fn(voxel.faces[loc.faceI]);
        return voxel != null;
    }

    public VoxelEdge EdgeAt(VoxelEdgeLoc loc) =>
        VoxelAt(loc.position, false)?.edges?[loc.edgeI] ?? default;

    public bool SetEdge(VoxelEdgeLoc loc, VoxelEdge edge)
    {
        var voxel = VoxelAt(loc.position, edge.hasBevel);
        if (voxel != null)
            voxel.edges[loc.edgeI] = edge;
        return voxel != null;
    }

    public bool ChangeEdge(VoxelEdgeLoc loc, Func<VoxelEdge, VoxelEdge> fn, bool createIfMissing = true)
    {
        var voxel = VoxelAt(loc.position, createIfMissing);
        if (voxel != null)
            voxel.edges[loc.edgeI] = fn(voxel.edges[loc.edgeI]);
        return voxel != null;
    }

    private void RemoveVoxel(Vector3Int position)
    {
        var substance = SubstanceAt(position);
        voxels.Remove(position);
        if (substance != null)
            substance.voxelGroup.RemoveVoxel(position);
        else
            worldGroup.RemoveVoxel(position);
    }

    public Substance SubstanceAt(Vector3Int position) => VoxelAt(position, false)?.substance;

    public virtual Substance SetSubstance(Vector3Int position, Substance substance)
    {
        var voxel = VoxelAt(position, substance != null);
        var oldSubstance = voxel?.substance;

        if (oldSubstance != null)
            oldSubstance.voxelGroup.RemoveVoxel(position);
        else
            worldGroup.RemoveVoxel(position);

        if (voxel != null)
            voxel.substance = substance;

        if (substance != null)
            substance.voxelGroup.AddVoxel(position, this);
        else
            worldGroup.AddVoxel(position, this);

        return oldSubstance;
    }

    public void UpdateVoxel(Vector3Int position)
    {
        var substance = SubstanceAt(position);
        VoxelGroup group = (substance != null) ? substance.voxelGroup : worldGroup;
        group.ComponentAt(position, null).UpdateVoxel();
    }

    public virtual void VoxelModified(Vector3Int position)
    {
        var voxel = VoxelAt(position, false);
        if (voxel == null)
        {
        }
        else if (voxel.IsEmpty())
        {
            RemoveVoxel(position);
            AssetManager.UnusedAssets();
        }
        else
        {
            UpdateVoxel(position);
        }
    }

    public virtual void ObjectModified(ObjectEntity obj)
    {
        // do nothing, to be extended
    }

    public bool IsEmpty()
    {
        foreach (var _ in IterateVoxelPositions())
            return false;
        return true;
    }

    public IEnumerable<Vector3Int> IterateVoxelPositions() => voxels.Keys;

    public IEnumerable<Voxel> IterateVoxels() => voxels.Values;

    public IEnumerable<(Vector3Int, Voxel)> IterateVoxelPairs() =>
        voxels.Select(p => (p.Key, p.Value));

    public ObjectEntity ObjectAt(Vector3Int pos)
    {
        if (objects.TryGetValue(pos, out ObjectEntity obj))
            return obj;
        return null;
    }

    public virtual void AddObject(ObjectEntity obj)
    {
        if (objects.ContainsKey(obj.position))
            Debug.Log("Object already at position!!");
        objects[obj.position] = obj;
    }

    public virtual void DeleteObject(ObjectEntity obj)
    {
        if (objects.TryGetValue(obj.position, out ObjectEntity existing) && existing == obj)
            objects.Remove(obj.position);
        else
            Debug.Log("This object wasn't in the voxel array!");
        if (obj.marker != null)
        {
            Destroy(obj.marker.gameObject);
            obj.marker = null;
        }
    }

    // return success
    public bool MoveObject(ObjectEntity obj, Vector3Int newPosition)
    {
        if (newPosition == obj.position)
            return true;
        if (objects.ContainsKey(newPosition))
            return false; // can't move here

        if (objects.TryGetValue(obj.position, out ObjectEntity existing) && existing == obj)
            objects.Remove(obj.position);
        else
            Debug.Log("This object wasn't in the voxel array!");

        objects[newPosition] = obj;
        obj.position = newPosition;
        ObjectModified(obj);
        return true;
    }

    public IEnumerable<ObjectEntity> IterateObjects() => objects.Values;

    public void DeleteSubstance(Substance substance)
    {
        foreach (var (pos, voxel) in new List<(Vector3Int, Voxel)>(substance.voxelGroup.IterateVoxelPairs()))
        {
            voxel.ClearFaces();
            SetSubstance(pos, null);
            VoxelModified(pos);
        }
    }
}

public class VoxelGroup
{
    public const int COMPONENT_BLOCK_SIZE = 4; // must be power of 2
    private Dictionary<Vector3Int, VoxelComponent> components = new Dictionary<Vector3Int, VoxelComponent>();

    private Vector3Int ComponentPos(Vector3Int voxelPos)
    {
        var mask = ~(COMPONENT_BLOCK_SIZE - 1);
        return new Vector3Int(voxelPos.x & mask, voxelPos.y & mask, voxelPos.z & mask);
    }

    public VoxelComponent ComponentAt(Vector3Int position, VoxelArray createInArray)
    {
        position = ComponentPos(position);
        if (!components.TryGetValue(position, out VoxelComponent component) && createInArray != null)
        {
            GameObject voxelObject = new GameObject();
            voxelObject.transform.parent = createInArray.transform;
            voxelObject.name = position.ToString();
            component = voxelObject.AddComponent<VoxelComponent>();
            component.voxelArray = createInArray;
            components[position] = component;
        }
        return component;
    }

    public void AddVoxel(Vector3Int position, VoxelArray voxelArray)
    {
        var component = ComponentAt(position, voxelArray);
        component.positions.Add(position);
    }

    public void RemoveVoxel(Vector3Int position)
    {
        var pos = ComponentPos(position);
        if (components.TryGetValue(pos, out VoxelComponent component))
        {
            component.positions.Remove(position);
            if (component.positions.Count == 0)
            {
                GameObject.Destroy(component.gameObject);
                components.Remove(pos);
            }
            else
            {
                component.UpdateVoxel();
            }
        }
    }

    public IEnumerable<VoxelComponent> IterateComponents() => components.Values;

    public IEnumerable<Vector3Int> IteratePositions()
    {
        foreach (var vc in components.Values)
        {
            foreach (var pos in vc.positions)
                yield return pos;
        }
    }

    public IEnumerable<(Vector3Int, Voxel)> IterateVoxelPairs()
    {
        foreach (var vc in components.Values)
        {
            foreach (var pos in vc.positions)
            {
                var voxel = vc.voxelArray.VoxelAt(pos, false);
                if (voxel != null)
                    yield return (pos, voxel);
            }
        }
    }
}
