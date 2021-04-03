using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapReadException : Exception
{
    private const string MESSAGE_APPEND = "\n\nPlease email me about this!\nvantjac.dev@gmail.com";
    public MapReadException() { }
    public MapReadException(string message) : base(message + MESSAGE_APPEND) { }
    public MapReadException(string message, Exception inner) : base(message + MESSAGE_APPEND, inner) { }
}

public class InvalidMapFileException : MapReadException
{
    public InvalidMapFileException() : base("Not a recognized file format") { }
    public InvalidMapFileException(Exception inner) : base("Not a recognized file format", inner) { }
}

public interface WorldFileReader
{
    void ReadStream(Stream fileStream);
    // return warnings
    List<string> BuildWorld(Transform cameraPivot, VoxelArray voxelArray, bool editor);
    List<EmbeddedData> FindEmbeddedData(EmbeddedDataType type);
    List<Material> FindCustomTextures(bool overlay);
}

public static class ReadWorldFile
{
    private static Material missingMaterial, missingOverlay;

    // return warnings. disposes stream when done!
    public static List<string> Read(Stream stream, Transform cameraPivot, VoxelArray voxelArray, bool editor)
    {
        WorldFileReader reader;
        using (stream)
            reader = ReadStream(stream);
        return BuildWorld(reader, cameraPivot, voxelArray, editor);
    }

    public static List<string> Read(TextAsset asset, Transform cameraPivot, VoxelArray voxelArray, bool editor)
    {
        return Read(new MemoryStream(asset.bytes), cameraPivot, voxelArray, editor);
    }

    public static List<EmbeddedData> ReadEmbeddedData(string filePath, EmbeddedDataType type)
    {
        WorldFileReader reader;
        try
        {
            using (FileStream stream = File.Open(filePath, FileMode.Open))
                reader = ReadStream(stream);
        }
        catch (System.Exception e)
        {
            throw new MapReadException("Error opening file", e);
        }
        return reader.FindEmbeddedData(type);
    }

    public static List<EmbeddedData> ReadEmbeddedData(Stream stream, EmbeddedDataType type)
    {
        WorldFileReader reader = ReadStream(stream);
        return reader.FindEmbeddedData(type);
    }

    public static List<Material> ReadCustomTextures(string filePath, bool overlay)
    {
        // TODO: copied from ReadEmbeddedData
        WorldFileReader reader;
        try
        {
            using (FileStream stream = File.Open(filePath, FileMode.Open))
                reader = ReadStream(stream);
        }
        catch (System.Exception e)
        {
            throw new MapReadException("Error opening file", e);
        }
        return reader.FindCustomTextures(overlay);
    }

    private static WorldFileReader ReadStream(Stream stream)
    {
        try
        {
            WorldFileReader reader = GetReaderForStream(stream);
            reader.ReadStream(stream);
            return reader;
        }
        catch (MapReadException e)
        {
            throw e;
        }
        catch (Exception e)
        {
            throw new MapReadException("An error occurred while reading the file", e);
        }
    }

    private static List<string> BuildWorld(WorldFileReader reader,
        Transform cameraPivot, VoxelArray voxelArray, bool editor)
    {
        try
        {
            return reader.BuildWorld(cameraPivot, voxelArray, editor);
        }
        catch (MapReadException e)
        {
            throw e;
        }
        catch (Exception e)
        {
            throw new MapReadException("An error occurred while reading the file", e);
        }
    }

    public static Material MissingMaterial(bool overlay)
    {
        if (missingMaterial == null)
        {
            missingMaterial = ResourcesDirectory.InstantiateMaterial(
                ResourcesDirectory.FindMaterial("MISSING", true));
            missingOverlay = ResourcesDirectory.InstantiateMaterial(
                ResourcesDirectory.FindMaterial("MISSING_overlay", true));
        }
        return overlay ? missingOverlay : missingMaterial;
    }

    private static WorldFileReader GetReaderForStream(Stream stream)
    {
        byte[] firstBytes = new byte[4];
        stream.Read(firstBytes, 0, 4);
        stream.Seek(0, SeekOrigin.Begin);
        if (firstBytes[0] == 'm')
        {
            Debug.Log("Reading MessagePack file " + stream);
            return new MessagePackWorldReader();
        }
        else if (firstBytes[0] == '{')
        {
            Debug.Log("Reading JSON file " + stream);
            return new JSONWorldReader();
        }
        else if ((firstBytes[0] == 'I'
               && firstBytes[1] == 'D'
               && firstBytes[2] == '3')
              || (firstBytes[0] == 255 // frame header sync (first 11 bits)
               && firstBytes[1] >> 5 == 7))
        {
            Debug.Log("Reading MP3 file " + stream);
            return new AudioClipWorldReader(AudioType.MPEG);
        }
        else if (firstBytes[0] == 'R'
              && firstBytes[1] == 'I'
              && firstBytes[2] == 'F'
              && firstBytes[3] == 'F')
        {
            Debug.Log("Reading WAV file " + stream);
            return new AudioClipWorldReader(AudioType.WAV);
        }
        else if (firstBytes[0] == 'O'
              && firstBytes[1] == 'g'
              && firstBytes[2] == 'g'
              && firstBytes[3] == 'S')
        {
            Debug.Log("Reading Ogg file " + stream);
            return new AudioClipWorldReader(AudioType.OGGVORBIS);
        }
        else if (firstBytes[0] == 'F'
              && firstBytes[1] == 'O'
              && firstBytes[2] == 'R'
              && firstBytes[3] == 'M')
        {
            Debug.Log("Reading AIFF file " + stream);
            return new AudioClipWorldReader(AudioType.AIFF);
        }
        else if (firstBytes[0] == 'E'
              && firstBytes[1] == 'x'
              && firstBytes[2] == 't'
              && firstBytes[3] == 'e')
        {
            Debug.Log("Reading XM file " + stream);
            return new AudioClipWorldReader(AudioType.XM);
        }
        else if (firstBytes[0] == 'I'
              && firstBytes[1] == 'M'
              && firstBytes[2] == 'P'
              && firstBytes[3] == 'M')
        {
            Debug.Log("Reading IT file " + stream);
            return new AudioClipWorldReader(AudioType.IT);
        }
        else
        {
            throw new InvalidMapFileException();
        }
    }
}