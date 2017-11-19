using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelArray : MonoBehaviour {

    struct FaceChange
    {
        public VoxelFaceReference faceRef;
        public VoxelFace newFace;
        public bool updateSelect, updateFace;

        public FaceChange(Voxel voxel, int faceI, VoxelFace newFace)
        {
            faceRef = new VoxelFaceReference(voxel, faceI);
            this.newFace = newFace;
            updateSelect = false;
            updateFace = true;
        }
    }

    class OctreeNode
    {
        public Vector3Int position;
        public int size;
        public OctreeNode[] branches = new OctreeNode[8];
        public Voxel voxel;

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

    public Transform axes;
    public GameObject voxelPrefab;
    public Material selectedMaterial;

    private OctreeNode rootNode;

    // select state
    bool selection = false;
    Bounds selectStartBounds = new Bounds(Vector3.zero, Vector3.zero);
    Bounds selectCurrentBounds = new Bounds(Vector3.zero, Vector3.zero);
    List<VoxelFaceReference> selectedFaces = new List<VoxelFaceReference>();

    bool unloadUnusedAssets = false;

	void Awake () {
        rootNode = new OctreeNode(new Vector3Int(-2, -2, -2), 8);

        Voxel.selectedMaterial = selectedMaterial;

        Material[] testMaterials = new Material[]
        {
            Resources.Load<Material>("GameAssets/Materials/colors/Red"),
            Resources.Load<Material>("GameAssets/Materials/colors/Yellow"),
            Resources.Load<Material>("GameAssets/Materials/colors/Green"),
            Resources.Load<Material>("GameAssets/Materials/colors/Cyan"),
            Resources.Load<Material>("GameAssets/Materials/colors/Blue"),
            Resources.Load<Material>("GameAssets/Materials/colors/Magenta")
        };

        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    Voxel newVoxel = VoxelAt(new Vector3(x, y, z), true);
                    if (x == -2)
                        newVoxel.faces[0].material = testMaterials[0];
                    if (x == 2)
                        newVoxel.faces[1].material = testMaterials[1];
                    if (y == -2)
                        newVoxel.faces[2].material = testMaterials[2];
                    if (y == 2)
                        newVoxel.faces[3].material = testMaterials[3];
                    if (z == -2)
                        newVoxel.faces[4].material = testMaterials[4];
                    if (z == 2)
                        newVoxel.faces[5].material = testMaterials[5];
                    VoxelModified(newVoxel);
                }
            }
        }

        UpdateSelection();
	}

    void Update()
    {
        if (unloadUnusedAssets)
        {
            unloadUnusedAssets = false;
            Resources.UnloadUnusedAssets();
        }
    }

    private Vector3Int Vector3ToInt(Vector3 v)
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
        if (!rootNode.InBounds(intPosition))
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
                GameObject voxelObject = Instantiate(voxelPrefab);
                voxelObject.transform.position = position;
                voxelObject.transform.parent = transform;
                voxelObject.name = "Voxel at " + position;
                node.voxel = voxelObject.GetComponent<Voxel>();
                return node.voxel;
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

    public void VoxelModified(Voxel voxel)
    {
        if (voxel.IsEmpty())
            RemoveVoxel(voxel);
        else
            voxel.UpdateVoxel();
    }

    public void RemoveVoxel(Voxel voxel)
    {
        Destroy(voxel.gameObject);
        unloadUnusedAssets = true;
    }

    // called by voxels that are being destroyed
    public void VoxelDestroyed(Voxel voxel)
    {
        RemoveVoxelRecursive(rootNode, Vector3ToInt(voxel.transform.position), voxel);
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

    public void ClearAll()
    {
        SelectBackground();
        foreach (Voxel voxel in IterateVoxels())
        {
            voxel.Clear();
        }
    }

    public void UpdateAll()
    {
        foreach (Voxel voxel in IterateVoxels())
        {
            VoxelModified(voxel);
        }
    }

    // below Select functions are called by the camera

    public void SelectBackground()
    {
        selectStartBounds = new Bounds(Vector3.zero, Vector3.zero);
        selectCurrentBounds = new Bounds(Vector3.zero, Vector3.zero);
        selection = false;
        UpdateSelection();
        if (selectedFaces.Count > 0)
        {
            Debug.Log("There were faces in the selected faces list that weren't marked as selected!");
            selectedFaces.Clear();
        }
    }

    public void SelectDown(Voxel voxel, int faceI)
    {
        selectStartBounds = voxel.GetFaceBounds(faceI);
        selectCurrentBounds = selectStartBounds;
        selection = true;
        UpdateSelection();
    }

    public void SelectDrag(Voxel voxel, int faceI)
    {
        if (!selection)
            return;
        Bounds oldSelectCurrentBounds = selectCurrentBounds;
        selectCurrentBounds = selectStartBounds;
        selectCurrentBounds.Encapsulate(voxel.GetFaceBounds(faceI));
        if (oldSelectCurrentBounds != selectCurrentBounds)
            UpdateSelection();
    }

    public void UpdateSelection()
    {
        if (axes == null)
            return;
        if (selection)
        {
            axes.position = selectCurrentBounds.center;
            axes.gameObject.SetActive(true);
        }
        else
        {
            axes.gameObject.SetActive(false);
        }

        Bounds largerSelectCurrentBounds = selectCurrentBounds;
        // Bounds is by reference, not value, so this won't modify selectCurrentBounds
        largerSelectCurrentBounds.Expand(new Vector3(0.1f, 0.1f, 0.1f));

        // update selection...
        foreach (Voxel checkVoxel in IterateVoxels())
        {
            bool updateCheckVoxel = false;
            for (int checkFaceI = 0; checkFaceI < 6; checkFaceI++)
            {
                if (checkVoxel.faces[checkFaceI].IsEmpty())
                    continue;
                Bounds checkBounds = checkVoxel.GetFaceBounds(checkFaceI);
                bool checkFaceSelected;
                // if checkBounds fully inside selectCurrentBounds
                checkFaceSelected = largerSelectCurrentBounds.Contains(checkBounds.min)
                    && largerSelectCurrentBounds.Contains(checkBounds.max);
                if (checkVoxel.faces[checkFaceI].selected != checkFaceSelected)
                {
                    VoxelFaceReference faceRef = new VoxelFaceReference(checkVoxel, checkFaceI);
                    updateCheckVoxel = true;
                    if (checkFaceSelected)
                        selectedFaces.Add(faceRef);
                    else
                        selectedFaces.Remove(faceRef);
                }
                checkVoxel.faces[checkFaceI].selected = checkFaceSelected;
            }
            if (updateCheckVoxel)
                checkVoxel.UpdateVoxel();
        }
    }

    public void Adjust(Vector3 adjustDirection)
    {
        var faceChangeQueue = new List<FaceChange>();

        for (int i = 0; i < selectedFaces.Count; i++)
        {
            VoxelFaceReference faceRef = selectedFaces[i];
            int faceI = faceRef.faceI;
            int oppositeFaceI = Voxel.OppositeFaceI(faceI);
            bool pulling = Voxel.FaceIForNormal(adjustDirection) == faceI;
            bool pushing = Voxel.FaceIForNormal(adjustDirection) == oppositeFaceI;
            bool pushingOrPulling = pushing || pulling;
            Voxel oldVoxel = faceRef.voxel;
            Vector3 oldPos = oldVoxel.transform.position;
            Vector3 newPos = oldPos + adjustDirection;
            Voxel newVoxel = VoxelAt(newPos, true);
            Voxel voxelMovingToOldPos = VoxelAt(oldPos - adjustDirection, false);
            bool oldPosWillNotBeReplaced = voxelMovingToOldPos == null || !voxelMovingToOldPos.faces[faceI].selected;

            if (oldPosWillNotBeReplaced)
            {
                FaceChange faceChange = new FaceChange(oldVoxel, faceI, Voxel.EMPTY_FACE);
                faceChange.updateSelect = true;
                if (!pushingOrPulling)
                    faceChange.updateFace = false;
                faceChangeQueue.Add(faceChange);
            }

            bool blocked = false; // is something blocking the face from moving here?
            if((!pushingOrPulling) && newVoxel.faces[faceI].IsEmpty())
                blocked = true;
            if (pulling && !oldVoxel.faces[oppositeFaceI].IsEmpty() && !oldVoxel.faces[oppositeFaceI].selected)
            {
                blocked = true;
                faceChangeQueue.Add(new FaceChange(oldVoxel, oppositeFaceI, Voxel.EMPTY_FACE));
            }
            if (pushing)
            {
                Voxel voxelAhead = VoxelAt(newPos + adjustDirection, false);
                if (voxelAhead != null && !voxelAhead.faces[oppositeFaceI].IsEmpty() && !voxelAhead.faces[oppositeFaceI].selected)
                {
                    blocked = true;
                    faceChangeQueue.Add(new FaceChange(voxelAhead, oppositeFaceI, Voxel.EMPTY_FACE));
                }
            }

            if (blocked)
            {
                // make sure the new voxel updates, in case it needs to be deleted
                FaceChange faceChange = new FaceChange(newVoxel, faceI, Voxel.EMPTY_FACE);
                faceChange.updateFace = false;
                faceChange.updateSelect = false;
                faceChangeQueue.Add(faceChange);
            }
            else
            {
                FaceChange moveFaceChange = new FaceChange(newVoxel, faceI, faceRef.face);
                moveFaceChange.updateSelect = true;
                faceChangeQueue.Add(moveFaceChange);
            }

            if (pulling)
            {
                for (int sideFaceI = 0; sideFaceI < 6; sideFaceI++)
                {
                    if (sideFaceI == faceI || sideFaceI == oppositeFaceI)
                        continue;

                    Voxel sideVoxel = VoxelAt(oldPos - adjustDirection + Voxel.NormalForFaceI(sideFaceI), false);
                    if (sideVoxel != null && (!sideVoxel.faces[sideFaceI].IsEmpty()))
                    {
                        // expand side
                        Voxel newSideVoxel = VoxelAt(oldPos + Voxel.NormalForFaceI(sideFaceI), true);
                        faceChangeQueue.Add(new FaceChange(newSideVoxel, sideFaceI, sideVoxel.faces[sideFaceI]));
                    }

                    if (!oldVoxel.faces[sideFaceI].IsEmpty())
                    {
                        // contract side
                        faceChangeQueue.Add(new FaceChange(oldVoxel, sideFaceI, Voxel.EMPTY_FACE));
                    }

                    Voxel adjacentVoxel = VoxelAt(oldPos + Voxel.NormalForFaceI(sideFaceI), false);
                    if (adjacentVoxel != null && (!adjacentVoxel.faces[faceI].IsEmpty())
                        && (!adjacentVoxel.faces[faceI].selected))
                    {
                        // create side
                        Voxel newSideVoxel = VoxelAt(oldPos + Voxel.NormalForFaceI(sideFaceI), true);
                        faceChangeQueue.Add(new FaceChange(newSideVoxel, sideFaceI, faceRef.face));
                    }
                }
            } // end if pulling
            else if (pushing)
            {
                for (int sideFaceI = 0; sideFaceI < 6; sideFaceI++)
                {
                    if (sideFaceI == faceI || sideFaceI == oppositeFaceI)
                        continue;
                    int oppositeSideFaceI = Voxel.OppositeFaceI(sideFaceI);

                    if (!oldVoxel.faces[sideFaceI].IsEmpty())
                    {
                        // expand side
                        Voxel sideVoxelCheck = VoxelAt(newPos + Voxel.NormalForFaceI(oppositeSideFaceI), false);
                        if (sideVoxelCheck == null || sideVoxelCheck.faces[oppositeSideFaceI].IsEmpty())
                            // prevent edge case when expanding directly next to an open region
                            // the other face will be deleted with the contract side code below
                            faceChangeQueue.Add(new FaceChange(newVoxel, sideFaceI, oldVoxel.faces[sideFaceI]));
                    }

                    Voxel sideVoxel = VoxelAt(newPos + Voxel.NormalForFaceI(sideFaceI), false);
                    if (sideVoxel != null && !sideVoxel.faces[sideFaceI].IsEmpty())
                    {
                        // contract side
                        faceChangeQueue.Add(new FaceChange(sideVoxel, sideFaceI, Voxel.EMPTY_FACE));
                    }

                    Voxel adjacentVoxel = VoxelAt(oldPos + Voxel.NormalForFaceI(sideFaceI), false);
                    if (adjacentVoxel != null && (!adjacentVoxel.faces[faceI].IsEmpty())
                        && (!adjacentVoxel.faces[faceI].selected))
                    {
                        // create side
                        faceChangeQueue.Add(new FaceChange(newVoxel, oppositeSideFaceI, faceRef.face));
                    }
                }
            } // end if pushing

            //if (!blocked)
                selectedFaces[i] = new VoxelFaceReference(newVoxel, faceI);
        } // end for each selected face

        foreach (FaceChange faceChange in faceChangeQueue)
        {
            Voxel voxel = faceChange.faceRef.voxel;
            int faceI = faceChange.faceRef.faceI;
            if (faceChange.updateFace)
            {
                bool oldSelect = voxel.faces[faceI].selected;
                if (voxel.faces[faceI].IsEmpty())
                    oldSelect = false;
                voxel.faces[faceI] = faceChange.newFace;
                if (voxel.faces[faceI].IsEmpty())
                    voxel.faces[faceI].selected = false;
                else
                    voxel.faces[faceI].selected = oldSelect;
            }
            if (faceChange.updateSelect && !voxel.faces[faceI].IsEmpty())
            {
                voxel.faces[faceI].selected = faceChange.newFace.selected;
            }
        }
        foreach (FaceChange faceChange in faceChangeQueue)
            VoxelModified(faceChange.faceRef.voxel);

        for (int i = selectedFaces.Count - 1; i >= 0; i--)
        {
            VoxelFaceReference faceRef = selectedFaces[i];
            if (faceRef.voxel == null || faceRef.voxel.gameObject == null || faceRef.face.IsEmpty())
            {
                selectedFaces.RemoveAt(i);
                continue;
            }
            if (!faceRef.face.selected)
            {
                faceRef.voxel.faces[faceRef.faceI].selected = true;
            }
        }

        selectStartBounds.center += adjustDirection;
        selectCurrentBounds.center += adjustDirection;
    } // end Adjust()


    public void TestAssignMaterial(Material mat)
    {
        foreach (VoxelFaceReference faceRef in selectedFaces)
        {
            faceRef.voxel.faces[faceRef.faceI].material = mat;
        }
    }
}
