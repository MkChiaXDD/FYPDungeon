using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fade in and out a specific Colour
/// Used for the warning Alert when enemy punches
/// </summary>
public class FadingInAndOut : MonoBehaviour
{
    //Image to fade in and out
    [SerializeField]
    private RawImage Image;

    //Variables for Script
    private readonly float Red = 1;
    private readonly float Green = 0;
    private readonly float Blue = 0;

    [SerializeField]
    private float MinAlpha = 0.4f;

    [SerializeField]
    private float MaxAlpha = 0.8f;

    private float CurrAlpha;
    private bool fadeOut;

    [SerializeField]
    private float speedMultiplier = 0.5f;




    // Start is called before the first frame update
    void Start()
    {
        CurrAlpha = MaxAlpha;
    }

    // Update is called once per frame
    void Update()
    {
        LerpColourAlpha();
    }

    //<summary>
    //Lerps the Colour Alpha of the image
    //flashes out the red warning to make it more eye-catching by mimicing warning sirens
    //<summary>
    private void LerpColourAlpha()
    {
        if (CurrAlpha >= MaxAlpha)
        {
            fadeOut = true;
        }

        if (CurrAlpha <= MinAlpha)
        {
            fadeOut = false;
        }

        if (fadeOut)
        {
            CurrAlpha -= Time.deltaTime * speedMultiplier;
        }
        else
        {
            CurrAlpha += Time.deltaTime * speedMultiplier;
        }

        Image.color = new Color(Red, Green, Blue, CurrAlpha);
    }
}
