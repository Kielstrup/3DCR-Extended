using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public GameObject copPrefab;
    public GameObject patientPrefab;
    public GameObject soccerPlayerBluePrefab;

    void Start()
    {
        // Reemplaza todos los personajes al inicio
        ReplaceAllCharacters();
    }

    public void ReplaceAllCharacters()
    {
        GameObject[] characters = GameObject.FindGameObjectsWithTag("Character");
        
        foreach (GameObject character in characters)
        {
            ReplaceCharacter(character);
        }
    }

    void ReplaceCharacter(GameObject character)
    {
        int selectedSkin = PlayerPrefs.GetInt("SelectedSkin", 0);
        GameObject newPrefab = null;

        switch (selectedSkin)
        {
            case 1: newPrefab = copPrefab; break;
            case 2: newPrefab = patientPrefab; break;
            case 3: newPrefab = soccerPlayerBluePrefab; break;
            default: return; // Si el valor no es válido, no hacer nada
        }

        if (newPrefab != null)
        {
            GameObject newCharacter = Instantiate(newPrefab, character.transform.position, character.transform.rotation);
            newCharacter.transform.parent = character.transform.parent;
            Destroy(character);
        }
    }
}
