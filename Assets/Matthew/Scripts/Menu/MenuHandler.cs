using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MenuHandler : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string tutorialSceneName = "TutorialScene";

    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject loadingScreen;

    [SerializeField] private TMP_Text versionText;

    [Header("Loading Settings")]
    [SerializeField] private float minLoadingTime = 2f;
    [SerializeField] private bool simulateSlowLoad = false;
    [SerializeField] private float simulatedLoadDelay = 0.5f;

    private SceneSwitcher sceneSwitcher;
    private bool isLoading = false;

    private void Awake()
    {
        // Initialize references
        sceneSwitcher = gameObject.AddComponent<SceneSwitcher>();

        // Set up button listeners
        playButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(ToggleSettings);
        tutorialButton.onClick.AddListener(StartTutorial);
        quitButton.onClick.AddListener(QuitGame);

        // Ensure settings panel is hidden on start
        //settingsPanel.SetActive(false);
        loadingScreen.SetActive(false);

        // Display version number
        versionText.text = $"v{Application.version}";

        // Prevent double-clicks
        playButton.interactable = true;
    }

    private void StartGame()
    {
        if (isLoading) return;

        // Disable button to prevent multiple clicks
        playButton.interactable = false;
        isLoading = true;

        // Start loading process
        StartCoroutine(LoadGameScene());
    }

    private void StartTutorial()
    {
        if (isLoading) return;

        // Disable button to prevent multiple clicks
        playButton.interactable = false;
        isLoading = true;

        // Start loading process
        StartCoroutine(LoadTutorialScene());
    }

    private IEnumerator LoadGameScene()
    {
        // Show loading screen
        loadingScreen.SetActive(true);

        float startTime = Time.time;
        float progress = 0f;

        // Initialize scene loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);
        asyncLoad.allowSceneActivation = false;

        // Simulate slow load for testing if enabled
        if (simulateSlowLoad)
        {
            yield return new WaitForSeconds(simulatedLoadDelay);
        }

        // Loading progress loop
        while (!asyncLoad.isDone)
        {
            // Calculate progress (0-0.9 for loading, 0.9-1.0 for activation)
            progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Update UI
           // loadingBar.value = progress;
           

            // Check if loading is complete
            if (asyncLoad.progress >= 0.9f)
            {
                // Enforce minimum loading time
                if (Time.time - startTime < minLoadingTime)
                {
                    // Continue showing loading screen
                
                }
                else
                {
                    // Allow scene activation
                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }

    private IEnumerator LoadTutorialScene()
    {
        // Show loading screen
        loadingScreen.SetActive(true);

        float startTime = Time.time;
        float progress = 0f;

        // Initialize scene loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(tutorialSceneName);
        asyncLoad.allowSceneActivation = false;

        // Simulate slow load for testing if enabled
        if (simulateSlowLoad)
        {
            yield return new WaitForSeconds(simulatedLoadDelay);
        }

        // Loading progress loop
        while (!asyncLoad.isDone)
        {
            // Calculate progress (0-0.9 for loading, 0.9-1.0 for activation)
            progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

     

            // Check if loading is complete
            if (asyncLoad.progress >= 0.9f)
            {
                // Enforce minimum loading time
                if (Time.time - startTime < minLoadingTime)
                {
                    // Continue showing loading screen
                   
                }
                else
                {
                    // Allow scene activation
                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }



    private void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);

        // Play appropriate sound
        //AudioManager.Instance.Play(settingsPanel.activeSelf ? "SettingsOpen" : "SettingsClose");
    }

    private void QuitGame()
    {
        quitButton.interactable = false;

        // Save any game data
        //SaveSystem.SaveGame();

#if UNITY_EDITOR
        Debug.Log("<color=red>Application quit requested</color>");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // For accessibility and controller support
    private void Update()
    {
        // Back button closes settings
        if (settingsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }
    }
}