using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class ExitScript : MonoBehaviour
{
    [SerializeField] InteractIcon icon;
    [SerializeField] int keysNeeded = 3;
    [SerializeField] SceneManagerScript sceneManager;
    [SerializeField] string nextLevel;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Input.GetKeyDown(KeyCode.Space))
        {
            if (collision.gameObject.GetComponent<PlayerController>().keyCount >= keysNeeded)
            {
                RuntimeManager.PlayOneShot("event:/Player/Ladder");
                collision.gameObject.GetComponent<PlayerController>().hasWon = true;
                sceneManager.LevelNav(nextLevel);
            }
            else
            {
                RuntimeManager.PlayOneShot("event:/Interactibles/Locked_Door");
            }
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
}
