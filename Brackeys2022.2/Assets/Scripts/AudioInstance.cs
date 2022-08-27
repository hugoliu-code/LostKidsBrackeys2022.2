using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioInstance : MonoBehaviour
{
    public string AudioDest;
    [SerializeField] private float permDelay = 200f;
    private float delay;

    void Start()
    {
        delay = permDelay;
    }

    // Update is called once per frame
    void Update()
    {
        //InvokeRepeating("PlayRHerring", .01, delay);
        PlayRHerring();
    }

    void PlayRHerring()
    {
        delay -= Time.deltaTime * 10;
        if (delay <= 0)
        {
            FMODUnity.RuntimeManager.PlayOneShot(AudioDest, GetComponent<Transform>().position);
            //Debug.Log("played a sound");
            delay = permDelay;
        }

    }
}
