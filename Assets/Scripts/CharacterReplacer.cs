using System.Collections.Generic;
using UnityEngine;

public class CharacterReplacer : MonoBehaviour
{
    public GameObject copPrefab;
    public GameObject patientPrefab;
    public GameObject soccerPlayerBluePrefab;

    private int selectedSkin;
    private Dictionary<int, GameObject> skinPrefabs;

    void Start()
    {
        skinPrefabs = new Dictionary<int, GameObject>()
        {
            { 1, copPrefab },
            { 2, patientPrefab },
            { 3, soccerPlayerBluePrefab }
        };

        selectedSkin = PlayerPrefs.GetInt("SelectedSkin", 0);
        ReplaceCharactersInScene();
    }

    public void ReplaceCharactersInScene()
    {
        GameObject[] characters = GameObject.FindGameObjectsWithTag("Character");

        foreach (GameObject character in characters)
        {
            ReplaceCharacter(character);
        }
    }

    public void ReplaceCharacter(GameObject character)
    {
        if (selectedSkin == 0)
        {
            // Si el skin seleccionado es 0, no hacemos nada
            return;
        }

        if (skinPrefabs.ContainsKey(selectedSkin))
        {
            GameObject newCharacter = Instantiate(skinPrefabs[selectedSkin], character.transform.position, character.transform.rotation);
            newCharacter.transform.parent = character.transform.parent;

            // Ajustar el tamaño del nuevo personaje
            newCharacter.transform.localScale = new Vector3(3f, 3f, 3f);

            // Ajustar la posición del nuevo personaje
            Vector3 newPosition = newCharacter.transform.position;
            if (character.name != "SickCoworker")
            {
                newPosition.y = 1.7f;
            }
            newCharacter.transform.position = newPosition;

            Destroy(character);
        }
    }
}
