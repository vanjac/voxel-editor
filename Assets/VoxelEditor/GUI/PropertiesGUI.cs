using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class StoredPropertiesObject : PropertiesObject
{
    private const string NOT_EQUAL_VALUE = "not equal!!";
    private static readonly PropertiesObjectType DIFFERENT_OBJECT_TYPE
        = new PropertiesObjectType("(different)", null);

    private readonly PropertiesObjectType type;
    private readonly ICollection<Property> properties;

    public StoredPropertiesObject(PropertiesObject store)
    {
        type = store.ObjectType();
        properties = store.Properties();
    }

    // merge properties of objects
    public StoredPropertiesObject(PropertiesObject[] objects)
    {
        properties = new List<Property>();
        if (objects.Length == 0)
            return;
        if (objects[0] != null)
            type = objects[0].ObjectType();
        // check that all objects have the same type. if they don't, fail
        foreach (PropertiesObject obj in objects)
        {
            PropertiesObjectType objType = null;
            if (obj != null)
                objType = obj.ObjectType();
            if (objType != type)
            {
                type = DIFFERENT_OBJECT_TYPE;
                return;
            }
        }
        if (type == null)
            return; // null objects, no properties
        // first index: object
        // second index: property
        List<List<Property>> objectPropertiesSets = new List<List<Property>>();
        foreach (PropertiesObject obj in objects)
            objectPropertiesSets.Add(new List<Property>(obj.Properties()));

        int numObjects = objectPropertiesSets.Count;
        int numProperties = objectPropertiesSets[0].Count;
        for (int propertyI = 0; propertyI < numProperties; propertyI++)
        {
            int _propertyI = propertyI; // for use in lambda functions -- won't change
            Property firstProperty = objectPropertiesSets[0][propertyI];

            GetProperty getter = () =>
            {
                object value = objectPropertiesSets[0][_propertyI].getter();
                for (int objectI = 0; objectI < numObjects; objectI++)
                {
                    if (!(objectPropertiesSets[objectI][_propertyI].getter().Equals(value)))
                        return NOT_EQUAL_VALUE;
                }
                return value;
            };

            SetProperty setter = value =>
            {
                for (int objectI = 0; objectI < numObjects; objectI++)
                    objectPropertiesSets[objectI][_propertyI].setter(value);
            };

            PropertyGUI gui = property =>
            {
                if (property.getter() == (object)NOT_EQUAL_VALUE)
                {
                    GUILayout.BeginHorizontal();
                    PropertyGUIs.AlignedLabel(property);
                    if (GUILayout.Button("different"))
                    {
                        // set all properties to one value
                        property.setter(firstProperty.getter());
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    firstProperty.gui(property);
                }
            };

            properties.Add(new Property(
                firstProperty.name, getter, setter, gui, firstProperty.explicitType));
        }
    }

    public PropertiesObjectType ObjectType()
    {
        return type;
    }

    public ICollection<Property> Properties()
    {
        return properties;
    }
}

class StoredEntityBehavior : StoredPropertiesObject
{
    public readonly EntityBehavior[] allBehaviors;
    // behavior target entity
    // only set if all behaviors share the same target entity
    public readonly Entity target = null;

    public StoredEntityBehavior(EntityBehavior store)
        : base(store)
    {
        allBehaviors = new EntityBehavior[] { store };
        target = store.targetEntity.entity;
    }

    public StoredEntityBehavior(EntityBehavior[] behaviors)
        : base(behaviors)
    {
        allBehaviors = behaviors;
        if (behaviors.Length != 0)
        {
            target = behaviors[0].targetEntity.entity;
            foreach (EntityBehavior behavior in behaviors)
            {
                if (behavior.targetEntity.entity != target)
                {
                    target = null;
                    break;
                }
            }
        }
    }
}

public class PropertiesGUI : GUIPanel
{
    public const float SLIDE_HIDDEN = - GUIPanel.targetHeight * .45f;

    public float slide = SLIDE_HIDDEN;
    public VoxelArrayEditor voxelArray;
    private bool slidingPanel = false;
    private bool adjustingSlider = false;
    public bool normallyOpen = true;
    public bool worldSelected = false;
    public bool freezeUpdates = false;

    List<Entity> selectedEntities = new List<Entity>();
    PropertiesObject editEntity;
    PropertiesObject editSensor;
    List<StoredEntityBehavior> editBehaviors = new List<StoredEntityBehavior>();
    bool mismatchedSelectedBehaviorCounts; // selected entities have different numbers of behaviors

    private static readonly Lazy<GUIStyle> iconStyle = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.label);
        style.padding = new RectOffset(0, 0, 0, 0);
        style.margin = new RectOffset(0, 0, 0, 0);
        return style;
    });

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        base.OnEnable();
    }

    public override Rect GetRect(float width, float height)
    {
        return new Rect(slide, 0, height / 2, height);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void WindowGUI()
    {
        if (slidingPanel && GUI.enabled)
        {
            GUI.enabled = false;
            GUI.color = new Color(1, 1, 1, 2); // reverse disabled tinting
        }

        scroll = GUILayout.BeginScrollView(scroll);

        if (voxelArray.selectionChanged && !freezeUpdates)
        {
            worldSelected = false;
            voxelArray.selectionChanged = false;
            scroll = Vector2.zero;
            scrollVelocity = Vector2.zero;
            selectedEntities = new List<Entity>(voxelArray.GetSelectedEntities());
            UpdateEditEntity();
        }

        bool propertiesDisplayed = false;

        if (worldSelected)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            PropertiesObjectGUI(voxelArray.world);
            GUILayout.EndVertical();
            propertiesDisplayed = true;
            EntityReferencePropertyManager.Reset(null);
        }
        else if (editEntity != null)
        {
            EntityPropertiesGUI();
            propertiesDisplayed = true;
        }
        else
            EntityReferencePropertyManager.Reset(null);

        if (Input.touchCount == 1)
        {
            if (horizontalSlide && (!adjustingSlider) && PanelContainsPoint(touchStartPos))
            {
                slidingPanel = true;
            }
        }
        else
        {
            adjustingSlider = false;
            slidingPanel = false;
        }

        if (slidingPanel)
        {
            Touch touch = Input.GetTouch(0);
            if (Event.current.type == EventType.Repaint) // scroll at correct rate
                slide += touch.deltaPosition.x / scaleFactor;
            normallyOpen = slide > SLIDE_HIDDEN / 2;
        }
        else
        {
            if (Event.current.type == EventType.Repaint)
            {
                bool shouldOpen = normallyOpen && propertiesDisplayed;
                slide += 2000 * Time.deltaTime * (shouldOpen ? 1 : -1);
            }
        }
        if (slide > 0)
            slide = 0;
        if (slide < SLIDE_HIDDEN)
            slide = SLIDE_HIDDEN;

        GUILayout.EndScrollView();
    }

    private void UpdateEditEntity()
    {
        editBehaviors.Clear();
        mismatchedSelectedBehaviorCounts = false;
        if (selectedEntities.Count == 0)
        {
            editEntity = null;
            editSensor = null;
        }
        else if (selectedEntities.Count == 1)
        {
            Entity e = selectedEntities[0];
            editEntity = new StoredPropertiesObject(e);
            if (e.sensor != null)
                editSensor = new StoredPropertiesObject(e.sensor);
            else
                editSensor = null;
            foreach (EntityBehavior behavior in e.behaviors)
                editBehaviors.Add(new StoredEntityBehavior(behavior));
        }
        else
        {
            // types don't match so we need to make a new array
            PropertiesObject[] selectedPropertiesObjects = new PropertiesObject[selectedEntities.Count];
            for (int i = 0; i < selectedEntities.Count; i++)
                selectedPropertiesObjects[i] = selectedEntities[i];
            editEntity = new StoredPropertiesObject(selectedPropertiesObjects);
            PropertiesObject[] selectedSensors = new PropertiesObject[selectedEntities.Count];
            for (int i = 0; i < selectedEntities.Count; i++)
                selectedSensors[i] = selectedEntities[i].sensor;
            editSensor = new StoredPropertiesObject(selectedSensors);
            if (editSensor.ObjectType() == null)
                editSensor = null;

            int numBehaviors = selectedEntities[0].behaviors.Count; // minimum number of behaviors
            foreach (Entity entity in selectedEntities)
            {
                int entityBehaviorCount = entity.behaviors.Count;
                if (entityBehaviorCount != numBehaviors)
                    mismatchedSelectedBehaviorCounts = true;
                if (entityBehaviorCount < numBehaviors)
                    numBehaviors = entityBehaviorCount;
            }

            for (int behaviorI = 0; behaviorI < numBehaviors; behaviorI++)
            {
                EntityBehavior[] selectedBehaviors = new EntityBehavior[selectedEntities.Count];
                for (int entityI = 0; entityI < selectedEntities.Count; entityI++)
                    selectedBehaviors[entityI] = selectedEntities[entityI].behaviors[behaviorI];
                StoredEntityBehavior editBehavior = new StoredEntityBehavior(selectedBehaviors);
                editBehaviors.Add(editBehavior);
            }
        }
    }

    private void EntityPropertiesGUI()
    {
        Entity singleSelectedEntity = null;
        if (selectedEntities.Count == 1)
            singleSelectedEntity = selectedEntities[0];

        EntityReferencePropertyManager.Reset(singleSelectedEntity); // could be null and that's fine (?)

        GUILayout.BeginVertical(GUI.skin.box);
        PropertiesObjectGUI(editEntity);
        GUILayout.EndVertical();

        if (singleSelectedEntity != null && !(singleSelectedEntity is PlayerObject))
        {
            GUILayout.BeginHorizontal();
            if (GUIUtils.HighlightedButton("Clone"))
            {
                if (singleSelectedEntity is ObjectEntity)
                {
                    ObjectEntity clone = (ObjectEntity)(singleSelectedEntity.Clone());
                    var pickerGUI = gameObject.AddComponent<FacePickerGUI>();
                    pickerGUI.voxelArray = voxelArray;
                    pickerGUI.message = "Tap to place clone";
                    pickerGUI.pickAction = () =>
                    {
                        voxelArray.PlaceObject(clone);
                    };
                }
                else if (singleSelectedEntity is Substance)
                {
                    Substance clone = (Substance)(singleSelectedEntity.Clone());
                    clone.defaultPaint = voxelArray.GetSelectedPaint();
                    clone.defaultPaint.addSelected = false;
                    clone.defaultPaint.storedSelected = false;
                    voxelArray.substanceToCreate = clone;
                    var createGUI = gameObject.AddComponent<CreateSubstanceGUI>();
                    createGUI.voxelArray = voxelArray;
                }
            }
            if (GUIUtils.HighlightedButton("Delete"))
            {
                DeleteButton();
            }
            GUILayout.EndHorizontal();
        }
        if (selectedEntities.Count > 1)
        {
            if (GUIUtils.HighlightedButton("Delete"))
            {
                DeleteButton();
            }
        }

        TutorialGUI.TutorialHighlight("change sensor");
        if (GUILayout.Button("Change Sensor"))
        {
            TypePickerGUI sensorMenu = gameObject.AddComponent<TypePickerGUI>();
            sensorMenu.title = "Change Sensor";
            sensorMenu.categories = new PropertiesObjectType[][] { GameScripts.sensors };
            sensorMenu.handler = (PropertiesObjectType type) =>
            {
                Sensor newSensor = (Sensor)type.Create();
                foreach (Entity entity in selectedEntities)
                    entity.sensor = newSensor;
                voxelArray.unsavedChanges = true;
                UpdateEditEntity();
            };
        }
        TutorialGUI.ClearHighlight();
        GUILayout.BeginVertical(GUI.skin.box);
        PropertiesObjectGUI(editSensor, " Sensor");
        GUILayout.EndVertical();

        TutorialGUI.TutorialHighlight("add behavior");
        if (GUILayout.Button("Add Behavior"))
        {
            NewBehaviorGUI behaviorMenu = gameObject.AddComponent<NewBehaviorGUI>();
            behaviorMenu.title = "Add Behavior";
            behaviorMenu.self = singleSelectedEntity;
            behaviorMenu.voxelArray = voxelArray;
            behaviorMenu.handler = (EntityBehavior newBehavior) =>
            {
                foreach (Entity entity in selectedEntities)
                {
                    if (!newBehavior.BehaviorObjectType().rule(entity))
                        continue;
                    entity.behaviors.Add(newBehavior);
                }
                voxelArray.unsavedChanges = true;
                UpdateEditEntity();
                scrollVelocity = new Vector2(0, 2000 * editBehaviors.Count); // scroll to bottom
                EntityPreviewManager.BehaviorUpdated(singleSelectedEntity, newBehavior);
            };
        }
        TutorialGUI.ClearHighlight();

        Color guiBaseColor = GUI.backgroundColor;
        StoredEntityBehavior behaviorToRemove = null;
        foreach (StoredEntityBehavior storedBehavior in editBehaviors)
        {
            TutorialGUI.TutorialHighlight("behaviors");
            if (storedBehavior.target != null)
            {
                EntityReferencePropertyManager.Next(storedBehavior.target);
                GUI.backgroundColor = guiBaseColor * EntityReferencePropertyManager.GetColor();
            }
            EntityReferencePropertyManager.SetBehaviorTarget(storedBehavior.target);
            GUILayout.BeginVertical(GUI.skin.box);
            GUI.backgroundColor = guiBaseColor;
            PropertiesObjectGUI(storedBehavior, " Behavior",
                () => EntityPreviewManager.BehaviorUpdated(singleSelectedEntity,
                    storedBehavior.allBehaviors[0]));
            if (GUILayout.Button("Remove"))
                behaviorToRemove = storedBehavior;
            GUILayout.EndVertical();
            // clear this every time, in case the next target is the same
            EntityReferencePropertyManager.SetBehaviorTarget(null);
        }

        if (behaviorToRemove != null)
        {
            foreach (Entity entity in selectedEntities)
            {
                foreach (EntityBehavior remove in behaviorToRemove.allBehaviors)
                {
                    if (entity.behaviors.Remove(remove))
                        break;
                }
            }
            voxelArray.unsavedChanges = true;
            UpdateEditEntity();
            EntityPreviewManager.BehaviorUpdated(singleSelectedEntity, behaviorToRemove.allBehaviors[0]);
        }

        if (mismatchedSelectedBehaviorCounts)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("(other behaviors...)", GUI.skin.GetStyle("label_title"));
            GUILayout.EndVertical();
        }
    }

    private void PropertiesObjectGUI(PropertiesObject obj, string suffix = "",
        System.Action changedAction = null)
    {
        string title;
        if (obj == null)
        {
            if (suffix.Length != 0)
                title = "No" + suffix;
            else
                title = "None";
        }
        else
        {
            title = obj.ObjectType().fullName + suffix;
            if (obj.Properties().Count > 0)
                title += ":";
        }
        GUILayout.BeginHorizontal();
        if (obj != null && GUILayout.Button(obj.ObjectType().icon, iconStyle.Value))
        {
            var typeInfo = gameObject.AddComponent<TypeInfoGUI>();
            typeInfo.type = obj.ObjectType();
        }
        GUILayout.Label(title, GUI.skin.GetStyle("label_title"));
        GUILayout.EndHorizontal();

        if (obj == null)
            return;
        var props = obj.Properties();
        foreach (Property prop in props)
        {
            Property wrappedProp = prop;
            wrappedProp.setter = v =>
            {
                prop.setter(v);
                voxelArray.unsavedChanges = true;
                adjustingSlider = true;
                if (changedAction != null)
                    changedAction();
            };
            prop.gui(wrappedProp);
        }
    }


    private void DeleteButton()
    {
        // TODO: only deselect deleted objects
        voxelArray.ClearSelection();
        voxelArray.ClearStoredSelection();
        foreach (Entity entity in selectedEntities)
        {
            if (entity is PlayerObject)
                continue;
            else if (entity is ObjectEntity)
            {
                voxelArray.DeleteObject((ObjectEntity)entity);
                voxelArray.unsavedChanges = true;
            }
            else if (entity is Substance)
                voxelArray.DeleteSubstance((Substance)entity);
        }
    }
}


