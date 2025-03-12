using UnityEngine;

public class ShowCreatePromt : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject worldSelector;
    public GameObject createWindow;

    public void Show()
    {
        worldSelector.SetActive(false);
        createWindow.SetActive(true);
    }
}
