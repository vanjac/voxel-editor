using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class StoredPropertiesObject : PropertiesObject
{
    private PropertiesObjectType type;
    private ICollection<Property> properties;

    public StoredPropertiesObject(PropertiesObject store)
    {
        type = store.ObjectType();
        properties = store.Properties();
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

public class PropertiesGUI : GUIPanel
{
    const float SLIDE_HIDDEN = - GUIPanel.targetHeight * .45f;

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
    List<PropertiesObject> editBehaviors = new List<PropertiesObject>();

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
        if (selectedEntities.Count == 1)
        {
            Entity e = null;
            foreach (Entity e1 in selectedEntities)
                e = e1;
            editEntity = new StoredPropertiesObject(e);
            if (e.sensor != null)
                editSensor = new StoredPropertiesObject(e.sensor);
            else
                editSensor = null;
            foreach (EntityBehavior behavior in e.behaviors)
                editBehaviors.Add(new StoredPropertiesObject(behavior));
        }
        else
        {
            editEntity = null;
            editSensor = null;
        }
    }

    private void EntityPropertiesGUI()
    {
        Entity singleSelectedEntity;
        if (selectedEntities.Count == 1)
            singleSelectedEntity = selectedEntities[0];
        if (singleSelectedEntity != null)
            EntityReferencePropertyManager.Reset(singleSelectedEntity);
        else
            ; // TODO: what to do?

        GUILayout.BeginVertical(GUI.skin.box);
        PropertiesObjectGUI(editEntity);
        GUILayout.EndVertical();

        if (singleSelectedEntity != null && singleSelectedEntity is Substance)
        {
            if (!GUILayout.Toggle(true, "Clone", GUI.skin.button))
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

        if (GUILayout.Button("Change Sensor"))
        {
            TypePickerGUI sensorMenu = gameObject.AddComponent<TypePickerGUI>();
            sensorMenu.title = "Change Sensor";
            sensorMenu.items = GameScripts.sensors;
            sensorMenu.handler = (PropertiesObjectType type) =>
            {
                Sensor newSensor = (Sensor)type.Create();
                foreach (Entity entity in selectedEntities)
                    entity.sensor = newSensor;
                voxelArray.unsavedChanges = true;
                UpdateEditEntity();
            };
        }
        GUILayout.BeginVertical(GUI.skin.box);
        PropertiesObjectGUI(editSensor, " Sensor");
        GUILayout.EndVertical();

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
            };
        }

        Color guiBaseColor = GUI.backgroundColor;
        EntityBehavior behaviorToRemove = null;
        foreach (EntityBehavior behavior in editBehaviors) // TODO: editBehaviors doesn't contain EntityBehaviors
        {
            Entity behaviorTarget = behavior.targetEntity.entity;
            string suffix = " Behavior";
            if (behavior.targetEntityIsActivator)
            {
                suffix += "\n▶  Activator";
            }
            else if (behaviorTarget != null)
            {
                EntityReferencePropertyManager.Next(behaviorTarget);
                // behavior target has not been set, so the actual name of the entity will be used
                suffix += "\n▶  " + EntityReferencePropertyManager.GetName();
                GUI.backgroundColor = guiBaseColor * EntityReferencePropertyManager.GetColor();
            }
            EntityReferencePropertyManager.SetBehaviorTarget(behaviorTarget);
            GUILayout.BeginVertical(GUI.skin.box);
            GUI.backgroundColor = guiBaseColor;
            PropertiesObjectGUI(behavior, suffix);
            if (GUILayout.Button("Remove"))
                behaviorToRemove = behavior;
            GUILayout.EndVertical();
            // clear this every time, in case the next target is the same
            EntityReferencePropertyManager.SetBehaviorTarget(null);
        }

        if (behaviorToRemove != null)
        {
            entity.behaviors.Remove(behaviorToRemove);
            voxelArray.unsavedChanges = true;
        }
    }

    private void PropertiesObjectGUI(PropertiesObject obj, string suffix = "")
    {
        string title;
        if (obj == null)
        {
            if (suffix.Length != 0)
                title = "No " + suffix;
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
        if (obj != null)
        {
            GUILayout.Label(obj.ObjectType().icon, GUI.skin.customStyles[2]);
        }
        GUILayout.Label(title, GUI.skin.customStyles[0]);
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
            };
            prop.gui(wrappedProp);
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
            typePicker.items = GameScripts.behaviors;
            return;
        }
        var filteredTypes = new List<BehaviorType>();
        foreach(BehaviorType type in GameScripts.behaviors) {
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
        typePicker.items = filteredTypes.ToArray();
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
            targetButtonText = "Target:  Activator";
        else if (targetEntity != null)
            targetButtonText = "Target:  " + targetEntity.ToString();
        if (!GUILayout.Toggle(true, targetButtonText, GUI.skin.button))
        {
            entityPicker = gameObject.AddComponent<EntityPickerGUI>();
            entityPicker.voxelArray = voxelArray;
            entityPicker.allowNone = true;
            entityPicker.allowMultiple = false;
            entityPicker.activatorOption = true;
            entityPicker.handler = (ICollection<Entity> entities) =>
            {
                entityPicker = null;
                foreach (Entity entity in entities)
                {
                    if (entity == self)
                    {
                        targetEntity = null;
                        targetEntityIsActivator = false;
                    }
                    else if (entity == EntityPickerGUI.ACTIVATOR)
                    {
                        targetEntity = null;
                        targetEntityIsActivator = true;
                    }
                    else
                    {
                        targetEntity = entity;
                        targetEntityIsActivator = false;
                    }
                    UpdateBehaviorList();
                    return;
                }
                targetEntity = null;
                targetEntityIsActivator = false;
                UpdateBehaviorList();
            };
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