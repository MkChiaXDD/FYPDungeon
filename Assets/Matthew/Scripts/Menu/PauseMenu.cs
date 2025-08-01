using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private GameObject settingsMenuCanvas;
    [SerializeField] private GameObject exitConfirmationCanvas;
    [SerializeField] private Button firstSelectedButton;

    [Header("Settings")]
    [SerializeField] private float originalTimeScale = 1f;


    private bool isPaused = false;

    private StandaloneInputModule inputModule;

    private void Start()
    {
        // Ensure all menus are closed at start
        pauseMenuCanvas.SetActive(false);
        settingsMenuCanvas.SetActive(false);
        exitConfirmationCanvas.SetActive(false);

        // Store the original time scale (in case it's not 1)
        originalTimeScale = Time.timeScale;
        EnsureEventSystemExists();
    }

    private void EnsureEventSystemExists()
    {
        if (EventSystem.current == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // Configure input module to work with paused game
        inputModule = FindObjectOfType<StandaloneInputModule>();
        if (inputModule != null)
        {
            inputModule.forceModuleActive = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsMenuCanvas.activeSelf)
            {
                CloseSettings();
            }
            else if (exitConfirmationCanvas.activeSelf)
            {
                CloseExitConfirmation();
            }
            else
            {
                TogglePause();
            }
        }
    }






    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        Debug.Log("UNResumte game");

        // Time.timeScale = 0f;

        pauseMenuCanvas.SetActive(true);

        // Set the first selected button for controller/keyboard navigation
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }

        // Enable input processing for UI
        EnableUIInput();
    }

    private void ResumeGame()
    {
        Debug.Log("Resumte game");

        Time.timeScale = originalTimeScale;
        pauseMenuCanvas.SetActive(false);

        // Clear selected object when closing menu
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OpenSettings()
    {
        Debug.Log("Open settings");

        settingsMenuCanvas.SetActive(true);
        pauseMenuCanvas.SetActive(false);
    }

    public void CloseSettings()
    {
        Debug.Log("Close settings");
        settingsMenuCanvas.SetActive(false);
        pauseMenuCanvas.SetActive(true);

        // Reset selection to first button
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }

    public void OpenExitConfirmation()
    {
        Debug.Log("Open Exit Confirmed");
        exitConfirmationCanvas.SetActive(true);
        pauseMenuCanvas.SetActive(false);
    }

    public void CloseExitConfirmation()
    {
        Debug.Log("Closed Exit Confirmed");
        exitConfirmationCanvas.SetActive(false);
        pauseMenuCanvas.SetActive(true);

        // Reset selection to first button
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }

    private void EnableUIInput()
    {
        // Force UI to process input even when time is stopped
        if (inputModule != null)
        {
            inputModule.Process();
        }
    }

    public void ExitGame()
    {
        Debug.Log("you have exited");
        // Note: In editor this won't work, only in built game
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}