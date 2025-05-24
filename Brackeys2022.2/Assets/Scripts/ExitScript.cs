using System.Collections;
using UnityEngine;
using FMODUnity;

public class ExitScript : MonoBehaviour
{
    [SerializeField] private InteractIcon icon;
    [Tooltip("Number of keys required to exit")]
    [SerializeField] private int keysNeeded = 3;
    [SerializeField] private SceneManagerScript sceneManager;
    [SerializeField] private string nextLevel;
    [Tooltip("Cooldown after interacting (seconds)")]
    [SerializeField] private float interactionCooldown = 0.5f;

    private bool isPlayerNear = false;
    private bool canInteract = true;
    private PlayerController player;

    private void Update()
    {
        if (!isPlayerNear || player == null || player.isDead || !canInteract)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            AttemptExit();
        }
    }

    private void AttemptExit()
    {
        canInteract = false;

        if (player.keyCount >= keysNeeded)
        {
            RuntimeManager.PlayOneShot("event:/Interactibles/Open_Door");
            player.hasWon = true;
            sceneManager.LevelNav(nextLevel);
        }
        else
        {
            RuntimeManager.PlayOneShot("event:/Interactibles/Locked_Door");
        }

        StartCoroutine(ResetInteraction());
    }

    private IEnumerator ResetInteraction()
    {
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerController>();
            isPlayerNear = true;
            icon.ShowIcon();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            icon.HideIcon();
        }
    }
}
