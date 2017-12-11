using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PropertiesGUI : GUIPanel {

    const float SLIDE_HIDDEN = - GUIPanel.targetHeight * .45f;

    float slide = 0;
    public VoxelArrayEditor voxelArray;
    private bool slidingPanel = false;
    private bool adjustingSlider = false;

    List<List<Property>> selectedEntityProperties = new List<List<Property>>();

    public override void OnGUI()
    {
        base.OnGUI();

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
            UpdateSelectedEntityProperties();
        }

        foreach (List<Property> propList in selectedEntityProperties)
        {
            object commonValue = propList[0].getter();
            foreach (Property prop in propList)
            {
                if (!prop.getter().Equals(commonValue))
                {
                    commonValue = null;
                    break;
                }
            }

            if(commonValue == null)
                GUILayout.Label(propList[0].name + " (different)");
            else
            {
                GUILayout.Label(propList[0].name);
                object newValue = propList[0].gui(commonValue);
                if (!newValue.Equals(commonValue))
                {
                    foreach (Property prop in propList)
                        prop.setter(newValue);
                    voxelArray.unsavedChanges = true;
                }
            }
        }

        GUILayout.Label("Outputs");
        if (GUILayout.Button("New Output"))
        {
            EntityPickerGUI picker = gameObject.AddComponent<EntityPickerGUI>();
            picker.voxelArray = voxelArray;
            picker.handler = (ICollection<Entity> c) => Debug.Log(c);
        }
    }

    private void MapPropertiesGUI()
    {
        if (GUILayout.Button("Set Sky"))
        {
            MaterialSelectorGUI materialSelector = gameObject.AddComponent<MaterialSelectorGUI>();
            materialSelector.voxelArray = voxelArray;
            materialSelector.materialDirectory = "GameAssets/Skies";
            materialSelector.handler = SetSkybox;
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
            colorPicker.handler = SetSunColor;
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

    private void SetSkybox(Material sky)
    {
        RenderSettings.skybox = sky;
        voxelArray.unsavedChanges = true;
    }

    private void SetSunColor(Color color)
    {
        RenderSettings.sun.color = color;
        voxelArray.unsavedChanges = true;
    }

    private void UpdateSelectedEntityProperties()
    {
        selectedEntityProperties.Clear();
        ICollection<Entity> entities = voxelArray.GetSelectedEntities();
        if (entities.Count != 0)
        {
            // get the first entity (probably a simpler way to do this)
            var entitiesEnumerator = entities.GetEnumerator();
            entitiesEnumerator.MoveNext(); // moves to the first entity
            Entity firstEntity = entitiesEnumerator.Current;
            entitiesEnumerator.Reset();

            // find properties that are common to all selected entities
            // by searching through the properties of the first one.
            // probably a simpler way to this also
            foreach (Property prop in firstEntity.Properties())
            {
                var identicalProperties = new List<Property>();
                foreach (Entity otherEntity in entities)
                {
                    bool otherEntityHasProp = false;
                    foreach (Property otherProp in otherEntity.Properties())
                        if (otherProp.name == prop.name)
                        {
                            otherEntityHasProp = true;
                            identicalProperties.Add(otherProp);
                            break;
                        }
                    if (!otherEntityHasProp)
                    {
                        identicalProperties = null;
                        break;
                    }
                }
                if (identicalProperties == null)
                    continue;
                selectedEntityProperties.Add(identicalProperties);
            }
        }
    }

}
