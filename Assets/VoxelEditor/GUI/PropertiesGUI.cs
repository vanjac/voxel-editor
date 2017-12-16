using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PropertiesGUI : GUIPanel {

    const float SLIDE_HIDDEN = - GUIPanel.targetHeight * .45f;

    float slide = 0;
    public VoxelArrayEditor voxelArray;
    public Font titleFont;
    private bool slidingPanel = false;
    private bool adjustingSlider = false;

    private bool guiInit = false;
    private GUIStyle titleStyle;

    List<Entity> selectedEntities;

    public override void OnGUI()
    {
        base.OnGUI();

        if (!guiInit)
        {
            guiInit = true;
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.font = titleFont;
        }

        panelRect = new Rect(slide, 0, targetHeight / 2, targetHeight);
        GUILayout.BeginArea(panelRect, GUI.skin.box);

        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        GUILayout.Label(voxelArray.SomethingIsSelected() ? "Properties" : "Map Properties");
        GUI.skin.label.alignment = TextAnchor.UpperLeft;

        if (slidingPanel)
        {
            GUI.enabled = false;
            GUI.color = new Color(1, 1, 1, 2); // reverse disabled tinting
        }

        scroll = GUILayout.BeginScrollView(scroll);

        if (voxelArray.SomethingIsSelected())
            SelectionPropertiesGUI();
        else
            MapPropertiesGUI();

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
        GUILayout.EndArea();
    }

    private void SelectionPropertiesGUI()
    {
        if (GUILayout.Button("Set Material"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.voxelArray = voxelArray;
            materialSelector.allowNullMaterial = true; // TODO: disable if no substances selected
            materialSelector.handler = voxelArray.AssignMaterial;
        }

        if (GUILayout.Button("Set Overlay"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.voxelArray = voxelArray;
            materialSelector.materialDirectory = "GameAssets/Overlays";
            materialSelector.allowNullMaterial = true;
            materialSelector.handler = voxelArray.AssignOverlay;
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Left"))
        {
            voxelArray.OrientFaces(3);
        }

        if (GUILayout.Button("Right"))
        {
            voxelArray.OrientFaces(1);
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Flip H"))
        {
            voxelArray.OrientFaces(7);
        }

        if (GUILayout.Button("Flip V"))
        {
            voxelArray.OrientFaces(5);
        }

        GUILayout.EndHorizontal();

        if (voxelArray.selectionChanged)
        {
            selectedEntities = new List<Entity>(voxelArray.GetSelectedEntities());
        }

        if (selectedEntities.Count == 1)
            EntityPropertiesGUI(selectedEntities[0]);
    }

    private void EntityPropertiesGUI(Entity entity)
    {
        PropertiesObjectGUI(entity);

        GUILayout.Label("Behaviors");
        if (GUILayout.Button("Add Behavior"))
        {
            SimpleMenuGUI behaviorMenu = gameObject.AddComponent<SimpleMenuGUI>();
            var behaviorNames = new List<string>();
            foreach (var behaviorType in BuiltInEntities.behaviors)
            {
                behaviorNames.Add(behaviorType.ToString());
            }
            behaviorMenu.items = behaviorNames.ToArray();
            behaviorMenu.handler = (int itemI, string itemName) =>
            {
                var selectedBehaviorType = BuiltInEntities.behaviors[itemI];
                EntityBehavior newBehavior =
                    (EntityBehavior)System.Activator.CreateInstance(selectedBehaviorType);
                entity.behaviors.Add(newBehavior);
            };
        }
        foreach (EntityBehavior behavior in entity.behaviors)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            PropertiesObjectGUI(behavior);
            GUILayout.EndVertical();
        }
    }

    private void PropertiesObjectGUI(PropertiesObject obj)
    {
        GUILayout.Label(obj.TypeName() + ":", titleStyle);
        foreach (Property prop in obj.Properties())
        {
            GUILayout.Label(prop.name);
            object oldValue = prop.getter();
            object newValue = prop.gui(oldValue);
            if (!newValue.Equals(oldValue))
            {
                prop.setter(newValue);
                voxelArray.unsavedChanges = true;
            }
        }
    }

    private void MapPropertiesGUI()
    {
        if (GUILayout.Button("Set Sky"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.voxelArray = voxelArray;
            materialSelector.materialDirectory = "GameAssets/Skies";
            materialSelector.handler = (Material sky) =>
            {
                RenderSettings.skybox = sky;
                voxelArray.unsavedChanges = true;
            };
        }

        GUILayout.Label("Ambient light intensity:");

        float oldValue = RenderSettings.ambientIntensity;
        float newValue = GUILayout.HorizontalSlider(oldValue, 0, 3);
        if (newValue != oldValue)
        {
            RenderSettings.ambientIntensity = newValue;
            voxelArray.unsavedChanges = true;
            adjustingSlider = true;
        }

        GUILayout.Label("Sun intensity:");

        oldValue = RenderSettings.sun.intensity;
        newValue = GUILayout.HorizontalSlider(oldValue, 0, 3);
        if (newValue != oldValue)
        {
            RenderSettings.sun.intensity = newValue;
            voxelArray.unsavedChanges = true;
            adjustingSlider = true;
        }

        if (GUILayout.Button("Sun Color"))
        {
            ColorPickerGUI colorPicker = gameObject.AddComponent<ColorPickerGUI>();
            colorPicker.color = RenderSettings.sun.color;
            colorPicker.handler = (Color color) =>
            {
                RenderSettings.sun.color = color;
                voxelArray.unsavedChanges = true;
            };
        }

        GUILayout.Label("Sun Pitch:");

        oldValue = RenderSettings.sun.transform.rotation.eulerAngles.x;
        if (oldValue > 270)
            oldValue -= 360;
        newValue = GUILayout.HorizontalSlider(oldValue, -90, 90);
        if (newValue != oldValue)
        {
            Vector3 eulerAngles = RenderSettings.sun.transform.rotation.eulerAngles;
            eulerAngles.x = newValue;
            RenderSettings.sun.transform.rotation = Quaternion.Euler(eulerAngles);
            voxelArray.unsavedChanges = true;
            adjustingSlider = true;
        }

        GUILayout.Label("Sun Yaw:");

        oldValue = RenderSettings.sun.transform.rotation.eulerAngles.y;
        newValue = GUILayout.HorizontalSlider(oldValue, 0, 360);
        if (newValue != oldValue)
        {
            Vector3 eulerAngles = RenderSettings.sun.transform.rotation.eulerAngles;
            eulerAngles.y = newValue;
            RenderSettings.sun.transform.rotation = Quaternion.Euler(eulerAngles);
            voxelArray.unsavedChanges = true;
            adjustingSlider = true;
        }
    }

}
