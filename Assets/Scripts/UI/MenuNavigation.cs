using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNavigation : MonoBehaviour
{
    public void NextMenu(GameObject menuUI)
    {
        this.gameObject.SetActive(false);
        menuUI.SetActive(true);
    }

    public void PlayScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("InitialPrototypeScene");
    }

    public void Exit()
    {
        Application.Quit();
    }

}
