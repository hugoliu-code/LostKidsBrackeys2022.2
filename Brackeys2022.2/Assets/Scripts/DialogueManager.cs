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

    private float nextContinueTime = 0;

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

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextContinueTime)
        {
            nextContinueTime = Time.time + 0.1f;
            ContinueStory();
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);
        nextContinueTime = Time.time + 0.1f;
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
            StopAllCoroutines();
            StartCoroutine(DisplayText(currentStory.Continue()));
        }
        else
        {
            StartCoroutine(ExitDialogueMode());
        }
    }
    private IEnumerator DisplayText(string text)
    {
        dialogueText.text = "";
        foreach(char n in text.ToCharArray())
        {
            yield return null;
            dialogueText.text += n;
        }
 
    }
}
