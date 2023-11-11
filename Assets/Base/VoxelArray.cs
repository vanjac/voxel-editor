using System.Collections.Generic;
using UnityEngine;

public class VoxelArray : MonoBehaviour
{
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
            voxel.position = position;
            voxels[position] = voxel;
            worldGroup.AddVoxel(voxel, this);
        }
        return voxel;
    }

    private void RemoveVoxel(Voxel voxel)
    {
        voxels.Remove(voxel.position);
        if (voxel.substance != null)
            voxel.substance.voxelGroup.RemoveVoxel(voxel);
        else
            worldGroup.RemoveVoxel(voxel);
    }

    public virtual void SetVoxelSubstance(Voxel voxel, Substance substance)
    {
        if (voxel.substance != null)
            voxel.substance.voxelGroup.RemoveVoxel(voxel);
        else
            worldGroup.RemoveVoxel(voxel);
        voxel.substance = substance;
        if (substance != null)
            substance.voxelGroup.AddVoxel(voxel, this);
        else
            worldGroup.AddVoxel(voxel, this);
    }

    public void UpdateVoxel(Voxel voxel)
    {
        VoxelGroup group = (voxel.substance != null) ? voxel.substance.voxelGroup : worldGroup;
        group.ComponentAt(voxel.position, null).UpdateVoxel();
    }

    public virtual void VoxelModified(Voxel voxel)
    {
        if (voxel.IsEmpty())
        {
            if (voxels.TryGetValue(voxel.position, out Voxel existing))
            {
                // the voxel could have alreay been replaced with a new one
                // TODO: specify by position to prevent this problem
                if (existing == voxel)
                    RemoveVoxel(voxel);
            }
            AssetManager.UnusedAssets();
        }
        else
        {
            UpdateVoxel(voxel);
        }
    }

    public virtual void ObjectModified(ObjectEntity obj)
    {
        // do nothing, to be extended
    }

    public bool IsEmpty()
    {
        foreach (Voxel v in IterateVoxels())
            return false;
        return true;
    }

    public IEnumerable<Voxel> IterateVoxels() => voxels.Values;

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
        foreach (var voxel in new HashSet<Voxel>(substance.voxelGroup.IterateVoxels()))
        {
            voxel.ClearFaces();
            SetVoxelSubstance(voxel, null);
            VoxelModified(voxel);
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

    public void AddVoxel(Voxel voxel, VoxelArray voxelArray)
    {
        var component = ComponentAt(voxel.position, voxelArray);
        component.voxels.Add(voxel);
    }

    public void RemoveVoxel(Voxel voxel)
    {
        var pos = ComponentPos(voxel.position);
        if (components.TryGetValue(pos, out VoxelComponent component))
        {
            component.voxels.Remove(voxel);
            if (component.voxels.Count == 0)
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

    public IEnumerable<Voxel> IterateVoxels()
    {
        foreach (var vc in components.Values)
        {
            foreach (var voxel in vc.voxels)
                yield return voxel;
        }
    }
}