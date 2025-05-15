using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestScript : MonoBehaviour
{
    private bool isInteracting = false;
    [SerializeField] Transform enterMarker;
    [SerializeField] Transform exitMarker;
    [SerializeField] GameObject physicalCollider;
    [SerializeField] InteractIcon icon;
    private PlayerController player;
    private Animator anim;
    private void Start()
    {
        anim = GetComponent<Animator>();
        player = FindFirstObjectByType<PlayerController>();
    }
    private void Update()
    {
        if (isInteracting && Input.GetKeyDown(KeyCode.Space) && !player.isEnteringBox && !player.isDead && player.isInBox)
        {
            ExitChest();
        }
        if (isInteracting)
        {
            icon.HideIcon();
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Input.GetKeyDown(KeyCode.Space) && collision.CompareTag("Player") && !isInteracting && !player.isInBox && !player.isEnteringBox)
        {
            EnterChest();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            icon.ShowIcon();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            icon.HideIcon();
        }
    }
    public void Toggle()
    {
        if (isInteracting)
        {
            player.ChestToggle(false);
            player.isInBox = true;
            player.isEnteringBox = false;
            anim.SetBool("IsOpening", false);
            FMODUnity.RuntimeManager.PlayOneShot("event:/Interactibles/Chest_Open");
        }
        else
        {
            player.ChestToggle(true);
            player.isInBox = false;
            anim.SetBool("IsOpening", false);
            FMODUnity.RuntimeManager.PlayOneShot("event:/Interactibles/Chest_Open");
        }
        
    }
    private void EnterChest()
    {
        
        isInteracting = true;
        //do the native chest animation
        anim.SetBool("IsOpening", true);
        //Remove The chest's collider
        physicalCollider.SetActive(false);
        //Move player towards chest
        player.isEnteringBox = true;
        StartCoroutine(player.MovePlayer(player.transform.position, enterMarker.position, player.enterChestTime));
        //Diminish player's light
        StartCoroutine(player.ChangePercentage(0.3f, player.enterChestTime));

    }
    private void ExitChest()
    {
        isInteracting = false;
        anim.SetBool("IsOpening", true);
        physicalCollider.SetActive(true);
        icon.ShowIcon();
        StartCoroutine(player.ChangePercentage(1f, player.enterChestTime));
    }
    private void InteractOff()
    {
        isInteracting = false;
    }
    
}
