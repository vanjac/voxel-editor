using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class AudioClipWorldReader : WorldFileReader
{
    private EmbeddedData data;
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
        var asyncOp = www.SendWebRequest();
        // TODO this seems like a very bad idea
        while (!asyncOp.isDone)
            System.Threading.Thread.Sleep(10);
        Debug.Log("done!");
        AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
        float[] samples = new float[audioClip.samples * audioClip.channels];
        audioClip.LoadAudioData();
        audioClip.GetData(samples, 0);
        byte[] bytes = new byte[samples.Length * 4];
        System.Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);
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