using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class WandererAI : MonoBehaviour
{
    // A list for all locations for the AI to visit
    [SerializeField] private GameObject[] dest;

    [SerializeField] private bool reached = false;

    [SerializeField] private float maxWaitTime = 25f;
    private float delay;

    [SerializeField] private float maxStuckTime = 40f;

    [SerializeField] GameObject player;
    private PlayerController playerScript;
    public bool chasingPlayer = false;
    [SerializeField] private float maxRange = 100f;
    [SerializeField] LayerMask playerLayer;

    [SerializeField] float maxLightEffectDistance = 5f;
    [SerializeField] float minLightPercentage = 0.3f;
    [SerializeField] float playerKillDistance = 1f;

    [SerializeField] float chaseSpeed = 2.8f;
    [SerializeField] float wanderSpeed = 1.5f;
    [SerializeField] AIPath aiPath;
    IAstarAI ai;
    GameObject enemy;
    private Rigidbody2D rb;
    private FMOD.Studio.EventInstance heartBeat;
    private float stopChasingTime = -1;
    // Start is called before the first frame update
    void Start()
    {
        ai = GetComponent<IAstarAI>();
        rb = GetComponent<Rigidbody2D>();
        delay = maxWaitTime;
        playerScript = player.GetComponent<PlayerController>();
        StartCoroutine(FootSteps());
        //FMODUnity.RuntimeManager.CreateInstance("event:/Monster/Heartbeat");
        //heartBeat.start();
        //heartBeat.release(); 
        //heartBeat.setParameterByName("EndLoop", 0f);
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfStuck();

        if (CanSeePlayer())
        {
            if(!chasingPlayer)
            {
                FMODUnity.RuntimeManager.PlayOneShot("event:/Monster/Monster_Screech", transform.position);
            }
            stopChasingTime = Time.time + 1f; 
            chasingPlayer = true;
            ChasingPlayer();
            aiPath.maxSpeed = chaseSpeed;
        }
        else if(Time.time <= stopChasingTime)
        {
            ChasingPlayer();
        }
        else
        {
            Wandering();
            chasingPlayer = false;
            ai.maxSpeed = wanderSpeed;
        }
        UpdatePlayer(); //Checks the player and monster distance, and diminishes the player's light based on distance. If too close, kill the player

    }
    private void UpdatePlayer()
    {
        if (playerScript.isDead)
        {
            return;
        }
        float playerDistance = Vector2.Distance(player.transform.position, transform.position);
        if(!playerScript.isEnteringBox && !playerScript.isInBox && !playerScript.isEnteringBox && playerDistance < maxLightEffectDistance)
        {
            playerScript.SetPercentage(Mathf.Lerp(minLightPercentage, 1, (playerDistance-playerKillDistance) / (maxLightEffectDistance-playerKillDistance)));
        }
        if(!playerScript.isEnteringBox &&  !playerScript.isInBox && !playerScript.isEnteringBox && playerDistance > maxLightEffectDistance)
        {
            playerScript.SetPercentage(1f);
        }
        if (playerDistance <= playerKillDistance && !playerScript.isInBox && !playerScript.isEnteringBox)
        {
            //heartBeat.setParameterByName("EndLoop", 1f);
            playerScript.Death();
        }
    }
    // Picks a random, different point to go to
    Vector2 PickDest()
    {
        int indexNumber = Random.Range(0, dest.Length);

        // This is in case the AI somehow gets the same point multiple times, keeping it stuck
        // if it is the same number as the previous roll, then reroll until a new one
        int prevNumber = indexNumber;
        while (indexNumber == prevNumber && dest.Length > 1)
        {
            indexNumber = Random.Range(0, dest.Length);
        }

        //Debug.Log(dest[indexNumber].name);
        return dest[indexNumber].transform.position;
    }

    void Wandering()
    {
        // ai.reachedDestination doesn't work, so I had to access it from another script
        reached = GameObject.FindGameObjectWithTag("Monster").GetComponent<AIPath>().reachedDestination;
        // Update the destination of the AI if
        // the AI is not already calculating a path and
        // the ai has reached the end of the path or it has no path at all
        if ((reached || !ai.hasPath))
        {
            // Makes the AI wait at each point before choosing another one
            delay -= Time.deltaTime * 10;
            if (delay <= 0)
            {
                //Debug.Log("pause");

                ai.destination = PickDest();
                ai.SearchPath();
                
                delay = Random.Range(0, maxWaitTime);
                //Debug.Log(delay);
            }
        }
    }

    // in case the AI ever gets stuck, this will remove a majority of those cases
    // however, if it barely moves, it will be stuck forever unfortunately
    void CheckIfStuck()
    {
        if (this.rb.IsSleeping())
        {
            maxStuckTime -= Time.deltaTime * 10;
            if (maxStuckTime <= 0)
            {
                ai.destination = PickDest();
                ai.SearchPath();

                maxStuckTime = 40f;
            }
        }
        else
            maxStuckTime = 40f;
    }

    void ChasingPlayer()
    {
        ai.destination = player.transform.position;
    }

    bool CanSeePlayer()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, player.transform.position - transform.position, maxRange, playerLayer);
        Debug.DrawRay(transform.position, player.transform.position - transform.position, Color.red);

        bool val = false;

        if (hit)
        {
            if (hit.collider.gameObject.CompareTag("Player"))
                val = true;
            else
                val = false;

            Debug.DrawRay(transform.position, player.transform.position - transform.position, Color.yellow);
        }
        else
        {
            val =  false;
            Debug.DrawRay(transform.position, player.transform.position - transform.position, Color.red);
        }

        return val;
    }

    public void ActivateTrap(Vector2 trapPos)
    {
        ai.destination = trapPos;
        FMODUnity.RuntimeManager.PlayOneShot("event:/Monster/Monster_Screech", transform.position);
    }


    IEnumerator FootSteps()
    {
        while (!playerScript.isDead)
        {
            if (chasingPlayer)
            {
                yield return new WaitForSeconds(Random.Range(0.3f,0.4f));
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(0.8f, 1f));
            }
            FMODUnity.RuntimeManager.PlayOneShot("event:/Monster/Monster_Footsteps", transform.position);
        }
    }
}
