using UnityEngine;
using UnityEngine.UI;

public class MenuLoader : MonoBehaviour
{
    public GameObject menuPrefab; // Menu Prefab (Assign in Inspector)

    void Start()
    {
        // 1️⃣ Spawn the initial menu
        GameObject menuInstance = Instantiate(menuPrefab);
        menuInstance.tag = "Menu"; // Ensure correct tag

    }
}
