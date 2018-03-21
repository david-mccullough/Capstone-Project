using UnityEngine;
using UnityEngine.UI;

public class DisplayHelpText : MonoBehaviour {

    public string myString;
    public Text myText;
    private float fadeTime = 6f;
    public bool displayInfo = false;
    private bool isHovering = false;

    private Color clear;
    private Color white;

    void Start() {
        clear = new Color(1f, 1f, 1f, 0f);
        white = new Color(1f, 1f, 1f, .8f);
        myText.color = clear;
    }

    void Update() {
        FadeText();
        isHovering = false;
    }

    public void MouseOver() {
        displayInfo = true;
        isHovering = true;
    }

    public void MouseExit() {
        if (!isHovering) {
            displayInfo = false;
        }
    }

    void FadeText() {
        if (displayInfo) {
            myText.text = myString;
            myText.color = Color.Lerp(myText.color, white, fadeTime * Time.deltaTime);
        }
        else {
            myText.color = Color.Lerp(myText.color, clear, fadeTime * Time.deltaTime);
        }
    }

}
