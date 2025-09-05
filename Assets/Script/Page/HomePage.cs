using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomePageController : MonoBehaviour
{
    public Button RistoranteButton;
    public Button PizzeriaButton;

    void Start()
    {
        RistoranteButton.onClick.AddListener(GoToCliente);
        PizzeriaButton.onClick.AddListener(GoToStaff);
    }

    public void GoToCliente()
    {
        SceneManager.LoadScene("Ristorante");
    }

    public void GoToStaff()
    {
        SceneManager.LoadScene("Pizzeria");
    }
}
