using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;



public class FunFactShuffler : MonoBehaviour
{

    [Header("Fun Facts Settings")]   
    [SerializeField] private List<FunFact> funFacts = new List<FunFact>();
    [SerializeField] private float displayDuration = 5f; // Time between fact changes

    [Header("UI References")]
    [SerializeField] private TMP_Text factTextComponent;
    [SerializeField] private Image factImageComponent; // Optional: for displaying images

    private List<FunFact> unusedFacts = new List<FunFact>();
    private float timer;
    private int currentIndex = -1;

    [System.Serializable]
    public class FunFact
    {
        [TextArea(2, 5)] // Makes the text area in the Inspector larger for better readability
        public string factText;
        public Sprite optionalImage; // Optional: if you want to include images with facts
    }

    private void Awake()
    {
        // Initialize the unused facts list
        ResetUnusedFacts();
    }

    private void OnEnable()
    {
        // Show a new fact when the object becomes active
        ShowNextFact();
    }

    private void Update()
    {
        // Cycle through facts after displayDuration
        timer += Time.deltaTime;
        if (timer >= displayDuration)
        {
            timer = 0f;
            ShowNextFact();
        }
    }

    public void ShowNextFact()
    {
        // If we've used all facts, reset the unused list
        if (unusedFacts.Count == 0)
        {
            ResetUnusedFacts();
        }

        // Get a random index
        int randomIndex = Random.Range(0, unusedFacts.Count);
        FunFact selectedFact = unusedFacts[randomIndex];

        // Remove the fact so we don't repeat it until we've shown all facts
        unusedFacts.RemoveAt(randomIndex);

        // Display the fact
        DisplayFact(selectedFact);
    }

    private void DisplayFact(FunFact fact)
    {
        if (factTextComponent != null)
        {
            factTextComponent.text = fact.factText;
        }

        // Optional image display
        if (factImageComponent != null)
        {
            factImageComponent.sprite = fact.optionalImage;
            factImageComponent.gameObject.SetActive(fact.optionalImage != null);
        }
    }

    private void ResetUnusedFacts()
    {
        unusedFacts.Clear();
        unusedFacts.AddRange(funFacts);
    }

    // Editor utility to add a new fact quickly
    [ContextMenu("Add New Fact")]
    private void AddNewFact()
    {
        funFacts.Add(new FunFact());
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}