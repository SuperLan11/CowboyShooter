using UnityEngine;
using System.Collections;
using TMPro;

public class TextTrigger : MonoBehaviour
{
    [SerializeField] private string text;
    [SerializeField] private TextMeshProUGUI textObj;
    [SerializeField] private float timePerLetter;
    private AudioSource typewriterSfx;
    private bool placingLetters = false;
    private bool waitDone = true;
    private int curLetterIdx = 0;

    private bool fadingText = false;
    [SerializeField] private float fadeTime;

    // Start is called before the first frame update
    void Start()
    {
        typewriterSfx = GetComponent<AudioSource>();
        GetComponent<MeshRenderer>().enabled = false;        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "Player")
        {
            textObj.enabled = true;
            textObj.text = "";
            textObj.GetComponent<CanvasGroup>().alpha = 1;
            if (typewriterSfx != null)            
                typewriterSfx.Play();            
            placingLetters = true;            
        }
    }

    private IEnumerator Wait(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        waitDone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Player")
        {            
            fadingText = true;
            FadeOutText();
        }
    }

    private void FadeOutText()
    {
        textObj.GetComponent<CanvasGroup>().alpha -= (Time.deltaTime / fadeTime);
        if (textObj.GetComponent<CanvasGroup>().alpha <= 0)
            fadingText = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (fadingText)
            FadeOutText();

        if (placingLetters && waitDone && curLetterIdx < text.Length)
        {
            textObj.text += text[curLetterIdx];
            curLetterIdx++;
            waitDone = false;
            StartCoroutine(Wait(timePerLetter));
        }
        else if (curLetterIdx >= text.Length)
        {
            if (typewriterSfx != null && typewriterSfx.isPlaying)
                typewriterSfx.Stop();
            placingLetters = false;
            return;
        }
    }
}
