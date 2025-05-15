using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class Cutscene : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Cut());
    }
    IEnumerator Cut()
    {

        yield return new WaitForSeconds(1f);
        RuntimeManager.PlayOneShot("event:/Player/Player_Death");
        yield return new WaitForSeconds(3f);
        RuntimeManager.PlayOneShot("event:/Player/Player_Landing");
        FindFirstObjectByType<SceneManagerScript>().SceneNav("Dialogue1");
    }

}
