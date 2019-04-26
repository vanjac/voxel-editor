using System;
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
    public InvalidMapFileException() : base("Invalid world file") { }
    public InvalidMapFileException(Exception inner) : base("Invalid world file", inner) { }
}

public interface WorldFileReader
{
    void ReadStream(FileStream fileStream);
    // return warnings
    List<string> BuildWorld(Transform cameraPivot, VoxelArray voxelArray, bool editor);
}

public class ReadWorldFile
{
    public static Material missingMaterial;

    // return warnings
    public static List<string> Read(string filePath, Transform cameraPivot, VoxelArray voxelArray, bool editor)
    {
        // allowTransparency is true in case the material is used for an overlay, so the alpha value can be adjusted
        missingMaterial = ResourcesDirectory.MakeCustomMaterial(ColorMode.UNLIT, true);
        missingMaterial.color = Color.magenta;

        WorldFileReader reader;
        try
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Open))
            {
                int firstByte = fileStream.ReadByte();
                if (firstByte == 'm')
                {
                    Debug.Log("Reading MessagePack file " + filePath);
                    reader = new MessagePackWorldReader();
                }
                else if (firstByte == '{')
                {
                    Debug.Log("Reading JSON file " + filePath);
                    reader = new JSONWorldReader();
                }
                else
                {
                    throw new InvalidMapFileException();
                }
                reader.ReadStream(fileStream);
            }

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
        finally
        {
            missingMaterial = null;
        }
    }
}