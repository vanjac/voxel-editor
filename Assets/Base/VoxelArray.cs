using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelArray : MonoBehaviour
{
    private const int COMPONENT_BLOCK_SIZE = 4;

    public class OctreeNode
    {
        public Vector3Int position;
        public int size;
        public OctreeNode parent;
        public OctreeNode[] branches = new OctreeNode[8];
        public Voxel voxel;
        public VoxelComponent voxelComponent;
        public Bounds bounds
        {
            get
            {
                Vector3 sizeVector = new Vector3(size, size, size);
                return new Bounds(position + sizeVector / 2, sizeVector);
            }
        }

        public OctreeNode(Vector3Int position, int size)
        {
            this.position = position;
            this.size = size;
        }

        public bool InBounds(Vector3Int point)
        {
            return point.x >= position.x && point.x < position.x + size
                && point.y >= position.y && point.y < position.y + size
                && point.z >= position.z && point.z < position.z + size;
        }

        public override string ToString()
        {
            return position + "(" + size + ")";
        }
    }

    public enum WorldType {
        INDOOR, FLOATING, OUTDOOR
    }

    public WorldProperties world = new WorldProperties();
    public WorldType type = WorldType.INDOOR;
    public List<Material> customMaterials = new List<Material>();
    public List<Material> customOverlays = new List<Material>();
    protected OctreeNode rootNode;
    private List<ObjectEntity> objects = new List<ObjectEntity>();
    private VoxelComponent voxelComponent;

    public virtual void Awake()
    {
        rootNode = new OctreeNode(new Vector3Int(-4, -4, -4), 8);
    }

    // will NOT create if missing
    public static Voxel VoxelAtAdjacent(Vector3Int position, Voxel searchStart)
    {
        if (searchStart == null)
            return null;
        OctreeNode currentNode = searchStart.octreeNode;
        if (currentNode == null)
            return null;
        while (!currentNode.InBounds(position))
        {
            currentNode = currentNode.parent;
            if (currentNode == null)
                return null;
        }
        return SearchDown(currentNode, position, false, null);
    }

    public Voxel VoxelAt(Vector3Int position, bool createIfMissing)
    {
        while (!rootNode.InBounds(position))
        {
            if (!createIfMissing)
                return null;
            // create new root node...
            // will it be the large end of the new node that will be created
            bool xLarge = position.x < rootNode.position.x;
            bool yLarge = position.y < rootNode.position.y;
            bool zLarge = position.z < rootNode.position.z;
            int branchI = (xLarge ? 1 : 0) + (yLarge ? 2 : 0) + (zLarge ? 4 : 0);
            Vector3Int newRootPos = new Vector3Int(
                rootNode.position.x - (xLarge ? rootNode.size : 0),
                rootNode.position.y - (yLarge ? rootNode.size : 0),
                rootNode.position.z - (zLarge ? rootNode.size : 0)
                );
            OctreeNode newRoot = new OctreeNode(newRootPos, rootNode.size * 2);
            newRoot.branches[branchI] = rootNode;
            rootNode.parent = newRoot;
            rootNode = newRoot;
        }
        return SearchDown(rootNode, position, createIfMissing, this);
    }

    private Voxel InstantiateVoxel(Vector3Int position, VoxelComponent useComponent)
    {
        Voxel voxel = new Voxel();
        voxel.position = position;

        if (useComponent == null)
        {
            if (voxelComponent == null)
            {
                GameObject voxelObject = new GameObject();
                voxelObject.transform.parent = transform;
                voxelObject.name = "megavoxel";
                voxelComponent = voxelObject.AddComponent<VoxelComponent>();
            }
            voxel.voxelComponent = voxelComponent;
        }
        else
        {
            voxel.voxelComponent = useComponent;
        }
        voxel.voxelComponent.AddVoxel(voxel);
        return voxel;
    }

    // voxelArray is only used if createIfMissing is true
    private static Voxel SearchDown(OctreeNode node, Vector3Int position, bool createIfMissing, VoxelArray voxelArray)
    {
        VoxelComponent useComponent = null;
        while (node.size != 1)
        {
            // initial root node is larger than COMPONENT_BLOCK_SIZE so a component node should always be found
            if (createIfMissing && useComponent == null && node.size == COMPONENT_BLOCK_SIZE)
            {
                if (node.voxelComponent == null || node.voxelComponent.isDestroyed)
                {
                    GameObject voxelObject = new GameObject();
                    voxelObject.transform.parent = voxelArray.transform;
                    voxelObject.name = node.position.ToString();
                    node.voxelComponent = voxelObject.AddComponent<VoxelComponent>();
                }
                useComponent = node.voxelComponent;
            }

            int halfSize = node.size / 2;
            bool xLarge = position.x >= node.position.x + halfSize;
            bool yLarge = position.y >= node.position.y + halfSize;
            bool zLarge = position.z >= node.position.z + halfSize;
            int branchI = (xLarge ? 1 : 0) + (yLarge ? 2 : 0) + (zLarge ? 4 : 0);
            OctreeNode branch = node.branches[branchI];
            if (branch == null)
            {
                if (!createIfMissing)
                    return null;
                Vector3Int branchPos = new Vector3Int(
                    node.position.x + (xLarge ? halfSize : 0),
                    node.position.y + (yLarge ? halfSize : 0),
                    node.position.z + (zLarge ? halfSize : 0)
                    );
                branch = new OctreeNode(branchPos, halfSize);
                node.branches[branchI] = branch;
                branch.parent = node;
            }
            node = branch;
        }

        if (!createIfMissing)
            return node.voxel;
        else if (node.voxel != null)
            return node.voxel;
        else
        {
            Voxel newVoxel = voxelArray.InstantiateVoxel(position, useComponent);
            node.voxel = newVoxel;
            newVoxel.octreeNode = node;
            return newVoxel;
        }
    }

    // return if empty
    private bool RemoveVoxelRecursive(OctreeNode node, Vector3Int position, Voxel voxelToRemove)
    {
        if (node.size == 1)
        {
            // the voxel could have alreay been replaced with a new one
            if (node.voxel == voxelToRemove)
            {
                node.voxel = null;
                voxelToRemove.octreeNode = null;
                return true;
            }
            return false;
        }

        int halfSize = node.size / 2;
        bool xLarge = position.x >= node.position.x + halfSize;
        bool yLarge = position.y >= node.position.y + halfSize;
        bool zLarge = position.z >= node.position.z + halfSize;
        int branchI = (xLarge ? 1 : 0) + (yLarge ? 2 : 0) + (zLarge ? 4 : 0);
        OctreeNode branch = node.branches[branchI];

        if (branch != null)
            if (RemoveVoxelRecursive(branch, position, voxelToRemove))
            {
                node.branches[branchI].parent = null;
                node.branches[branchI] = null;
            }

        for (int i = 0; i < 8; i++)
        {
            if (node.branches[i] != null)
                return false;
        }
        return true;
    }

    public virtual void VoxelModified(Voxel voxel)
    {
        if (voxel.CanBeDeleted())
        {
            voxel.VoxelDeleted();
            RemoveVoxelRecursive(rootNode, voxel.position, voxel);
            AssetManager.UnusedAssets();
        }
        else
        {
            voxel.UpdateVoxel();
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

    public IEnumerable<Voxel> IterateVoxels()
    {
        foreach (Voxel v in IterateOctree(rootNode))
            yield return v;
    }

    private IEnumerable<Voxel> IterateOctree(OctreeNode node)
    {
        if (node == null)
        { }
        else if (node.size == 1)
        {
            if (node.voxel != null)
                yield return node.voxel;
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                foreach (Voxel v in IterateOctree(node.branches[i]))
                    yield return v;
            }
        }
    }

    public void AddObject(ObjectEntity obj)
    {
        objects.Add(obj);
        Voxel objVoxel = VoxelAt(obj.position, true);
        if (objVoxel.objectEntity != null)
            Debug.Log("Object already at position!!");
        objVoxel.objectEntity = obj;
        // not necessary to call VoxelModified
    }

    public void DeleteObject(ObjectEntity obj)
    {
        objects.Remove(obj);
        if (obj.marker != null)
        {
            Destroy(obj.marker.gameObject);
            obj.marker = null;
        }
        Voxel objVoxel = VoxelAt(obj.position, false);
        if (objVoxel != null)
        {
            objVoxel.objectEntity = null;
            VoxelModified(objVoxel);
        }
        else
            Debug.Log("This object wasn't in the voxel array!");
    }

    // return success
    public bool MoveObject(ObjectEntity obj, Vector3Int newPosition)
    {
        if (newPosition == obj.position)
            return true;

        Voxel newObjVoxel = VoxelAt(newPosition, true);
        if (newObjVoxel.objectEntity != null)
            return false;
        newObjVoxel.objectEntity = obj;
        // not necessary to call VoxelModified

        Voxel oldObjVoxel = VoxelAt(obj.position, false);
        if (oldObjVoxel != null)
        {
            oldObjVoxel.objectEntity = null;
            VoxelModified(oldObjVoxel);
        }
        else
            Debug.Log("This object wasn't in the voxel array!");
        obj.position = newPosition;
        ObjectModified(obj);
        return true;
    }

    public IEnumerable<ObjectEntity> IterateObjects()
    {
        foreach (ObjectEntity obj in objects)
            yield return obj;
    }

    public void DeleteSubstance(Substance substance)
    {
        foreach (var voxel in new HashSet<Voxel>(substance.voxels))
        {
            voxel.Clear();
            VoxelModified(voxel);
        }
    }
}
