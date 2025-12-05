using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void SetVanillaSkin()
    {
        PlayerPrefs.SetInt("SelectedSkin", 0);
        SceneManager.LoadScene("Office_English");
    }

    public void SetCopSkin()
    {
        PlayerPrefs.SetInt("SelectedSkin", 1);
        SceneManager.LoadScene("Office_English");
    }

    public void SetPatientSkin()
    {
        PlayerPrefs.SetInt("SelectedSkin", 2);
        SceneManager.LoadScene("Office_English");
    }

    public void SetSoccerPlayerBlueSkin()
    {
        PlayerPrefs.SetInt("SelectedSkin", 3);
        SceneManager.LoadScene("Office_English");
    }
}
