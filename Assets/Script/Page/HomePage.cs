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

    public void GoToCliente()
    {
        SceneManager.LoadScene("ClienteScene");
    }

    public void GoToStaff()
    {
        SceneManager.LoadScene("StaffScene");
    }
}
