using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WorldButton : MonoBehaviour
{
    public TMP_Text worldNameText;  // Assign the TextMeshPro component in Unity
    private string worldId;

    public void Setup(string worldName, string worldId)
    {
        this.worldId = worldId;
        worldNameText.text = worldName;

        // Add a listener to the button
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Debug.Log("Selected World ID: " + worldId);
        PlayerPrefs.SetString("SelectedWorldId", worldId);
        SceneManager.LoadScene("WorldScene"); // Make sure "WorldScene" is added to build settings
        
    }
}
