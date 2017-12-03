using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelArray : MonoBehaviour {

    private class OctreeNode
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

    public Transform axes;
    public GameObject voxelPrefab;
    public Material selectedMaterial;

    public bool unsavedChanges = false; // set by VoxelArray, checked and cleared by EditorFile

    private OctreeNode rootNode;

    public enum SelectMode
    {
        NONE, // nothing selected
        ADJUSTED, // selection has moved since it was set
        BOX, // select inside a 3D box
        FACE // fill-select adjacent faces
    }

    public SelectMode selectMode = SelectMode.NONE; // only for the "add" selection
    // all faces where face.addSelected == true
    private List<VoxelFaceReference> selectedFaces = new List<VoxelFaceReference>();
    // all faces where face.storedSelected == true
    private List<VoxelFaceReference> storedSelectedFaces = new List<VoxelFaceReference>();
    private Bounds boxSelectStartBounds = new Bounds(Vector3.zero, Vector3.zero);
    public Bounds selectionBounds = new Bounds(Vector3.zero, Vector3.zero);

    private bool unloadUnusedAssets = false;

	void Awake () {
        rootNode = new OctreeNode(new Vector3Int(-2, -2, -2), 8);

        Voxel.selectedMaterial = selectedMaterial;

        ClearSelection();
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

    public void VoxelModified(Voxel voxel)
    {
        unsavedChanges = true;
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


    // called by TouchListener
    public void SelectBackground()
    {
        ClearSelection();
    }

    // called by TouchListener
    public void SelectDown(Voxel voxel, int faceI)
    {
        selectMode = SelectMode.BOX;
        boxSelectStartBounds = voxel.GetFaceBounds(faceI);
        selectionBounds = boxSelectStartBounds;
        UpdateBoxSelection();
    }

    // called by TouchListener
    public void SelectDrag(Voxel voxel, int faceI)
    {
        if (selectMode != SelectMode.BOX)
            return;
        Bounds oldSelectionBounds = selectionBounds;
        selectionBounds = boxSelectStartBounds;
        selectionBounds.Encapsulate(voxel.GetFaceBounds(faceI));
        if (oldSelectionBounds != selectionBounds)
            UpdateBoxSelection();
    }

    private void ClearMoveAxes()
    {
        if (axes == null)
            return;
        axes.gameObject.SetActive(false);
    }

    private void SetMoveAxes(Vector3 position)
    {
        if (axes == null)
            return;
        axes.position = position;
        axes.gameObject.SetActive(true);
    }

    public void ClearSelection()
    {
        foreach (VoxelFaceReference faceRef in selectedFaces)
        {
            faceRef.voxel.faces[faceRef.faceI].addSelected = false;
            faceRef.voxel.UpdateVoxel();
        }
        selectedFaces.Clear();
        if(storedSelectedFaces.Count == 0)
            ClearMoveAxes();
        selectMode = SelectMode.NONE;
        selectionBounds = new Bounds(Vector3.zero, Vector3.zero);
    }

    private void SelectFace(VoxelFaceReference faceRef)
    {
        if (faceRef.face.addSelected)
            return;
        faceRef.voxel.faces[faceRef.faceI].addSelected = true;
        selectedFaces.Add(faceRef);
        faceRef.voxel.UpdateVoxel();
    }

    private void SelectFace(Voxel voxel, int faceI)
    {
        SelectFace(new VoxelFaceReference(voxel, faceI));
    }

    private void DeselectFace(VoxelFaceReference faceRef)
    {
        if (!faceRef.face.addSelected)
            return;
        faceRef.voxel.faces[faceRef.faceI].addSelected = false;
        selectedFaces.Remove(faceRef);
        faceRef.voxel.UpdateVoxel();
    }

    private void DeselectFace(Voxel voxel, int faceI)
    {
        DeselectFace(new VoxelFaceReference(voxel, faceI));
    }

    private System.Collections.Generic.IEnumerable<VoxelFaceReference> IterateSelected()
    {
        foreach (VoxelFaceReference faceRef in selectedFaces)
            yield return faceRef;
        foreach (VoxelFaceReference faceRef in storedSelectedFaces)
            if (!faceRef.face.addSelected) // make sure the face isn't also in selectedFaces
                yield return faceRef;
    }

    public void StoreSelection()
    {
        // move faces out of storedSelectedFaces and into selectedFaces
        foreach (VoxelFaceReference faceRef in selectedFaces)
        {
            faceRef.voxel.faces[faceRef.faceI].addSelected = false;
            if (faceRef.face.storedSelected)
                continue; // already in storedSelectedFaces
            faceRef.voxel.faces[faceRef.faceI].storedSelected = true;
            storedSelectedFaces.Add(faceRef);
            // shouldn't need to update the voxel since it should have already been selected
        }
        selectedFaces.Clear();
        selectMode = SelectMode.NONE;
        selectionBounds = new Bounds(Vector3.zero, Vector3.zero);
    }

    private void MergeStoredSelected()
    {
        // move faces out of storedSelectedFaces and into selectedFaces
        // opposite of StoreSelection()
        foreach (VoxelFaceReference faceRef in storedSelectedFaces)
        {
            faceRef.voxel.faces[faceRef.faceI].storedSelected = false;
            if (faceRef.face.addSelected)
                continue; // already in selectedFaces
            faceRef.voxel.faces[faceRef.faceI].addSelected = true;
            selectedFaces.Add(faceRef);
            // shouldn't need to update the voxel since it should have already been selected
        }
        storedSelectedFaces.Clear();
        selectMode = SelectMode.ADJUSTED;
    }

    public void ClearStoredSelection()
    {
        foreach (VoxelFaceReference faceRef in storedSelectedFaces)
        {
            faceRef.voxel.faces[faceRef.faceI].storedSelected = false;
            faceRef.voxel.UpdateVoxel();
        }
        storedSelectedFaces.Clear();
        if (selectedFaces.Count == 0)
            ClearMoveAxes();
    }

    // including stored selection
    public bool SomethingIsSelected()
    {
        return storedSelectedFaces.Count != 0 || selectedFaces.Count != 0;
    }

    private void UpdateBoxSelection()
    {
        if (selectMode != SelectMode.BOX)
            return;
        SetMoveAxes(selectionBounds.center);

        // update selection...
        for (int i = selectedFaces.Count - 1; i >= 0; i--)
        {
            VoxelFaceReference faceRef = selectedFaces[i];
            if (!FaceInBoxSelection(faceRef.voxel, faceRef.faceI, selectionBounds))
                DeselectFace(faceRef);
        }
        UpdateBoxSelectionRecursive(rootNode, selectionBounds);
    }

    private void UpdateBoxSelectionRecursive(OctreeNode node, Bounds bounds)
    {
        if (node == null)
            return;
        if (!bounds.Intersects(node.bounds))
            return;
        if (node.size == 1)
        {
            Voxel voxel = node.voxel;
            for (int faceI = 0; faceI < voxel.faces.Length; faceI++)
            {
                if (voxel.faces[faceI].IsEmpty())
                    continue;
                if (FaceInBoxSelection(voxel, faceI, bounds))
                    SelectFace(voxel, faceI);
                else
                    DeselectFace(voxel, faceI);
            }
        }
        else
        {
            foreach (OctreeNode branch in node.branches)
                UpdateBoxSelectionRecursive(branch, bounds);
        }
    }

    private bool FaceInBoxSelection(Voxel voxel, int faceI, Bounds bounds)
    {
        bounds.Expand(new Vector3(0.1f, 0.1f, 0.1f));
        Bounds faceBounds = voxel.GetFaceBounds(faceI);
        return bounds.Contains(faceBounds.min) && bounds.Contains(faceBounds.max);
    }

    public void FaceSelectFloodFill(Voxel voxel, int faceI)
    {
        if (voxel == null)
            return;
        VoxelFace face = voxel.faces[faceI];
        if (face.IsEmpty())
            return;
        if (face.selected) // stop at boundaries of stored selection
            return;
        SelectFace(voxel, faceI);

        Vector3 position = voxel.transform.position;
        for (int sideNum = 0; sideNum < 4; sideNum++)
        {
            int sideFaceI = Voxel.SideFaceI(faceI, sideNum);
            Vector3 newPos = position + Voxel.OppositeDirectionForFaceI(sideFaceI);
            FaceSelectFloodFill(VoxelAt(newPos, false), faceI);
        }

        if (selectMode != SelectMode.FACE)
            selectionBounds = voxel.GetFaceBounds(faceI);
        else
            selectionBounds.Encapsulate(voxel.GetFaceBounds(faceI));
        selectMode = SelectMode.FACE;
        SetMoveAxes(position + new Vector3(0.5f, 0.5f, 0.5f) - Voxel.OppositeDirectionForFaceI(faceI) / 2);
    }

    public void Adjust(Vector3 adjustDirection)
    {
        MergeStoredSelected();
        // now we can safely look only the face addSelected property and the selectedFaces list
        // and ignore the storedSelected property and the storedSelectedFaces list
        // face.selected can be a substitute for face.addSelected

        int adjustDirFaceI = Voxel.FaceIForDirection(adjustDirection);
        int adjustAxis = Voxel.FaceIAxis(adjustDirFaceI);
        bool negativeAdjustAxis = adjustDirFaceI % 2 == 0;

        // sort selectedFaces in order along the adjustDirection vector
        selectedFaces.Sort(delegate(VoxelFaceReference a, VoxelFaceReference b)
        {
            // positive means A is greater than B
            // so positive means B will be adjusted before A
            Vector3 aCenter = a.voxel.GetFaceBounds(a.faceI).center;
            Vector3 bCenter = b.voxel.GetFaceBounds(b.faceI).center;
            float diff = 0;
            switch (adjustAxis)
            {
                case 0:
                    diff = bCenter.x - aCenter.x;
                    break;
                case 1:
                    diff = bCenter.y - aCenter.y;
                    break;
                case 2:
                    diff = bCenter.z - aCenter.z;
                    break;
            }
            if (negativeAdjustAxis)
                diff = -diff;
            if (diff > 0)
                return 1;
            if (diff < 0)
                return -1;
            return 0;
        });

        // HashSets prevent duplicate elements
        var voxelsToUpdate = new HashSet<Voxel>();

        for (int i = 0; i < selectedFaces.Count; i++)
        {
            VoxelFaceReference faceRef = selectedFaces[i];

            Voxel oldVoxel = faceRef.voxel;
            Vector3 oldPos = oldVoxel.transform.position;
            Vector3 newPos = oldPos + adjustDirection;
            Voxel newVoxel = VoxelAt(newPos, true);

            int faceI = faceRef.faceI;
            int oppositeFaceI = Voxel.OppositeFaceI(faceI);
            bool pushing = adjustDirFaceI == oppositeFaceI;
            bool pulling = adjustDirFaceI == faceI;

            VoxelFace movingFace = oldVoxel.faces[faceI];
            movingFace.addSelected = false;
            Substance movingSubstance = oldVoxel.substance;

            bool blocked = false; // is movement blocked?

            if (pushing)
            {
                for (int sideNum = 0; sideNum < 4; sideNum++)
                {
                    int sideFaceI = Voxel.SideFaceI(faceI, sideNum);
                    if (oldVoxel.faces[sideFaceI].IsEmpty())
                    {
                        Voxel sideVoxel = VoxelAt(oldPos + Voxel.DirectionForFaceI(sideFaceI), true);
                        sideVoxel.faces[Voxel.OppositeFaceI(sideFaceI)] = movingFace;
                        voxelsToUpdate.Add(sideVoxel);
                    }
                }

                if (!oldVoxel.faces[oppositeFaceI].IsEmpty())
                    blocked = true;
                oldVoxel.Clear();
            }
            else if (pulling)
            {
                for (int sideNum = 0; sideNum < 4; sideNum++)
                {
                    int sideFaceI = Voxel.SideFaceI(faceI, sideNum);
                    int oppositeSideFaceI = Voxel.OppositeFaceI(sideFaceI);
                    Voxel sideVoxel = VoxelAt(newPos + Voxel.DirectionForFaceI(sideFaceI), true);
                    if (sideVoxel.faces[oppositeSideFaceI].IsEmpty())
                        newVoxel.faces[sideFaceI] = movingFace;
                    else
                    {
                        sideVoxel.faces[oppositeSideFaceI].Clear();
                        voxelsToUpdate.Add(sideVoxel);
                    }
                }

                Voxel blockingVoxel = VoxelAt(newPos + adjustDirection, false);
                if (blockingVoxel != null && !blockingVoxel.faces[oppositeFaceI].IsEmpty())
                {
                    blocked = true;
                    blockingVoxel.faces[oppositeFaceI].Clear();
                    voxelsToUpdate.Add(blockingVoxel);
                }
                oldVoxel.faces[faceI].Clear();
            }
            else // sliding
            {
                oldVoxel.faces[faceI].addSelected = false;

                if (newVoxel.faces[faceI].IsEmpty())
                    blocked = true;
            }

            if (!blocked)
            {
                // move the face
                newVoxel.faces[faceI] = movingFace;
                newVoxel.faces[faceI].addSelected = true;
                newVoxel.substance = movingSubstance;
                selectedFaces[i] = new VoxelFaceReference(newVoxel, faceI);
            }
            else
            {
                // clear the selection; will be deleted later
                selectedFaces[i] = new VoxelFaceReference(null, -1);
            }

            voxelsToUpdate.Add(newVoxel);
            voxelsToUpdate.Add(oldVoxel);
        } // end for each selected face

        foreach (Voxel voxel in voxelsToUpdate)
            VoxelModified(voxel);

        for (int i = selectedFaces.Count - 1; i >= 0; i--)
        {
            if (selectedFaces[i].voxel == null)
                selectedFaces.RemoveAt(i);
        }
    } // end Adjust()


    public void AssignMaterial(Material mat)
    {
        foreach (VoxelFaceReference faceRef in IterateSelected())
        {
            if (mat == null && (faceRef.voxel.substance == null || faceRef.face.overlay == null))
                continue; // only allow null material on substances with an overlay
            faceRef.voxel.faces[faceRef.faceI].material = mat;
            VoxelModified(faceRef.voxel);
        }
    }

    public void AssignOverlay(Material mat)
    {
        foreach (VoxelFaceReference faceRef in IterateSelected())
        {
            faceRef.voxel.faces[faceRef.faceI].overlay = mat;
            VoxelModified(faceRef.voxel);
        }
    }

    public void OrientFaces(byte change)
    {
        int changeRotation = VoxelFace.GetOrientationRotation(change);
        bool changeFlip = VoxelFace.GetOrientationMirror(change);
        foreach (VoxelFaceReference faceRef in IterateSelected())
        {
            byte faceOrientation = faceRef.face.orientation;
            int faceRotation = VoxelFace.GetOrientationRotation(faceOrientation);
            bool faceFlip = VoxelFace.GetOrientationMirror(faceOrientation);
            int faceI = faceRef.faceI;
            if (faceFlip ^ (changeFlip && (faceI == 0 || faceI == 3 || faceI == 5)))
                faceRotation += 4 - changeRotation;
            else
                faceRotation += changeRotation;
            if (changeFlip)
                faceFlip = !faceFlip;
            faceOrientation = VoxelFace.Orientation(faceRotation, faceFlip);
            faceRef.voxel.faces[faceRef.faceI].orientation = faceOrientation;
            VoxelModified(faceRef.voxel);
        }
    }

    public void SubstanceTest()
    {
        Substance substance = new Substance();
        foreach (VoxelFaceReference faceRef in IterateSelected())
        {
            faceRef.voxel.substance = substance;
        }
    }
}
