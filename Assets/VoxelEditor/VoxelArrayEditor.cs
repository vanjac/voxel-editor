using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelArrayEditor : VoxelArray
{
    public interface Selectable : System.IEquatable<Selectable>
    {
        Bounds GetBounds();
        void SelectionStateUpdated(VoxelArray voxelArray);
    }

    private struct VoxelFaceSelect : Selectable, System.IEquatable<VoxelFaceSelect>
    {
        public VoxelFaceLoc loc;

        public Bounds GetBounds() => Voxel.FaceBounds(loc);

        public VoxelFaceSelect(VoxelFaceLoc loc)
        {
            this.loc = loc;
        }

        public VoxelFaceSelect(Vector3Int position, int faceI)
        {
            loc = new VoxelFaceLoc(position, faceI);
        }

        public void SelectionStateUpdated(VoxelArray voxelArray)
        {
            voxelArray.UpdateVoxel(loc.position);
        }

        public override bool Equals(object obj) => obj is VoxelFaceSelect other && Equals(other);
        public bool Equals(Selectable sel) => sel is VoxelFaceSelect other && Equals(other);
        public bool Equals(VoxelFaceSelect other) => loc.Equals(other.loc);
        public override int GetHashCode() => loc.GetHashCode();
    }

    private struct VoxelEdgeSelect : Selectable, System.IEquatable<VoxelEdgeSelect>
    {
        public VoxelEdgeLoc loc;

        public Bounds GetBounds() => Voxel.EdgeBounds(loc);

        public VoxelEdgeSelect(VoxelEdgeLoc loc)
        {
            this.loc = loc;
        }

        public VoxelEdgeSelect(Vector3Int position, int edgeI)
        {
            loc = new VoxelEdgeLoc(position, edgeI);
        }

        public void SelectionStateUpdated(VoxelArray voxelArray)
        {
            voxelArray.UpdateVoxel(loc.position);
        }

        public override bool Equals(object obj) => obj is VoxelEdgeSelect other && Equals(other);
        public bool Equals(Selectable sel) => sel is VoxelEdgeSelect other && Equals(other);
        public bool Equals(VoxelEdgeSelect other) => loc.Equals(other.loc);
        public override int GetHashCode() => loc.GetHashCode();
    }

    private class AdjustComparer : IComparer<Selectable>
    {
        private int adjustAxis;
        private bool negativeAdjustAxis;
        private int oppositeAdjustDirFaceI;

        public AdjustComparer(int adjustDirFaceI)
        {
            oppositeAdjustDirFaceI = Voxel.OppositeFaceI(adjustDirFaceI);
            adjustAxis = Voxel.FaceIAxis(adjustDirFaceI);
            negativeAdjustAxis = adjustDirFaceI % 2 == 0;
        }

        public int Compare(Selectable a, Selectable b)
        {
            // positive means A is greater than B
            // so positive means B will be adjusted before A
            Vector3 aCenter = a.GetBounds().center;
            Vector3 bCenter = b.GetBounds().center;
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
            if (a is VoxelFaceSelect aFace && b is VoxelFaceSelect bFace)
            {
                if (aFace.loc.faceI == oppositeAdjustDirFaceI
                        && bFace.loc.faceI != oppositeAdjustDirFaceI)
                    return -1; // move one substance back before moving other forward
                if (bFace.loc.faceI == oppositeAdjustDirFaceI
                        && aFace.loc.faceI != oppositeAdjustDirFaceI)
                    return 1;
            }
            return 0;
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
    private HashSet<Selectable> selectedThings = new HashSet<Selectable>();
    // all faces where face.storedSelected == true
    private HashSet<Selectable> storedSelectedThings = new HashSet<Selectable>();
    public Bounds boxSelectStartBounds = new Bounds(Vector3.zero, Vector3.zero);
    private Substance boxSelectSubstance = null;
    // dummy Substance to use for boxSelectSubstance when selecting objects
    private readonly Substance selectObjectSubstance = new Substance();
    // Used for: box selection, axes placement, object creation
    public Bounds selectionBounds = new Bounds(Vector3.zero, Vector3.zero);

    public Substance substanceToCreate = null;

    public struct SelectionState
    {
        public HashSet<Selectable> selectedThings;
        public HashSet<Selectable> storedSelectedThings;

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

    public override void VoxelModified(Vector3Int position)
    {
        unsavedChanges = true;
        base.VoxelModified(position);
    }

    public override Substance SetSubstance(Vector3Int position, Substance substance)
    {
        if (substance != null && !substance.AliveInEditor())
            EntityPreviewManager.AddEntity(substance);
        Substance oldSubstance = base.SetSubstance(position, substance);
        if (oldSubstance != null && !oldSubstance.AliveInEditor())
            EntityPreviewManager.RemoveEntity(oldSubstance);
        return oldSubstance;
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
    public void TouchDown(Vector3Int position, int elementI, VoxelElement elementType)
    {
        if (elementType == VoxelElement.FACES)
            TouchDown(new VoxelFaceSelect(position, elementI));
        else if (elementType == VoxelElement.EDGES)
        {
            var edgeSel = new VoxelEdgeSelect(position, elementI);
            if (EdgeIsSelectable(edgeSel.loc))
                TouchDown(edgeSel);
        }
    }

    public void TouchDown(Selectable thing)
    {
        if (thing == null)
        {
            ClearSelection();
            return;
        }
        if (thing is VoxelFaceSelect faceSel)
            boxSelectSubstance = SubstanceAt(faceSel.loc.position);
        else if (thing is VoxelEdgeSelect edgeSel)
            boxSelectSubstance = SubstanceAt(edgeSel.loc.position);
        else if (thing is ObjectMarker)
            boxSelectSubstance = selectObjectSubstance;
        else
            boxSelectSubstance = null;
        if (drawSelect)
        {
            MergeStoredSelected();
            if (IsAddSelected(thing))
            {
                DeselectThing(thing);
                selectMode = SelectMode.DRAW_DESELECT;
            }
            else
            {
                SelectThing(thing);
                SetMoveAxes(thing.GetBounds().center);
                selectMode = SelectMode.DRAW_SELECT;
            }
        }
        else
        {
            ClearSelection();
            selectMode = thing is VoxelEdgeSelect ? SelectMode.BOX_EDGES : SelectMode.BOX;
            boxSelectStartBounds = thing.GetBounds();
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
    public void TouchDrag(Vector3Int position, int elementI, VoxelElement elementType)
    {
        if (elementType == VoxelElement.FACES)
            TouchDrag(new VoxelFaceSelect(position, elementI));
        else if (elementType == VoxelElement.EDGES)
        {
            var edgeSel = new VoxelEdgeSelect(position, elementI);
            if (EdgeIsSelectable(edgeSel.loc))
                TouchDrag(edgeSel);
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
        selectionBounds.Encapsulate(thing.GetBounds());
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
            if (!(thing is VoxelFaceSelect faceSel)
                    || SubstanceAt(faceSel.loc.position) != boxSelectSubstance)
                return;
        }
        if (selectMode == SelectMode.DRAW_SELECT)
        {
            SelectThing(thing);
            SetMoveAxes(thing.GetBounds().center);
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
    public void DoubleTouch(Vector3Int position, int elementI, VoxelElement elementType)
    {
        if (drawSelect && elementType == VoxelElement.FACES)
            // flood fill inside existing selection
            DeselectThing(new VoxelFaceSelect(position, elementI));
        else
            ClearSelection();
        if (elementType == VoxelElement.FACES)
            FaceSelectFloodFill(new VoxelFaceLoc(position, elementI), stayOnPlane: true);
        else if (elementType == VoxelElement.EDGES)
        {
            var edgeSel = new VoxelEdgeSelect(position, elementI);
            if (EdgeIsSelectable(edgeSel.loc))
                EdgeSelectFloodFill(edgeSel.loc, SubstanceAt(position));
        }
        AutoSetMoveAxesEnabled();
    }

    // called by TouchListener
    public void TripleTouch(Vector3Int position, int elementI, VoxelElement elementType)
    {
        if (elementType != VoxelElement.FACES)
            return;
        var substance = SubstanceAt(position);
        if (substance == null)
        {
            ClearSelection();
            FaceSelectFloodFill(new VoxelFaceLoc(position, elementI), stayOnPlane: false);
        }
        else
        {
            ClearSelection();
            SubstanceSelect(substance);
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
        axes.gameObject.SetActive(SomethingIsSelected() && !TypeIsSelected<VoxelEdgeSelect>());
        rotateAxis.gameObject.SetActive(TypeIsSelected<ObjectMarker>());
    }

    public void ClearSelection()
    {
        var prevSelectedThings = selectedThings;
        selectedThings = new HashSet<Selectable>();
        foreach (Selectable thing in prevSelectedThings)
        {
            thing.SelectionStateUpdated(this);
        }
        AutoSetMoveAxesEnabled();
        selectMode = SelectMode.NONE;
        selectionBounds = new Bounds(Vector3.zero, Vector3.zero);
        selectionChanged = true;
    }

    private void SelectThing(Selectable thing)
    {
        if (selectedThings.Add(thing))
        {
            thing.SelectionStateUpdated(this);
            selectionChanged = true;
        }
    }

    private void SelectFace(Vector3Int position, int faceI)
    {
        SelectThing(new VoxelFaceSelect(position, faceI));
    }

    private void DeselectThing(Selectable thing)
    {
        if (selectedThings.Remove(thing))
        {
            thing.SelectionStateUpdated(this);
            selectionChanged = true;
        }
    }

    private bool IsAddSelected(Selectable thing) => selectedThings.Contains(thing);

    private bool IsStoredSelected(Selectable thing) => storedSelectedThings.Contains(thing);

    public override bool IsSelected(Selectable thing) => IsAddSelected(thing) || IsStoredSelected(thing);
    public override bool FaceIsSelected(VoxelFaceLoc faceLoc) => IsSelected(new VoxelFaceSelect(faceLoc));
    public override bool EdgeIsSelected(VoxelEdgeLoc edgeLoc) => IsSelected(new VoxelEdgeSelect(edgeLoc));

    private EdgeType GetEdgeType(VoxelEdgeLoc loc)
    {
        var voxel = VoxelAt(loc.position, false);
        if (voxel == null || voxel.EdgeIsEmpty(loc.edgeI))
            return EdgeType.EMPTY;
        if (voxel.EdgeIsConvex(loc.edgeI))
            return EdgeType.CONVEX;
        // concave...
        var oppEdgeLoc = OpposingEdgeLoc(loc);
        var oppVoxel = VoxelAt(oppEdgeLoc.position, false);
        if (oppVoxel == null || oppVoxel.EdgeIsEmpty(oppEdgeLoc.edgeI))
            return EdgeType.FLAT;
        return EdgeType.CONCAVE;
    }

    private bool EdgeIsCorner(EdgeType edgeType) =>
        edgeType == EdgeType.CONVEX || edgeType == EdgeType.CONCAVE;

    private bool EdgeIsSelectable(VoxelEdgeLoc loc) => EdgeIsCorner(GetEdgeType(loc));

    // add selected things come before stored selection
    // this is important for functions like GetSelectedPaint
    private IEnumerable<Selectable> IterateSelected()
    {
        foreach (Selectable thing in selectedThings)
            yield return thing;
        foreach (Selectable thing in storedSelectedThings)
            if (!IsAddSelected(thing)) // make sure the thing isn't also in selectedThings
                yield return thing;
    }

    private IEnumerable<T> IterateSelected<T>() where T : Selectable
    {
        foreach (Selectable thing in IterateSelected())
            if (thing is T tThing)
                yield return tThing;
    }

    // including stored selection
    public bool SomethingIsSelected() => SomethingIsAddSelected() || SomethingIsStoredSelected();

    public bool SomethingIsAddSelected() => selectedThings.Count != 0;

    public bool SomethingIsStoredSelected() => storedSelectedThings.Count != 0;

    private bool TypeIsSelected<T>() where T : Selectable
    {
        foreach (var _ in IterateSelected<T>())
            return true;
        return false;
    }

    public bool FacesAreSelected() => TypeIsSelected<VoxelFaceSelect>();

    public bool FacesAreAddSelected()
    {
        foreach (var thing in selectedThings)
            if (thing is VoxelFaceSelect)
                return true;
        return false;
    }

    public ICollection<Entity> GetSelectedEntities()
    {
        var selectedEntities = new HashSet<Entity>();
        foreach (Selectable thing in IterateSelected())
        {
            if (thing is VoxelFaceSelect faceSel)
            {
                var substance = SubstanceAt(faceSel.loc.position);
                if (substance != null)
                    selectedEntities.Add(substance); // HashSet will prevent duplicates
            }
            else if (thing is ObjectMarker marker)
            {
                selectedEntities.Add(marker.objectEntity);
            }
        }
        return selectedEntities;
    }

    public void StoreSelection()
    {
        // move things out of storedSelectedThings and into selectedThings
        storedSelectedThings.UnionWith(selectedThings);
        // shouldn't need to update the things since they should have already been selected
        selectedThings.Clear();
        selectMode = SelectMode.NONE;
        selectionBounds = new Bounds(Vector3.zero, Vector3.zero);
    }

    public void MergeStoredSelected()
    {
        // move things out of storedSelectedThings and into selectedThings
        // opposite of StoreSelection()
        selectedThings.UnionWith(storedSelectedThings);
        // shouldn't need to update the things since they should have already been selected
        storedSelectedThings.Clear();
        selectMode = SelectMode.ADJUSTED;
    }

    public void ClearStoredSelection()
    {
        var prevStored = storedSelectedThings;
        storedSelectedThings = new HashSet<Selectable>();
        foreach (Selectable thing in prevStored)
        {
            thing.SelectionStateUpdated(this);
        }
        AutoSetMoveAxesEnabled();
        selectionChanged = true;
    }

    private void UpdateBoxSelection()
    {
        var bounds = selectionBounds;
        SetMoveAxes(bounds.center);

        // update selection...
        var toDeselect = new List<Selectable>();
        foreach (var thing in selectedThings)
        {
            Substance thingSubstance = null;
            if (thing is VoxelFaceSelect faceSel)
                thingSubstance = SubstanceAt(faceSel.loc.position);
            else if (thing is VoxelEdgeSelect edgeSel)
                thingSubstance = SubstanceAt(edgeSel.loc.position);
            else if (thing is ObjectEntity)
                thingSubstance = selectObjectSubstance;

            if (thingSubstance != boxSelectSubstance || !ThingInBoxSelection(thing, bounds))
                toDeselect.Add(thing);
        }
        foreach (var thing in toDeselect)
        {
            DeselectThing(thing);
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
                        foreach (var pos in component.positions)
                            UpdateBoxSelectionVoxel(pos, bounds, selectMode == SelectMode.BOX_EDGES);
                    }
                }
            }
        }
    }

    private void UpdateBoxSelectionVoxel(Vector3Int position, Bounds bounds, bool edges)
    {
        if (!bounds.Intersects(Voxel.Bounds(position)))
            return;
        if (!edges)
        {
            var voxel = VoxelAt(position, false);
            if (voxel == null)
                return;
            for (int faceI = 0; faceI < Voxel.NUM_FACES; faceI++)
            {
                if (voxel.faces[faceI].IsEmpty())
                    continue;
                var faceSel = new VoxelFaceSelect(position, faceI);
                if (ThingInBoxSelection(faceSel, bounds))
                    SelectThing(faceSel);
            }
        }
        else // edges
        {
            for (int edgeI = 0; edgeI < Voxel.NUM_EDGES; edgeI++)
            {
                var edgeSel = new VoxelEdgeSelect(position, edgeI);
                if (EdgeIsSelectable(edgeSel.loc) && ThingInBoxSelection(edgeSel, bounds))
                    SelectThing(edgeSel);
            }
        }
    }

    private bool ThingInBoxSelection(Selectable thing, Bounds bounds)
    {
        bounds.Expand(new Vector3(0.1f, 0.1f, 0.1f));
        Bounds thingBounds = thing.GetBounds();
        return bounds.Contains(thingBounds.min) && bounds.Contains(thingBounds.max);
    }

    private void FaceSelectFloodFill(VoxelFaceLoc start, bool stayOnPlane, bool matchPaint = false)
    {
        Substance substance = SubstanceAt(start.position);
        VoxelFace paint = FaceAt(start);
        if (paint.IsEmpty())
            return;

        // this used to be a recursive algorithm but it would cause stack overflow exceptions
        Queue<VoxelFaceLoc> facesToSelect = new Queue<VoxelFaceLoc>();
        facesToSelect.Enqueue(start);

        // reset selection bounds
        selectMode = SelectMode.FACE_FLOOD_FILL;
        selectionBounds = Voxel.FaceBounds(start);

        while (facesToSelect.Count != 0)
        {
            var faceLoc = facesToSelect.Dequeue();
            var face = FaceAt(faceLoc);
            // stop at boundaries of stored selection
            if (face.IsEmpty() || FaceIsSelected(faceLoc) || SubstanceAt(faceLoc.position) != substance)
                continue;
            if (matchPaint && face != paint)
                continue;
            SelectThing(new VoxelFaceSelect(faceLoc));

            Vector3Int position = faceLoc.position;
            for (int sideNum = 0; sideNum < 4; sideNum++)
            {
                int sideFaceI = Voxel.SideFaceI(faceLoc.faceI, sideNum);
                Vector3Int newPos = position + Voxel.IntDirectionForFaceI(sideFaceI);
                facesToSelect.Enqueue(new VoxelFaceLoc(newPos, faceLoc.faceI));

                if (!stayOnPlane)
                {
                    facesToSelect.Enqueue(new VoxelFaceLoc(faceLoc.position, sideFaceI));
                    newPos += Voxel.IntDirectionForFaceI(faceLoc.faceI);
                    facesToSelect.Enqueue(new VoxelFaceLoc(newPos, Voxel.OppositeFaceI(sideFaceI)));
                }
            }

            // grow bounds
            selectionBounds.Encapsulate(Voxel.FaceBounds(faceLoc));
        }

        SetMoveAxes(start.position + new Vector3(0.5f, 0.5f, 0.5f) - Voxel.OppositeDirectionForFaceI(start.faceI) / 2);
    }

    public void FillSelectPaint()
    {
        if (GetSelectedPaint().IsEmpty())
            return;
        foreach (VoxelFaceSelect faceSel in IterateSelected<VoxelFaceSelect>())
        {
            ClearSelection();
            FaceSelectFloodFill(faceSel.loc, true, true);
            break;
        }
    }

    private void EdgeSelectFloodFill(VoxelEdgeLoc edgeLoc, Substance substance)
    {
        selectMode = SelectMode.EDGE_FLOOD_FILL;
        selectionBounds = Voxel.EdgeBounds(edgeLoc);
        int minFaceI = Voxel.EdgeIAxis(edgeLoc.edgeI) * 2;
        Vector3Int minDir = Voxel.IntDirectionForFaceI(minFaceI);
        var edgeType = GetEdgeType(edgeLoc);
        SelectContiguousEdges(edgeLoc, substance, minDir, edgeType);
        SelectContiguousEdges(edgeLoc, substance, minDir * -1, edgeType);
    }

    private void SelectContiguousEdges(VoxelEdgeLoc edgeLoc, Substance substance,
        Vector3Int direction, EdgeType edgeType)
    {
        for (Vector3Int voxelPos = edgeLoc.position; true; voxelPos += direction)
        {
            if (SubstanceAt(voxelPos) != substance)
                break;
            var contigEdgeLoc = new VoxelEdgeLoc(voxelPos, edgeLoc.edgeI);
            var contigEdgeType = GetEdgeType(contigEdgeLoc);
            if (contigEdgeType != edgeType)
                break;
            SelectThing(new VoxelEdgeSelect(contigEdgeLoc));
            selectionBounds.Encapsulate(Voxel.EdgeBounds(contigEdgeLoc));

            var oppEdgeLoc = OpposingEdgeLoc(contigEdgeLoc);
            var oppVoxel = VoxelAt(oppEdgeLoc.position, false);
            if (oppVoxel != null && !oppVoxel.EdgeIsEmpty(oppEdgeLoc.edgeI))
                SelectThing(new VoxelEdgeSelect(oppEdgeLoc));
        }
    }

    private void SubstanceSelect(Substance substance)
    {
        foreach (var (pos, voxel) in substance.voxelGroup.IterateVoxelPairs())
        {
            for (int i = 0; i < 6; i++)
            {
                if (!voxel.faces[i].IsEmpty())
                {
                    SelectFace(pos, i);
                    var faceBounds = Voxel.FaceBounds(new VoxelFaceLoc(pos, i));
                    if (selectMode != SelectMode.FACE_FLOOD_FILL)
                        selectionBounds = faceBounds;
                    else
                        selectionBounds.Encapsulate(faceBounds);
                    selectMode = SelectMode.FACE_FLOOD_FILL;
                }
            }
        }
    }

    private void SelectEntireEntity(Entity e)
    {
        if (e is ObjectEntity objectEntity)
        {
            SelectThing(objectEntity.marker);
        }
        else if (e is Substance substance)
        {
            SubstanceSelect(substance);
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
        foreach (var (pos, voxel) in IterateVoxelPairs())
        {
            if (voxel.substance != null && voxel.substance.tag == tag)
            {
                for (int faceI = 0; faceI < 6; faceI++)
                    if (!voxel.faces[faceI].IsEmpty())
                        SelectFace(pos, faceI);
            }
        }
        AutoSetMoveAxesEnabled();
    }

    public void SelectAllWithPaint(VoxelFace paint)
    {
        foreach (var (pos, voxel) in IterateVoxelPairs())
        {
            for (int faceI = 0; faceI < 6; faceI++)
                if (voxel.faces[faceI].Equals(paint))
                    SelectFace(pos, faceI);
        }
        foreach (ObjectEntity obj in IterateObjects())
        {
            if (obj.paint.Equals(paint))
                SelectThing(obj.marker);
        }
        AutoSetMoveAxesEnabled();
    }

    private IEnumerable<Property> AllEntityProperties(Entity e)
    {
        foreach (var prop in e.Properties())
            yield return prop;
        if (e.sensor != null)
        {
            foreach (var prop in e.sensor.Properties())
                yield return prop;
        }
        foreach (var behavior in e.behaviors)
        {
            foreach (var prop in behavior.Properties())
                yield return prop;
        }
    }

    private IEnumerable<Entity> OutgoingConnections(Entity e)
    {
        foreach (var prop in AllEntityProperties(e))
        {
            foreach (var entitySel in Properties.EntityReferences(prop))
            {
                if (entitySel.entity != null)
                {
                    yield return entitySel.entity;
                }
            }
        }
    }

    public void SelectOutgoingConnections()
    {
        var outgoing = new HashSet<Entity>();
        foreach (var entity in GetSelectedEntities())
        {
            foreach (var connection in OutgoingConnections(entity))
            {
                outgoing.Add(connection);
            }
        }
        ClearSelection();
        foreach (var entity in outgoing)
        {
            SelectEntireEntity(entity);
        }
        AutoSetMoveAxesEnabled(); // TODO also set position
    }

    public void SelectIncomingConnections()
    {
        var selectedEntities = new HashSet<Entity>(GetSelectedEntities());
        var incoming = new HashSet<Entity>();

        foreach (ObjectEntity entity in IterateObjects())
        {
            foreach (var connection in OutgoingConnections(entity))
            {
                if (selectedEntities.Contains(connection))
                {
                    incoming.Add(entity);
                    break;
                }
            }
        }

        var visited = new HashSet<Substance>();
        foreach (Voxel voxel in IterateVoxels())
        {
            if (voxel.substance != null && visited.Add(voxel.substance))
            {
                foreach (var connection in OutgoingConnections(voxel.substance))
                {
                    if (selectedEntities.Contains(connection))
                    {
                        incoming.Add(voxel.substance);
                        break;
                    }
                }
            }
        }

        ClearSelection();
        foreach (var entity in incoming)
        {
            SelectEntireEntity(entity);
        }
        AutoSetMoveAxesEnabled(); // TODO also set position
    }

    public SelectionState GetSelectionState()
    {
        SelectionState state;
        state.selectedThings = new HashSet<Selectable>(selectedThings);
        state.storedSelectedThings = new HashSet<Selectable>(storedSelectedThings);
        state.selectMode = selectMode;
        state.axes = axes.position;
        return state;
    }

    public void RecallSelectionState(SelectionState state)
    {
        ClearSelection();
        ClearStoredSelection();

        selectedThings = state.selectedThings;
        storedSelectedThings = state.storedSelectedThings;
        foreach (Selectable thing in selectedThings)
            thing.SelectionStateUpdated(this);
        foreach (Selectable thing in storedSelectedThings)
            thing.SelectionStateUpdated(this);
        selectMode = state.selectMode;
        axes.position = state.axes;
        AutoSetMoveAxesEnabled();
    }

    public float AllowedAdjustScale() => FacesAreSelected() ? 1.0f : OBJECT_GRID;

    public void Adjust(Vector3Int adjustDirection, int count, float scale)
    {
        var voxelsToUpdate = new HashSet<Vector3Int>();
        for (int i = 0; i < count; i++)
            SingleAdjust(adjustDirection, voxelsToUpdate, scale);
        foreach (var pos in voxelsToUpdate)
            VoxelModified(pos);
    }

    private void SingleAdjust(Vector3Int adjustDirection, HashSet<Vector3Int> voxelsToUpdate, float scale)
    {
        MergeStoredSelected();
        // now we can safely look only the addSelected property and the selectedThings list
        // and ignore the storedSelected property and the storedSelectedThings list

        // sort selectedThings in order along the adjustDirection vector
        var comparer = new AdjustComparer(Voxel.FaceIForDirection(adjustDirection));
        var sortedThings = new List<Selectable>(selectedThings.OrderBy(s => s, comparer));

        bool createdSubstance = false;

        foreach (Selectable thing in sortedThings)
        {
            if (thing is ObjectMarker marker)
            {
                PushObject(marker.objectEntity, adjustDirection, scale);
            }
            else if (thing is VoxelFaceSelect faceSel)
            {
                if (AdjustSelectedFace(faceSel, adjustDirection, substanceToCreate, voxelsToUpdate))
                    createdSubstance = true;
            }
        }

        selectionChanged = true;

        if (substanceToCreate != null && createdSubstance)
            substanceToCreate = null;

        selectionBounds.center += (Vector3)adjustDirection * scale;
        AutoSetMoveAxesEnabled();
    }

    private bool AdjustSelectedFace(VoxelFaceSelect faceSel, Vector3Int adjustDirection,
        Substance createSubstance, HashSet<Vector3Int> voxelsToUpdate)
    {
        int adjustDirFaceI = Voxel.FaceIForDirection(adjustDirection);
        int adjustAxis = Voxel.FaceIAxis(adjustDirFaceI);

        Vector3Int oldPos = faceSel.loc.position;
        Voxel oldVoxel = VoxelAt(oldPos, true);
        Vector3Int newPos = oldPos + adjustDirection;
        Voxel newVoxel = VoxelAt(newPos, true);

        int faceI = faceSel.loc.faceI;
        int oppositeFaceI = Voxel.OppositeFaceI(faceI);
        bool pushing = adjustDirFaceI == oppositeFaceI;
        bool pulling = adjustDirFaceI == faceI;

        var bevelsToUpdate = new HashSet<VoxelEdgeLoc>();

        var opposingFaceSel = new VoxelFaceSelect(newPos, oppositeFaceI);
        if (pulling && (!newVoxel.faces[oppositeFaceI].IsEmpty()) && !IsAddSelected(opposingFaceSel))
        {
            // usually this means there's another substance. push it away before this face
            if (createSubstance != null && newVoxel.substance == createSubstance)
            {
                // substance has already been created there!
                // createSubstance has never existed in the map before Adjust() was called
                // so it must have been created earlier in the loop
                // remove selection
                selectedThings.Remove(faceSel);
                voxelsToUpdate.Add(oldPos);
            }
            else
            {
                selectedThings.Add(opposingFaceSel);
                AdjustSelectedFace(opposingFaceSel, adjustDirection, null, voxelsToUpdate); // recurse!
                // need to move the other substance out of the way first
            }
            return false;
        }

        VoxelFace movingFace = oldVoxel.faces[faceI];
        Substance movingSubstance = oldVoxel.substance;

        VoxelEdge[] movingEdges = new VoxelEdge[4];
        int movingEdgesI = 0;
        foreach (int edgeI in Voxel.FaceSurroundingEdges(faceI))
        {
            movingEdges[movingEdgesI++] = oldVoxel.edges[edgeI];
        }

        bool blocked = false; // is movement blocked?
        Vector3Int newSubstanceBlock = NONE;

        if (pushing)
        {
            foreach (int edgeI in Voxel.FaceSurroundingEdges(faceI))
            {
                AdjustClearEdge(new VoxelEdgeLoc(oldPos, edgeI), bevelsToUpdate, voxelsToUpdate);
            }
            for (int sideNum = 0; sideNum < 4; sideNum++)
            {
                int sideFaceI = Voxel.SideFaceI(faceI, sideNum);
                if (oldVoxel.faces[sideFaceI].IsEmpty())
                {
                    // add side
                    Vector3Int sideFaceDir = Voxel.IntDirectionForFaceI(sideFaceI);
                    Vector3Int sidePos = oldPos + sideFaceDir;
                    Voxel sideVoxel = VoxelAt(sidePos, true);
                    int oppositeSideFaceI = Voxel.OppositeFaceI(sideFaceI);

                    // if possible, the new side should have the properties of the adjacent side
                    Voxel adjacentSideVoxel = VoxelAt(oldPos - adjustDirection + sideFaceDir, false);
                    if (adjacentSideVoxel != null && !adjacentSideVoxel.faces[oppositeSideFaceI].IsEmpty()
                        && movingSubstance == adjacentSideVoxel.substance)
                    {
                        sideVoxel.faces[oppositeSideFaceI] = adjacentSideVoxel.faces[oppositeSideFaceI];
                        foreach (int edgeI in FaceSurroundingEdgesAlongAxis(oppositeSideFaceI, adjustAxis))
                        {
                            sideVoxel.edges[edgeI] = adjacentSideVoxel.edges[edgeI];
                            bevelsToUpdate.Add(new VoxelEdgeLoc(sidePos, edgeI));
                        }
                    }
                    else
                    {
                        sideVoxel.faces[oppositeSideFaceI] = movingFace;
                    }
                    voxelsToUpdate.Add(sidePos);
                }
                else
                {
                    // side will be deleted when voxel is cleared but we'll remove/update the bevels now
                    foreach (int edgeI in FaceSurroundingEdgesAlongAxis(sideFaceI, adjustAxis))
                    {
                        AdjustClearEdge(new VoxelEdgeLoc(oldPos, edgeI), bevelsToUpdate, voxelsToUpdate);
                    }
                }
            }

            if (!oldVoxel.faces[oppositeFaceI].IsEmpty())
            {
                blocked = true;
                // make sure any concave edges are cleared
                foreach (int edgeI in Voxel.FaceSurroundingEdges(oppositeFaceI))
                {
                    AdjustClearEdge(new VoxelEdgeLoc(oldPos, edgeI), bevelsToUpdate, voxelsToUpdate);
                }
            }
            oldVoxel.ClearFaces();
            SetSubstance(oldPos, null);
            if (createSubstance != null)
            {
                if (CreateSubstanceBlock(oldPos, createSubstance, movingFace))
                    newSubstanceBlock = oldPos;
            }
        }
        else if (pulling && createSubstance != null)
        {
            if (CreateSubstanceBlock(newPos, createSubstance, movingFace))
                newSubstanceBlock = newPos;
            blocked = true;
        }
        else if (pulling)
        {
            foreach (int edgeI in Voxel.FaceSurroundingEdges(faceI))
            {
                AdjustClearEdge(new VoxelEdgeLoc(oldPos, edgeI), bevelsToUpdate, voxelsToUpdate);
            }

            for (int sideNum = 0; sideNum < 4; sideNum++)
            {
                int sideFaceI = Voxel.SideFaceI(faceI, sideNum);
                int oppositeSideFaceI = Voxel.OppositeFaceI(sideFaceI);
                var sidePos = newPos + Voxel.IntDirectionForFaceI(sideFaceI);
                Voxel sideVoxel = VoxelAt(sidePos, false);
                if (sideVoxel == null || sideVoxel.faces[oppositeSideFaceI].IsEmpty() || movingSubstance != sideVoxel.substance)
                {
                    // add side
                    // if possible, the new side should have the properties of the adjacent side
                    if (!oldVoxel.faces[sideFaceI].IsEmpty())
                    {
                        newVoxel.faces[sideFaceI] = oldVoxel.faces[sideFaceI];
                        foreach (int edgeI in FaceSurroundingEdgesAlongAxis(sideFaceI, adjustAxis))
                        {
                            newVoxel.edges[edgeI] = oldVoxel.edges[edgeI];
                            bevelsToUpdate.Add(new VoxelEdgeLoc(newPos, edgeI));
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
                        AdjustClearEdge(new VoxelEdgeLoc(sidePos, edgeI), bevelsToUpdate, voxelsToUpdate);
                    }
                    sideVoxel.faces[oppositeSideFaceI] = default;
                    voxelsToUpdate.Add(sidePos);
                }
            }

            var blockingPos = newPos + adjustDirection;
            Voxel blockingVoxel = VoxelAt(blockingPos, false);
            if (blockingVoxel != null && !blockingVoxel.faces[oppositeFaceI].IsEmpty())
            {
                if (movingSubstance == blockingVoxel.substance)
                {
                    blocked = true;
                    foreach (int edgeI in Voxel.FaceSurroundingEdges(oppositeFaceI))
                    {
                        // clear any bevels on the face that will be deleted
                        AdjustClearEdge(new VoxelEdgeLoc(blockingPos, edgeI), bevelsToUpdate, voxelsToUpdate);
                    }
                    blockingVoxel.faces[oppositeFaceI] = default;
                    voxelsToUpdate.Add(blockingPos);
                }
            }
            oldVoxel.faces[faceI] = default;
        }
        else // sliding
        {
            if (newVoxel.faces[faceI].IsEmpty() || newVoxel.substance != movingSubstance)
                blocked = true;
        }

        selectedThings.Remove(faceSel);
        if (!blocked)
        {
            // move the face
            newVoxel.faces[faceI] = movingFace;
            SetSubstance(newPos, movingSubstance);
            selectedThings.Add(new VoxelFaceSelect(newPos, faceI));
            if (pushing || pulling)
            {
                movingEdgesI = 0;
                foreach (int edgeI in Voxel.FaceSurroundingEdges(faceI))
                {
                    var edgeLoc = new VoxelEdgeLoc(newPos, edgeI);
                    newVoxel.edges[edgeI] = movingEdges[movingEdgesI];
                    // if the face was pushed/pulled to be coplanar with surrounding faces,
                    // UpdateBevel will automatically catch this, and clear the bevel along with the
                    // surrounding face's bevel (with alsoBevelOppositeFlatEdge=true)
                    UpdateBevel(edgeLoc, voxelsToUpdate, alsoBevelOppositeConcaveEdge: true,
                        dontUpdateThisVoxel: true, alsoBevelOppositeFlatEdge: true);
                    movingEdgesI++;
                }
            }
        }
        else
        {
            // selection is cleared
            if (pulling && createSubstance == null)
                SetSubstance(newPos, movingSubstance);
        }

        foreach (var edgeLoc in bevelsToUpdate)
        {
            UpdateBevel(edgeLoc, voxelsToUpdate, alsoBevelOppositeConcaveEdge: true,
                dontUpdateThisVoxel: true, alsoBevelOppositeFlatEdge: true);
        }

        if (newSubstanceBlock != NONE)
        {
            Voxel voxel = VoxelAt(newSubstanceBlock, true);
            if (!voxel.faces[adjustDirFaceI].IsEmpty())
            {
                selectedThings.Add(new VoxelFaceSelect(newSubstanceBlock, adjustDirFaceI));
            }
        }

        voxelsToUpdate.Add(newPos);
        voxelsToUpdate.Add(oldPos);

        return newSubstanceBlock != NONE;
    } // end AdjustSelectedFace()

    // fix for an issue when clearing concave edges on a face that will be deleted
    private void AdjustClearEdge(VoxelEdgeLoc edgeLoc,
            HashSet<VoxelEdgeLoc> bevelsToUpdate, HashSet<Vector3Int> voxelsToUpdate)
    {
        SetEdge(edgeLoc, default);
        bevelsToUpdate.Add(edgeLoc);
        if (GetEdgeType(edgeLoc) == EdgeType.CONCAVE)
        {
            // the face might be deleted later, so UpdateBevel won't know to update the other halves
            var oppEdgeLoc = OpposingEdgeLoc(edgeLoc);
            if (oppEdgeLoc.position != NONE)
            {
                bevelsToUpdate.Add(oppEdgeLoc);
                voxelsToUpdate.Add(oppEdgeLoc.position);
            }
        }
    }

    private bool CreateSubstanceBlock(Vector3Int position, Substance substance, VoxelFace faceTemplate)
    {
        if (!substance.defaultPaint.IsEmpty())
            faceTemplate = substance.defaultPaint;
        Voxel voxel = VoxelAt(position, true);
        if (!voxel.IsEmpty())
        {
            if (voxel.substance == substance)
                return true;
            return false; // doesn't work
        }
        SetSubstance(position, substance);
        for (int faceI = 0; faceI < 6; faceI++)
        {
            Voxel adjacentVoxel = VoxelAt(position + Voxel.IntDirectionForFaceI(faceI), false);
            if (adjacentVoxel == null || adjacentVoxel.substance != substance)
            {
                // create boundary
                voxel.faces[faceI] = faceTemplate;
            }
            else
            {
                // remove boundary
                adjacentVoxel.faces[Voxel.OppositeFaceI(faceI)] = default;
                voxel.faces[faceI] = default;
            }
        }
        return true;
    }

    private IEnumerable<int> FaceSurroundingEdgesAlongAxis(int faceI, int axis)
    {
        foreach (int edgeI in Voxel.FaceSurroundingEdges(faceI))
            if (Voxel.EdgeIAxis(edgeI) == axis)
                yield return edgeI;
    }

    private void PushObject(ObjectEntity obj, Vector3Int direction, float scale)
    {
        var newPos = SnapToObjectGrid(obj.position + (Vector3)direction * scale);
        var existingObj = ObjectAt(newPos);
        if (existingObj != null)
            PushObject(existingObj, direction, OBJECT_GRID); // nudge
        MoveObject(obj, newPos);
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
            if (thing is VoxelFaceSelect faceSel)
                return FaceAt(faceSel.loc);
            else if (thing is ObjectMarker marker)
                return marker.objectEntity.paint;
        }
        return new VoxelFace();
    }

    public void PaintSelectedFaces(VoxelFace paint)
    {
        foreach (var faceSel in IterateSelected<VoxelFaceSelect>())
        {
            var voxel = VoxelAt(faceSel.loc.position, true);
            if (paint.material != null || voxel.substance != null)
                voxel.faces[faceSel.loc.faceI].material = paint.material;
            voxel.faces[faceSel.loc.faceI].overlay = paint.overlay;
            voxel.faces[faceSel.loc.faceI].orientation = paint.orientation;
            VoxelModified(faceSel.loc.position);
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
        foreach (var (pos, voxel) in IterateVoxelPairs())
        {
            for (int faceI = 0; faceI < 6; faceI++)
            {
                if (voxel.faces[faceI].material == oldMat)
                    voxel.faces[faceI].material = newMat;
                if (voxel.faces[faceI].overlay == oldMat)
                    voxel.faces[faceI].overlay = newMat;
                VoxelModified(pos);
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
        foreach (var edgeSel in IterateSelected<VoxelEdgeSelect>())
            return EdgeAt(edgeSel.loc);
        return new VoxelEdge();
    }

    public void BevelSelectedEdges(VoxelEdge applyBevel)
    {
        var voxelsToUpdate = new HashSet<Vector3Int>();
        foreach (var edgeSel in IterateSelected<VoxelEdgeSelect>())
        {
            var voxel = VoxelAt(edgeSel.loc.position, false);
            if (voxel == null || voxel.EdgeIsEmpty(edgeSel.loc.edgeI))
                continue;
            voxel.edges[edgeSel.loc.edgeI].bevel = applyBevel.bevel;
            UpdateBevel(edgeSel.loc, voxelsToUpdate, alsoBevelOppositeConcaveEdge: true, dontUpdateThisVoxel: false);
        }
        foreach (var pos in voxelsToUpdate)
            VoxelModified(pos);
    }

    // Call this function after an edge's bevel settings have been set. It will:
    //   - Clear the bevel if the edge is empty or flat
    //   - Update the caps on the edge and adjacent edges
    //   - Add all modified voxels to voxelsToUpdate (except given voxel with dontUpdateThisVoxel=true)
    //   - Optionally repeat all of this for the other "half" of the edge
    private void UpdateBevel(VoxelEdgeLoc edgeLoc, HashSet<Vector3Int> voxelsToUpdate, bool alsoBevelOppositeConcaveEdge,
        bool dontUpdateThisVoxel, bool alsoBevelOppositeFlatEdge = false, EdgeType? type = null)
    {
        Voxel voxel = VoxelAt(edgeLoc.position, true);
        var edge = voxel.edges[edgeLoc.edgeI];
        int minFaceI = Voxel.EdgeIAxis(edgeLoc.edgeI) * 2;
        var minFaceDir = Voxel.IntDirectionForFaceI(minFaceI);
        var minPos = edgeLoc.position + minFaceDir;
        var maxPos = edgeLoc.position - minFaceDir;
        Voxel minVoxel = VoxelAt(minPos, false);
        Voxel maxVoxel = VoxelAt(maxPos, false);

        if (type == null)
            type = GetEdgeType(edgeLoc);

        if (type == EdgeType.EMPTY || type == EdgeType.FLAT)
        {
            voxel.edges[edgeLoc.edgeI] = default;
        }

        // update adjacent edges if they are beveled (for caps)
        // TODO this often won't be necessary
        if (minVoxel != null && minVoxel.edges[edgeLoc.edgeI].hasBevel)
        {
            voxelsToUpdate.Add(minPos);
            if (!minVoxel.EdgeIsConvex(edgeLoc.edgeI))
            {
                // probably concave, unless the bevel hasn't been cleared yet
                var oppMinEdgeLoc = OpposingEdgeLoc(new VoxelEdgeLoc(minPos, edgeLoc.edgeI));
                if (EdgeAt(oppMinEdgeLoc).hasBevel)
                    voxelsToUpdate.Add(oppMinEdgeLoc.position);
            }
        }
        if (maxVoxel != null && maxVoxel.edges[edgeLoc.edgeI].hasBevel)
        {
            voxelsToUpdate.Add(maxPos);
            if (!maxVoxel.EdgeIsConvex(edgeLoc.edgeI))
            {
                // probably concave, unless the bevel hasn't been cleared yet
                var oppMaxEdgeLoc = OpposingEdgeLoc(new VoxelEdgeLoc(maxPos, edgeLoc.edgeI));
                if (EdgeAt(oppMaxEdgeLoc).hasBevel)
                    voxelsToUpdate.Add(oppMaxEdgeLoc.position);
            }
        }

        if (edge.hasBevel && EdgeIsCorner(type.Value))
        {
            // don't allow full convex bevels to overlap with other bevels
            foreach (int unconnectedEdgeI in Voxel.UnconnectedEdges(edgeLoc.edgeI))
            {
                if (voxel.EdgeIsEmpty(unconnectedEdgeI))
                    continue;
                var unconnectedEdgeLoc = new VoxelEdgeLoc(edgeLoc.position, unconnectedEdgeI);
                var unconnectedEdge = EdgeAt(unconnectedEdgeLoc);
                if (unconnectedEdge.hasBevel)
                {
                    if ((edge.bevelSize == VoxelEdge.BevelSize.FULL && type == EdgeType.CONVEX)
                        || (unconnectedEdge.bevelSize == VoxelEdge.BevelSize.FULL)
                            && voxel.EdgeIsConvex(unconnectedEdgeI))
                    {
                        // full convex bevel overlaps with another bevel! this won't work!
                        voxel.edges[unconnectedEdgeI].bevelType = VoxelEdge.BevelType.NONE;
                        UpdateBevel(unconnectedEdgeLoc, voxelsToUpdate, alsoBevelOppositeConcaveEdge: true,
                            dontUpdateThisVoxel: true);
                    }
                }
            }
        }

        if (alsoBevelOppositeConcaveEdge && type == EdgeType.CONCAVE)
        {
            var oppEdgeLoc = OpposingEdgeLoc(edgeLoc);
            bool oppEdgeExists = ChangeEdge(oppEdgeLoc, e => new VoxelEdge(e) { bevel = edge.bevel }, false);
            if (oppEdgeExists)
            {
                UpdateBevel(oppEdgeLoc, voxelsToUpdate, alsoBevelOppositeConcaveEdge: false,
                    dontUpdateThisVoxel: false, type: type);
            }
        }
        else if (alsoBevelOppositeFlatEdge && type == EdgeType.FLAT)
        {
            var oppEdgeLoc = OpposingFlatEdgeLoc(edgeLoc);
            bool oppEdgeExists = ChangeEdge(oppEdgeLoc, e => new VoxelEdge(e) { bevel = edge.bevel }, false);
            if (oppEdgeExists)
            {
                UpdateBevel(oppEdgeLoc, voxelsToUpdate, alsoBevelOppositeConcaveEdge: false,
                    dontUpdateThisVoxel: false, type: type);
            }
        }
        if (!dontUpdateThisVoxel)
            voxelsToUpdate.Add(edgeLoc.position);
    }

    // return non-existing voxel if edge doesn't exist or substances don't match
    private VoxelEdgeLoc OpposingEdgeLoc(VoxelEdgeLoc edgeLoc)
    {
        Voxel.EdgeFaces(edgeLoc.edgeI, out int faceA, out int faceB);
        Vector3Int opposingPos = edgeLoc.position
            + Voxel.IntDirectionForFaceI(faceA) + Voxel.IntDirectionForFaceI(faceB);
        if (SubstanceAt(opposingPos) != SubstanceAt(edgeLoc.position))
            return VoxelEdgeLoc.NONE;
        int opposingEdgeI = Voxel.EdgeIAxis(edgeLoc.edgeI) * 4 + ((edgeLoc.edgeI + 2) % 4);
        return new VoxelEdgeLoc(opposingPos, opposingEdgeI);
    }

    private VoxelEdgeLoc OpposingFlatEdgeLoc(VoxelEdgeLoc edgeLoc)
    {
        // find voxel next to this one
        Voxel.EdgeFaces(edgeLoc.edgeI, out int faceA, out int faceB);
        var voxel = VoxelAt(edgeLoc.position, false);
        int emptyFace = (voxel == null || voxel.faces[faceA].IsEmpty()) ? faceA : faceB;
        int notEmptyFace = emptyFace == faceA ? faceB : faceA;
        var adjacentPos = edgeLoc.position + Voxel.IntDirectionForFaceI(emptyFace);
        Voxel adjacentVoxel = VoxelAt(adjacentPos, false);
        if (adjacentVoxel != null && adjacentVoxel.substance == voxel.substance)
        {
            // find edge on adjacent voxel connected to edgeLoc
            foreach (int otherEdgeI in FaceSurroundingEdgesAlongAxis(notEmptyFace,
                Voxel.EdgeIAxis(edgeLoc.edgeI)))
            {
                if (otherEdgeI != edgeLoc.edgeI)
                    return new VoxelEdgeLoc(adjacentPos, otherEdgeI);
            }
        }
        return VoxelEdgeLoc.NONE;
    }

    public int GetSelectedFaceNormal()
    {
        int faceI = -1;
        foreach (var faceSel in IterateSelected<VoxelFaceSelect>())
        {
            if (faceI == -1)
                faceI = faceSel.loc.faceI;
            else if (faceSel.loc.faceI != faceI)
                return -1;
        }
        return faceI;
    }

    public void PlaceObject(ObjectEntity obj)
    {
        Vector3 createPosition = selectionBounds.center; // not int
        Vector3 createDirection = Vector3.up;
        int faceNormal = GetSelectedFaceNormal();
        if (faceNormal != -1) // same normal for all faces
        {
            createDirection = Voxel.DirectionForFaceI(faceNormal);
            int createAxis = Voxel.FaceIAxis(faceNormal);
            var bounds = obj.PlacementBounds();
            var boundsVec = (faceNormal % 2 == 0) ? bounds.max : bounds.min;
            Vector3 createOffset = Vector3.zero;
            createOffset[createAxis] = boundsVec[createAxis];
            createPosition -= createOffset;
        }

        // don't create the object at the same location of an existing object
        // keep moving in the direction of the face normal until an empty space is found
        while (ObjectAt(createPosition) != null)
            createPosition += createDirection * OBJECT_GRID;
        obj.position = SnapToObjectGrid(createPosition);

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