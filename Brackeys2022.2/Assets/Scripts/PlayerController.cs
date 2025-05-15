using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float runSpeed = 1;
    [SerializeField] float puddleSpeed = 0.8f;
    private float currentSpeed = 1;
    [SerializeField] Animator anim;
    private Vector2 movementVector = new(0, 0);
    private Rigidbody2D rb;
    [Header("Lights")]
    [SerializeField] UnityEngine.Rendering.Universal.Light2D innerLight;
    [SerializeField] UnityEngine.Rendering.Universal.Light2D outerLight;
    [SerializeField] float maxInnerLight;
    [SerializeField] float minInnerLight;
    [SerializeField] float maxOuterLight;
    [SerializeField] float minOuterLight;
    [SerializeField] float pulseTime;
    [SerializeField] float lightPercentage;
    private float nextPulseTime = 0;
    private float startPulseTime = 0;
    private bool pulseOut;
    private bool canPulse;

    [Header("Components")]
    [SerializeField] GameObject MonsterCollider;
    [SerializeField] GameObject spriteObject;
    [SerializeField] GameObject DeathObject;
    private EventInstance Footsteps;
    [Header("Conditions")]
    public float enterChestTime;
    public bool isEnteringBox;
    public bool isInBox;
    public bool isDead = false;
    public int keyCount = 0;
    public bool hasWon = false;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = runSpeed;
        lightPercentage = 0;
        StartCoroutine(ChangePercentage(1f, 3f));
        Footsteps = RuntimeManager.CreateInstance("event:/UI/Static_Effect");
        Cursor.visible = false;
    }
    void Update()
    {
        Movement();
        Pulse();
    }
    #region Light
    private void Pulse()
    {
        if(Time.time > nextPulseTime)
        {
            startPulseTime = Time.time;
            nextPulseTime = Time.time + pulseTime;
            pulseOut = !pulseOut;
        }
        float lerpFloat = (Time.time - startPulseTime) / (nextPulseTime - startPulseTime);
        if (pulseOut) {
            innerLight.pointLightInnerRadius = Mathf.SmoothStep(minInnerLight * lightPercentage, maxInnerLight * lightPercentage, lerpFloat);
            innerLight.pointLightOuterRadius = Mathf.SmoothStep(minInnerLight * lightPercentage, maxInnerLight * lightPercentage, lerpFloat);
            outerLight.pointLightInnerRadius = Mathf.SmoothStep(minOuterLight * lightPercentage, maxOuterLight * lightPercentage, lerpFloat);
            outerLight.pointLightOuterRadius = Mathf.SmoothStep(minOuterLight * lightPercentage, maxOuterLight * lightPercentage, lerpFloat);
        }
        else
        {
            innerLight.pointLightInnerRadius = Mathf.SmoothStep(minInnerLight * lightPercentage, maxInnerLight * lightPercentage, 1 - lerpFloat);
            innerLight.pointLightOuterRadius = Mathf.SmoothStep(minInnerLight * lightPercentage, maxInnerLight * lightPercentage, 1 - lerpFloat);
            outerLight.pointLightInnerRadius = Mathf.SmoothStep(minOuterLight * lightPercentage, maxOuterLight * lightPercentage, 1 - lerpFloat);
            outerLight.pointLightOuterRadius = Mathf.SmoothStep(minOuterLight * lightPercentage, maxOuterLight * lightPercentage, 1 - lerpFloat);
        }
    }
    public IEnumerator ChangePercentage(float targetPercentage, float time)
    {
        float startPercentage = lightPercentage;
        float endTime = Time.time + time;
        float startTime = Time.time;
        while(Time.time < endTime)
        {
            lightPercentage = Mathf.Lerp(startPercentage, targetPercentage, (Time.time - startTime) / (endTime - startTime));
            yield return null;
        }
    }
    public void SetPercentage(float targetPercentage)
    {
        lightPercentage = targetPercentage;
    }
    #endregion
    private void Movement()
    {
        if(isInBox || isEnteringBox || isDead || DialogueManager.GetInstance().dialogueIsPlaying)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("Running", false);
            return;
        }
        movementVector = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
            movementVector.x = -1;
        if (Input.GetKey(KeyCode.D))
            movementVector.x = 1;
        if (Input.GetKey(KeyCode.W))
            movementVector.y = 1;
        if (Input.GetKey(KeyCode.S))
            movementVector.y = -1;
        movementVector = movementVector.normalized * currentSpeed;
        if (movementVector.magnitude > 0.1)
            anim.SetBool("Running", true);
        else
            anim.SetBool("Running", false);

            rb.linearVelocity = movementVector;
    }

    public void ChestToggle(bool toggle)
    {
        //Will turn both the sprite and monster tracker on and off
        MonsterCollider.SetActive(toggle);
        spriteObject.SetActive(toggle);
    }
    public IEnumerator MovePlayer(Vector2 from, Vector2 to, float time)
    {
        float endTime = Time.time + time;
        float startTime = Time.time;
        while (Time.time < endTime)
        {
            transform.position = Vector2.Lerp(from, to, (Time.time - startTime) / (endTime - startTime));
            yield return null;
        }
    }

    public void Death()
    {
        if (isDead || hasWon)
        {
            return;
        }
        isDead = true;
        StopAllCoroutines();
        RuntimeManager.PlayOneShot("event:/Player/Player_Death");
        RuntimeManager.PlayOneShot("event:/Monster/Monster_Kill");
        StartCoroutine(ChangePercentage(0f, 1f));
        StartCoroutine(DeathCoroutine());
    }
    IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(2f);
        //FMODUnity.RuntimeManager.PlayOneShot("event:/UI/Static_Effect");
        Cursor.visible = true;
        Footsteps.start();
        Footsteps.release();
        DeathObject.SetActive(true);
    }
    private void OnDestroy()
    {
        Footsteps.setParameterByName("EndLoop", 1f);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Key"))
        {
            keyCount++;
            collision.gameObject.SetActive(false);
            RuntimeManager.PlayOneShot("event:/Interactibles/Key");
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Puddles"))
        {
            currentSpeed = puddleSpeed;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Puddles"))
        {
            currentSpeed = runSpeed;
        }
    }
}
