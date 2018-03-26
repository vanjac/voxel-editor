using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PropertiesGUI : GUIPanel
{
    const float SLIDE_HIDDEN = - GUIPanel.targetHeight * .45f;

    public float slide = SLIDE_HIDDEN;
    public VoxelArrayEditor voxelArray;
    private bool slidingPanel = false;
    private bool adjustingSlider = false;
    public bool normallyOpen = true;
    public bool worldSelected = false;

    List<Entity> selectedEntities = new List<Entity>();

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

        if (voxelArray.selectionChanged)
        {
            selectedEntities = new List<Entity>(voxelArray.GetSelectedEntities());
            worldSelected = false;
            voxelArray.selectionChanged = false;
            scroll = Vector2.zero;
            scrollVelocity = Vector2.zero;
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
        else if (selectedEntities.Count == 1)
        {
            EntityPropertiesGUI(selectedEntities[0]);
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

    private void EntityPropertiesGUI(Entity entity)
    {
        EntityReferencePropertyManager.Reset(entity);

        GUILayout.BeginVertical(GUI.skin.box);
        PropertiesObjectGUI(entity);
        GUILayout.EndVertical();

        if (entity is Substance)
        {
            if (!GUILayout.Toggle(true, "Clone", GUI.skin.button))
            {
                Substance clone = (Substance)(entity.Clone());
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
                entity.sensor = (Sensor)type.Create();
                voxelArray.unsavedChanges = true;
            };
        }
        GUILayout.BeginVertical(GUI.skin.box);
        PropertiesObjectGUI(entity.sensor, " Sensor");
        GUILayout.EndVertical();

        if (GUILayout.Button("Add Behavior"))
        {
            NewBehaviorGUI behaviorMenu = gameObject.AddComponent<NewBehaviorGUI>();
            behaviorMenu.title = "Add Behavior";
            behaviorMenu.voxelArray = voxelArray;
            behaviorMenu.handler = (EntityBehavior newBehavior) =>
            {
                entity.behaviors.Add(newBehavior);
                voxelArray.unsavedChanges = true;
                scrollVelocity = new Vector2(0, 2000 * entity.behaviors.Count); // scroll to bottom
            };
        }

        Color guiBaseColor = GUI.backgroundColor;
        EntityBehavior behaviorToRemove = null;
        foreach (EntityBehavior behavior in entity.behaviors)
        {
            string suffix = " Behavior";
            if (behavior.targetEntity.entity != null)
            {
                EntityReferencePropertyManager.Next(behavior.targetEntity.entity);
                suffix += "\n▶  " + EntityReferencePropertyManager.GetName();
                GUI.backgroundColor = guiBaseColor * EntityReferencePropertyManager.GetColor();
            }
            GUILayout.BeginVertical(GUI.skin.box);
            GUI.backgroundColor = guiBaseColor;
            PropertiesObjectGUI(behavior, suffix);
            if (GUILayout.Button("Remove"))
                behaviorToRemove = behavior;
            GUILayout.EndVertical();
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
    public VoxelArrayEditor voxelArray;

    private TypePickerGUI typePicker;
    private EntityPickerGUI entityPicker;
    private Entity targetEntity;

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
        typePicker.items = GameScripts.behaviors;
        typePicker.handler = (PropertiesObjectType type) =>
        {
            EntityBehavior behavior = (EntityBehavior)type.Create();
            if (targetEntity != null)
                behavior.targetEntity = new EntityReference(targetEntity);
            handler(behavior);
            Destroy(this);
        };
        typePicker.enabled = false;
    }

    void OnDestroy()
    {
        if (typePicker != null)
            Destroy(typePicker);
    }

    public override void WindowGUI()
    {
        string targetButtonText = "Target...";
        if (targetEntity != null)
            targetButtonText = "Target:  " + targetEntity.ToString();
        if (!GUILayout.Toggle(true, targetButtonText, GUI.skin.button))
        {
            entityPicker = gameObject.AddComponent<EntityPickerGUI>();
            entityPicker.voxelArray = voxelArray;
            entityPicker.allowNone = true;
            entityPicker.allowMultiple = false;
            entityPicker.handler = (ICollection<Entity> entities) =>
            {
                entityPicker = null;
                foreach (Entity entity in entities)
                {
                    targetEntity = entity;
                    return;
                }
                targetEntity = null;
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