using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;

public class ApiClient : MonoBehaviour
{
    public TMP_InputField username;
    public TMP_InputField password;
    public TMP_InputField WorldName;        // ✅ Matches "name" in JSON
    public TMP_InputField MaxHeight;      // ✅ Matches "maxHeight" in JSON
    public TMP_InputField MaxWidth;       // ✅ Matches "maxWidth" in JSON
    public TextMeshProUGUI Title;

    public GameObject loginCanvas;
    public GameObject worldSelector;
    public GameObject CreateScreen;
    public GameObject CreateButton;

    public GameObject errorMessage;

    private string url = "https://avansict2231011.azurewebsites.net";
    private ApicCallLogic apiHelper = new ApicCallLogic();
    private string accessToken; // Store the token here
    private Guid IdentityID; // Now using Guid instead of string

    public async void Register()
    {
        var request = new PostRegisterRequestDto
        {
            email = username.text,
            password = password.text,
            displayName = username.text

        };

        var json = JsonUtility.ToJson(request);
        await apiHelper.PerformApiCall($"{url}/custom/Auth/register", "POST", json);
    }

    public async void Login()
    {
        var request = new PostLoginRequestDto
        {
            email = username.text,
            password = password.text
        };

        var json = JsonUtility.ToJson(request);
        Debug.Log($"{url}/auth/login");
        string responseString = await apiHelper.PerformApiCall($"{url}/auth/login", "POST", json);

        if (!string.IsNullOrEmpty(responseString))
        {
            PostLoginResponseDto responseDto = JsonUtility.FromJson<PostLoginResponseDto>(responseString);
            accessToken = responseDto.accessToken; // Store token globally

            Debug.Log("Access Token: " + accessToken);

            // Now call the /AppUser/current endpoint
            await GetCurrentUser();
            string UserName = responseDto.UserName;
            loginCanvas.SetActive(false);
            worldSelector.SetActive(true);
            Title.text = $"Welcome to SmartOffice {UserName}, please Select your world";
            await GetWorlds();

            PlayerPrefs.SetString("access_token", accessToken);
        }
        else
        {
            Debug.Log("Login failed: Username or password is not correct.");
            errorMessage.SetActive(true);
            
        }
    }




    public async Task GetCurrentUser()
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("No access token found! Please log in first.");
            return;
        }

        Debug.Log("Fetching current user data...");
        string responseString = await apiHelper.PerformApiCall($"{url}/AppUser/current", "GET", null, accessToken);

        if (!string.IsNullOrEmpty(responseString))
        {
            // Convert response to a GUID
            if (Guid.TryParse(responseString.Trim('"'), out Guid parsedGuid))
            {
                IdentityID = parsedGuid;
                Debug.Log("User ID Retrieved: " + IdentityID);
            }
            else
            {
                Debug.LogError("Failed to parse user ID as GUID: " + responseString);
            }
        }
    }

    public async Task GetWorlds()
    {
        if (IdentityID == Guid.Empty)
        {
            Debug.LogError("IdentityID is empty! Cannot fetch worlds.");
            return;
        }

        string responseString = await apiHelper.PerformApiCall($"{url}/AppUser/worlds/{IdentityID}", "GET", null, accessToken);

        if (string.IsNullOrEmpty(responseString))
        {
            Debug.LogError("API Response is null or empty!");
            return;
        }

        string wrappedJson = "{\"environments\":" + responseString + "}";
        Environment2dDTOList worldsList = JsonUtility.FromJson<Environment2dDTOList>(wrappedJson);

        if (worldsList == null || worldsList.environments == null)
        {
            Debug.LogError("Deserialization failed!");
            return;
        }

        if(worldsList.environments.Count >= 5)
        {
            CreateButton.SetActive(false);
        }
        // Pass data to the WorldButtonSpawner
        WorldButtonSpawner spawner = FindFirstObjectByType<WorldButtonSpawner>();
        if (spawner != null)
        {
            spawner.CreateWorldButtons(worldsList.environments);
        }
    }

    public async Task CreateWorld()
    {
        if (IdentityID == Guid.Empty)
        {
            Debug.LogError("IdentityID is empty! Cannot create world.");
            return;
        }

        var request = new PostCreationEnvironment2dDTO
        {
            name = $"{WorldName.text}",
            maxHeight = int.Parse(MaxHeight.text),
            maxWidth = int.Parse(MaxWidth.text),
            AppUserId = $"{IdentityID}"
        };


        var json = JsonUtility.ToJson(request);
        Debug.Log("Verstuurde JSON: " + json);
        await apiHelper.PerformApiCall($"{url}/Environment2D", "POST", json, accessToken);


        Debug.Log("World created successfully!");
        CreateScreen.SetActive(false);
        worldSelector.SetActive(true);
        await GetWorlds();

    }
    public async Task DeleteWorld(Guid id)
    {
        if (IdentityID == Guid.Empty)
        {
            Debug.LogError("IdentityID is empty! Cannot delete world.");
            return;
        }

        await apiHelper.PerformApiCall($"{url}/Environment2D/{id}", "DELETE", null, accessToken);
        await GetWorlds();
    }


    public void Create()
    {
        CreateWorld();
    }

    [Serializable]
    public class Environment2dDTOList
    {
        public List<Environment2dDTO> environments;
    }

}