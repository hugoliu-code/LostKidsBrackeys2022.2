using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunAnimationSound : MonoBehaviour
{
    public void FootSteps()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Player/Player_Footsteps");
    }
}
