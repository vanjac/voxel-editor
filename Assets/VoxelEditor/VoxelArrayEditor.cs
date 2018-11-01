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

    private struct VoxelEdgeReference : VoxelArrayEditor.Selectable
    {
        public Voxel voxel;
        public int edgeI;

        public bool addSelected
        {
            get
            {
                return edge.addSelected;
            }
            set
            {
                voxel.edges[edgeI].addSelected = value;
            }
        }
        public bool storedSelected
        {
            get
            {
                return edge.storedSelected;
            }
            set
            {
                voxel.edges[edgeI].storedSelected = value;
            }
        }
        public bool selected
        {
            get
            {
                return edge.addSelected || edge.storedSelected;
            }
        }
        public Bounds bounds
        {
            get
            {
                return voxel.GetEdgeBounds(edgeI);
            }
        }

        public VoxelEdgeReference(Voxel voxel, int edgeI)
        {
            this.voxel = voxel;
            this.edgeI = edgeI;
        }

        public VoxelEdge edge
        {
            get
            {
                return voxel.edges[edgeI];
            }
        }

        public void SelectionStateUpdated()
        {
            voxel.UpdateVoxel();
        }
    }


    public static VoxelArrayEditor instance = null;

    public Transform axes;
    public RotateAxis rotateAxis;

    public Material selectedMaterial;
    public Material xRayMaterial;
    public Material[] highlightMaterials;

    public bool unsavedChanges = false; // set by VoxelArrayEditor, checked and cleared by EditorFile
    public bool selectionChanged = false; // set by VoxelArrayEditor, checked and cleared by PropertiesGUI/BevelGUI

    public enum SelectMode
    {
        NONE, // nothing selected
        ADJUSTED, // selection has moved since it was set
        BOX, // select inside a 3D box
        BOX_EDGES,
        FACE_FLOOD_FILL
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
        Voxel.highlightMaterials = highlightMaterials;

        ClearSelection();
        selectionChanged = false;
    }

    public override void VoxelModified(Voxel voxel)
    {
        unsavedChanges = true;
        base.VoxelModified(voxel);
    }

    public override void ObjectModified(ObjectEntity obj)
    {
        unsavedChanges = true;
        base.ObjectModified(obj);
        obj.UpdateEntityEditor();
    }

    // called by TouchListener
    public void TouchDown(Voxel voxel, int elementI, VoxelElement elementType)
    {
        if (elementType == VoxelElement.FACES)
            TouchDown(new VoxelFaceReference(voxel, elementI));
        else if (elementType == VoxelElement.EDGES)
        {
            if (EdgeIsSelectable(new VoxelEdgeReference(voxel, elementI)))
                TouchDown(new VoxelEdgeReference(voxel, elementI));
        }
    }

    public void TouchDown(Selectable thing)
    {
        DisableMoveAxes();
        if (thing == null)
        {
            ClearSelection();
            return;
        }
        selectMode = thing is VoxelEdgeReference ? SelectMode.BOX_EDGES : SelectMode.BOX;
        boxSelectStartBounds = thing.bounds;
        selectionBounds = boxSelectStartBounds;
        if (thing is VoxelFaceReference)
            boxSelectSubstance = ((VoxelFaceReference)thing).voxel.substance;
        else if (thing is VoxelEdgeReference)
            boxSelectSubstance = ((VoxelEdgeReference)thing).voxel.substance;
        else if (thing is ObjectMarker)
            boxSelectSubstance = selectObjectSubstance;
        else
            boxSelectSubstance = null;
        UpdateBoxSelection();
    }

    // called by TouchListener
    public void TouchDrag(Voxel voxel, int elementI, VoxelElement elementType)
    {
        if (elementType == VoxelElement.FACES)
            TouchDrag(new VoxelFaceReference(voxel, elementI));
        else if (elementType == VoxelElement.EDGES)
        {
            if (EdgeIsSelectable(new VoxelEdgeReference(voxel, elementI)))
                TouchDrag(new VoxelEdgeReference(voxel, elementI));
        }
    }

    public void TouchDrag(Selectable thing)
    {
        if (selectMode != SelectMode.BOX && selectMode != SelectMode.BOX_EDGES)
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
        AutoSetMoveAxesEnabled();
    }

    // called by TouchListener
    public void DoubleTouch(Voxel voxel, int elementI, VoxelElement elementType)
    {
        ClearSelection();
        if (elementType == VoxelElement.FACES)
            FaceSelectFloodFill(new VoxelFaceReference(voxel, elementI), voxel.substance, stayOnPlane: true);
        AutoSetMoveAxesEnabled();
    }

    // called by TouchListener
    public void TripleTouch(Voxel voxel, int elementI, VoxelElement elementType)
    {
        if (voxel == null)
            return;
        ClearSelection();
        if (voxel.substance == null)
        {
            if (elementType == VoxelElement.FACES)
                FaceSelectFloodFill(new VoxelFaceReference(voxel, elementI), voxel.substance, stayOnPlane: false);
        }
        else
        {
            SubstanceSelect(voxel.substance);
        }
        AutoSetMoveAxesEnabled();
    }

    private void SetMoveAxes(Vector3 position)
    {
        if (axes == null)
            return;
        axes.position = position;
    }

    private void DisableMoveAxes()
    {
        if (axes == null)
            return;
        axes.gameObject.SetActive(false);
    }

    private void AutoSetMoveAxesEnabled()
    {
        if (axes == null)
            return;
        axes.gameObject.SetActive(SomethingIsSelected() && selectMode != SelectMode.BOX_EDGES);
        rotateAxis.gameObject.SetActive(TypeIsSelected<ObjectMarker>());
    }

    public void ClearSelection()
    {
        foreach (Selectable thing in selectedThings)
        {
            thing.addSelected = false;
            thing.SelectionStateUpdated();
        }
        selectedThings.Clear();
        AutoSetMoveAxesEnabled();
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

    private bool EdgeIsSelectable(VoxelEdgeReference edgeRef)
    {
        if (edgeRef.voxel.EdgeIsEmpty(edgeRef.edgeI))
            return false;
        if (edgeRef.voxel.EdgeIsConvex(edgeRef.edgeI))
            return true;
        // concave...
        var oppEdgeRef = OpposingEdgeRef(edgeRef, false);
        if (oppEdgeRef.voxel == null)
            return false;
        return !oppEdgeRef.voxel.EdgeIsEmpty(oppEdgeRef.edgeI);
    }

    // add selected things come before stored selection
    // this is important for functions like GetSelectedPaint
    private System.Collections.Generic.IEnumerable<Selectable> IterateSelected()
    {
        foreach (Selectable thing in selectedThings)
            yield return thing;
        foreach (Selectable thing in storedSelectedThings)
            if (!thing.addSelected) // make sure the thing isn't also in selectedThings
                yield return thing;
    }

    private System.Collections.Generic.IEnumerable<T> IterateSelected<T>() where T : Selectable
    {
        foreach (Selectable thing in IterateSelected())
            if (thing is T)
                yield return (T)thing;
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

    private bool TypeIsSelected<T>() where T : Selectable
    {
        foreach (var thing in IterateSelected<T>())
            return true;
        return false;
    }

    public bool FacesAreSelected()
    {
        return TypeIsSelected<VoxelFaceReference>();
    }

    public ICollection<Entity> GetSelectedEntities()
    {
        var selectedEntities = new HashSet<Entity>();
        foreach (Selectable thing in IterateSelected())
        {
            if (thing is VoxelFaceReference)
            {
                var faceRef = (VoxelFaceReference)thing;
                if (faceRef.voxel.substance != null)
                    selectedEntities.Add(faceRef.voxel.substance); // HashSet will prevent duplicates
            }
            else if (thing is ObjectMarker)
            {
                selectedEntities.Add(((ObjectMarker)thing).objectEntity);
            }
        }
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
        AutoSetMoveAxesEnabled();
        selectionChanged = true;
    }

    private void UpdateBoxSelection()
    {
        SetMoveAxes(selectionBounds.center);

        // update selection...
        for (int i = selectedThings.Count - 1; i >= 0; i--)
        {
            Selectable thing = selectedThings[i];
            Substance thingSubstance = null;
            if (thing is VoxelFaceReference)
                thingSubstance = ((VoxelFaceReference)thing).voxel.substance;
            else if (thing is VoxelEdgeReference)
                thingSubstance = ((VoxelEdgeReference)thing).voxel.substance;
            else if (thing is ObjectEntity)
                thingSubstance = selectObjectSubstance;
            if (thingSubstance != boxSelectSubstance || !ThingInBoxSelection(thing, selectionBounds))
                DeselectThing(thing);
        }
        UpdateBoxSelectionRecursive(rootNode, selectionBounds, boxSelectSubstance, selectMode == SelectMode.BOX_EDGES);
    }

    private void UpdateBoxSelectionRecursive(OctreeNode node, Bounds bounds, Substance substance, bool edges)
    {
        if (node == null)
            return;
        if (!bounds.Intersects(node.bounds))
            return;
        if (node.size == 1)
        {
            Voxel voxel = node.voxel;
            if (substance == selectObjectSubstance
                && voxel.objectEntity != null
                && ThingInBoxSelection(voxel.objectEntity.marker, bounds))
            {
                SelectThing(voxel.objectEntity.marker);
                return;
            }
            if (voxel.substance != substance)
                return;
            if (!edges)
            {
                for (int faceI = 0; faceI < voxel.faces.Length; faceI++)
                {
                    if (voxel.faces[faceI].IsEmpty())
                        continue;
                    if (ThingInBoxSelection(new VoxelFaceReference(voxel, faceI), bounds))
                        SelectFace(voxel, faceI);
                }
            }
            else // edges
            {
                for (int edgeI = 0; edgeI < voxel.edges.Length; edgeI++)
                {
                    var edgeRef = new VoxelEdgeReference(voxel, edgeI);
                    if (EdgeIsSelectable(edgeRef) && ThingInBoxSelection(edgeRef, bounds))
                        SelectThing(edgeRef);
                }
            }
        }
        else
        {
            foreach (OctreeNode branch in node.branches)
                UpdateBoxSelectionRecursive(branch, bounds, substance, edges);
        }
    }

    private bool ThingInBoxSelection(Selectable thing, Bounds bounds)
    {
        bounds.Expand(new Vector3(0.1f, 0.1f, 0.1f));
        Bounds thingBounds = thing.bounds;
        return bounds.Contains(thingBounds.min) && bounds.Contains(thingBounds.max);
    }

    private void FaceSelectFloodFill(VoxelFaceReference faceRef, Substance substance, bool stayOnPlane)
    {
        if (faceRef.voxel == null || faceRef.voxel.substance != substance
            || faceRef.face.IsEmpty() || faceRef.selected) // stop at boundaries of stored selection
            return;
        SelectThing(faceRef);

        Vector3 position = faceRef.voxel.transform.position;
        for (int sideNum = 0; sideNum < 4; sideNum++)
        {
            int sideFaceI = Voxel.SideFaceI(faceRef.faceI, sideNum);
            Vector3 newPos = position + Voxel.DirectionForFaceI(sideFaceI);
            FaceSelectFloodFill(new VoxelFaceReference(VoxelAt(newPos, false), faceRef.faceI), substance, stayOnPlane);

            if (!stayOnPlane)
            {
                FaceSelectFloodFill(new VoxelFaceReference(faceRef.voxel, sideFaceI), substance, stayOnPlane);
                newPos += Voxel.DirectionForFaceI(faceRef.faceI);
                FaceSelectFloodFill(new VoxelFaceReference(VoxelAt(newPos, false), Voxel.OppositeFaceI(sideFaceI)),
                    substance, stayOnPlane);
            }
        }

        var faceBounds = faceRef.voxel.GetFaceBounds(faceRef.faceI);
        if (selectMode != SelectMode.FACE_FLOOD_FILL)
            selectionBounds = faceBounds;
        else
            selectionBounds.Encapsulate(faceBounds);
        selectMode = SelectMode.FACE_FLOOD_FILL;
        SetMoveAxes(position + new Vector3(0.5f, 0.5f, 0.5f) - Voxel.OppositeDirectionForFaceI(faceRef.faceI) / 2);
    }

    private void SubstanceSelect(Substance substance)
    {
        foreach (Voxel v in substance.voxels)
            for (int i = 0; i < 6; i++)
                if (!v.faces[i].IsEmpty())
                {
                    SelectFace(v, i);
                    var faceBounds = v.GetFaceBounds(i);
                    if (selectMode != SelectMode.FACE_FLOOD_FILL)
                        selectionBounds = faceBounds;
                    else
                        selectionBounds.Encapsulate(faceBounds);
                    selectMode = SelectMode.FACE_FLOOD_FILL;
                }
    }

    public void SelectAllWithTag(byte tag)
    {
        // TODO: set position of move axes
        foreach (ObjectEntity entity in IterateObjects())
        {
            if (entity.tag == tag)
                SelectThing(entity.marker);
        }
        foreach (Voxel voxel in IterateVoxels())
        {
            if (voxel.substance != null && voxel.substance.tag == tag)
            {
                for (int faceI = 0; faceI < 6; faceI++)
                    if (!voxel.faces[faceI].IsEmpty())
                        SelectFace(voxel, faceI);
            }
        }
        AutoSetMoveAxesEnabled();
    }

    public void SelectAllWithPaint(VoxelFace paint)
    {
        foreach (Voxel voxel in IterateVoxels())
        {
            for (int faceI = 0; faceI < 6; faceI++)
                if (voxel.faces[faceI].Equals(paint))
                    SelectFace(voxel, faceI);
        }
        AutoSetMoveAxesEnabled();
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
        AutoSetMoveAxesEnabled();
    }

    public void Adjust(Vector3 adjustDirection)
    {
        MergeStoredSelected();
        // now we can safely look only the addSelected property and the selectedThings list
        // and ignore the storedSelected property and the storedSelectedThings list

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
            if (thing is ObjectMarker)
            {
                var obj = ((ObjectMarker)thing).objectEntity;
                Vector3Int objNewPos = obj.position + Vector3ToInt(adjustDirection);
                MoveObject(obj, objNewPos);

                Voxel objNewVoxel = VoxelAt(objNewPos, false);
                if (objNewVoxel != null && objNewVoxel.substance == null
                    && !objNewVoxel.faces[oppositeAdjustDirFaceI].IsEmpty()
                    && !objNewVoxel.faces[oppositeAdjustDirFaceI].addSelected)
                {
                    // carve a hole for the object if it's being pushed into a wall
                    objNewVoxel.faces[oppositeAdjustDirFaceI].addSelected = true;
                    selectedThings.Insert(i + 1, new VoxelFaceReference(objNewVoxel, oppositeAdjustDirFaceI));
                }
                continue;
            }
            else if (!(thing is VoxelFaceReference))
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
                if (movingSubstance == null && newVoxel != null && newVoxel.objectEntity != null)
                {
                    // blocked by object
                    oldVoxel.faces[faceI].addSelected = false;
                    selectedThings[i] = new VoxelFaceReference(null, -1);
                    voxelsToUpdate.Add(oldVoxel);
                    continue;
                }

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

        AutoSetMoveAxesEnabled();
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

    public void RotateObjects(float amount)
    {
        foreach (var obj in IterateSelected<ObjectMarker>())
        {
            obj.objectEntity.rotation += amount;
            ObjectModified(obj.objectEntity);
        }
    }

    public VoxelFace GetSelectedPaint()
    {
        // because of the order of IterateSelected, add selected faces will be preferred
        foreach (var faceRef in IterateSelected<VoxelFaceReference>())
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
        foreach (var faceRef in IterateSelected<VoxelFaceReference>())
        {
            if (paint.material != null || faceRef.voxel.substance != null)
                faceRef.voxel.faces[faceRef.faceI].material = paint.material;
            faceRef.voxel.faces[faceRef.faceI].overlay = paint.overlay;
            faceRef.voxel.faces[faceRef.faceI].orientation = paint.orientation;
            VoxelModified(faceRef.voxel);
        }
    }

    public VoxelEdge GetSelectedBevel()
    {
        foreach (var edgeRef in IterateSelected<VoxelEdgeReference>())
        {
            var edge = edgeRef.edge;
            edge.addSelected = false;
            edge.storedSelected = false;
            edge.capMin = edge.capMax = false;
            return edge;
        }
        return new VoxelEdge();
    }

    public void BevelSelectedEdges(VoxelEdge applyBevel)
    {
        if (applyBevel.hasBevel)
        {
            // Make sure no edges with different bevels could intersect by corners
            // by selecting all beveled edges connected to the selected edges by a corner.
            // TODO: this is ugly and bad
            // copy the selected edges so nothing breaks when more objects are selected while iterating
            var selectedEdges = new List<VoxelEdgeReference>();
            foreach (var edgeRef in IterateSelected<VoxelEdgeReference>())
                selectedEdges.Add(edgeRef);
            foreach (VoxelEdgeReference edgeRef in selectedEdges)
                FloodSelectConnectedBeveledEdges(edgeRef, edgeRef.voxel.substance,
                    edgeRef.voxel.EdgeIsConcave(edgeRef.edgeI), true);
        }

        foreach (var edgeRef in IterateSelected<VoxelEdgeReference>())
        {
            if (edgeRef.voxel.EdgeIsEmpty(edgeRef.edgeI))
                continue;
            edgeRef.voxel.edges[edgeRef.edgeI].bevel = applyBevel.bevel;

            int minFaceI = Voxel.EdgeIAxis(edgeRef.edgeI) * 2;
            int maxFaceI = minFaceI + 1;
            Voxel minVoxel = VoxelAt(edgeRef.voxel.transform.position + Voxel.DirectionForFaceI(minFaceI), false);
            Voxel maxVoxel = VoxelAt(edgeRef.voxel.transform.position + Voxel.DirectionForFaceI(maxFaceI), false);

            bool convex = edgeRef.voxel.EdgeIsConvex(edgeRef.edgeI);

            if (minVoxel != null && minVoxel.substance == edgeRef.voxel.substance
                && (convex ? minVoxel.EdgeIsConvex(edgeRef.edgeI)
                : minVoxel.EdgeIsConcave(edgeRef.edgeI)))
            { // continuous edge in the (-) direction
                if (BevelsMatch(minVoxel.edges[edgeRef.edgeI], applyBevel))
                { // remove cap
                    edgeRef.voxel.edges[edgeRef.edgeI].capMin = false;
                    minVoxel.edges[edgeRef.edgeI].capMax = false;
                }
                else
                { // add cap
                    edgeRef.voxel.edges[edgeRef.edgeI].capMin = true;
                    minVoxel.edges[edgeRef.edgeI].capMax = true;
                }
                VoxelModified(minVoxel);
            }
            else
            {
                edgeRef.voxel.edges[edgeRef.edgeI].capMin = !convex ^ edgeRef.voxel.faces[minFaceI].IsEmpty();
            }

            if (maxVoxel != null && maxVoxel.substance == edgeRef.voxel.substance
                && (convex ? maxVoxel.EdgeIsConvex(edgeRef.edgeI)
                : maxVoxel.EdgeIsConcave(edgeRef.edgeI)))
            { // continuous edge in the (+) direction
                if (BevelsMatch(maxVoxel.edges[edgeRef.edgeI], applyBevel))
                { // remove cap
                    edgeRef.voxel.edges[edgeRef.edgeI].capMax = false;
                    maxVoxel.edges[edgeRef.edgeI].capMin = false;
                }
                else
                { // add cap
                    edgeRef.voxel.edges[edgeRef.edgeI].capMax = true;
                    maxVoxel.edges[edgeRef.edgeI].capMin = true;
                }
                VoxelModified(maxVoxel);
            }
            else
            {
                edgeRef.voxel.edges[edgeRef.edgeI].capMax = !convex ^ edgeRef.voxel.faces[maxFaceI].IsEmpty();
            }
        } // end foreach selected

        // don't update until all bevels have been set
        // in case bevels temporarily didn't match
        foreach (var edgeRef in IterateSelected<VoxelEdgeReference>())
            VoxelModified(edgeRef.voxel);
    }

    private bool BevelsMatch(VoxelEdge e1, VoxelEdge e2)
    {
        if (!e1.hasBevel && !e2.hasBevel)
            return true;
        return e1.bevelType == e2.bevelType && e1.bevelSize == e2.bevelSize;
    }

    private void FloodSelectConnectedBeveledEdges(VoxelEdgeReference edgeRef, Substance substance,
        bool concave, bool firstEdge)
    {
        if (edgeRef.voxel == null || edgeRef.voxel.substance != substance
            || edgeRef.voxel.EdgeIsEmpty(edgeRef.edgeI))
            return;
        if (concave ^ edgeRef.voxel.EdgeIsConcave(edgeRef.edgeI))
        {
            // bevel directions don't match!
            // this won't work!
            edgeRef.voxel.edges[edgeRef.edgeI].bevelType = VoxelEdge.BevelType.NONE;
            DeselectThing(edgeRef);
            VoxelModified(edgeRef.voxel);

            if(!concave) // so edgeRef IS concave!
            {
                var oppEdgeRef = OpposingEdgeRef(edgeRef, false);
                if (oppEdgeRef.voxel != null)
                {
                    oppEdgeRef.voxel.edges[oppEdgeRef.edgeI].bevelType = VoxelEdge.BevelType.NONE;
                    DeselectThing(oppEdgeRef);
                    VoxelModified(oppEdgeRef.voxel);
                }
            }

            return;
        }
        if (!firstEdge) {
            if (!edgeRef.voxel.edges[edgeRef.edgeI].hasBevel || edgeRef.selected)
                return;
        }

        SelectThing(edgeRef);

        foreach (int connectedEdgeI in Voxel.ConnectedEdges(edgeRef.edgeI))
            FloodSelectConnectedBeveledEdges(new VoxelEdgeReference(edgeRef.voxel, connectedEdgeI),
                substance, concave, false);

        if (edgeRef.voxel.EdgeIsConcave(edgeRef.edgeI))
        {
            FloodSelectConnectedBeveledEdges(OpposingEdgeRef(edgeRef, false), substance, concave, false);
        }
    }

    private VoxelEdgeReference OpposingEdgeRef(VoxelEdgeReference edgeRef, bool createIfMissing)
    {
        int faceA, faceB;
        Voxel.EdgeFaces(edgeRef.edgeI, out faceA, out faceB);
        Vector3 opposingPos = edgeRef.voxel.transform.position
            + Voxel.DirectionForFaceI(faceA) + Voxel.DirectionForFaceI(faceB);
        int opposingEdgeI = Voxel.EdgeIAxis(edgeRef.edgeI) * 4 + ((edgeRef.edgeI + 2) % 4);
        return new VoxelEdgeReference(VoxelAt(opposingPos, createIfMissing), opposingEdgeI);
    }

    public int GetSelectedFaceNormal()
    {
        int faceI = -1;
        foreach (var faceRef in IterateSelected<VoxelFaceReference>())
        {
            if (faceI == -1)
                faceI = faceRef.faceI;
            else if (faceRef.faceI != faceI)
                return -1;
        }
        return faceI;
    }

    // return false if object could not be placed
    public bool PlaceObject(ObjectEntity obj)
    {
        Vector3 createPosition = selectionBounds.center;
        int faceNormal = GetSelectedFaceNormal();
        Vector3 createDirection = Voxel.DirectionForFaceI(faceNormal);
        if (faceNormal != -1)
            createPosition += createDirection / 2;
        else
        {
            faceNormal = 3;
            createDirection = Vector3.up;
        }
        createPosition -= new Vector3(0.5f, 0.5f, 0.5f);

        // don't create the object at the same location of an existing object
        // keep moving in the direction of the face normal until an empty space is found
        while (true)
        {
            Voxel voxel = VoxelAt(createPosition, false);
            if (voxel != null && voxel.substance == null && !voxel.faces[Voxel.OppositeFaceI(faceNormal)].IsEmpty())
                return false; // blocked by wall. no room to create object
            if (voxel == null || voxel.objectEntity == null)
                break;
            createPosition += createDirection;
        }
        obj.position = VoxelArray.Vector3ToInt(createPosition);

        obj.InitObjectMarker(this);
        AddObject(obj);
        unsavedChanges = true;
        // select the object. Wait one frame so the position is correct
        StartCoroutine(SelectNewObjectCoroutine(obj));
        return true;
    }

    private IEnumerator SelectNewObjectCoroutine(ObjectEntity obj)
    {
        yield return null;
        ClearStoredSelection();
        TouchDown(obj.marker);
        TouchUp();
    }
}