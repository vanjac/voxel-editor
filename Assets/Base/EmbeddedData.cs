using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmbeddedData
{
    public byte[] bytes;

    public EmbeddedData()
    {
        bytes = new byte[0];
    }

    public EmbeddedData(byte[] bytes)
    {
        this.bytes = bytes;
    }
}