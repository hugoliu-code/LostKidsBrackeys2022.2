using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunAnimationSound : MonoBehaviour
{
    private float distance = 1f;
    private string Material = "Dirt";

    [SerializeField] private LayerMask layer;

    void FixedUpdate()
    {
        MaterialCheck();
        Debug.DrawRay(transform.position, Vector2.down * distance, Color.blue);
    }

    void MaterialCheck()
    {
        RaycastHit2D hit;

        hit = Physics2D.Raycast(transform.position, Vector2.down, distance, layer);

        if (hit.collider)
        {
            if (hit.collider.tag == "Puddles")
                Material = "Water";
            else if (hit.collider.tag == "Dirt")
                Material = "Dirt";
            else
                Material = "Stone";
        }
    }

    public void PlayFootstepsEvent(string path)
    {
        FMOD.Studio.EventInstance Footsteps = FMODUnity.RuntimeManager.CreateInstance(path);
        Footsteps.setParameterByNameWithLabel("Material", Material);
        Footsteps.start();
        Footsteps.release();
    }
}
