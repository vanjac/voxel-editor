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

        void SelectionStateUpdated(VoxelArray voxelArray);
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

        public void SelectionStateUpdated(VoxelArray voxelArray)
        {
            voxelArray.UpdateVoxel(voxel);
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

        public void SelectionStateUpdated(VoxelArray voxelArray)
        {
            voxelArray.UpdateVoxel(voxel);
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

    public void Awake()
    {
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

    public override void SetVoxelSubstance(Voxel voxel, Substance substance)
    {
        if (substance != null && !substance.AliveInEditor())
            EntityPreviewManager.AddEntity(substance);
        Substance oldSubstance = voxel.substance;
        base.SetVoxelSubstance(voxel, substance);
        if (oldSubstance != null && !oldSubstance.AliveInEditor())
            EntityPreviewManager.RemoveEntity(oldSubstance);
    }

    public override void AddObject(ObjectEntity obj)
    {
        unsavedChanges = true;
        base.AddObject(obj);
        EntityPreviewManager.AddEntity(obj);
    }

    public override void DeleteObject(ObjectEntity obj)
    {
        unsavedChanges = true;
        base.DeleteObject(obj);
        EntityPreviewManager.RemoveEntity(obj);
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
        }
        else
        {
            ClearSelection();
            selectMode = thing is VoxelEdgeReference ? SelectMode.BOX_EDGES : SelectMode.BOX;
            boxSelectStartBounds = thing.bounds;
            selectionBounds = boxSelectStartBounds;
            // don't actually call this, only select the tapped thing
            // avoid issues when tapping things with overlapping bounds
            //UpdateBoxSelection();
            SelectThing(thing);
            SetMoveAxes(selectionBounds.center);
        }
        DisableMoveAxes();
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
        if (voxel == null || elementType != VoxelElement.FACES)
            return;
        if (voxel.substance == null)
        {
            ClearSelection();
            FaceSelectFloodFill(new VoxelFaceReference(voxel, elementI), stayOnPlane: false);

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
            thing.SelectionStateUpdated(this);
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
        thing.SelectionStateUpdated(this);
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
        thing.SelectionStateUpdated(this);
        selectionChanged = true;
    }

    private void DeselectThing(int index)
    {
        Selectable thing = selectedThings[index];
        if (!thing.addSelected)
            return;
        thing.addSelected = false;
        selectedThings.RemoveAt(index);
        thing.SelectionStateUpdated(this);
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

    private bool EdgeIsCorner(EdgeType edgeType) =>
        edgeType == EdgeType.CONVEX || edgeType == EdgeType.CONCAVE;

    private bool EdgeIsSelectable(VoxelEdgeReference edgeRef) => EdgeIsCorner(GetEdgeType(edgeRef));

    // add selected things come before stored selection
    // this is important for functions like GetSelectedPaint
    private IEnumerable<Selectable> IterateSelected()
    {
        foreach (Selectable thing in selectedThings)
            yield return thing;
        foreach (Selectable thing in storedSelectedThings)
            if (!thing.addSelected) // make sure the thing isn't also in selectedThings
                yield return thing;
    }

    private IEnumerable<T> IterateSelected<T>() where T : Selectable
    {
        foreach (Selectable thing in IterateSelected())
            if (thing is T)
                yield return (T)thing;
    }

    // including stored selection
    public bool SomethingIsSelected() => SomethingIsAddSelected() || SomethingIsStoredSelected();

    public bool SomethingIsAddSelected() => selectedThings.Count != 0;

    public bool SomethingIsStoredSelected() => storedSelectedThings.Count != 0;

    private bool TypeIsSelected<T>() where T : Selectable
    {
        foreach (var thing in IterateSelected<T>())
            return true;
        return false;
    }

    public bool FacesAreSelected() => TypeIsSelected<VoxelFaceReference>();

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
            thing.SelectionStateUpdated(this);
        }
        storedSelectedThings.Clear();
        AutoSetMoveAxesEnabled();
        selectionChanged = true;
    }

    private void UpdateBoxSelection()
    {
        var bounds = selectionBounds;
        SetMoveAxes(bounds.center);

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
            if (thingSubstance != boxSelectSubstance || !ThingInBoxSelection(thing, bounds))
                DeselectThing(i);
        }
        if (boxSelectSubstance == selectObjectSubstance)
        {
            foreach (var obj in IterateObjects())
            {
                if (ThingInBoxSelection(obj.marker, bounds))
                    SelectThing(obj.marker);
            }
        }
        else
        {
            VoxelGroup group = (boxSelectSubstance != null) ? boxSelectSubstance.voxelGroup : worldGroup;
            const int BLOCK = VoxelGroup.COMPONENT_BLOCK_SIZE;
            for (int z = (int)bounds.min.z - 1; z < bounds.max.z + BLOCK; z += BLOCK)
            {
                for (int y = (int)bounds.min.y - 1; y < bounds.max.y + BLOCK; y += BLOCK)
                {
                    for (int x = (int)bounds.min.x - 1; x < bounds.max.x + BLOCK; x += BLOCK)
                    {
                        VoxelComponent component = group.ComponentAt(new Vector3Int(x, y, z), null);
                        if (!component)
                            continue;
                        foreach (Voxel voxel in component.voxels)
                            UpdateBoxSelectionVoxel(voxel, bounds, boxSelectSubstance, selectMode == SelectMode.BOX_EDGES);
                    }
                }
            }
        }
    }

    private void UpdateBoxSelectionVoxel(Voxel voxel, Bounds bounds, Substance substance, bool edges)
    {
        if (!bounds.Intersects(voxel.GetBounds()))
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
                facesToSelect.Enqueue(new VoxelFaceReference(VoxelAt(newPos, false), faceRef.faceI));

                if (!stayOnPlane)
                {
                    facesToSelect.Enqueue(new VoxelFaceReference(faceRef.voxel, sideFaceI));
                    newPos += Voxel.DirectionForFaceI(faceRef.faceI).ToInt();
                    facesToSelect.Enqueue(new VoxelFaceReference(VoxelAt(newPos, false), Voxel.OppositeFaceI(sideFaceI)));
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
        foreach (Voxel v in substance.voxelGroup.IterateVoxels())
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
                if (voxel.faces[faceI].PaintOnly().Equals(paint))
                    SelectFace(voxel, faceI);
        }
        foreach (ObjectEntity obj in IterateObjects())
        {
            if (obj.paint.PaintOnly().Equals(paint))
                SelectThing(obj.marker);
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
        MergeStoredSelected();
        // now we can safely look only the addSelected property and the selectedThings list
        // and ignore the storedSelected property and the storedSelectedThings list

        int adjustDirFaceI = Voxel.FaceIForDirection(adjustDirection);
        int oppositeAdjustDirFaceI = Voxel.OppositeFaceI(adjustDirFaceI);
        int adjustAxis = Voxel.FaceIAxis(adjustDirFaceI);
        bool negativeAdjustAxis = adjustDirFaceI % 2 == 0;

        // sort selectedThings in order along the adjustDirection vector
        selectedThings.Sort((Selectable a, Selectable b) =>
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
        bool temporarilyBlockPushingANewSubstance = false; // :(

        // reuse arrays for each face
        VoxelEdge[] movingEdges = new VoxelEdge[4];
        var bevelsToUpdate = new HashSet<VoxelEdgeReference>();

        for (int i = 0; i < selectedThings.Count; i++)
        {
            Selectable thing = selectedThings[i];
            if (thing is ObjectMarker)
            {
                PushObject(((ObjectMarker)thing).objectEntity, adjustDirection);
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
                    AdjustClearEdge(new VoxelEdgeReference(oldVoxel, edgeI), bevelsToUpdate, voxelsToUpdate);
                }
                for (int sideNum = 0; sideNum < 4; sideNum++)
                {
                    int sideFaceI = Voxel.SideFaceI(faceI, sideNum);
                    if (oldVoxel.faces[sideFaceI].IsEmpty())
                    {
                        // add side
                        Vector3Int sideFaceDir = Voxel.DirectionForFaceI(sideFaceI).ToInt();
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
                            AdjustClearEdge(new VoxelEdgeReference(oldVoxel, edgeI), bevelsToUpdate, voxelsToUpdate);
                        }
                    }
                }

                if (!oldVoxel.faces[oppositeFaceI].IsEmpty())
                {
                    blocked = true;
                    // make sure any concave edges are cleared
                    foreach (int edgeI in Voxel.FaceSurroundingEdges(oppositeFaceI))
                    {
                        AdjustClearEdge(new VoxelEdgeReference(oldVoxel, edgeI), bevelsToUpdate, voxelsToUpdate);
                    }
                }
                oldVoxel.ClearFaces();
                SetVoxelSubstance(oldVoxel, null);
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
                    AdjustClearEdge(new VoxelEdgeReference(oldVoxel, edgeI), bevelsToUpdate, voxelsToUpdate);
                }

                for (int sideNum = 0; sideNum < 4; sideNum++)
                {
                    int sideFaceI = Voxel.SideFaceI(faceI, sideNum);
                    int oppositeSideFaceI = Voxel.OppositeFaceI(sideFaceI);
                    Voxel sideVoxel = VoxelAt(newPos + Voxel.DirectionForFaceI(sideFaceI).ToInt(), false);
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
                            AdjustClearEdge(new VoxelEdgeReference(sideVoxel, edgeI), bevelsToUpdate, voxelsToUpdate);
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
                        foreach (int edgeI in Voxel.FaceSurroundingEdges(oppositeFaceI))
                        {
                            // clear any bevels on the face that will be deleted
                            AdjustClearEdge(new VoxelEdgeReference(blockingVoxel, edgeI), bevelsToUpdate, voxelsToUpdate);
                        }
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
                SetVoxelSubstance(newVoxel, movingSubstance);
                selectedThings[i] = new VoxelFaceReference(newVoxel, faceI);
                if (pushing || pulling)
                {
                    movingEdgesI = 0;
                    foreach (int edgeI in Voxel.FaceSurroundingEdges(faceI))
                    {
                        var edgeRef = new VoxelEdgeReference(newVoxel, edgeI);
                        newVoxel.edges[edgeI] = movingEdges[movingEdgesI];
                        // if the face was pushed/pulled to be coplanar with surrounding faces,
                        // UpdateBevel will automatically catch this, and clear the bevel along with the
                        // surrounding face's bevel (with alsoBevelOppositeFlatEdge=true)
                        UpdateBevel(edgeRef, voxelsToUpdate, alsoBevelOppositeConcaveEdge: true,
                            dontUpdateThisVoxel: true, alsoBevelOppositeFlatEdge: true);
                        movingEdgesI++;
                    }
                }
            }
            else
            {
                // clear the selection; will be deleted later
                selectedThings[i] = new VoxelFaceReference(null, -1);
                if (pulling && substanceToCreate == null)
                    SetVoxelSubstance(newVoxel, movingSubstance);
            }

            foreach (VoxelEdgeReference edgeRef in bevelsToUpdate)
            {
                UpdateBevel(edgeRef, voxelsToUpdate, alsoBevelOppositeConcaveEdge: true,
                    dontUpdateThisVoxel: true, alsoBevelOppositeFlatEdge: true);
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
    } // end SingleAdjust()

    // fix for an issue when clearing concave edges on a face that will be deleted
    private void AdjustClearEdge(VoxelEdgeReference edgeRef,
            HashSet<VoxelEdgeReference> bevelsToUpdate, HashSet<Voxel> voxelsToUpdate)
    {
        edgeRef.voxel.edges[edgeRef.edgeI].Clear();
        bevelsToUpdate.Add(edgeRef);
        if (GetEdgeType(edgeRef) == EdgeType.CONCAVE)
        {
            // the face might be deleted later, so UpdateBevel won't know to update the other halves
            var oppEdgeRef = OpposingEdgeRef(edgeRef);
            if (oppEdgeRef.voxel != null)
            {
                bevelsToUpdate.Add(oppEdgeRef);
                voxelsToUpdate.Add(oppEdgeRef.voxel);
            }
        }
    }

    private Voxel CreateSubstanceBlock(Vector3Int position, Substance substance, VoxelFace faceTemplate)
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
        SetVoxelSubstance(voxel, substance);
        for (int faceI = 0; faceI < 6; faceI++)
        {
            Voxel adjacentVoxel = VoxelAt(position + Voxel.DirectionForFaceI(faceI).ToInt(), false);
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

    private void PushObject(ObjectEntity obj, Vector3Int direction)
    {
        var existingObj = ObjectAt(obj.position + direction);
        if (existingObj != null)
            PushObject(existingObj, direction);
        MoveObject(obj, obj.position + direction);
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

        foreach (Selectable thing in IterateSelected())
        {
            if (thing is VoxelFaceReference)
                return ((VoxelFaceReference)thing).face.PaintOnly();
            else if (thing is ObjectMarker)
                return ((ObjectMarker)thing).objectEntity.paint.PaintOnly();
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
        foreach (var obj in IterateSelected<ObjectMarker>())
        {
            obj.objectEntity.paint.material = paint.material;
            obj.objectEntity.paint.overlay = paint.overlay;
            obj.objectEntity.paint.orientation = paint.orientation;
            ObjectModified(obj.objectEntity);
        }
    }

    public void ReplaceMaterial(Material oldMat, Material newMat)
    {
        foreach (Voxel voxel in IterateVoxels())
        {
            for (int faceI = 0; faceI < 6; faceI++)
            {
                if (voxel.faces[faceI].material == oldMat)
                    voxel.faces[faceI].material = newMat;
                if (voxel.faces[faceI].overlay == oldMat)
                    voxel.faces[faceI].overlay = newMat;
                VoxelModified(voxel);
            }
        }
        foreach (ObjectEntity obj in IterateObjects())
        {
            if (obj.paint.material == oldMat)
                obj.paint.material = newMat;
            if (obj.paint.overlay == oldMat)
                obj.paint.overlay = newMat;
            ObjectModified(obj);
        }
    }

    public VoxelEdge GetSelectedBevel()
    {
        foreach (var edgeRef in IterateSelected<VoxelEdgeReference>())
        {
            var edge = edgeRef.edge;
            edge.addSelected = false;
            edge.storedSelected = false;
            return edge;
        }
        return new VoxelEdge();
    }

    public void BevelSelectedEdges(VoxelEdge applyBevel)
    {
        var voxelsToUpdate = new HashSet<Voxel>();
        foreach (var edgeRef in IterateSelected<VoxelEdgeReference>())
        {
            if (edgeRef.voxel.EdgeIsEmpty(edgeRef.edgeI))
                continue;
            edgeRef.voxel.edges[edgeRef.edgeI].bevel = applyBevel.bevel;
            UpdateBevel(edgeRef, voxelsToUpdate, alsoBevelOppositeConcaveEdge: true, dontUpdateThisVoxel: false);
        }
        foreach (Voxel voxel in voxelsToUpdate)
            VoxelModified(voxel);
    }

    // Call this function after an edgeRef's bevel settings have been set. It will:
    //   - Clear the bevel if the edge is empty or flat
    //   - Update the caps on the edge and adjacent edges
    //   - Add all modified voxels to voxelsToUpdate (except given voxel with dontUpdateThisVoxel=true)
    //   - Optionally repeat all of this for the other "half" of the edge
    private void UpdateBevel(VoxelEdgeReference edgeRef, HashSet<Voxel> voxelsToUpdate, bool alsoBevelOppositeConcaveEdge,
        bool dontUpdateThisVoxel, bool alsoBevelOppositeFlatEdge = false, EdgeType? type = null)
    {
        Voxel voxel = edgeRef.voxel;
        int minFaceI = Voxel.EdgeIAxis(edgeRef.edgeI) * 2;
        var minFaceDir = Voxel.DirectionForFaceI(minFaceI).ToInt();
        Voxel minVoxel = VoxelAt(voxel.position + minFaceDir, false);
        Voxel maxVoxel = VoxelAt(voxel.position - minFaceDir, false);
        var minEdgeRef = new VoxelEdgeReference(minVoxel, edgeRef.edgeI);
        var maxEdgeRef = new VoxelEdgeReference(maxVoxel, edgeRef.edgeI);

        if (type == null)
            type = GetEdgeType(edgeRef);

        if (type == EdgeType.EMPTY || type == EdgeType.FLAT)
        {
            voxel.edges[edgeRef.edgeI].Clear();
        }

        // update adjacent edges if they are beveled (for caps)
        // TODO this often won't be necessary
        if (minVoxel != null && minEdgeRef.edge.hasBevel)
        {
            voxelsToUpdate.Add(minVoxel);
            if (!minVoxel.EdgeIsConvex(minEdgeRef.edgeI))
            {
                // probably concave, unless the bevel hasn't been cleared yet
                var oppMinEdgeRef = OpposingEdgeRef(minEdgeRef);
                if (oppMinEdgeRef.voxel != null && oppMinEdgeRef.edge.hasBevel)
                    voxelsToUpdate.Add(oppMinEdgeRef.voxel);
            }
        }
        if (maxVoxel != null && maxEdgeRef.edge.hasBevel)
        {
            voxelsToUpdate.Add(maxVoxel);
            if (!maxVoxel.EdgeIsConvex(maxEdgeRef.edgeI))
            {
                // probably concave, unless the bevel hasn't been cleared yet
                var oppMaxEdgeRef = OpposingEdgeRef(maxEdgeRef);
                if (oppMaxEdgeRef.voxel != null && oppMaxEdgeRef.edge.hasBevel)
                    voxelsToUpdate.Add(oppMaxEdgeRef.voxel);
            }
        }

        if (edgeRef.edge.hasBevel && EdgeIsCorner(type.Value))
        {
            // don't allow full convex bevels to overlap with other bevels
            foreach (int unconnectedEdgeI in Voxel.UnconnectedEdges(edgeRef.edgeI))
            {
                if (voxel.EdgeIsEmpty(unconnectedEdgeI))
                    continue;
                var unconnectedEdgeRef = new VoxelEdgeReference(voxel, unconnectedEdgeI);
                if (unconnectedEdgeRef.edge.hasBevel)
                {
                    if ((edgeRef.edge.bevelSize == VoxelEdge.BevelSize.FULL
                        && type == EdgeType.CONVEX)
                        || (unconnectedEdgeRef.edge.bevelSize == VoxelEdge.BevelSize.FULL)
                            && voxel.EdgeIsConvex(unconnectedEdgeI))
                    {
                        // full convex bevel overlaps with another bevel! this won't work!
                        voxel.edges[unconnectedEdgeI].bevelType = VoxelEdge.BevelType.NONE;
                        UpdateBevel(unconnectedEdgeRef, voxelsToUpdate, alsoBevelOppositeConcaveEdge: true,
                            dontUpdateThisVoxel: true);
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
                UpdateBevel(oppEdgeRef, voxelsToUpdate, alsoBevelOppositeConcaveEdge: false,
                    dontUpdateThisVoxel: false, type: type);
            }
        }
        else if (alsoBevelOppositeFlatEdge && type == EdgeType.FLAT)
        {
            var oppEdgeRef = OpposingFlatEdgeRef(edgeRef);
            if (oppEdgeRef.voxel != null)
            {
                oppEdgeRef.voxel.edges[oppEdgeRef.edgeI].bevel = edgeRef.edge.bevel;
                UpdateBevel(oppEdgeRef, voxelsToUpdate, alsoBevelOppositeConcaveEdge: false,
                    dontUpdateThisVoxel: false, type: type);
            }
        }
        if (!dontUpdateThisVoxel)
            voxelsToUpdate.Add(voxel);
    }

    // return null voxel if edge doesn't exist or substances don't match
    private VoxelEdgeReference OpposingEdgeRef(VoxelEdgeReference edgeRef)
    {
        int faceA, faceB;
        Voxel.EdgeFaces(edgeRef.edgeI, out faceA, out faceB);
        Vector3Int opposingPos = edgeRef.voxel.position
            + Voxel.DirectionForFaceI(faceA).ToInt() + Voxel.DirectionForFaceI(faceB).ToInt();
        Voxel opposingVoxel = VoxelAt(opposingPos, false);
        if (opposingVoxel != null && opposingVoxel.substance != edgeRef.voxel.substance)
            opposingVoxel = null;
        int opposingEdgeI = Voxel.EdgeIAxis(edgeRef.edgeI) * 4 + ((edgeRef.edgeI + 2) % 4);
        return new VoxelEdgeReference(opposingVoxel, opposingEdgeI);
    }

    private VoxelEdgeReference OpposingFlatEdgeRef(VoxelEdgeReference edgeRef)
    {
        // find voxel next to this one
        int faceA, faceB;
        Voxel.EdgeFaces(edgeRef.edgeI, out faceA, out faceB);
        int emptyFace = edgeRef.voxel.faces[faceA].IsEmpty() ? faceA : faceB;
        int notEmptyFace = emptyFace == faceA ? faceB : faceA;
        Voxel adjacentVoxel = VoxelAt(edgeRef.voxel.position
            + Voxel.DirectionForFaceI(emptyFace).ToInt(), false);
        if (adjacentVoxel != null && adjacentVoxel.substance == edgeRef.voxel.substance)
        {
            // find edge on adjacentVoxel connected to edgeRef
            foreach (int otherEdgeI in FaceSurroundingEdgesAlongAxis(notEmptyFace,
                Voxel.EdgeIAxis(edgeRef.edgeI)))
            {
                if (otherEdgeI != edgeRef.edgeI)
                    return new VoxelEdgeReference(adjacentVoxel, otherEdgeI);
            }
        }
        return new VoxelEdgeReference(null, -1);
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

    public void PlaceObject(ObjectEntity obj)
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
        while (ObjectAt(createPosition.ToInt()) != null)
            createPosition += createDirection;
        obj.position = createPosition.ToInt();

        obj.InitObjectMarker(this);
        AddObject(obj);
        // select the object. Wait one frame so the position is correct
        StartCoroutine(SelectNewObjectCoroutine(obj));
    }

    private IEnumerator SelectNewObjectCoroutine(ObjectEntity obj)
    {
        yield return null;
        ClearStoredSelection();
        TouchDown(obj.marker);
        TouchUp();
    }
}