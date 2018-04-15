using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelArray : MonoBehaviour
{
    protected class OctreeNode
    {
        public Vector3Int position;
        public int size;
        public OctreeNode[] branches = new OctreeNode[8];
        public Voxel voxel;
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

    public GameObject voxelPrefab;

    public ObjectEntity playerObject;
    protected OctreeNode rootNode;

    private bool unloadUnusedAssets = false;

    public virtual void Awake ()
    {
        rootNode = new OctreeNode(new Vector3Int(-2, -2, -2), 8);
    }

    void Update()
    {
        if (unloadUnusedAssets)
        {
            unloadUnusedAssets = false;
            Resources.UnloadUnusedAssets();
        }
    }

    protected static Vector3Int Vector3ToInt(Vector3 v)
    {
        return new Vector3Int(
            Mathf.RoundToInt(v.x),
            Mathf.RoundToInt(v.y),
            Mathf.RoundToInt(v.z)
            );
    }

    public Voxel VoxelAt(Vector3 position, bool createIfMissing)
    {
        Vector3Int intPosition = Vector3ToInt(position);
        while (!rootNode.InBounds(intPosition))
        {
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
            rootNode = newRoot;
        }
        return SearchOctreeRecursive(rootNode, intPosition, createIfMissing);
    }

    public Voxel InstantiateVoxel(Vector3 position)
    {
        GameObject voxelObject = Instantiate(voxelPrefab);
        voxelObject.transform.position = position;
        voxelObject.transform.parent = transform;
        voxelObject.name = "Voxel at " + position;
        return voxelObject.GetComponent<Voxel>();
    }

    private Voxel SearchOctreeRecursive(OctreeNode node, Vector3Int position, bool createIfMissing)
    {
        if (node.size == 1)
        {
            if (!createIfMissing)
                return node.voxel;
            else if (node.voxel != null)
                return node.voxel;
            else
            {
                Voxel newVoxel = InstantiateVoxel(position);
                node.voxel = newVoxel;
                return newVoxel;
            }
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
        }
        return SearchOctreeRecursive(branch, position, createIfMissing);
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
        if (voxel.IsEmpty())
        {
            Destroy(voxel.gameObject);
            unloadUnusedAssets = true;
        }
        else
        {
            voxel.UpdateVoxel();
        }
    }

    // called by voxels that are being destroyed
    public void VoxelDestroyed(Voxel voxel)
    {
        RemoveVoxelRecursive(rootNode, Vector3ToInt(voxel.transform.position), voxel);
    }

    public bool IsEmpty()
    {
        foreach (Voxel v in IterateVoxels())
            return false;
        return true;
    }

    public System.Collections.Generic.IEnumerable<Voxel> IterateVoxels()
    {
        foreach (Voxel v in IterateOctree(rootNode))
            yield return v;
    }

    private System.Collections.Generic.IEnumerable<Voxel> IterateOctree(OctreeNode node)
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

}
