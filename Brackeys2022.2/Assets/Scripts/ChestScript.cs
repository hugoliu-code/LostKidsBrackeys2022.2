using System.Collections;
using UnityEngine;

public class ChestScript : MonoBehaviour
{
    [Header("Chest Settings")]
    [SerializeField] Transform enterMarker;
    [SerializeField] GameObject physicalCollider;
    [SerializeField] InteractIcon icon;
    [SerializeField] float interactionCooldownTime = 1f;

    Animator anim;
    PlayerController player;

    bool isPlayerNear = false;
    bool isInteracting = false;
    bool isExiting = false;
    bool canInteract = true;

    void Start()
    {
        anim = GetComponent<Animator>();
        player = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (!isPlayerNear || player == null || player.isDead || !canInteract)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (player.isInBox && !player.isEnteringBox)
            {
                BeginExit();
            }
            else if (!isInteracting && !player.isInBox && !player.isEnteringBox)
            {
                BeginEnter();
            }
        }

        if (isInteracting)
            icon.HideIcon();
    }

    void BeginEnter()
    {
        isInteracting = true;
        isExiting = false;
        canInteract = false;

        player.isEnteringBox = true;
        anim.SetBool("IsOpening", true);
        physicalCollider.SetActive(false);

        StartCoroutine(player.MovePlayer(player.transform.position, enterMarker.position, player.enterChestTime));
        StartCoroutine(player.ChangePercentage(0.3f, player.enterChestTime));
        Invoke(nameof(ResetInteraction), interactionCooldownTime);
    }

    void BeginExit()
    {
        isInteracting = true;
        isExiting = true;
        canInteract = false;

        anim.SetBool("IsOpening", true);
        physicalCollider.SetActive(true);
        icon.ShowIcon();

        StartCoroutine(player.ChangePercentage(1f, player.enterChestTime));
        Invoke(nameof(ResetInteraction), interactionCooldownTime);
    }

    public void OnChestAnimationComplete()
    {
        anim.SetBool("IsOpening", false);

        if (isExiting)
        {
            player.ChestToggle(true);
            player.isInBox = false;
        }
        else
        {
            player.ChestToggle(false);
            player.isInBox = true;
            player.isEnteringBox = false;
        }

        isInteracting = false;
        FMODUnity.RuntimeManager.PlayOneShot("event:/Interactibles/Chest_Open");
    }

    void ResetInteraction()
    {
        canInteract = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (!player.isInBox)
                icon.ShowIcon();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            icon.HideIcon();
        }
    }
}