public class NewBehaviorGUI : GUIPanel
{
    public delegate void BehaviorHandler(EntityBehavior behavior);
    public BehaviorHandler handler;
    public Entity self;
    public VoxelArrayEditor voxelArray;

    private TypePickerGUI typePicker;
    private EntityPickerGUI entityPicker;
    private Entity targetEntity;
    private bool targetEntityIsActivator = false;

    public override Rect GetRect(float width, float height)
    {
        if (entityPicker != null)
            // move panel offscreen
            return new Rect(width, height, width * .5f, height * .8f);
        else
            return new Rect(width * .25f, height * .1f, width * .5f, height * .8f);
    }

    void Start()
    {
        typePicker = gameObject.AddComponent<TypePickerGUI>();
        typePicker.categoryNames = GameScripts.behaviorTabNames;
        UpdateBehaviorList();
        typePicker.handler = (PropertiesObjectType type) =>
        {
            EntityBehavior behavior = (EntityBehavior)type.Create();
            if (targetEntityIsActivator)
                behavior.targetEntityIsActivator = true;
            else if (targetEntity != null)
                behavior.targetEntity = new EntityReference(targetEntity);
            handler(behavior);
            Destroy(this);
        };
        typePicker.enabled = false;
    }

    private void UpdateBehaviorList()
    {
        if (targetEntityIsActivator)
        {
            // all behaviors
            typePicker.categories = GameScripts.behaviorTabs;
            return;
        }
        typePicker.categories = new PropertiesObjectType[GameScripts.behaviorTabs.Length][];
        for (int tabI = 0; tabI < GameScripts.behaviorTabs.Length; tabI++)
        {
            var filteredTypes = new List<BehaviorType>();
            foreach (BehaviorType type in GameScripts.behaviorTabs[tabI])
            {
                if (targetEntity == null)
                {
                    if (self == null || type.rule(self))
                        filteredTypes.Add(type);
                }
                else
                {
                    if (type.rule(targetEntity))
                        filteredTypes.Add(type);
                }
            }
            typePicker.categories[tabI] = filteredTypes.ToArray();
        }
    }

    void OnDestroy()
    {
        if (typePicker != null)
            Destroy(typePicker);
    }

    public override void WindowGUI()
    {
        string targetButtonText = "Target:  Self";
        if (targetEntityIsActivator)
            targetButtonText = "Target:  Activators";
        else if (targetEntity != null)
            targetButtonText = "Target:  " + targetEntity.ToString();
        if (GUIUtils.HighlightedButton(targetButtonText))
        {
            entityPicker = PropertyGUIs.BehaviorTargetPicker(gameObject, voxelArray, self, value =>
            {
                targetEntity = value.targetEntity.entity;
                targetEntityIsActivator = value.targetEntityIsActivator;
                UpdateBehaviorList();
            });
        }
        if (typePicker != null)
        {
            typePicker.scroll = scroll;
            typePicker.WindowGUI();
            scroll = typePicker.scroll;
        }

        // prevent panel from closing when entity picker closes
        holdOpen = entityPicker != null;
    }
}
