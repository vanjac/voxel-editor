﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelArrayEditor : VoxelArray
{
    public interface Selectable
    {
        bool addSelected { get; set; }
        bool storedSelected { get; set; }
        bool selected { get; }
        Bounds bounds { get; }

        void SelectionStateUpdated();
    }

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
    private List<Selectable> selectedThings = new List<Selectable>();
    // all faces where face.storedSelected == true
    private List<Selectable> storedSelectedThings = new List<Selectable>();
    private Bounds boxSelectStartBounds = new Bounds(Vector3.zero, Vector3.zero);
    private Substance boxSelectSubstance = null;
    public Bounds selectionBounds = new Bounds(Vector3.zero, Vector3.zero);

    public Substance substanceToCreate = null;

    public struct SelectionState
    {
        public List<Selectable> selectedThings;
        public List<Selectable> storedSelectedThings;

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
        foreach (Selectable thing in selectedThings)
        {
            thing.addSelected = false;
            thing.SelectionStateUpdated();
        }
        selectedThings.Clear();
        if (!SomethingIsSelected())
            SetMoveAxesEnabled(false);
        selectMode = SelectMode.NONE;
        selectionBounds = new Bounds(Vector3.zero, Vector3.zero);
        selectionChanged = true;
    }

    private void SelectThing(Selectable thing)
    {
        if (thing.addSelected)
            return;
        thing.addSelected = true;
        selectedThings.Add(thing);
        thing.SelectionStateUpdated();
        selectionChanged = true;
    }

    private void SelectFace(Voxel voxel, int faceI)
    {
        SelectThing(new VoxelFaceReference(voxel, faceI));
    }

    private void DeselectThing(Selectable thing)
    {
        if (!thing.addSelected)
            return;
        thing.addSelected = false;
        selectedThings.Remove(thing);
        thing.SelectionStateUpdated();
        selectionChanged = true;
    }

    private void DeselectFace(Voxel voxel, int faceI)
    {
        DeselectThing(new VoxelFaceReference(voxel, faceI));
    }

    private System.Collections.Generic.IEnumerable<VoxelFaceReference> IterateSelected()
    {
        foreach (VoxelFaceReference faceRef in selectedThings)
            yield return faceRef;
        foreach (VoxelFaceReference faceRef in storedSelectedThings)
            if (!faceRef.face.addSelected) // make sure the face isn't also in selectedThings
                yield return faceRef;
    }

    // including stored selection
    public bool SomethingIsSelected()
    {
        return SomethingIsAddSelected() || SomethingIsStoredSelected();
    }

    public bool SomethingIsAddSelected()
    {
        return selectedThings.Count != 0;
    }

    public bool SomethingIsStoredSelected()
    {
        return storedSelectedThings.Count != 0;
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
        // move faces out of storedSelectedThings and into selectedThings
        foreach (VoxelFaceReference faceRef in selectedThings)
        {
            faceRef.voxel.faces[faceRef.faceI].addSelected = false;
            if (faceRef.face.storedSelected)
                continue; // already in storedSelectedThings
            faceRef.voxel.faces[faceRef.faceI].storedSelected = true;
            storedSelectedThings.Add(faceRef);
            // shouldn't need to update the voxel since it should have already been selected
        }
        selectedThings.Clear();
        selectMode = SelectMode.NONE;
        selectionBounds = new Bounds(Vector3.zero, Vector3.zero);
    }

    public void MergeStoredSelected()
    {
        // move faces out of storedSelectedThings and into selectedThings
        // opposite of StoreSelection()
        foreach (VoxelFaceReference faceRef in storedSelectedThings)
        {
            faceRef.voxel.faces[faceRef.faceI].storedSelected = false;
            if (faceRef.face.addSelected)
                continue; // already in selectedThings
            faceRef.voxel.faces[faceRef.faceI].addSelected = true;
            selectedThings.Add(faceRef);
            // shouldn't need to update the voxel since it should have already been selected
        }
        storedSelectedThings.Clear();
        selectMode = SelectMode.ADJUSTED;
    }

    public void ClearStoredSelection()
    {
        foreach (VoxelFaceReference faceRef in storedSelectedThings)
        {
            faceRef.voxel.faces[faceRef.faceI].storedSelected = false;
            faceRef.voxel.UpdateVoxel();
        }
        storedSelectedThings.Clear();
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
        for (int i = selectedThings.Count - 1; i >= 0; i--)
        {
            Selectable thing = selectedThings[i];
            if (!ThingInBoxSelection(thing, selectionBounds))
                DeselectThing(thing);
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
                if (ThingInBoxSelection(new VoxelFaceReference(voxel, faceI), bounds))
                    SelectFace(voxel, faceI);
            }
        }
        else
        {
            foreach (OctreeNode branch in node.branches)
                UpdateBoxSelectionRecursive(branch, bounds, substance);
        }
    }

    private bool ThingInBoxSelection(Selectable thing, Bounds bounds)
    {
        bounds.Expand(new Vector3(0.1f, 0.1f, 0.1f));
        Bounds thingBounds = thing.bounds;
        return bounds.Contains(thingBounds.min) && bounds.Contains(thingBounds.max);
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
        state.selectedThings = new List<Selectable>(selectedThings);
        state.storedSelectedThings = new List<Selectable>(storedSelectedThings);
        state.selectMode = selectMode;
        state.axes = axes.position;
        return state;
    }

    public void RecallSelectionState(SelectionState state)
    {
        ClearSelection();
        ClearStoredSelection();
        foreach (VoxelFaceReference faceRef in state.storedSelectedThings)
            SelectThing(faceRef);
        StoreSelection();
        foreach (VoxelFaceReference faceRef in state.selectedThings)
            SelectThing(faceRef);
        selectMode = state.selectMode;
        axes.position = state.axes;
        if (SomethingIsSelected())
            SetMoveAxesEnabled(true);
    }

    public void Adjust(Vector3 adjustDirection)
    {
        MergeStoredSelected();
        // now we can safely look only the face addSelected property and the selectedThings list
        // and ignore the storedSelected property and the storedSelectedThings list
        // face.selected can be a substitute for face.addSelected

        int adjustDirFaceI = Voxel.FaceIForDirection(adjustDirection);
        int oppositeAdjustDirFaceI = Voxel.OppositeFaceI(adjustDirFaceI);
        int adjustAxis = Voxel.FaceIAxis(adjustDirFaceI);
        bool negativeAdjustAxis = adjustDirFaceI % 2 == 0;

        // sort selectedThings in order along the adjustDirection vector
        selectedThings.Sort(delegate(Selectable a, Selectable b)
        {
            // positive means A is greater than B
            // so positive means B will be adjusted before A
            Vector3 aCenter = a.bounds.center;
            Vector3 bCenter = b.bounds.center;
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
            if (a is VoxelFaceReference && b is VoxelFaceReference)
            {
                var aFace = (VoxelFaceReference)a;
                var bFace = (VoxelFaceReference)b;
                if (aFace.faceI == oppositeAdjustDirFaceI)
                    if (bFace.faceI != oppositeAdjustDirFaceI)
                        return -1; // move one substance back before moving other forward
                    else if (bFace.faceI == oppositeAdjustDirFaceI)
                        return 1;
            }
            return 0;
        });

        // HashSets prevent duplicate elements
        var voxelsToUpdate = new HashSet<Voxel>();
        bool createdSubstance = false;

        for (int i = 0; i < selectedThings.Count; i++)
        {
            Selectable thing = selectedThings[i];
            if (!(thing is VoxelFaceReference))
                continue;
            VoxelFaceReference faceRef = (VoxelFaceReference)thing;

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
                selectedThings.Insert(i, new VoxelFaceReference(newVoxel, oppositeFaceI));
                i -= 1;
                continue;
            }

            VoxelFace movingFace = oldVoxel.faces[faceI];
            movingFace.addSelected = false;
            Substance movingSubstance = oldVoxel.substance;

            bool blocked = false; // is movement blocked?
            Voxel newSubstanceBlock = null;

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
            else if (pulling && substanceToCreate != null)
            {
                newSubstanceBlock = CreateSubstanceBlock(newPos, substanceToCreate, movingFace);
                oldVoxel.faces[faceI].addSelected = false;
                blocked = true;
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
                selectedThings[i] = new VoxelFaceReference(newVoxel, faceI);

                if (pushing && substanceToCreate != null)
                    newSubstanceBlock = CreateSubstanceBlock(oldPos, substanceToCreate, movingFace);
            }
            else
            {
                // clear the selection; will be deleted later
                selectedThings[i] = new VoxelFaceReference(null, -1);
            }

            if (newSubstanceBlock != null)
            {
                createdSubstance = true;
                if (!newSubstanceBlock.faces[adjustDirFaceI].IsEmpty())
                {
                    newSubstanceBlock.faces[adjustDirFaceI].addSelected = true;
                    selectedThings.Insert(0, new VoxelFaceReference(newSubstanceBlock, adjustDirFaceI));
                    i += 1;
                }
            }

            voxelsToUpdate.Add(newVoxel);
            voxelsToUpdate.Add(oldVoxel);
        } // end for each selected face

        foreach (Voxel voxel in voxelsToUpdate)
            VoxelModified(voxel);

        for (int i = selectedThings.Count - 1; i >= 0; i--)
        {
            Selectable thing = selectedThings[i];
            if ((thing is VoxelFaceReference)
                    && ((VoxelFaceReference)thing).voxel == null)
                selectedThings.RemoveAt(i);
        }
        selectionChanged = true;

        if (substanceToCreate != null && createdSubstance)
            substanceToCreate = null;
    } // end Adjust()

    private Voxel CreateSubstanceBlock(Vector3 position, Substance substance, VoxelFace faceTemplate)
    {
        Voxel voxel = VoxelAt(position, true);
        if (!voxel.IsEmpty())
        {
            if (voxel.substance == substance)
                return voxel;
            return null; // doesn't work
        }
        voxel.substance = substance;
        for (int faceI = 0; faceI < 6; faceI++)
        {
            Voxel adjacentVoxel = VoxelAt(position + Voxel.DirectionForFaceI(faceI), false);
            if (adjacentVoxel == null || adjacentVoxel.substance != substance)
            {
                // create boundary
                voxel.faces[faceI] = faceTemplate;
            }
            else
            {
                // remove boundary
                adjacentVoxel.faces[Voxel.OppositeFaceI(faceI)].Clear();
                voxel.faces[faceI].Clear();
            }
        }
        return voxel;
    }

    public VoxelFace GetSelectedPaint()
    {
        foreach (VoxelFaceReference faceRef in IterateSelected())
        {
            VoxelFace face = faceRef.face;
            face.addSelected = false;
            face.storedSelected = false;
            return face;
        }
        return new VoxelFace();
    }

    public void PaintSelectedFaces(VoxelFace paint)
    {
        foreach (VoxelFaceReference faceRef in IterateSelected())
        {
            if (paint.material != null || faceRef.voxel.substance != null)
                faceRef.voxel.faces[faceRef.faceI].material = paint.material;
            faceRef.voxel.faces[faceRef.faceI].overlay = paint.overlay;
            faceRef.voxel.faces[faceRef.faceI].orientation = paint.orientation;
            VoxelModified(faceRef.voxel);
        }
    }

}