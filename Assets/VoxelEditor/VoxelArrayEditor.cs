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
        EDGE_FLOOD_FILL,
        DRAW_SELECT,
        DRAW_DESELECT
    }

    public SelectMode selectMode = SelectMode.NONE; // only for the "add" selection
    public bool drawSelect = false;
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
        VoxelComponent.selectedMaterial = selectedMaterial;
        VoxelComponent.xRayMaterial = xRayMaterial;
        VoxelComponent.highlightMaterials = highlightMaterials;

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
        if (thing is VoxelFaceReference)
            boxSelectSubstance = ((VoxelFaceReference)thing).voxel.substance;
        else if (thing is VoxelEdgeReference)
            boxSelectSubstance = ((VoxelEdgeReference)thing).voxel.substance;
        else if (thing is ObjectMarker)
            boxSelectSubstance = selectObjectSubstance;
        else
            boxSelectSubstance = null;
        if (drawSelect)
        {
            MergeStoredSelected();
            if (thing.addSelected)
            {
                DeselectThing(thing);
                selectMode = SelectMode.DRAW_DESELECT;
            }
            else
            {
                SelectThing(thing);
                SetMoveAxes(thing.bounds.center);
                selectMode = SelectMode.DRAW_SELECT;
            }
            return;
        }
        else
        {
            selectMode = thing is VoxelEdgeReference ? SelectMode.BOX_EDGES : SelectMode.BOX;
            boxSelectStartBounds = thing.bounds;
            selectionBounds = boxSelectStartBounds;
            UpdateBoxSelection();
        }
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
        if (selectMode == SelectMode.DRAW_SELECT || selectMode == SelectMode.DRAW_DESELECT)
        {
            DrawTouchDrag(thing);
            return;
        }
        else if (selectMode != SelectMode.BOX && selectMode != SelectMode.BOX_EDGES)
            return;
        Bounds oldSelectionBounds = selectionBounds;
        selectionBounds = boxSelectStartBounds;
        selectionBounds.Encapsulate(thing.bounds);
        if (oldSelectionBounds != selectionBounds)
            UpdateBoxSelection();
    }

    private void DrawTouchDrag(Selectable thing)
    {
        // make sure substance matches start of selection
        if (boxSelectSubstance == selectObjectSubstance)
        {
            if (!(thing is ObjectMarker))
                return;
        }
        else
        {
            if (!(thing is VoxelFaceReference)
                || ((VoxelFaceReference)thing).voxel.substance != boxSelectSubstance)
                return;
        }
        if (selectMode == SelectMode.DRAW_SELECT)
        {
            SelectThing(thing);
            SetMoveAxes(thing.bounds.center);
        }
        else
            DeselectThing(thing);
    }

    // called by TouchListener
    public void TouchUp()
    {
        AutoSetMoveAxesEnabled();
    }

    // called by TouchListener
    public void DoubleTouch(Voxel voxel, int elementI, VoxelElement elementType)
    {
        if (drawSelect && elementType == VoxelElement.FACES)
            // flood fill inside existing selection
            DeselectThing(new VoxelFaceReference(voxel, elementI));
        else
            ClearSelection();
        if (elementType == VoxelElement.FACES)
            FaceSelectFloodFill(new VoxelFaceReference(voxel, elementI), stayOnPlane: true);
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
                FaceSelectFloodFill(new VoxelFaceReference(voxel, elementI), stayOnPlane: false);
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
        axes.gameObject.SetActive(SomethingIsSelected() && !TypeIsSelected<VoxelEdgeReference>());
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

    private void DeselectThing(int index)
    {
        Selectable thing = selectedThings[index];
        if (!thing.addSelected)
            return;
        thing.addSelected = false;
        selectedThings.RemoveAt(index);
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

    public bool FacesAreAddSelected()
    {
        foreach (var thing in selectedThings)
            if (thing is VoxelFaceReference)
                return true;
        return false;
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
                DeselectThing(i);
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

    private void FaceSelectFloodFill(VoxelFaceReference start, bool stayOnPlane, bool matchPaint = false)
    {
        Substance substance = start.voxel.substance;
        VoxelFace paint = start.face;

        // this used to be a recursive algorithm but it would cause stack overflow exceptions
        Queue<VoxelFaceReference> facesToSelect = new Queue<VoxelFaceReference>();
        facesToSelect.Enqueue(start);

        // reset selection bounds
        selectMode = SelectMode.FACE_FLOOD_FILL;
        selectionBounds = start.bounds;

        while (facesToSelect.Count != 0)
        {
            VoxelFaceReference faceRef = facesToSelect.Dequeue();
            if (faceRef.voxel == null || faceRef.voxel.substance != substance
                || faceRef.face.IsEmpty() || faceRef.selected) // stop at boundaries of stored selection
                continue;
            if (matchPaint && faceRef.face != paint)
                continue;
            SelectThing(faceRef);

            Vector3Int position = faceRef.voxel.position;
            for (int sideNum = 0; sideNum < 4; sideNum++)
            {
                int sideFaceI = Voxel.SideFaceI(faceRef.faceI, sideNum);
                Vector3Int newPos = position + Voxel.DirectionForFaceI(sideFaceI).ToInt();
                facesToSelect.Enqueue(new VoxelFaceReference(VoxelArray.VoxelAtAdjacent(newPos, faceRef.voxel), faceRef.faceI));

                if (!stayOnPlane)
                {
                    facesToSelect.Enqueue(new VoxelFaceReference(faceRef.voxel, sideFaceI));
                    newPos += Voxel.DirectionForFaceI(faceRef.faceI).ToInt();
                    facesToSelect.Enqueue(new VoxelFaceReference(VoxelArray.VoxelAtAdjacent(newPos, faceRef.voxel), Voxel.OppositeFaceI(sideFaceI)));
                }
            }

            // grow bounds
            selectionBounds.Encapsulate(faceRef.bounds);
        }

        SetMoveAxes(start.voxel.position + new Vector3(0.5f, 0.5f, 0.5f) - Voxel.OppositeDirectionForFaceI(start.faceI) / 2);
    }

    public void FillSelectPaint()
    {
        if (GetSelectedPaint().IsEmpty())
            return;
        foreach (VoxelFaceReference face in IterateSelected<VoxelFaceReference>())
        {
            ClearSelection();
            FaceSelectFloodFill(face, true, true);
            break;
        }
    }

    private void EdgeSelectFloodFill(VoxelEdgeReference edgeRef, Substance substance)
    {
        selectMode = SelectMode.EDGE_FLOOD_FILL;
        selectionBounds = edgeRef.bounds;
        int minFaceI = Voxel.EdgeIAxis(edgeRef.edgeI) * 2;
        Vector3Int minDir = Voxel.DirectionForFaceI(minFaceI).ToInt();
        var edgeType = GetEdgeType(edgeRef);
        SelectContiguousEdges(edgeRef, substance, minDir, edgeType);
        SelectContiguousEdges(edgeRef, substance, minDir * -1, edgeType);
    }

    private void SelectContiguousEdges(VoxelEdgeReference edgeRef, Substance substance,
        Vector3Int direction, EdgeType edgeType)
    {
        for (Vector3Int voxelPos = edgeRef.voxel.position; true; voxelPos += direction)
        {
            Voxel voxel = VoxelAt(voxelPos, false);
            if (voxel == null || voxel.substance != substance)
                break;
            var contigEdgeRef = new VoxelEdgeReference(voxel, edgeRef.edgeI);
            var contigEdgeType = GetEdgeType(contigEdgeRef);
            if (contigEdgeType != edgeType)
                break;
            SelectThing(contigEdgeRef);
            selectionBounds.Encapsulate(contigEdgeRef.bounds);

            var oppEdgeRef = OpposingEdgeRef(contigEdgeRef);
            if (oppEdgeRef.voxel != null && !oppEdgeRef.voxel.EdgeIsEmpty(oppEdgeRef.edgeI))
                SelectThing(oppEdgeRef);
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

    public void Adjust(Vector3Int adjustDirection, int count)
    {
        var voxelsToUpdate = new HashSet<Voxel>();
        for (int i = 0; i < count; i++)
            SingleAdjust(adjustDirection, voxelsToUpdate);
        foreach (Voxel voxel in voxelsToUpdate)
            if (voxel != null)
                VoxelModified(voxel);
    }

    private void SingleAdjust(Vector3Int adjustDirection, HashSet<Voxel> voxelsToUpdate)
    {
        // TODO: only once per adjust
        MergeStoredSelected();
        // now we can safely look only the addSelected property and the selectedThings list
        // and ignore the storedSelected property and the storedSelectedThings list

        int adjustDirFaceI = Voxel.FaceIForDirection(adjustDirection);
        int oppositeAdjustDirFaceI = Voxel.OppositeFaceI(adjustDirFaceI);
        int adjustAxis = Voxel.FaceIAxis(adjustDirFaceI);
        bool negativeAdjustAxis = adjustDirFaceI % 2 == 0;

        // sort selectedThings in order along the adjustDirection vector
        // TODO: need to sort every time? is order maintained?
        selectedThings.Sort(delegate (Selectable a, Selectable b)
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
                if (aFace.faceI == oppositeAdjustDirFaceI
                        && bFace.faceI != oppositeAdjustDirFaceI)
                    return -1; // move one substance back before moving other forward
                if (bFace.faceI == oppositeAdjustDirFaceI
                        && aFace.faceI != oppositeAdjustDirFaceI)
                    return 1;
            }
            return 0;
        });

        bool createdSubstance = false;

        for (int i = 0; i < selectedThings.Count; i++)
        {
            Selectable thing = selectedThings[i];
            if (thing is ObjectMarker)
            {
                var obj = ((ObjectMarker)thing).objectEntity;
                PushObject(obj, adjustDirFaceI, voxelsToUpdate);
                continue;
            }
            else if (!(thing is VoxelFaceReference))
                continue;
            VoxelFaceReference faceRef = (VoxelFaceReference)thing;

            Voxel oldVoxel = faceRef.voxel;
            Vector3Int oldPos = oldVoxel.position;
            Vector3Int newPos = oldPos + adjustDirection;
            Voxel newVoxel = VoxelAt(newPos, true);

            int faceI = faceRef.faceI;
            int oppositeFaceI = Voxel.OppositeFaceI(faceI);
            bool pushing = adjustDirFaceI == oppositeFaceI;
            bool pulling = adjustDirFaceI == faceI;

            VoxelFace movingFace = oldVoxel.faces[faceI].PaintOnly();
            Substance movingSubstance = oldVoxel.substance;
            if (substanceToCreate != null)
                movingSubstance = substanceToCreate;

            if (pushing)
            {
                CarveBlock(oldVoxel, adjustDirFaceI, movingFace, voxelsToUpdate);

                if (substanceToCreate != null)
                {
                    BuildBlock(oldVoxel, substanceToCreate, adjustDirFaceI, movingFace, voxelsToUpdate);
                    createdSubstance = true;
                }
            }
            else if (pulling)
            {
                if (movingSubstance == null && newVoxel != null && newVoxel.objectEntity != null)
                {
                    // blocked by object
                    PushObject(newVoxel.objectEntity, adjustDirFaceI, voxelsToUpdate);
                }
                
                BuildBlock(newVoxel, movingSubstance, adjustDirFaceI, movingFace, voxelsToUpdate);
                if (substanceToCreate != null)
                    createdSubstance = true;
            }
            else  // sliding
            {
                if (!newVoxel.faces[faceI].IsEmpty() && newVoxel.substance == movingSubstance)
                {
                    newVoxel.faces[faceI] = movingFace;
                }
            }

            // in case it wasn't deleted
            oldVoxel.faces[faceI].addSelected = false;

            if (!newVoxel.faces[faceI].IsEmpty() && newVoxel.substance == movingSubstance)
            {
                newVoxel.faces[faceI].addSelected = true;
                selectedThings[i] = new VoxelFaceReference(newVoxel, faceI);
            }
            else if (pushing && substanceToCreate != null)
            {
                oldVoxel.faces[oppositeFaceI].addSelected = true;
                selectedThings[i] = new VoxelFaceReference(oldVoxel, oppositeFaceI);
            }
            else
            {
                // blocked
                // clear the selection; will be deleted later
                selectedThings[i] = new VoxelFaceReference(null, -1);
            }

            voxelsToUpdate.Add(newVoxel);
            voxelsToUpdate.Add(oldVoxel);
        } // end for each selected face

        for (int i = selectedThings.Count - 1; i >= 0; i--)
        {
            Selectable thing = selectedThings[i];
            if ((thing is VoxelFaceReference)
                    && ((VoxelFaceReference)thing).voxel == null)
                selectedThings.RemoveAt(i);
        }
        selectionChanged = true;

        if (substanceToCreate != null && createdSubstance)
        {
            substanceToCreate.defaultPaint.Clear();
            substanceToCreate = null;
        }

        // TODO: only once per adjust
        AutoSetMoveAxesEnabled();
    } // end SingleAdjust()

    // doesn't add given voxel to voxelsToUpdate
    private void BuildBlock(Voxel voxel, Substance substance,
        int adjustDirFaceI, VoxelFace faceTemplate,
        HashSet<Voxel> voxelsToUpdate)
    {
        if (!voxel.IsEmpty())
        {
            if (voxel.substance == substance)
                return;  // already done
            VoxelFace carveTemplate;
            int opposite = Voxel.OppositeFaceI(adjustDirFaceI);
            if (adjustDirFaceI >= 0 && !voxel.faces[opposite].IsEmpty())
                carveTemplate = voxel.faces[opposite];
            else
                carveTemplate = voxel.faces[voxel.FirstNonEmptyFace()];  // TODO??
            CarveBlock(voxel, adjustDirFaceI, carveTemplate, voxelsToUpdate);  // carve then build
        }
        // voxel is definitely empty now

        voxel.substance = substance;
        if (substance != null && !substance.defaultPaint.IsEmpty())
            faceTemplate = substance.defaultPaint;  // TODO

        Vector3Int adjustDirection = Voxel.DirectionForFaceI(adjustDirFaceI).ToInt();
        Voxel adjacentVoxel = null;  // same as "oldVoxel"
        if (adjustDirection != Vector3Int.zero)
            adjacentVoxel = VoxelArray.VoxelAtAdjacent(voxel.position - adjustDirection, voxel);
        if (adjacentVoxel != null && adjacentVoxel.substance != substance)
            adjacentVoxel = null;

        Voxel[] clearVoxels = new Voxel[6];  // TODO don't allocate?
        for (int faceI = 0; faceI < 6; faceI++)
        {
            int oppositeFaceI = Voxel.OppositeFaceI(faceI);
            Voxel sideVoxel = VoxelArray.VoxelAtAdjacent(
                voxel.position + Voxel.DirectionForFaceI(faceI).ToInt(), voxel);
            if (sideVoxel == null || sideVoxel.faces[oppositeFaceI].IsEmpty()
                || sideVoxel.substance != substance)
            {
                // create boundary
                // if possible, the new side should have the properties of the adjacent side
                if (adjacentVoxel != null && !adjacentVoxel.faces[faceI].IsEmpty())
                {
                    voxel.faces[faceI] = adjacentVoxel.faces[faceI].PaintOnly();
                    for (int faceEdgeNum = 0; faceEdgeNum < 4; faceEdgeNum++)
                    {
                        int edgeI = Voxel.FaceSurroundingEdge(faceI, faceEdgeNum);
                        BevelEdge(new VoxelEdgeReference(voxel, edgeI),
                            adjacentVoxel.edges[edgeI].hasBevel, adjacentVoxel.bevelType);
                    }
                }
                else
                {
                    voxel.faces[faceI] = faceTemplate;
                }
                clearVoxels[oppositeFaceI] = null;
            }
            else  // face is filled with same substance
            {
                // remove boundary later
                clearVoxels[oppositeFaceI] = sideVoxel;
                voxelsToUpdate.Add(sideVoxel);
            }
        }

        // clear faces only after adding faces
        // because the bevels from these faces might be used
        for (int i = 0; i < 6; i++)
        {
            if (clearVoxels[i] != null)
                ClearFaceAndBevels(clearVoxels[i], i);
        }
    }

    // doesn't add given voxel to voxelsToUpdate
    private void CarveBlock(Voxel voxel, int adjustDirFaceI,
        VoxelFace faceTemplate, HashSet<Voxel> voxelsToUpdate)
    {
        if (voxel.IsEmpty())
            return;  // already done
        Vector3Int adjustDirection = Voxel.DirectionForFaceI(adjustDirFaceI).ToInt();

        for (int faceI = 0; faceI < 6; faceI++)
        {
            if (!voxel.faces[faceI].IsEmpty())
                continue;  // don't create boundary

            int oppositeFaceI = Voxel.OppositeFaceI(faceI);
            Voxel sideVoxel = VoxelAt(
                voxel.position + Voxel.DirectionForFaceI(faceI).ToInt(), true);

            // if possible, the new side should have the properties of the adjacent side
            Voxel adjacentVoxel = null;
            if (adjustDirection != Vector3Int.zero)
                adjacentVoxel = VoxelArray.VoxelAtAdjacent(sideVoxel.position - adjustDirection, sideVoxel);
            if (adjacentVoxel != null && !adjacentVoxel.faces[oppositeFaceI].IsEmpty()
                && adjacentVoxel.substance == voxel.substance)
            {
                sideVoxel.faces[oppositeFaceI] = adjacentVoxel.faces[oppositeFaceI].PaintOnly();
                for (int faceEdgeNum = 0; faceEdgeNum < 4; faceEdgeNum++)
                {
                    int edgeI = Voxel.FaceSurroundingEdge(oppositeFaceI, faceEdgeNum);
                    BevelEdge(new VoxelEdgeReference(sideVoxel, edgeI),
                        adjacentVoxel.edges[edgeI].hasBevel, adjacentVoxel.bevelType);
                }
            }
            else
            {
                sideVoxel.faces[oppositeFaceI] = faceTemplate;
            }

            voxelsToUpdate.Add(sideVoxel);
        }
        voxel.Clear();
    }

    private void ClearFaceAndBevels(Voxel voxel, int faceI)
    {
        voxel.faces[faceI].Clear();
        for (int faceEdgeNum = 0; faceEdgeNum < 4; faceEdgeNum++)
        {
            int edgeI = Voxel.FaceSurroundingEdge(faceI, faceEdgeNum);
            BevelEdge(new VoxelEdgeReference(voxel, edgeI), false);
        }
    }

    // return success
    public bool PushObject(ObjectEntity obj, int adjustDirFaceI, HashSet<Voxel> voxelsToUpdate)
    {
        Vector3Int adjustDirection = Voxel.DirectionForFaceI(adjustDirFaceI).ToInt();
        int oppositeAdjustDirFaceI = Voxel.OppositeFaceI(adjustDirFaceI);

        Vector3Int newPosition = obj.position + adjustDirection;
        Voxel newObjVoxel = VoxelAt(newPosition, true);
        // push walls and other objects out of the way
        if (newObjVoxel.objectEntity != null)
        {
            PushObject(newObjVoxel.objectEntity, adjustDirFaceI, voxelsToUpdate);
        }
        else if (newObjVoxel.substance == null
            && !newObjVoxel.faces[oppositeAdjustDirFaceI].IsEmpty())
        {
            // carve a hole for the object if it's being pushed into a wall
            CarveBlock(newObjVoxel, adjustDirFaceI, newObjVoxel.faces[oppositeAdjustDirFaceI], voxelsToUpdate);
        }
        newObjVoxel.objectEntity = obj;
        // not necessary to add to voxelsToUpdate

        Voxel oldObjVoxel = VoxelAt(obj.position, false);
        if (oldObjVoxel != null)
        {
            oldObjVoxel.objectEntity = null;
            voxelsToUpdate.Add(oldObjVoxel);
        }
        else
            Debug.Log("This object wasn't in the voxel array!");
        obj.position = newPosition;
        ObjectModified(obj);
        return true;
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
            VoxelFace face = faceRef.face.PaintOnly();
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

    public Voxel.BevelType GetSelectedBevelType(out bool concave)
    {
        foreach (var edgeRef in IterateSelected<VoxelEdgeReference>())
        {
            concave = edgeRef.voxel.concaveBevel;
            if (edgeRef.edge.hasBevel)
                return edgeRef.voxel.bevelType;
            else
                return Voxel.BevelType.NONE;
        }
        concave = false;
        return Voxel.BevelType.NONE;
    }

    public void BevelSelectedEdges(Voxel.BevelType bevelType, bool concave)
    {
        foreach (var edgeRef in IterateSelected<VoxelEdgeReference>())
        {
            if (edgeRef.voxel.EdgeIsEmpty(edgeRef.edgeI))
                continue;
            if (bevelType != Voxel.BevelType.NONE)
                edgeRef.voxel.concaveBevel = concave;
            BevelEdge(edgeRef, bevelType != Voxel.BevelType.NONE, bevelType);
            VoxelModified(edgeRef.voxel);
        }
    }

    private void BevelEdge(VoxelEdgeReference edgeRef, bool bevel,
        Voxel.BevelType bevelType = Voxel.BevelType.NONE)
    {
        Voxel voxel = edgeRef.voxel;
        if (!bevel || !voxel.EdgeIsConvex(edgeRef.edgeI))
        {
            voxel.edges[edgeRef.edgeI].hasBevel = false;
        }
        else
        {
            voxel.edges[edgeRef.edgeI].hasBevel = true;
            if (bevelType != Voxel.BevelType.NONE)
                voxel.bevelType = bevelType; 
            // don't allow full convex bevels to overlap with other bevels
            foreach (int unconnectedEdgeI in Voxel.UnconnectedEdges(edgeRef.edgeI))
            {
                if (voxel.EdgeIsConvex(unconnectedEdgeI))
                    voxel.edges[unconnectedEdgeI].hasBevel = false;
            }
        }
    }

    // return null voxel if edge doesn't exist or substances don't match
    private VoxelEdgeReference OpposingEdgeRef(VoxelEdgeReference edgeRef)
    {
        int faceA, faceB;
        Voxel.EdgeFaces(edgeRef.edgeI, out faceA, out faceB);
        Vector3Int opposingPos = edgeRef.voxel.position
            + Voxel.DirectionForFaceI(faceA).ToInt() + Voxel.DirectionForFaceI(faceB).ToInt();
        Voxel opposingVoxel = VoxelArray.VoxelAtAdjacent(opposingPos, edgeRef.voxel);
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
        Vector3 createPosition = selectionBounds.center; // not int
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
            Voxel voxel = VoxelAt(createPosition.ToInt(), false);
            if (voxel != null && voxel.substance == null && !voxel.faces[Voxel.OppositeFaceI(faceNormal)].IsEmpty())
                return false; // blocked by wall. no room to create object
            if (voxel == null || voxel.objectEntity == null)
                break;
            createPosition += createDirection;
        }
        obj.position = createPosition.ToInt();

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