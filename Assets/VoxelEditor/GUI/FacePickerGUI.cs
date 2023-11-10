using UnityEngine;

public class FacePickerGUI : ActionBarGUI
{
    public string message;
    public System.Action pickAction;
    public bool onlyFaces = false;
    public bool clearStoredSelection = true;

    public override void OnEnable()
    {
        // copied from CreateSubstanceGUI
        base.OnEnable();
        stealFocus = true;
        GetComponent<PropertiesGUI>().normallyOpen = false; // hide properties panel
    }

    public override void OnDisable()
    {
        // copied from CreateSubstanceGUI
        base.OnDisable();
        GetComponent<PropertiesGUI>().normallyOpen = true; // show properties panel
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        if (ActionBarButton(GUIIconSet.instance.close))
            Destroy(this);
        GUILayout.FlexibleSpace();
        ActionBarLabel(message);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    public override void Start()
    {
        base.Start();
        voxelArray.ClearSelection();
        if (clearStoredSelection)
            voxelArray.ClearStoredSelection();
    }

    void Update()
    {
        if (voxelArray.SomethingIsAddSelected())
        {
            if (onlyFaces && !voxelArray.FacesAreAddSelected())
            {
                voxelArray.ClearSelection();
            }
            else
            {
                pickAction();
                Destroy(this);
            }
        }
    }
}