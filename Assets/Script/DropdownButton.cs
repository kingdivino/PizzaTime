using UnityEngine;

public class DropdownButton : MonoBehaviour
{
    public GameObject objectList;

    public void ToggleMenu()
    {
        objectList.SetActive(!objectList.activeSelf);
    }
}

