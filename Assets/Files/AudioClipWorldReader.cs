using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// Only supports loading from FileStreams!
public class AudioClipWorldReader : WorldFileReader
{
    private EmbeddedData data = new EmbeddedData();
    private UnityEngine.AudioType audioType;

    public AudioClipWorldReader(UnityEngine.AudioType audioType)
    {
        this.audioType = audioType;
    }

    public void ReadStream(Stream stream)
    {
        FileStream fs = stream as FileStream;
        if (fs == null)
            return;
        string path = fs.Name;
        fs.Close();
        Debug.Log("Loading audio from " + path);

        // TODO file type
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, audioType);
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        var asyncOp = www.SendWebRequest();
        // TODO this seems like a very bad idea
        while (!asyncOp.isDone)
            System.Threading.Thread.Sleep(10);
        stopwatch.Stop();
        Debug.Log("Loading audio took " + stopwatch.ElapsedMilliseconds);
        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
        if (clip == null)
        {
            Debug.LogError("Couldn't get audio clip");
            return;
        }
        if (clip.samples == 0)
        {
            Debug.LogError("Audio data is empty");
            return;
        }

        byte[] bytes = AudioCompression.Compress(clip);
        data = new EmbeddedData(Path.GetFileNameWithoutExtension(path), bytes, EmbeddedDataType.Audio);
    }

    public List<string> BuildWorld(Transform cameraPivot, VoxelArray voxelArray, bool editor)
    {
        var warnings = ReadWorldFile.Read(Resources.Load<TextAsset>("default"),
            cameraPivot, voxelArray, editor);
        foreach (var obj in voxelArray.IterateObjects())
        {
            if (obj is PlayerObject)
            {
                var behavior = new SoundBehavior();
                PropertiesObjectType.SetProperty(behavior, "dat", data);
                obj.behaviors.Add(behavior);
                break;
            }
        }
        return warnings;
    }

    public List<EmbeddedData> FindEmbeddedData(EmbeddedDataType type)
    {
        List<EmbeddedData> dataList = new List<EmbeddedData>();
        if (type == EmbeddedDataType.Audio)
            dataList.Add(data);
        return dataList;
    }
}