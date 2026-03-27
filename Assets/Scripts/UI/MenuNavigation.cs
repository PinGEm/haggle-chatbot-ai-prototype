using LLMUnity;
using UndreamAI.LlamaLib;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNavigation : MonoBehaviour
{
    [SerializeField] private LLMUnity.LLMAgent _llmAgent;

    public void NextMenu(GameObject menuUI)
    {
        this.gameObject.SetActive(false);
        menuUI.SetActive(true);
    }
    public void OpenMenu(GameObject menuUI)
    {
        menuUI.SetActive(true);
    }

    public void PlayScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("InitialPrototypeScene");
    }

    public void MainMenuScene()
    {
        _llmAgent.CancelRequests();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }


    public void Exit()
    {
        Application.Quit();
    }

}
