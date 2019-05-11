using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunVoxUtils
{
    private static bool init = false;
    private static HashSet<int> openSlots = new HashSet<int>();

    public static int OpenUnusedSlot()
    {
        if (!init)
        {
            Debug.Log("SunVox init");
            init = true;
            int version = SunVox.sv_init("0", 44100, 2, 0);
            if (version < 0)
            {
                Debug.LogError("Error initializing SunVox");
                return -1;
            }

            int major = (version >> 16) & 255;
            int minor1 = (version >> 8) & 255;
            int minor2 = (version) & 255;
            Debug.Log(System.String.Format("SunVox lib version: {0}.{1}.{2}", major, minor1, minor2));
        }

        int slot = 0;
        while (true)
        {
            if (!openSlots.Contains(slot))
                break;
            slot++;
        }

        Debug.Log("SunVox: open slot " + slot);
        SunVox.sv_open_slot(slot);
        openSlots.Add(slot);
        return slot;
    }

    public static void CloseSlot(int slot)
    {
        Debug.Log("SunVox: close slot " + slot);
        openSlots.Remove(slot);
        SunVox.sv_close_slot(slot);
    }
}
