using UnityEngine;

public class MenuNavigation : MonoBehaviour
{
    public void NextMenu(GameObject menuUI)
    {
        this.gameObject.SetActive(false);
        menuUI.SetActive(true);
    }
}
