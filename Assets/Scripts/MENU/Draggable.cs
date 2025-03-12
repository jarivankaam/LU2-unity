using UnityEngine;
using System;

public class DragAndDrop : MonoBehaviour
{
    public GameObject dicePrefab; // Prefab to instantiate
    public string menuTag = "Menu"; // Tag for the dynamically spawned menu
    public float dropThresholdY = -3.0f; // Adjust based on your world
    private string environment2DID; // Set this in the inspector to link the prefab with an environment ID

    private Vector3 offset;
    public Vector3 LocationOfDice;
    private bool isDragging = false;
    private Transform menuContainer;
    private ApicCallLogic apiHelper = new ApicCallLogic();
    private string url = "https://avansict2231011.azurewebsites.net";
    private string access_token;
   

    private void Start()
    {
        environment2DID = PlayerPrefs.GetString("SelectedWorldId", "Unknown");
        access_token = PlayerPrefs.GetString("access_token", "Unknown access_token");
        FindMenu();
    }

    private void OnMouseDown()
    {
        isDragging = true;
        offset = transform.position - GetMouseWorldPosition();
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, 0);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // Capture and send the prefab data to the DB
        SendDataToDB();

        RespawnDice(); // Respawn the dice in the menu
    }

    private void RespawnDice()
    {
        FindMenu(); // Ensure menu is found before respawning

        if (menuContainer != null)
        {
            GameObject newDice = Instantiate(dicePrefab, menuContainer);
            newDice.transform.localPosition = LocationOfDice; // Adjust if needed
        }
        else
        {
            Debug.LogError("Menu container not found! Make sure it's tagged correctly.");
        }
    }

    private void FindMenu()
    {
        GameObject menuObject = GameObject.FindGameObjectWithTag(menuTag);

        if (menuObject != null)
        {
            menuContainer = menuObject.transform;
        }
        else
        {
            Debug.LogError("❌ Menu not found! Make sure it is tagged correctly.");
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(transform.position).z; // Maintain depth
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private async void SendDataToDB()
    {
        PostRequestObject2DDTO dto = new PostRequestObject2DDTO();

        // Remove any "(Clone)" parts from the prefab name and trim whitespace.
        string prefabName = dicePrefab.name.Replace("(Clone)", "").Trim();

        // Use the cleaned prefab name as the prefab ID.
        dto.prefabId = prefabName;
        dto.positionX = Mathf.RoundToInt(transform.position.x);
        dto.positionY = Mathf.RoundToInt(transform.position.y);
        dto.scaleX = Mathf.RoundToInt(transform.localScale.x);
        dto.scaleY = Mathf.RoundToInt(transform.localScale.y);
        dto.rotationZ = Mathf.RoundToInt(transform.eulerAngles.z);

        // Assume the GameObject has a SpriteRenderer to get the sorting order.
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        dto.sortingLayer = sr != null ? sr.sortingOrder : 0;

        dto.environment2DID = environment2DID;

        await apiHelper.PerformApiCall($"{url}/Object2D", "POST", JsonUtility.ToJson(dto), access_token);
        Debug.Log("Sending data to DB: " + JsonUtility.ToJson(dto));

    }
}