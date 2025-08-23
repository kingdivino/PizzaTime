using UnityEngine;
using UnityEngine.SceneManagement;

public class OrdiniManager : MonoBehaviour
{
    

    public void GoToNewPizza()
    {
        SceneManager.LoadScene("ClienteScene");
    }
}
