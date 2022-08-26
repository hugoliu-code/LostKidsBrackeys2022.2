using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float runSpeed = 1;
    [SerializeField] float puddleSpeed = 0.5f;
    private float currentSpeed = 1;
    [SerializeField] Animator anim;
    private Vector2 movementVector = new Vector2(0, 0);
    private Rigidbody2D rb;
    [Header("Lights")]
    [SerializeField] Light2D innerLight;
    [SerializeField] Light2D outerLight;
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
    
    [Header("Conditions")]
    public float enterChestTime;
    public bool isEnteringBox;
    public bool isInBox;
    public bool isDead = false;
    public int keyCount = 0; 
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = runSpeed;
        lightPercentage = 0;
        StartCoroutine(ChangePercentage(1f, 3f));
        
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
        if(isInBox || isEnteringBox || isDead)
        {
            rb.velocity = Vector2.zero;
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

            rb.velocity = movementVector;
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
        if (isDead)
        {
            return;
        }
        isDead = true;
        StopAllCoroutines();
        FMODUnity.RuntimeManager.PlayOneShot("event:/Player/Player_Death");
        FMODUnity.RuntimeManager.PlayOneShot("event:/Player/Player_Disem");
        FMODUnity.RuntimeManager.PlayOneShot("event:/Monster/Monster_Kill");
        StartCoroutine(ChangePercentage(0f, 1f));
        StartCoroutine("DeathCoroutine");
    }
    IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(2f);
        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/Static_Effect");
        DeathObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Key"))
        {
            keyCount++;
            collision.gameObject.SetActive(false);
            FMODUnity.RuntimeManager.PlayOneShot("event:/Interactibles/Key");
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
