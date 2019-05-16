using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmbeddedData
{
    public string name;
    public byte[] bytes;

    public EmbeddedData()
    {
        name = "(empty)";
        bytes = new byte[0];
    }

    public EmbeddedData(string name, byte[] bytes)
    {
        this.name = name;
        this.bytes = bytes;
    }
}