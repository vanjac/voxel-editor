using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PropertiesGUI : GUIPanel
{
    const float SLIDE_HIDDEN = - GUIPanel.targetHeight * .45f;

    public float slide = 0;
    public VoxelArrayEditor voxelArray;
    public Font titleFont;
    private bool slidingPanel = false;
    private bool adjustingSlider = false;

    private bool guiInit = false;
    private GUIStyle titleStyle;

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
        return new GUIStyle();
    }

    public override void WindowGUI()
    {
        if (!guiInit)
        {
            guiInit = true;
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.font = titleFont;
        }

        if (slidingPanel)
        {
            GUI.enabled = false;
            GUI.color = new Color(1, 1, 1, 2); // reverse disabled tinting
        }

        scroll = GUILayout.BeginScrollView(scroll);

        if (voxelArray.selectionChanged)
            selectedEntities = new List<Entity>(voxelArray.GetSelectedEntities());
        if (selectedEntities.Count == 1)
            EntityPropertiesGUI(selectedEntities[0]);
        else
        {
            EntityReferencePropertyManager.Reset(null);
            if (!voxelArray.SomethingIsSelected())
            {
                GUILayout.BeginVertical(GUI.skin.box);
                PropertiesObjectGUI(voxelArray.world);
                GUILayout.EndVertical();
            }
        }

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
            slidingPanel = true;
            if (Event.current.type == EventType.Repaint) // scroll at correct rate
                slide += touch.deltaPosition.x / scaleFactor;
        }
        else
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (slide > SLIDE_HIDDEN / 2)
                    slide += 30 * scaleFactor;
                else
                    slide -= 30 * scaleFactor;
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

        if (GUILayout.Button("Change Sensor"))
        {
            TypePickerGUI sensorMenu = gameObject.AddComponent<TypePickerGUI>();
            sensorMenu.items = GameScripts.sensors;
            sensorMenu.handler = (System.Type type) =>
            {
                if (type == null)
                    entity.sensor = null;
                else
                {
                    Sensor newSensor = (Sensor)System.Activator.CreateInstance(type);
                    entity.sensor = newSensor;
                }
                voxelArray.unsavedChanges = true;
            };
        }
        GUILayout.BeginVertical(GUI.skin.box);
        PropertiesObjectGUI(entity.sensor, " Sensor");
        GUILayout.EndVertical();

        if (GUILayout.Button("Add Behavior"))
        {
            TypePickerGUI behaviorMenu = gameObject.AddComponent<TypePickerGUI>();
            behaviorMenu.items = GameScripts.behaviors;
            behaviorMenu.handler = (System.Type type) =>
            {
                EntityBehavior newBehavior =
                    (EntityBehavior)System.Activator.CreateInstance(type);
                entity.behaviors.Add(newBehavior);
                voxelArray.unsavedChanges = true;
            };
        }

        EntityBehavior behaviorToRemove = null;
        foreach (EntityBehavior behavior in entity.behaviors)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            PropertiesObjectGUI(behavior, " Behavior");
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
        if (obj == null)
        {
            if (suffix.Length != 0)
                GUILayout.Label("No " + suffix, titleStyle);
            else
                GUILayout.Label("None", titleStyle);
            return;
        }
        var props = obj.Properties();
        if (props.Count == 0)
        {
            GUILayout.Label(obj.TypeName() + suffix, titleStyle);
            return;
        }
        GUILayout.Label(obj.TypeName() + suffix + ":", titleStyle);
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
