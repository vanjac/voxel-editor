using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileKeys
{
    // constants follow name pattern:
    // [containing object type]_[property]

    public const string WORLD_WRITER_VERSION = "w";
    public const string WORLD_MIN_READER_VERSION = "r";
    public const string WORLD_TYPE = "t";
    public const string WORLD_CAMERA = "c";
    public const string WORLD_CUSTOM_MATERIALS = "M";
    public const string WORLD_CUSTOM_OVERLAYS = "N";
    public const string WORLD_MATERIALS = "m";
    public const string WORLD_OVERLAYS = "n";
    public const string WORLD_SUBSTANCES = "s";
    public const string WORLD_GLOBAL = "g";
    public const string WORLD_VOXELS = "v";
    public const string WORLD_OBJECTS = "o";

    public const string CAMERA_PAN = "p";
    public const string CAMERA_ROTATE = "r";
    public const string CAMERA_SCALE = "s";

    public const string MATERIAL_MODE = "m";    // obselete since version 10
    public const string MATERIAL_COLOR = "c";
    public const string MATERIAL_ALPHA = "a";   // obselete since version 10
    public const string MATERIAL_NAME = "n";

    public const string PROPOBJ_NAME = "n";
    public const string PROPOBJ_PROPERTIES = "p";

    // ENTITY is type of PROPOBJ
    public const string ENTITY_SENSOR = "s";
    public const string ENTITY_BEHAVIORS = "b";
    public const string ENTITY_ID = "i";

    // OBJECT is type of ENTITY
    public const string OBJECT_POSITION = "a";
    public const string OBJECT_ROTATION = "r";
    public const string OBJECT_PAINT = "f";

    // CUSTOM_MATERIAL is type of ENTITY
    public const string CUSTOM_MATERIAL_NAME = "N";
}