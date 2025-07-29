using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialProggresion : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        public string _StepName;
        public string Instruction;
        public string HelpStuff;
        public KeyCode[] requiredKeys;
        public string requiredAction;
        public GameObject objectToHighlight;
        public GameObject UiNeeded;
        public bool isCompleted;
        public float TimeForHint = 5.0f;
    }

    public List<TutorialStep> steps = new List<TutorialStep>();
    public GameObject TutorialUI;
    public GameObject HelpPrompt;
    public TMP_Text instructionText;

    private bool AllTurtorialComplete;
    private int currentStepIndex = 0;
    private float StartTime;
    private bool showingHelp;
    private List<KeyCode> keysPressed = new List<KeyCode>();

    public static TutorialProggresion Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {


        TutorialUI.SetActive(false);
        HelpPrompt.SetActive(false);

        if (steps.Count > 0)
        {
            startStep(0);
        }

    }

    private void Update()
    {
        if (currentStepIndex >= steps.Count) return;

        TutorialStep currentStep = steps[currentStepIndex];
        if (!currentStep.isCompleted && !showingHelp &&
            Time.time - StartTime > currentStep.TimeForHint)
        {
            Help();
        }

        if (currentStep.requiredKeys.Length > 0)
        {
            foreach (KeyCode key in currentStep.requiredKeys)
            {
                if (Input.GetKeyDown(key) && !keysPressed.Contains(key))
                {
                    keysPressed.Add(key);
                }
            }
        }

        bool allKeysPressed = true;
        foreach (KeyCode key in currentStep.requiredKeys)
        {
            if (!keysPressed.Contains(key))
            {
                allKeysPressed = false;
                break;
            }
        }

        if (allKeysPressed)
        {
            CompleteStep();
        }

    }

    public void startStep(int StepPoint)
    {
        if (StepPoint >= steps.Count)
        {
            EndTutorial();
            return;
        }

        currentStepIndex = StepPoint;
        TutorialStep step = steps[StepPoint];
        StartTime = Time.time;
        showingHelp = false;

        instructionText.text = step.Instruction;

        TutorialUI.SetActive(true);
        HelpPrompt.SetActive(false);
    }

    public void CompleteStep()
    {
        steps[currentStepIndex].isCompleted = true;

        startStep(currentStepIndex + 1);
    }

    public void EndTutorial()
    {
        //TutorialUI.SetActive(false);
        instructionText.text = "Tutorial is complete";
        Debug.Log("Tutorial completed!");
        AllTurtorialComplete = true;
    }

    public void IfPlayerPerformAction(string actionName)
    {
        if ( currentStepIndex < steps.Count && steps[currentStepIndex].requiredAction == actionName)
        {
            CompleteStep();
        }
    }

    public void Help()
    {
        showingHelp = true;
        HelpPrompt.SetActive(true);
    }
}
