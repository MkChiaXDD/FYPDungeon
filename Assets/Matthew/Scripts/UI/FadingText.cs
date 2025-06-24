using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// FadingText class fades the Text Instantiated by TextManager
/// Placed on a Text Prefab to control its speed, direction and fadingTime
/// Used for the score, giving more awareness on the score given by each punch and enemy defeated
/// </summary>
public class FadingText : MonoBehaviour
{
    //Text values
    [Header("Text Fading Values")]
    [SerializeField] private Vector3 direction; //direction in which the text fades in
    [SerializeField] private float speed;       //speed of the Text as it fades
    [SerializeField] private float fadingTime;  //Time taken to fade from 100% to 0

    // Update is called once per frame
    void Update()
    {
        MoveText();      
    }

    private void OnEnable()
    {
        StartFadeOut();
    }


    /// <summary>
    /// Moves the Text based on direction and speed;
    /// gives more illusion of fading away
    /// </summary>
    private void MoveText()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    /// <summary>
    /// Initialises the variables for the text 
    /// for fonts, size of fonts and text itself, adjust in inspector
    /// </summary>
    public void Initialize(float speed, Vector3 direction, float fadingTime)
    {

        //this.speed = speed;
        //this.fadingTime = fadingTime;
        //this.direction = direction;
        //StartCoroutine(nameof(FadeOut));

        //this is unused rn as its being initialised in textmanager
    }

    /// <summary>
    /// helper function to start the fadeout couroutine
    /// </summary>
    private void StartFadeOut()
    {
        StartCoroutine(nameof(FadeOut));
    }


    /// <summary>
    /// Fades out the Text using alpha when text appears
    /// animation effect so it doesnt disappear out of nowhere
    /// destroys the text after finishing
    /// </summary>
    private IEnumerator FadeOut()
    {
        float startAlpha = GetComponent<TMP_Text>().color.a;

        float rate = 1.0f / fadingTime;
        float percentFinish = 0.0f;

        while (percentFinish < 1.0f)
        {
            Color TempColour = GetComponent<TMP_Text>().color;
            GetComponent<TMP_Text>().color = new Color(TempColour.r, TempColour.g, TempColour.b, Mathf.Lerp(startAlpha, 0, percentFinish));
            percentFinish += rate * Time.deltaTime;
            
            yield return null;
        }
       
        Destroy(gameObject);
    }
}
