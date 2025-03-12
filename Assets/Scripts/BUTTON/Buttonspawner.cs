using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class WorldButtonSpawner : MonoBehaviour
{
    public GameObject worldButtonPrefab;  // Prefab should have a LayoutElement with a preferred height (e.g., 60)
    public Transform worldListParent;     // Parent with a Vertical Layout Group (Child Alignment: Upper Center, spacing set)
    public static List<Environment2dDTO> worlds = new List<Environment2dDTO>(); // Global world data
    private string url = "https://avansict2231011.azurewebsites.net";
    private ApicCallLogic apiHelper = new ApicCallLogic();

    public async void CreateWorldButtons(List<Environment2dDTO> worldDataList)
    {
        if (worldButtonPrefab == null || worldListParent == null)
        {
            Debug.LogError("WorldButtonPrefab or WorldListParent is not assigned in the Inspector!");
            return;
        }

        // Clear existing buttons
        foreach (Transform child in worldListParent)
        {
            Destroy(child.gameObject);
        }

        // Store world data globally
        worlds = new List<Environment2dDTO>(worldDataList);

        int index = 0;
        foreach (var world in worldDataList)
        {
            // Instantiate the prefab as a child of the worldListParent
            GameObject prefabInstance = Instantiate(worldButtonPrefab, worldListParent, false);
            prefabInstance.name = world.name;

            // (Optional) Ensure the first button is at the very top of the layout
            if (index == 0)
            {
                prefabInstance.transform.SetSiblingIndex(0);
            }
            index++;

            // Since the button elements are now inside a Canvas, adjust the search path accordingly.
            // Find the main button child (assumed to be under Canvas, named "WorldButtonPrefab")
            Transform mainButtonTransform = prefabInstance.transform.Find("Canvas/WorldButtonPrefab");
            if (mainButtonTransform == null)
            {
                Debug.LogError($"Main button child 'Canvas/WorldButtonPrefab' not found in prefab for {world.name}.");
                continue;
            }

            Button mainButton = mainButtonTransform.GetComponent<Button>();
            if (mainButton == null)
            {
                Debug.LogError($"Button component missing on 'Canvas/WorldButtonPrefab' for {world.name}.");
                continue;
            }

            // Set the button text using a custom script if available or fallback to TMP_Text
            WorldButton worldButtonComponent = mainButtonTransform.GetComponent<WorldButton>();
            if (worldButtonComponent != null && worldButtonComponent.worldNameText != null)
            {
                worldButtonComponent.worldNameText.text = $"{world.name} - {world.maxWidth} X {world.maxHeight}";
            }
            else
            {
                TMP_Text buttonText = mainButtonTransform.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = $"{world.name} - {world.maxWidth} X {world.maxHeight}";
                }
            }

            string selectedWorldId = world.id.ToString();
            string selectedWorldName = world.name;

            // Remove previous listeners and add listener to load the world scene
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(() => LoadWorldScene(selectedWorldId, selectedWorldName));

            // Set up the Edit button (child named "EditWorld" under Canvas)
            Transform editButtonTransform = prefabInstance.transform.Find("Canvas/EditWorld");
            if (editButtonTransform != null)
            {
                Button editButton = editButtonTransform.GetComponent<Button>();
                if (editButton != null)
                {
                    editButton.onClick.RemoveAllListeners();
                    editButton.onClick.AddListener(() => Debug.Log("Edit clicked for " + world.name));
                }
            }
            else
            {
                Debug.LogWarning("EditWorld child not found in prefab for " + world.name);
            }

            // Set up the Delete button (child named "RemoveWorld" under Canvas)
            Transform deleteButtonTransform = prefabInstance.transform.Find("Canvas/RemoveWorld");
            if (deleteButtonTransform != null)
            {
                Button deleteButton = deleteButtonTransform.GetComponent<Button>();
                if (deleteButton != null)
                {
                    deleteButton.onClick.RemoveAllListeners();
                    deleteButton.onClick.AddListener(async () =>
                    {
                        // Locate the ApiClient in the scene
                        ApiClient apiClient = FindObjectOfType<ApiClient>();
                        if (apiClient != null)
                        {
                            // Call DeleteWorld which performs the deletion and then updates the list
                            await apiClient.DeleteWorld(Guid.Parse(world.id));
                        }
                        else
                        {
                            Debug.LogError("ApiClient instance not found!");
                        }
                    });

                }
            }
            else
            {
                Debug.LogWarning("RemoveWorld child not found in prefab for " + world.name);
            }
        }

        // Force the layout group to update after adding all buttons
        LayoutRebuilder.ForceRebuildLayoutImmediate(worldListParent.GetComponent<RectTransform>());
    }

    public void LoadWorldScene(string worldId, string worldName)
    {
        Debug.Log($"Loading World: {worldName} (ID: {worldId})");
        PlayerPrefs.SetString("SelectedWorldId", worldId);
        PlayerPrefs.SetString("SelectedWorldName", worldName);
        SceneManager.LoadScene("WorldScene");
    }
}
