using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenu;     // Holds your Play, Settings, Quit buttons
    public GameObject optionsMenu;  // Holds your Settings/Options UI

    // Call this from your PlayButton
    public void PlayGame()
    {
        // Loads your game scene by its exact name in Assets > Scenes
        SceneManager.LoadScene("SampleScene");
    }

    // Call this from your SettingsButton
    public void OpenOptions()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    // Call this from your BackButton inside OptionsMenu
    public void CloseOptions()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    // Call this from your QuitButton
    public void QuitGame()
    {
        Debug.Log("Quit Game!");

        #if UNITY_EDITOR
        // Stops Play mode when testing inside the Unity Editor
            UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Closes the application when running the exported/built game
            Application.Quit();
        #endif
}
}