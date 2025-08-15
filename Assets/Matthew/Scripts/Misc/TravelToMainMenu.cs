using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TravelToMainMenu : MonoBehaviour
{
    private bool playerHere = false;
    [SerializeField] private Canvas PortalInteractionCanvas;
    [SerializeField] private string gameSceneName;
    private SceneSwitcher sceneSwitcher;
    // Start is called before the first frame update
    void Start()
    {
        sceneSwitcher = gameObject.AddComponent<SceneSwitcher>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerHere)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                SoundManager.Instance.PlayVariationSFX("TeleportSFX");
                LoadGameScene();
            }
        }
    }

    private void LoadGameScene()
    {
        // Initialize scene loading
        SceneManager.LoadSceneAsync(gameSceneName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerBody"))
        {
            playerHere = true;
            if (!PortalInteractionCanvas.isActiveAndEnabled)
            {
                PortalInteractionCanvas.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerBody"))
        {
            playerHere = false;
            if (PortalInteractionCanvas.isActiveAndEnabled)
            {
                PortalInteractionCanvas.gameObject.SetActive(false);
            }
        }
    }
}
