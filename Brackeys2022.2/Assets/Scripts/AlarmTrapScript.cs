using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AlarmTrapScript : MonoBehaviour
{
    [SerializeField] float trapCooldown = 2f;
    private Animator anim;
    private float coolDownFinishTime= 0;
    private WandererAI monster;

    private void Start()
    {
        monster = FindFirstObjectByType<WandererAI>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        Animation();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Time.time > coolDownFinishTime)
        {
            RuntimeManager.PlayOneShot("event:/Interactibles/Button_Down");
            monster.ActivateTrap(transform.position);
            coolDownFinishTime = Time.time + trapCooldown;
            Invoke(nameof(ButtonUpSound), trapCooldown);
        }
    }

    private void ButtonUpSound()
    {
        RuntimeManager.PlayOneShot("event:/Interactibles/Button_Up");
    }

    void Animation()
    {
        if (Time.time < coolDownFinishTime)
        {
            anim.SetBool("IsActive", false);
        }
        else
        {
            anim.SetBool("IsActive", true);
        }
    }
}
