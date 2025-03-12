using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class WorldManager : MonoBehaviour
{
    void Start()
    {
        string selectedWorldId = PlayerPrefs.GetString("SelectedWorldId", "");

        if (string.IsNullOrEmpty(selectedWorldId))
        {
            Debug.LogError("No world selected!");
            return;
        }

        // Retrieve the selected world data
        Guid selectedWorldGuid = Guid.Parse(selectedWorldId);

        Environment2dDTO selectedWorld = WorldButtonSpawner.worlds.FirstOrDefault(w => Guid.Parse(w.id) == selectedWorldGuid);

        if (selectedWorld == null)
        {
            Debug.LogError("Selected world data not found!");
            return;
        }

        Debug.Log($"Loaded World: {selectedWorld.name}, Size: {selectedWorld.maxWidth}x{selectedWorld.maxHeight}");

        // You can now use selectedWorld data in this scene!
    }
}
