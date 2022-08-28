using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    private bool firstInteraction = true;
    private InteractIcon icon;
    private bool playerInRange = false;
    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJsonOne;
    [SerializeField] private TextAsset inkJsonTwo;
    private void Start()
    {
        icon = GetComponent<InteractIcon>();
        icon.HideIcon();
    }

    private void Update()
    {
        if (playerInRange && !DialogueManager.GetInstance().dialogueIsPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (firstInteraction)
                {
                    DialogueManager.GetInstance().EnterDialogueMode(inkJsonOne);
                    firstInteraction = false;
                }
                else
                {
                    DialogueManager.GetInstance().EnterDialogueMode(inkJsonTwo);
                }
            }
            icon.ShowIcon();

        }
        else
        {
            icon.HideIcon();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
            playerInRange = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = false;
    }
}
