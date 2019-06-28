﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapReadException : Exception
{
    public MapReadException() { }
    public MapReadException(string message) : base(message) { }
    public MapReadException(string message, Exception inner) : base(message, inner) { }
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
}

public class ReadWorldFile
{
    public static Material missingMaterial;

    // return warnings
    public static List<string> Read(string filePath, Transform cameraPivot, VoxelArray voxelArray, bool editor)
    {
        WorldFileReader reader;
        using (FileStream stream = File.Open(filePath, FileMode.Open))
            reader = ReadStream(stream);
        return BuildWorld(reader, cameraPivot, voxelArray, editor);
    }

    public static List<string> Read(TextAsset asset, Transform cameraPivot, VoxelArray voxelArray, bool editor)
    {
        WorldFileReader reader;
        using (MemoryStream stream = new MemoryStream(asset.bytes))
            reader = ReadStream(stream);
        return BuildWorld(reader, cameraPivot, voxelArray, editor);
    }

    public static List<EmbeddedData> ReadEmbeddedData(string filePath, EmbeddedDataType type)
    {
        WorldFileReader reader;
        using (FileStream stream = File.Open(filePath, FileMode.Open))
            reader = ReadStream(stream);
        return reader.FindEmbeddedData(type);
    }

    public static List<EmbeddedData> ReadEmbeddedData(Stream stream, EmbeddedDataType type)
    {
        WorldFileReader reader = ReadStream(stream);
        return reader.FindEmbeddedData(type);
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
        if (missingMaterial == null)
        {
            // allowTransparency is true in case the material is used for an overlay, so the alpha value can be adjusted
            missingMaterial = ResourcesDirectory.MakeCustomMaterial(ColorMode.UNLIT, true);
            missingMaterial.color = Color.magenta;
        }

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
        else if (firstBytes[0] == 'S'
              && firstBytes[1] == 'V'
              && firstBytes[2] == 'O'
              && firstBytes[3] == 'X')
        {
            Debug.Log("Reading SunVox file " + stream);
            return new SunVoxWorldReader();
        }
        else
        {
            throw new InvalidMapFileException();
        }
    }
}