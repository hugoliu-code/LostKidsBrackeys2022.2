using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;
    [SerializeField] GameObject dialoguePanel;

    [SerializeField] TextMeshProUGUI dialogueText;

    private Story currentStory;

    public bool dialogueIsPlaying;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one DialogueManager in scene");
        }
        instance = this;
    }
    private void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
    }
    public static DialogueManager GetInstance()
    {
        return instance;

    }
    private void Update()
    {
        if (!dialogueIsPlaying)
        {
            
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);

        ContinueStory();
    }
    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
    }
    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            StopCoroutine("DisplayText");
            dialogueText.text = "";
            StartCoroutine(DisplayText(currentStory.Continue()));
        }
        else
        {
            StartCoroutine(ExitDialogueMode());
        }
    }
    private IEnumerator DisplayText(string text)
    {
        foreach(char n in text.ToCharArray())
        {
            yield return null;
            dialogueText.text += n;
        }
 
    }
}
