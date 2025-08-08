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
        public enum TrainingType
        {
            Movement,
            Combat,
            FightEnemy,
            PickUpItems,
            BreakCrate,
            Misc,

        }

        public string _StepName;
        public string Instruction;
        public string HelpStuff;
        public KeyCode[] requiredKeys;
        public List<string> requiredAction;
        public GameObject objectToHighlight;
        public GameObject UiNeeded;
        public List<GameObject> ObjToInactive;
        public bool isCompleted;
        public float TimeForHint = 5.0f;
        public Npc _npc;
        public TrainingType Tutorial_Type;

        public bool hasWaypoint = false;
        public List<Transform> waypointTarget = new List<Transform>();
        public float waypointRadius = 2f;
        

    }

    [System.Serializable]
    public class WaypointData
    {
        public Transform waypointTarget;
        public float waypointRadius = 2f;
        public string arrivalMessage = "";
        public bool isReached = false;
    }

    public List<TutorialStep> steps = new List<TutorialStep>();
    public GameObject TutorialUI;
    public GameObject HelpPrompt;
    public TMP_Text instructionText;

    private bool AllTurtorialComplete;
    private int currentStepIndex = 0;
    private float StartTime;
    private bool showingHelp;
    public List<KeyCode> keysPressed = new List<KeyCode>();
    private List<string> RequiredAction = new List<string>();
    private bool IsDailogFinish;
    private bool ActionComplete;
    private bool AllwaypointReached = false;

    [SerializeField] private List<Transform> Targets = new List<Transform>();
    [SerializeField] private List<Transform> WayPointStore = new List<Transform>();
    [SerializeField] private DialogSystem _dialogSystem;
    [SerializeField] private PlayerCombat _playerCombat;
    [SerializeField] private DirectionTarget _TargetingSystem;
    [SerializeField] private GameObject _player;

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
        if (GamStates.instance.State == GamStates.GameState.Paused) return;

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
                    continue;
                }
            }

            if (RequiredAction.Count <= 0)
            {
                if (currentStepIndex < steps.Count)
                {
                    ActionComplete = true;
                }
            }

            CheckWayPoint();
            if (WayPointStore.Count <= 0)
            {
                AllwaypointReached = true;
            }

            if (allKeysPressed && ActionComplete && AllwaypointReached)
            {
                CompleteStep();
            }
        }
    }

    public void StartAfterDailog()
    {
        TutorialStep step = steps[currentStepIndex];
        RequiredAction.Clear();
        WayPointStore.Clear();
        StartTime = Time.time;
        showingHelp = false;

        foreach (string action in steps[currentStepIndex].requiredAction)
        {
            RequiredAction.Add(action);
        }

        foreach (Transform targets in steps[currentStepIndex].waypointTarget)
        {
            WayPointStore.Add(targets);
            _TargetingSystem.AddTargets(targets.gameObject);
            Debug.Log("Obj Target: " + targets.name);
        }
        //RequiredAction = steps[currentStepIndex].requiredAction;
        instructionText.text = step.Instruction;

        ActionComplete = false;
        if (RequiredAction.Count <= 0)
        {
            ActionComplete = true;
        }

        AllwaypointReached = false;
        if (WayPointStore.Count <= 0)
        {
            AllwaypointReached = true;
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

        if (step.Tutorial_Type == TutorialStep.TrainingType.Movement)
        {
            _playerCombat.DisableCombat = true;
        }

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

            WayPointStore.Clear();
            foreach (Transform targets in steps[currentStepIndex].waypointTarget)
            {
                WayPointStore.Add(targets);
                _TargetingSystem.AddTargets(targets.gameObject);
            }

            ActionComplete = false;
            if (RequiredAction.Count <= 0)
            {
                ActionComplete = true;
            }

            AllwaypointReached = false;
            if (WayPointStore.Count <= 0)
            {
                AllwaypointReached = true;
            }

            instructionText.text = step.Instruction;

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
        _playerCombat.DisableCombat = false;
        ActionComplete = false;
        foreach (GameObject inact in steps[currentStepIndex].ObjToInactive)
        {
            inact.SetActive(false);
        }
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

    public void CheckWayPoint()
    {
        List<Transform> waypointsDelete = new List<Transform>();

        foreach (Transform t in WayPointStore)
        {
            if (t != null) {
                if (Vector3.Distance(new Vector3(_player.transform.position.x, 0, _player.transform.position.z), new Vector3(t.position.x, 0, t.position.z)) <= steps[currentStepIndex].waypointRadius)
                {
                    waypointsDelete.Add(t);
                }
            }
        }

        foreach (Transform t in waypointsDelete)
        {
            if (t != null)
            {
                _TargetingSystem.RemoveTargets(t.gameObject);
                WayPointStore.Remove(t);
            }
        }
    }
    public void Help()
    {
        showingHelp = true;
        HelpPrompt.SetActive(true);
    }
}
