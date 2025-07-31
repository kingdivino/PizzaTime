using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomePageController : MonoBehaviour
{
    public Button clienteButton;
    public Button staffButton;

    void Start()
    {
        clienteButton.onClick.AddListener(GoToCliente);
        staffButton.onClick.AddListener(GoToStaff);
    }

    void GoToCliente()
    {
        SceneManager.LoadScene("ClienteScene");
    }

    void GoToStaff()
    {
        SceneManager.LoadScene("StaffScene");
    }
}
