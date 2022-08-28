using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Cut());
    }
    IEnumerator Cut()
    {

        FMODUnity.RuntimeManager.PlayOneShot("event:/Player/Player_Death");
        yield return new WaitForSeconds(4f);
        FMODUnity.RuntimeManager.PlayOneShot("event:/Player/Player_Landing");
        FindObjectOfType<SceneManagerScript>().SceneNav("Dialogue1");
    }

}
