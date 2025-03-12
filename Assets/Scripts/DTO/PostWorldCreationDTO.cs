using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class PostCreationEnvironment2dDTO
{
    public string name;
    public int maxHeight;
    public int maxWidth;
    public string AppUserId;   // GUID as string
}
