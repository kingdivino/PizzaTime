using UnityEngine;
using UnityEngine.SceneManagement;

public class DropdownButton : MonoBehaviour
{
    public GameObject objectList;

    public void ToggleMenu()
    {
        objectList.SetActive(!objectList.activeSelf);
    }

    public void AnnullaPizza()
    {
        SceneManager.LoadScene("OrdiniScene");
    }
}

