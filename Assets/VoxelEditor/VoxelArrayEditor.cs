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
        FACE_FLOOD_FILL,
        EDGE_FLOOD_FILL
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

    private enum EdgeType
    {
        EMPTY, CONVEX, CONCAVE, FLAT
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
            var edgeRef = new VoxelEdgeReference(voxel, elementI);
            if (EdgeIsSelectable(edgeRef))
                TouchDown(edgeRef);
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
            var edgeRef = new VoxelEdgeReference(voxel, elementI);
            if (EdgeIsSelectable(edgeRef))
                TouchDrag(edgeRef);
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
        else if (elementType == VoxelElement.EDGES)
        {
            var edgeRef = new VoxelEdgeReference(voxel, elementI);
            if (EdgeIsSelectable(edgeRef))
                EdgeSelectFloodFill(edgeRef, voxel.substance);
        }
        AutoSetMoveAxesEnabled();
    }

    // called by TouchListener
    public void TripleTouch(Voxel voxel, int elementI, VoxelElement elementType)
    {
        if (voxel == null)
            return;
        if (voxel.substance == null)
        {
            if (elementType == VoxelElement.FACES)
            {
                ClearSelection();
                FaceSelectFloodFill(new VoxelFaceReference(voxel, elementI), voxel.substance, stayOnPlane: false);
            }
        }
        else
        {
            ClearSelection();
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
        axes.gameObject.SetActive(SomethingIsSelected()
            && selectMode != SelectMode.BOX_EDGES && selectMode != SelectMode.EDGE_FLOOD_FILL);
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

    private EdgeType GetEdgeType(VoxelEdgeReference edgeRef)
    {
        if (edgeRef.voxel == null || edgeRef.voxel.EdgeIsEmpty(edgeRef.edgeI))
            return EdgeType.EMPTY;
        if (edgeRef.voxel.EdgeIsConvex(edgeRef.edgeI))
            return EdgeType.CONVEX;
        // concave...
        var oppEdgeRef = OpposingEdgeRef(edgeRef);
        if (oppEdgeRef.voxel == null || oppEdgeRef.voxel.EdgeIsEmpty(oppEdgeRef.edgeI))
            return EdgeType.FLAT;
        return EdgeType.CONCAVE;
    }

    private bool EdgeIsCorner(EdgeType edgeType)
    {
        return edgeType == EdgeType.CONVEX || edgeType == EdgeType.CONCAVE;
    }

    private bool EdgeIsSelectable(VoxelEdgeReference edgeRef)
    {
        return EdgeIsCorner(GetEdgeType(edgeRef));
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

    private void EdgeSelectFloodFill(VoxelEdgeReference edgeRef, Substance substance)
    {
        selectMode = SelectMode.EDGE_FLOOD_FILL;
        int minFaceI = Voxel.EdgeIAxis(edgeRef.edgeI) * 2;
        Vector3 minDir = Voxel.DirectionForFaceI(minFaceI);
        var edgeType = GetEdgeType(edgeRef);
        SelectContiguousEdges(edgeRef, substance, minDir, edgeType);
        SelectContiguousEdges(edgeRef, substance, -minDir, edgeType);
    }

    private void SelectContiguousEdges(VoxelEdgeReference edgeRef, Substance substance,
        Vector3 direction, EdgeType edgeType)
    {
        for (Vector3 voxelPos = edgeRef.voxel.transform.position; true; voxelPos += direction)
        {
            Voxel voxel = VoxelAt(voxelPos, false);
            if (voxel == null || voxel.substance != substance)
                break;
            var contigEdgeRef = new VoxelEdgeReference(voxel, edgeRef.edgeI);
            var contigEdgeType = GetEdgeType(contigEdgeRef);
            if (contigEdgeType != edgeType)
                break;
            SelectThing(contigEdgeRef);
        }
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

        // reuse arrays for each face
        VoxelEdge[] movingEdges = new VoxelEdge[4];
        var bevelsToUpdate = new HashSet<VoxelEdgeReference>();

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
            int movingEdgesI = 0;
            foreach (int edgeI in Voxel.FaceSurroundingEdges(faceI))
            {
                movingEdges[movingEdgesI++] = oldVoxel.edges[edgeI];
            }

            bool blocked = false; // is movement blocked?
            Voxel newSubstanceBlock = null;

            if (pushing)
            {
                foreach (int edgeI in Voxel.FaceSurroundingEdges(faceI))
                {
                    oldVoxel.edges[edgeI].Clear();
                    bevelsToUpdate.Add(new VoxelEdgeReference(oldVoxel, edgeI));
                }
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
                            foreach (int edgeI in FaceSurroundingEdgesAlongAxis(oppositeSideFaceI, adjustAxis))
                            {
                                sideVoxel.edges[edgeI] = adjacentSideVoxel.edges[edgeI];
                                bevelsToUpdate.Add(new VoxelEdgeReference(sideVoxel, edgeI));
                            }
                        }
                        else
                        {
                            sideVoxel.faces[oppositeSideFaceI] = movingFace;
                        }
                        voxelsToUpdate.Add(sideVoxel);
                    }
                    else
                    {
                        // side will be deleted when voxel is cleared but we'll remove/update the bevels now
                        foreach (int edgeI in FaceSurroundingEdgesAlongAxis(sideFaceI, adjustAxis))
                        {
                            oldVoxel.edges[edgeI].Clear();
                            bevelsToUpdate.Add(new VoxelEdgeReference(oldVoxel, edgeI));
                        }
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
                foreach (int edgeI in Voxel.FaceSurroundingEdges(faceI))
                {
                    oldVoxel.edges[edgeI].Clear();
                    bevelsToUpdate.Add(new VoxelEdgeReference(oldVoxel, edgeI));
                }
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
                            foreach (int edgeI in FaceSurroundingEdgesAlongAxis(sideFaceI, adjustAxis))
                            {
                                newVoxel.edges[edgeI] = oldVoxel.edges[edgeI];
                                bevelsToUpdate.Add(new VoxelEdgeReference(newVoxel, edgeI));
                            }
                        }
                        else
                            newVoxel.faces[sideFaceI] = movingFace;
                    }
                    else
                    {
                        // delete side
                        foreach (int edgeI in FaceSurroundingEdgesAlongAxis(oppositeSideFaceI, adjustAxis))
                        {
                            sideVoxel.edges[edgeI].Clear();
                            bevelsToUpdate.Add(new VoxelEdgeReference(sideVoxel, edgeI));
                        }
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
                if (pushing || pulling)
                {
                    movingEdgesI = 0;
                    foreach (int edgeI in Voxel.FaceSurroundingEdges(faceI))
                    {
                        var edgeRef = new VoxelEdgeReference(newVoxel, edgeI);
                        if (EdgeIsCorner(GetEdgeType(edgeRef)))
                        {
                            newVoxel.edges[edgeI] = movingEdges[movingEdgesI];
                            UpdateBevel(edgeRef, alsoBevelOppositeConcaveEdge: true,
                                dontUpdateThisVoxel: true, alwaysUpdateOppositeCaps: true);
                        }
                        else // no edge between faces, pulled/pushed to be coplanar with surrounding faces
                        {
                            // remove bevel from connected edge...
                            // find voxel next to this one
                            int faceA, faceB;
                            Voxel.EdgeFaces(edgeRef.edgeI, out faceA, out faceB);
                            int otherFace = faceA == faceI ? faceB : faceA;
                            Voxel adjacentVoxel = VoxelAt(newPos + Voxel.DirectionForFaceI(otherFace), false);
                            if (adjacentVoxel != null && adjacentVoxel.substance == movingSubstance)
                            {
                                // find edge on adjacentVoxel connected to edgeRef
                                foreach (int otherEdgeI in FaceSurroundingEdgesAlongAxis(faceI, Voxel.EdgeIAxis(edgeI)))
                                {
                                    if (otherEdgeI == edgeI)
                                        continue;
                                    adjacentVoxel.edges[otherEdgeI].Clear();
                                    UpdateBevel(new VoxelEdgeReference(adjacentVoxel, otherEdgeI), alsoBevelOppositeConcaveEdge: false,
                                        dontUpdateThisVoxel: true, alwaysUpdateOppositeCaps: true);
                                    voxelsToUpdate.Add(adjacentVoxel);
                                }
                            }
                        }
                        movingEdgesI++;
                    }
                }
            }
            else
            {
                // clear the selection; will be deleted later
                selectedThings[i] = new VoxelFaceReference(null, -1);
                if (pulling && substanceToCreate == null)
                    newVoxel.substance = movingSubstance;
            }
            foreach (VoxelEdgeReference edgeRef in bevelsToUpdate)
            {
                UpdateBevel(edgeRef, alsoBevelOppositeConcaveEdge: true,
                    dontUpdateThisVoxel: true, alwaysUpdateOppositeCaps: true);
            }
            bevelsToUpdate.Clear();

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

    private IEnumerable<int> FaceSurroundingEdgesAlongAxis(int faceI, int axis)
    {
        foreach (int edgeI in Voxel.FaceSurroundingEdges(faceI))
            if (Voxel.EdgeIAxis(edgeI) == axis)
                yield return edgeI;
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
        foreach (var edgeRef in IterateSelected<VoxelEdgeReference>())
        {
            if (edgeRef.voxel.EdgeIsEmpty(edgeRef.edgeI))
                continue;
            edgeRef.voxel.edges[edgeRef.edgeI].bevel = applyBevel.bevel;
            UpdateBevel(edgeRef, alsoBevelOppositeConcaveEdge: false, dontUpdateThisVoxel: false);
        }
    }

    private void UpdateBevel(VoxelEdgeReference edgeRef, bool alsoBevelOppositeConcaveEdge, bool dontUpdateThisVoxel,
        bool alwaysUpdateOppositeCaps = false)
    {
        int minFaceI = Voxel.EdgeIAxis(edgeRef.edgeI) * 2;
        int maxFaceI = minFaceI + 1;
        Voxel minVoxel = VoxelAt(edgeRef.voxel.transform.position + Voxel.DirectionForFaceI(minFaceI), false);
        Voxel maxVoxel = VoxelAt(edgeRef.voxel.transform.position + Voxel.DirectionForFaceI(maxFaceI), false);
        var minEdgeRef = new VoxelEdgeReference(minVoxel, edgeRef.edgeI);
        var maxEdgeRef = new VoxelEdgeReference(maxVoxel, edgeRef.edgeI);

        EdgeType type = GetEdgeType(edgeRef);
        EdgeType minType = GetEdgeType(minEdgeRef);
        EdgeType maxType = GetEdgeType(maxEdgeRef);

        bool alsoOther;
        bool cap = BevelCap(edgeRef, minEdgeRef, type, minType,
            !edgeRef.voxel.faces[minFaceI].IsEmpty(), out alsoOther);
        edgeRef.voxel.edges[edgeRef.edgeI].capMin = cap;
        if (alsoOther || (alwaysUpdateOppositeCaps && minVoxel != null))
        {
            if (!alsoOther)
            {
                cap = BevelCap(minEdgeRef, edgeRef, minType, type,
                    minEdgeRef.voxel != null && !minEdgeRef.voxel.faces[maxFaceI].IsEmpty(), out alsoOther);
            }
            minVoxel.edges[edgeRef.edgeI].capMax = cap;
            VoxelModified(minVoxel);
            if (minType == EdgeType.CONCAVE)
            {
                var oppMinEdgeRef = OpposingEdgeRef(minEdgeRef);
                if (oppMinEdgeRef.voxel != null)
                {
                    oppMinEdgeRef.voxel.edges[oppMinEdgeRef.edgeI].capMax = cap;
                    VoxelModified(oppMinEdgeRef.voxel);
                }
            }
        }

        cap = BevelCap(edgeRef, maxEdgeRef, type, maxType,
            !edgeRef.voxel.faces[maxFaceI].IsEmpty(), out alsoOther);
        edgeRef.voxel.edges[edgeRef.edgeI].capMax = cap;
        if (alsoOther || (alwaysUpdateOppositeCaps && maxVoxel != null))
        {
            if (!alsoOther)
            {
                cap = BevelCap(maxEdgeRef, edgeRef, maxType, type,
                    maxEdgeRef.voxel != null && !maxEdgeRef.voxel.faces[minFaceI].IsEmpty(), out alsoOther);
            }
            maxVoxel.edges[edgeRef.edgeI].capMin = cap;
            VoxelModified(maxVoxel);
            if (maxType == EdgeType.CONCAVE)
            {
                var oppMaxEdgeRef = OpposingEdgeRef(maxEdgeRef);
                if (oppMaxEdgeRef.voxel != null)
                {
                    oppMaxEdgeRef.voxel.edges[oppMaxEdgeRef.edgeI].capMin = cap;
                    VoxelModified(oppMaxEdgeRef.voxel);
                }
            }
        }

        if (edgeRef.edge.hasBevel && EdgeIsCorner(type))
        {
            // don't allow convex and concave bevels to be joined at a corner
            // don't allow bevels of different shapes/sizes to be joined at a corner
            foreach (int connectedEdgeI in Voxel.ConnectedEdges(edgeRef.edgeI))
            {
                var connectedEdgeRef = new VoxelEdgeReference(edgeRef.voxel, connectedEdgeI);
                var connectedEdgeType = GetEdgeType(connectedEdgeRef);
                if (!EdgeIsCorner(connectedEdgeType) || !connectedEdgeRef.edge.hasBevel)
                    continue;
                if (type != connectedEdgeType)
                {
                    // bevel directions don't match! this won't work!
                    edgeRef.voxel.edges[connectedEdgeI].bevelType = VoxelEdge.BevelType.NONE;
                    UpdateBevel(connectedEdgeRef, alsoBevelOppositeConcaveEdge: true,
                        dontUpdateThisVoxel: true, alwaysUpdateOppositeCaps: alwaysUpdateOppositeCaps);
                }
                else if (!BevelsMatch(connectedEdgeRef.edge, edgeRef.edge))
                {
                    // bevel shapes/sizes don't match! this won't work!
                    edgeRef.voxel.edges[connectedEdgeI].bevel = edgeRef.edge.bevel;
                    UpdateBevel(connectedEdgeRef, alsoBevelOppositeConcaveEdge: true,
                        dontUpdateThisVoxel: true, alwaysUpdateOppositeCaps: alwaysUpdateOppositeCaps);
                }
            }

            if (type == EdgeType.CONVEX)
            {
                // don't allow full bevels to overlap with other bevels
                foreach (int unconnectedEdgeI in Voxel.UnconnectedEdges(edgeRef.edgeI))
                {
                    if (edgeRef.voxel.EdgeIsEmpty(unconnectedEdgeI))
                        continue;
                    var unconnectedEdgeRef = new VoxelEdgeReference(edgeRef.voxel, unconnectedEdgeI);
                    if (unconnectedEdgeRef.edge.hasBevel && edgeRef.voxel.EdgeIsConvex(unconnectedEdgeI))
                    {
                        if (edgeRef.edge.bevelSize == VoxelEdge.BevelSize.FULL
                            || unconnectedEdgeRef.edge.bevelSize == VoxelEdge.BevelSize.FULL)
                        {
                            // bevels overlap! this won't work!
                            edgeRef.voxel.edges[unconnectedEdgeI].bevelType = VoxelEdge.BevelType.NONE;
                            UpdateBevel(unconnectedEdgeRef, alsoBevelOppositeConcaveEdge: true,
                                dontUpdateThisVoxel: true, alwaysUpdateOppositeCaps: alwaysUpdateOppositeCaps);
                        }
                    }
                }
            }
        }

        if (alsoBevelOppositeConcaveEdge && type == EdgeType.CONCAVE)
        {
            var oppEdgeRef = OpposingEdgeRef(edgeRef);
            if (oppEdgeRef.voxel != null)
            {
                oppEdgeRef.voxel.edges[oppEdgeRef.edgeI].bevel = edgeRef.edge.bevel;
                UpdateBevel(oppEdgeRef, alsoBevelOppositeConcaveEdge: false,
                    dontUpdateThisVoxel: false, alwaysUpdateOppositeCaps: alwaysUpdateOppositeCaps);
            }
        }
        if (!dontUpdateThisVoxel)
            VoxelModified(edgeRef.voxel);
    }

    private bool BevelsMatch(VoxelEdge e1, VoxelEdge e2)
    {
        if (!e1.hasBevel && !e2.hasBevel)
            return true;
        return e1.bevelType == e2.bevelType && e1.bevelSize == e2.bevelSize;
    }

    private bool BevelCap(VoxelEdgeReference thisBevel, VoxelEdgeReference otherBevel,
        EdgeType thisType, EdgeType otherType, bool face, out bool alsoChangeOtherCap)
    {
        bool match = false;
        if (otherBevel.voxel != null && thisBevel.voxel != null)
        {
            if (otherBevel.voxel.substance != thisBevel.voxel.substance)
                otherType = EdgeType.EMPTY;
            else
                match = BevelsMatch(thisBevel.edge, otherBevel.edge);
        }

        alsoChangeOtherCap = false;
        if (thisType == EdgeType.EMPTY || thisType == EdgeType.FLAT)
            return false;
        if ((thisType == EdgeType.CONVEX && otherType == EdgeType.CONVEX)
            || (thisType == EdgeType.CONCAVE && otherType == EdgeType.CONCAVE))
        {
            alsoChangeOtherCap = true;
            return !match;
        }
        if (thisType == EdgeType.CONVEX) // otherType != CONVEX
            return !face;
        if (thisType == EdgeType.CONCAVE && otherType == EdgeType.EMPTY)
            return face;
        if (thisType == EdgeType.CONCAVE) // otherType != CONCAVE or EMPTY
            return true;
        throw new System.Exception("Unrecognized bevel type"); // this will never happen
    }

    // return null voxel if edge doesn't exist or substances don't match
    private VoxelEdgeReference OpposingEdgeRef(VoxelEdgeReference edgeRef)
    {
        int faceA, faceB;
        Voxel.EdgeFaces(edgeRef.edgeI, out faceA, out faceB);
        Vector3 opposingPos = edgeRef.voxel.transform.position
            + Voxel.DirectionForFaceI(faceA) + Voxel.DirectionForFaceI(faceB);
        Voxel opposingVoxel = VoxelAt(opposingPos, false);
        if (opposingVoxel != null && opposingVoxel.substance != edgeRef.voxel.substance)
            opposingVoxel = null;
        int opposingEdgeI = Voxel.EdgeIAxis(edgeRef.edgeI) * 4 + ((edgeRef.edgeI + 2) % 4);
        return new VoxelEdgeReference(opposingVoxel, opposingEdgeI);
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