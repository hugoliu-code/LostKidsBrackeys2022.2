using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmTrapScript : MonoBehaviour
{
    [SerializeField] float trapCooldown = 5f;
    private Animator anim;
    private float coolDownFinishTime= 0;
    private WandererAI monster;
    private void Start()
    {
        monster = FindObjectOfType<WandererAI>();
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
            monster.ActivateTrap(transform.position);
            coolDownFinishTime = Time.time + trapCooldown;
        }
    }
    void Animation()
    {
        if(Time.time < coolDownFinishTime)
        {
            anim.SetBool("IsActive", false);
        }
        else
        {
            anim.SetBool("IsActive", true);
        }
    }

}
