using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelArrayEditor : VoxelArray
{
    public static VoxelArrayEditor instance = null;

    public Transform axes;

    public Material selectedMaterial;
    public Material xRayMaterial;

    public bool unsavedChanges = false; // set by VoxelArrayEditor, checked and cleared by EditorFile
    public bool selectionChanged = false; // set by VoxelArrayEditor, checked and cleared by PropertiesGUI

    public WorldProperties world = new WorldProperties();

    public enum SelectMode
    {
        NONE, // nothing selected
        ADJUSTED, // selection has moved since it was set
        BOX, // select inside a 3D box
        FACE // fill-select adjacent faces
    }

    private SelectMode selectMode = SelectMode.NONE; // only for the "add" selection
    // all faces where face.addSelected == true
    private List<VoxelFaceReference> selectedFaces = new List<VoxelFaceReference>();
    // all faces where face.storedSelected == true
    private List<VoxelFaceReference> storedSelectedFaces = new List<VoxelFaceReference>();
    private Bounds boxSelectStartBounds = new Bounds(Vector3.zero, Vector3.zero);
    private Substance boxSelectSubstance = null;
    public Bounds selectionBounds = new Bounds(Vector3.zero, Vector3.zero);

    public struct SelectionState
    {
        public List<VoxelFaceReference> selectedFaces;
        public List<VoxelFaceReference> storedSelectedFaces;

        public SelectMode selectMode;
        public Vector3 axes;
    }

    public override void Awake()
    {
        base.Awake();

        if (instance == null)
            instance = this;

        Voxel.selectedMaterial = selectedMaterial;
        Voxel.xRayMaterial = xRayMaterial;

        ClearSelection();
        selectionChanged = false;
    }

    public override void VoxelModified(Voxel voxel)
    {
        unsavedChanges = true;
        base.VoxelModified(voxel);
    }

    // called by TouchListener
    public void TouchDown(Voxel voxel, int faceI)
    {
        SetMoveAxesEnabled(false);
        if (voxel == null)
        {
            ClearSelection();
            return;
        }
        selectMode = SelectMode.BOX;
        boxSelectStartBounds = voxel.GetFaceBounds(faceI);
        selectionBounds = boxSelectStartBounds;
        boxSelectSubstance = voxel.substance;
        UpdateBoxSelection();
    }

    // called by TouchListener
    public void TouchDrag(Voxel voxel, int faceI)
    {
        if (selectMode != SelectMode.BOX)
            return;
        Bounds oldSelectionBounds = selectionBounds;
        selectionBounds = boxSelectStartBounds;
        selectionBounds.Encapsulate(voxel.GetFaceBounds(faceI));
        if (oldSelectionBounds != selectionBounds)
            UpdateBoxSelection();
    }

    // called by TouchListener
    public void TouchUp()
    {
        if (SomethingIsSelected())
            SetMoveAxesEnabled(true);
    }

    // called by TouchListener
    public void DoubleTouch(Voxel voxel, int faceI)
    {
        ClearSelection();
        FaceSelectFloodFill(voxel, faceI, voxel.substance);
        if(SomethingIsSelected())
            SetMoveAxesEnabled(true);
    }

    private void SetMoveAxes(Vector3 position)
    {
        if (axes == null)
            return;
        axes.position = position;
    }

    private void SetMoveAxesEnabled(bool enabled)
    {
        if (axes == null)
            return;
        axes.gameObject.SetActive(enabled);
    }

    public void ClearSelection()
    {
        foreach (VoxelFaceReference faceRef in selectedFaces)
        {
            faceRef.voxel.faces[faceRef.faceI].addSelected = false;
            faceRef.voxel.UpdateVoxel();
        }
        selectedFaces.Clear();
        if (!SomethingIsSelected())
            SetMoveAxesEnabled(false);
        selectMode = SelectMode.NONE;
        selectionBounds = new Bounds(Vector3.zero, Vector3.zero);
        selectionChanged = true;
    }

    private void SelectFace(VoxelFaceReference faceRef)
    {
        if (faceRef.face.addSelected)
            return;
        faceRef.voxel.faces[faceRef.faceI].addSelected = true;
        selectedFaces.Add(faceRef);
        faceRef.voxel.UpdateVoxel();
        selectionChanged = true;
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
        selectionChanged = true;
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

    // including stored selection
    public bool SomethingIsSelected()
    {
        return SomethingIsAddSelected() || SomethingIsStoredSelected();
    }

    public bool SomethingIsAddSelected()
    {
        return selectedFaces.Count != 0;
    }

    public bool SomethingIsStoredSelected()
    {
        return storedSelectedFaces.Count != 0;
    }

    public ICollection<Entity> GetSelectedEntities()
    {
        var selectedEntities = new HashSet<Entity>();
        foreach (VoxelFaceReference faceRef in IterateSelected())
            if (faceRef.voxel.substance != null)
                selectedEntities.Add(faceRef.voxel.substance); // HashSet will prevent duplicates
        return selectedEntities;
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

    public void MergeStoredSelected()
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
        if (!SomethingIsSelected())
            SetMoveAxesEnabled(false);
        selectionChanged = true;
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
        UpdateBoxSelectionRecursive(rootNode, selectionBounds, boxSelectSubstance);
    }

    private void UpdateBoxSelectionRecursive(OctreeNode node, Bounds bounds, Substance substance)
    {
        if (node == null)
            return;
        if (!bounds.Intersects(node.bounds))
            return;
        if (node.size == 1)
        {
            Voxel voxel = node.voxel;
            if (voxel.substance != substance)
                return;
            for (int faceI = 0; faceI < voxel.faces.Length; faceI++)
            {
                if (voxel.faces[faceI].IsEmpty())
                    continue;
                if (FaceInBoxSelection(voxel, faceI, bounds))
                    SelectFace(voxel, faceI);
            }
        }
        else
        {
            foreach (OctreeNode branch in node.branches)
                UpdateBoxSelectionRecursive(branch, bounds, substance);
        }
    }

    private bool FaceInBoxSelection(Voxel voxel, int faceI, Bounds bounds)
    {
        bounds.Expand(new Vector3(0.1f, 0.1f, 0.1f));
        Bounds faceBounds = voxel.GetFaceBounds(faceI);
        return bounds.Contains(faceBounds.min) && bounds.Contains(faceBounds.max);
    }

    public void FaceSelectFloodFill(Voxel voxel, int faceI, Substance substance)
    {
        if (voxel == null)
            return;
        if (voxel.substance != substance)
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
            FaceSelectFloodFill(VoxelAt(newPos, false), faceI, substance);
        }

        if (selectMode != SelectMode.FACE)
            selectionBounds = voxel.GetFaceBounds(faceI);
        else
            selectionBounds.Encapsulate(voxel.GetFaceBounds(faceI));
        selectMode = SelectMode.FACE;
        SetMoveAxes(position + new Vector3(0.5f, 0.5f, 0.5f) - Voxel.OppositeDirectionForFaceI(faceI) / 2);
    }

    public SelectionState GetSelectionState()
    {
        SelectionState state;
        state.selectedFaces = new List<VoxelFaceReference>(selectedFaces);
        state.storedSelectedFaces = new List<VoxelFaceReference>(storedSelectedFaces);
        state.selectMode = selectMode;
        state.axes = axes.position;
        return state;
    }

    public void RecallSelectionState(SelectionState state)
    {
        ClearSelection();
        ClearStoredSelection();
        foreach (VoxelFaceReference faceRef in state.storedSelectedFaces)
            SelectFace(faceRef);
        StoreSelection();
        foreach (VoxelFaceReference faceRef in state.selectedFaces)
            SelectFace(faceRef);
        selectMode = state.selectMode;
        axes.position = state.axes;
        if (SomethingIsSelected())
            SetMoveAxesEnabled(true);
    }

    public void Adjust(Vector3 adjustDirection)
    {
        MergeStoredSelected();
        // now we can safely look only the face addSelected property and the selectedFaces list
        // and ignore the storedSelected property and the storedSelectedFaces list
        // face.selected can be a substitute for face.addSelected

        int adjustDirFaceI = Voxel.FaceIForDirection(adjustDirection);
        int oppositeAdjustDirFaceI = Voxel.OppositeFaceI(adjustDirFaceI);
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
            if (a.faceI == oppositeAdjustDirFaceI)
                if (b.faceI != oppositeAdjustDirFaceI)
                    return -1; // move one substance back before moving other forward
                else if (b.faceI == oppositeAdjustDirFaceI)
                    return 1;
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

            if (pulling && (!newVoxel.faces[oppositeFaceI].IsEmpty()) && !newVoxel.faces[oppositeFaceI].selected)
            {
                // usually this means there's another substance. push it away before this face
                newVoxel.faces[oppositeFaceI].addSelected = true;
                selectedFaces.Insert(i, new VoxelFaceReference(newVoxel, oppositeFaceI));
                i -= 1;
                continue;
            }

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
                    Voxel sideVoxel = VoxelAt(newPos + Voxel.DirectionForFaceI(sideFaceI), false);
                    if (sideVoxel == null || sideVoxel.faces[oppositeSideFaceI].IsEmpty() || movingSubstance != sideVoxel.substance)
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
                    if (movingSubstance == blockingVoxel.substance)
                    {
                        blocked = true;
                        blockingVoxel.faces[oppositeFaceI].Clear();
                        voxelsToUpdate.Add(blockingVoxel);
                    }
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
        selectionChanged = true;
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
            if (mat == null && faceRef.face.material == null)
                continue; // can't have no material and no overlay
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
        Substance substance = new Substance(this);
        foreach (VoxelFaceReference faceRef in IterateSelected())
        {
            faceRef.voxel.substance = substance;
            VoxelModified(faceRef.voxel);
        }
    }
}