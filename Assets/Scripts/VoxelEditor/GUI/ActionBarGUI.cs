using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ActionBarGUI : GUIPanel
{
    public VoxelArray voxelArray;

    public override void OnGUI()
    {
        base.OnGUI();

        panelRect = new Rect(190, 10, scaledScreenWidth - 190, 20);

        if (GUI.Button(new Rect(panelRect.xMin, panelRect.yMin, 80, 20), "Reset")) {
            Material[] testMaterials = new Material[]
            {
                Resources.Load<Material>("GameAssets/Materials/colors/Red"),
                Resources.Load<Material>("GameAssets/Materials/colors/Yellow"),
                Resources.Load<Material>("GameAssets/Materials/colors/Green"),
                Resources.Load<Material>("GameAssets/Materials/colors/Cyan"),
                Resources.Load<Material>("GameAssets/Materials/colors/Blue"),
                Resources.Load<Material>("GameAssets/Materials/colors/Magenta")
            };

            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    for (int z = -2; z <= 2; z++)
                    {
                        Voxel newVoxel = voxelArray.VoxelAt(new Vector3(x, y, z), true);
                        if (x == -2)
                            newVoxel.faces[0].material = testMaterials[0];
                        if (x == 2)
                            newVoxel.faces[1].material = testMaterials[1];
                        if (y == -2)
                            newVoxel.faces[2].material = testMaterials[2];
                        if (y == 2)
                            newVoxel.faces[3].material = testMaterials[3];
                        if (z == -2)
                            newVoxel.faces[4].material = testMaterials[4];
                        if (z == 2)
                            newVoxel.faces[5].material = testMaterials[5];
                        voxelArray.VoxelModified(newVoxel);
                    }
                }
            }
        }

        if (GUI.Button(new Rect(panelRect.xMin + 180, panelRect.yMin, 80, 20), "Play"))
        {
            SceneManager.LoadScene("playScene");
        }
    }
}
