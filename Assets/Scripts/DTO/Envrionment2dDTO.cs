using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class Environment2dDTO
{
    public string id;          // GUID as string
    public string name;
    public int maxHeight;
    public int maxWidth;
    public Guid UserId;   // GUID as string
}
