using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenButton : MonoBehaviour
{
    public string scena;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenScena()
    {
        SceneManager.LoadScene(scena);
    }
}
