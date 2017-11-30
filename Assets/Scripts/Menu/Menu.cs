using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour {
    public Dropdown mapSelect;
    public InputField mapNameInput;
    public TextAsset defaultMap;

    public void Start()
    {
        UpdateMapList();
    }

    public void OpenEditor()
    {
        GameObject selectedMap = GameObject.Find("SelectedMap");
        if (selectedMap == null)
        {
            selectedMap = new GameObject("SelectedMap");
            selectedMap.AddComponent<SelectedMap>();
        }
        selectedMap.GetComponent<SelectedMap>().mapName = mapSelect.options[mapSelect.value].text;
        SceneManager.LoadScene("editScene");
    }

    public void NewMap()
    {
        string filePath = GetMapPath(mapNameInput.text);
        using (FileStream fileStream = File.Create(filePath))
        {
            using (var sw = new StreamWriter(fileStream))
            {
                sw.Write(defaultMap.text);
                sw.Flush();
            }
        }
        UpdateMapList();
    }

    public void DeleteMap()
    {
        File.Delete(GetMapPath(mapSelect.options[mapSelect.value].text));
        UpdateMapList();
    }

    private string GetMapPath(string name)
    {
        return Application.persistentDataPath + "/" + name + ".json";
    }

    void UpdateMapList()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath);
        var names = new List<string>();
        foreach (string name in files)
        {
            names.Add(Path.GetFileNameWithoutExtension(name));
        }
        mapSelect.ClearOptions();
        mapSelect.AddOptions(names);
    }
}
