using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNavigation : MonoBehaviour
{
    public void GoMenu()
    {
        SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
    }

    public void GoProfile()
    { }

    public void GoMap()
    {
        SceneManager.LoadScene("MapScene", LoadSceneMode.Single);
    }

    public void GoCamera()
    {
        SceneManager.LoadScene("CameraScene", LoadSceneMode.Single);
    }
}
