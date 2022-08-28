using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                collision.gameObject.GetComponent<PlayerController>().hasWon = true;
                sceneManager.LevelNav(nextLevel);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Interactibles/Open_Door");
            }
            else
            {
                FMODUnity.RuntimeManager.PlayOneShot("event:/Interactibles/Locked_Door");
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
