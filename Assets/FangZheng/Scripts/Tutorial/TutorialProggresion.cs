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
        public List<string> requiredAction;
        public GameObject objectToHighlight;
        public GameObject UiNeeded;
        public bool isCompleted;
        public float TimeForHint = 5.0f;
        public Npc _npc;
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
    private List<string> RequiredAction = new List<string>();
    private bool ActionComplete;
    private bool IsDailogFinish;

    [SerializeField] private DialogSystem _dialogSystem;
    [SerializeField] private PlayerCombat _playerCombat;

    public static TutorialProggresion Instance;

    private void OnEnable()
    {
        _dialogSystem.DailogFinish.AddListener(StartAfterDailog);
        _playerCombat.OnAction += IfPlayerPerformAction;
    }

    private void OnDisable()
    {
        _playerCombat.OnAction -= IfPlayerPerformAction;
    }
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
        

        if (IsDailogFinish == true) {
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

            if (allKeysPressed && ActionComplete)
            {
                CompleteStep();
            }
        }
    }

    public void StartAfterDailog()
    {
        TutorialStep step = steps[currentStepIndex];
        RequiredAction.Clear();
        StartTime = Time.time;
        showingHelp = false;
        foreach (string action in steps[currentStepIndex].requiredAction)
        {
            RequiredAction.Add(action);
        }
        //RequiredAction = steps[currentStepIndex].requiredAction;
        instructionText.text = step.Instruction;

        ActionComplete = false;
        if (RequiredAction.Count <= 0)
        {
            ActionComplete = true;
        }
        IsDailogFinish = _dialogSystem.DailogEnd;
        TutorialUI.SetActive(true);
        HelpPrompt.SetActive(false);
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


        if (step._npc == null) {
            IsDailogFinish = true;
            StartTime = Time.time;
            showingHelp = false;
            //RequiredAction = steps[StepPoint].requiredAction;
            RequiredAction.Clear();
            foreach (string action in steps[currentStepIndex].requiredAction)
            {
                RequiredAction.Add(action);
            }
            instructionText.text = step.Instruction;

            ActionComplete = false;
            if (RequiredAction.Count <= 0)
            {
                ActionComplete = true;
            }

            TutorialUI.SetActive(true);
            HelpPrompt.SetActive(false);
        }
        else
        {
            IsDailogFinish = false;
            _dialogSystem.Activate(step._npc);
            
        }
    }

    
    public void CompleteStep()
    {
        steps[currentStepIndex].isCompleted = true;
        ActionComplete = false;
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

        if (RequiredAction.Count <= 0) {
            if (currentStepIndex < steps.Count )
            {
                ActionComplete = true;
                //CompleteStep();
            }
        }
        else
        {

            foreach (string action in steps[currentStepIndex].requiredAction)
            {
                if (action == actionName)
                {
                    RequiredAction.Remove(actionName);
                }
            }

            if (RequiredAction.Count <= 0)
            {
                ActionComplete = true;
            }

        }
    }

    public void Help()
    {
        showingHelp = true;
        HelpPrompt.SetActive(true);
    }
}
