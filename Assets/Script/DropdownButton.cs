using UnityEngine;

public class DropdownButton : MonoBehaviour
{
    public GameObject ingredientList;

    public void ToggleMenu()
    {
        ingredientList.SetActive(!ingredientList.activeSelf);
    }
}

