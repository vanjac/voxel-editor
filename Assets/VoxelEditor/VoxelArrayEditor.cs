using System.Collections;
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

    private struct VoxelFaceReference : VoxelArrayEditor.Selectable
    {
        public Voxel voxel;
        public int faceI;

        public bool addSelected
        {
            get
            {
                return face.addSelected;
            }
            set
            {
                voxel.faces[faceI].addSelected = value;
            }
        }
        public bool storedSelected
        {
            get
            {
                return face.storedSelected;
            }
            set
            {
                voxel.faces[faceI].storedSelected = value;
            }
        }
        public bool selected
        {
            get
            {
                return (!face.IsEmpty()) && (face.addSelected || face.storedSelected);
            }
        }
        public Bounds bounds
        {
            get
            {
                return voxel.GetFaceBounds(faceI);
            }
        }

        public VoxelFaceReference(Voxel voxel, int faceI)
        {
            this.voxel = voxel;
            this.faceI = faceI;
        }

        public VoxelFace face
        {
            get
            {
                return voxel.faces[faceI];
            }
        }

        public void SelectionStateUpdated()
        {
            voxel.UpdateVoxel();
        }
    }

    public static VoxelArrayEditor instance = null;

    public Transform axes;

    public Material selectedMaterial;
    public Material xRayMaterial;

    public bool unsavedChanges = false; // set by VoxelArrayEditor, checked and cleared by EditorFile
    public bool selectionChanged = false; // set by VoxelArrayEditor, checked and cleared by PropertiesGUI

    public enum SelectMode
    {
        NONE, // nothing selected
        ADJUSTED, // selection has moved since it was set
        BOX, // select inside a 3D box
        FACE, // fill-select adjacent faces
        SURFACE // fill-select all connected faces
    }

    public SelectMode selectMode = SelectMode.NONE; // only for the "add" selection
    // all faces where face.addSelected == true
    private List<Selectable> selectedThings = new List<Selectable>();
    // all faces where face.storedSelected == true
    private List<Selectable> storedSelectedThings = new List<Selectable>();
    public Bounds boxSelectStartBounds = new Bounds(Vector3.zero, Vector3.zero);
    private Substance boxSelectSubstance = null;
    // dummy Substance to use for boxSelectSubstance when selecting objects
    private readonly Substance selectObjectSubstance = new Substance();
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
        TouchDown(new VoxelFaceReference(voxel, faceI));
    }

    public void TouchDown(Selectable thing)
    {
        SetMoveAxesEnabled(false);
        if (thing == null)
        {
            ClearSelection();
            return;
        }
        selectMode = SelectMode.BOX;
        boxSelectStartBounds = thing.bounds;
        selectionBounds = boxSelectStartBounds;
        if (thing is VoxelFaceReference)
            boxSelectSubstance = ((VoxelFaceReference)thing).voxel.substance;
        else if (thing is ObjectMarker)
            boxSelectSubstance = selectObjectSubstance;
        else
            boxSelectSubstance = null;
        UpdateBoxSelection();
    }

    // called by TouchListener
    public void TouchDrag(Voxel voxel, int faceI)
    {
        TouchDrag(new VoxelFaceReference(voxel, faceI));
    }

    public void TouchDrag(Selectable thing)
    {
        if (selectMode != SelectMode.BOX)
            return;
        Bounds oldSelectionBounds = selectionBounds;
        selectionBounds = boxSelectStartBounds;
        selectionBounds.Encapsulate(thing.bounds);
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
        if (SomethingIsSelected())
            SetMoveAxesEnabled(true);
    }

    // called by TouchListener
    public void TripleTouch(Voxel voxel, int faceI)
    {
        if (voxel == null)
            return;
        ClearSelection();
        if (voxel.substance == null)
        {
            SurfaceSelectFloodFill(voxel, faceI, voxel.substance);
            if (SomethingIsSelected())
                SetMoveAxesEnabled(true);
        }
        else
        {
            SubstanceSelect(voxel.substance);
            if (SomethingIsSelected())
                SetMoveAxesEnabled(true);
        }
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

    private System.Collections.Generic.IEnumerable<Selectable> IterateSelected()
    {
        foreach (Selectable thing in selectedThings)
            yield return thing;
        foreach (Selectable thing in storedSelectedThings)
            if (!thing.addSelected) // make sure the thing isn't also in selectedThings
                yield return thing;
    }

    private System.Collections.Generic.IEnumerable<VoxelFaceReference> IterateSelectedFaces()
    {
        foreach (Selectable thing in IterateSelected())
            if (thing is VoxelFaceReference)
                yield return (VoxelFaceReference)thing;
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

    public bool FacesAreSelected()
    {
        foreach (VoxelFaceReference faceRef in IterateSelectedFaces())
            return true;
        return false;
    }

    public ICollection<Entity> GetSelectedEntities()
    {
        var selectedEntities = new HashSet<Entity>();
        foreach (VoxelFaceReference faceRef in IterateSelectedFaces())
            if (faceRef.voxel.substance != null)
                selectedEntities.Add(faceRef.voxel.substance); // HashSet will prevent duplicates
        foreach (ObjectEntity obj in objects)
            if (obj.marker.selected)
                selectedEntities.Add(obj);
        return selectedEntities;
    }

    public void StoreSelection()
    {
        // move things out of storedSelectedThings and into selectedThings
        foreach (Selectable thing in selectedThings)
        {
            thing.addSelected = false;
            if (thing.storedSelected)
                continue; // already in storedSelectedThings
            thing.storedSelected = true;
            storedSelectedThings.Add(thing);
            // shouldn't need to update the voxel since it should have already been selected
        }
        selectedThings.Clear();
        selectMode = SelectMode.NONE;
        selectionBounds = new Bounds(Vector3.zero, Vector3.zero);
    }

    public void MergeStoredSelected()
    {
        // move things out of storedSelectedThings and into selectedThings
        // opposite of StoreSelection()
        foreach (Selectable thing in storedSelectedThings)
        {
            thing.storedSelected = false;
            if (thing.addSelected)
                continue; // already in selectedThings
            thing.addSelected = true;
            selectedThings.Add(thing);
            // shouldn't need to update the voxel since it should have already been selected
        }
        storedSelectedThings.Clear();
        selectMode = SelectMode.ADJUSTED;
    }

    public void ClearStoredSelection()
    {
        foreach (Selectable thing in storedSelectedThings)
        {
            thing.storedSelected = false;
            thing.SelectionStateUpdated();
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
        foreach (ObjectEntity obj in objects)
        {
            if (boxSelectSubstance == selectObjectSubstance
                    && ThingInBoxSelection(obj.marker, selectionBounds))
                SelectThing(obj.marker);
            else
                DeselectThing(obj.marker);
        }
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
        if (face.addSelected || face.storedSelected) // stop at boundaries of stored selection
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

    public void SurfaceSelectFloodFill(Voxel voxel, int faceI, Substance substance)
    {
        if (voxel == null)
            return;
        if (voxel.substance != substance)
            return;
        VoxelFace face = voxel.faces[faceI];
        if (face.IsEmpty())
            return;
        if (face.addSelected || face.storedSelected) // stop at boundaries of stored selection
            return;
        SelectFace(voxel, faceI);

        Vector3 position = voxel.transform.position;
        for (int sideNum = 0; sideNum < 4; sideNum++)
        {
            int sideFaceI = Voxel.SideFaceI(faceI, sideNum);
            SurfaceSelectFloodFill(voxel, sideFaceI, substance);
            Vector3 newPos = position + Voxel.DirectionForFaceI(sideFaceI);
            SurfaceSelectFloodFill(VoxelAt(newPos, false), faceI, substance);
            newPos += Voxel.DirectionForFaceI(faceI);
            SurfaceSelectFloodFill(VoxelAt(newPos, false), Voxel.OppositeFaceI(sideFaceI), substance);
        }

        if (selectMode != SelectMode.SURFACE)
            selectionBounds = voxel.GetFaceBounds(faceI);
        else
            selectionBounds.Encapsulate(voxel.GetFaceBounds(faceI));
        selectMode = SelectMode.SURFACE;
        SetMoveAxes(position + new Vector3(0.5f, 0.5f, 0.5f) - Voxel.OppositeDirectionForFaceI(faceI) / 2);
    }

    public void SubstanceSelect(Substance substance)
    {
        foreach (Voxel v in substance.voxels)
            for (int i = 0; i < 6; i++)
                if (!v.faces[i].IsEmpty())
                {
                    SelectFace(v, i);
                    if (selectMode != SelectMode.SURFACE)
                        selectionBounds = v.GetFaceBounds(i);
                    else
                        selectionBounds.Encapsulate(v.GetFaceBounds(i));
                    selectMode = SelectMode.SURFACE;
                }
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
        foreach (Selectable thing in state.storedSelectedThings)
            SelectThing(thing);
        StoreSelection();
        foreach (Selectable thing in state.selectedThings)
            SelectThing(thing);
        selectMode = state.selectMode;
        axes.position = state.axes;
        if (SomethingIsSelected())
            SetMoveAxesEnabled(true);
    }

    public void Adjust(Vector3 adjustDirection)
    {
        MergeStoredSelected();
        // now we can safely look only the addSelected property and the selectedThings list
        // and ignore the storedSelected property and the storedSelectedThings list

        foreach (ObjectEntity obj in objects)
        {
            if (obj.marker.addSelected)
            {
                obj.position += Vector3ToInt(adjustDirection);
                obj.UpdateEntityEditor();
                unsavedChanges = true;
            }
        }

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
        bool temporarilyBlockPushingANewSubstance = false;

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

            if (pulling && (!newVoxel.faces[oppositeFaceI].IsEmpty()) && !newVoxel.faces[oppositeFaceI].addSelected)
            {
                // usually this means there's another substance. push it away before this face
                if (substanceToCreate != null && newVoxel.substance == substanceToCreate)
                {
                    // substance has already been created there!
                    // substanceToCreate has never existed in the map before Adjust() was called
                    // so it must have been created earlier in the loop
                    // remove selection
                    oldVoxel.faces[faceI].addSelected = false;
                    selectedThings[i] = new VoxelFaceReference(null, -1);
                    voxelsToUpdate.Add(oldVoxel);
                }
                else
                {
                    newVoxel.faces[oppositeFaceI].addSelected = true;
                    selectedThings.Insert(i, new VoxelFaceReference(newVoxel, oppositeFaceI));
                    i -= 1;
                    // need to move the other substance out of the way first
                    temporarilyBlockPushingANewSubstance = true;
                }
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
                        // add side
                        Vector3 sideFaceDir = Voxel.DirectionForFaceI(sideFaceI);
                        Voxel sideVoxel = VoxelAt(oldPos + sideFaceDir, true);
                        int oppositeSideFaceI = Voxel.OppositeFaceI(sideFaceI);

                        // if possible, the new side should have the properties of the adjacent side
                        Voxel adjacentSideVoxel = VoxelAt(oldPos - adjustDirection + sideFaceDir, false);
                        if (adjacentSideVoxel != null && !adjacentSideVoxel.faces[oppositeSideFaceI].IsEmpty()
                            && movingSubstance == adjacentSideVoxel.substance)
                        {
                            sideVoxel.faces[oppositeSideFaceI] = adjacentSideVoxel.faces[oppositeSideFaceI];
                            sideVoxel.faces[oppositeSideFaceI].addSelected = false;
                        }
                        else
                        {
                            sideVoxel.faces[oppositeSideFaceI] = movingFace;
                        }
                        voxelsToUpdate.Add(sideVoxel);
                    }
                }

                if (!oldVoxel.faces[oppositeFaceI].IsEmpty())
                    blocked = true;
                oldVoxel.Clear();
                if (substanceToCreate != null && !temporarilyBlockPushingANewSubstance)
                    newSubstanceBlock = CreateSubstanceBlock(oldPos, substanceToCreate, movingFace);
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
                    {
                        // add side
                        // if possible, the new side should have the properties of the adjacent side
                        if (!oldVoxel.faces[sideFaceI].IsEmpty())
                        {
                            newVoxel.faces[sideFaceI] = oldVoxel.faces[sideFaceI];
                            newVoxel.faces[sideFaceI].addSelected = false;
                        }
                        else
                            newVoxel.faces[sideFaceI] = movingFace;
                    }
                    else
                    {
                        // delete side
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

                if (newVoxel.faces[faceI].IsEmpty() || newVoxel.substance != movingSubstance)
                    blocked = true;
            }

            if (!blocked)
            {
                // move the face
                newVoxel.faces[faceI] = movingFace;
                newVoxel.faces[faceI].addSelected = true;
                newVoxel.substance = movingSubstance;
                selectedThings[i] = new VoxelFaceReference(newVoxel, faceI);
            }
            else
            {
                // clear the selection; will be deleted later
                selectedThings[i] = new VoxelFaceReference(null, -1);
                if (pulling && substanceToCreate == null)
                    newVoxel.substance = movingSubstance;
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

            if (temporarilyBlockPushingANewSubstance)
                temporarilyBlockPushingANewSubstance = false;
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

        SetMoveAxesEnabled(SomethingIsSelected());
    } // end Adjust()

    private Voxel CreateSubstanceBlock(Vector3 position, Substance substance, VoxelFace faceTemplate)
    {
        if (!substance.defaultPaint.IsEmpty())
            faceTemplate = substance.defaultPaint;
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
        foreach (VoxelFaceReference faceRef in IterateSelectedFaces())
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
        foreach (VoxelFaceReference faceRef in IterateSelectedFaces())
        {
            if (paint.material != null || faceRef.voxel.substance != null)
                faceRef.voxel.faces[faceRef.faceI].material = paint.material;
            faceRef.voxel.faces[faceRef.faceI].overlay = paint.overlay;
            faceRef.voxel.faces[faceRef.faceI].orientation = paint.orientation;
            VoxelModified(faceRef.voxel);
        }
    }

    public int GetSelectedFaceNormal()
    {
        int faceI = -1;
        foreach (VoxelFaceReference faceRef in IterateSelectedFaces())
        {
            if (faceI == -1)
                faceI = faceRef.faceI;
            else if (faceRef.faceI != faceI)
                return -1;
        }
        return faceI;
    }
}