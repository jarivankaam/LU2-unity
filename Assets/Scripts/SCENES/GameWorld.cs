using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Unity.Mathematics;

public class GameWorldManager : MonoBehaviour
{
    private Guid WorldID;
    private Guid access_token;
    private string url = "https://avansict2231011.azurewebsites.net";
    private ApicCallLogic apiHelper = new ApicCallLogic();

    public GameObject menu;

    void Start()
    {
        // Retrieve selected world data
        string worldId = PlayerPrefs.GetString("SelectedWorldId", "Unknown");
        string token = PlayerPrefs.GetString("access_token", "Unknown access_token");
        GetObjects(worldId, token);
    }

    public async Task GetObjects(string IdentityID, string accessToken)
    {
        if (Guid.TryParse(IdentityID.Trim('"'), out Guid parsedGuid))
        {
            WorldID = parsedGuid;
            Debug.Log("User ID Retrieved: " + IdentityID);
        }
        else
        {
            Debug.LogError("Failed to parse user ID as GUID: " + IdentityID);
            return;
        }

        if (WorldID == Guid.Empty)
        {
            Debug.LogError("IdentityID is empty! Cannot fetch objects.");
            return;
        }

        string capitalizedGuid = WorldID.ToString().ToUpper();
        Debug.Log($"{url}/Environment2D/objects/{capitalizedGuid}");
        string responseString = await apiHelper.PerformApiCall($"{url}/Environment2D/objects/{capitalizedGuid}", "GET", null, accessToken);

        if (string.IsNullOrEmpty(responseString))
        {
            Debug.LogError("API Response is null or empty!");
            return;
        }

        try
        {
            Object2dDTO[] objectList = JsonConvert.DeserializeObject<Object2dDTO[]>(responseString);

            if (objectList == null || objectList.Length == 0)
            {
                Debug.LogError("Deserialization failed or empty response!");
                return;
            }


            Debug.Log($"Successfully retrieved {objectList.Length} objects.");

            // Find the parent GameObject named "gameplain"
            GameObject gameplain = GameObject.Find("GamePlain");
            if (gameplain == null)
            {
                Debug.LogError("GameObject 'gameplain' not found in the scene.");
                return;
            }

            foreach (var o in objectList)
            {
                // Assume each object has a property 'prefabId' that holds the prefab's name.
                string prefabName = o.prefabId;
                GameObject prefab = Resources.Load<GameObject>(prefabName);
                if (prefab == null)
                {
                    Debug.LogError($"Prefab with name '{prefabName}' not found in Resources.");
                    continue;
                }

                // Set the position using o.positionX and o.positionY (with z=0)
                Vector3 pos = new Vector3(o.positionX, o.positionY, 0);

                // Instantiate the prefab as a child of 'gameplain'
                GameObject instance = Instantiate(prefab, pos, Quaternion.identity, gameplain.transform);
                instance.transform.localScale = Vector3.one;
            }


        }
        catch (Exception ex)
        {
            Debug.LogError("Deserialization error: " + ex.Message);
        }
    }


}

