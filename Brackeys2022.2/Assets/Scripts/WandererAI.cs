using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// WandererAI controls the monster's navigation, sound occlusion, and interaction logic.
/// It handles wandering via waypoints, chasing the player, stuck detection, and audio cues.
/// </summary>
public class WandererAI : MonoBehaviour
{
    #region Serialized Fields
    [Header("AI Settings")]
    [Tooltip("Maximum time to wait at a destination before moving on")]
    [SerializeField] private float maxWaitTime = 25f;

    [Header("Movement Settings")]
    [Tooltip("Speed while chasing the player")]
    [SerializeField] private float chaseSpeed = 2.8f;

    [Tooltip("Speed while wandering")]
    [SerializeField] private float wanderSpeed = 1.5f;

    [Header("Detection & Targeting")]
    [Tooltip("Max range for line-of-sight checks")]
    [SerializeField] private float maxRange = 50f;
    [SerializeField] float detectionDelay = 0.5f;
    private float detectionTimer = 0f;

    [Tooltip("Layer mask used for raycasting to the player")]
    [SerializeField] private LayerMask detectionLayer;

    [Header("Player Interaction")]
    [Tooltip("Distance at which the player's light begins to dim")]
    [SerializeField] private float maxLightEffectDistance = 5f;

    [Tooltip("Minimum light percentage returned when close to monster")]
    [SerializeField] private float minLightPercentage = 0.3f;

    [Tooltip("Distance at which the monster kills the player instantly")]
    [SerializeField] private float playerKillDistance = 1f;

    [Header("FMOD Occlusion Settings")]
    [Tooltip("Max distance used for audio occlusion raycasts")]
    [SerializeField] private float maxSoundDist = 35f;

    [Tooltip("Number of bounces per occlusion ray")]
    [SerializeField] private int maxBounces = 3;

    [Tooltip("Number of rays in the occlusion cone")]
    [SerializeField] private int dirRayCount = 7;
    [SerializeField] private int spatRayCount = 5;
    [SerializeField] private float dirConeAngle = 80f;
    private float spatConeAngle;

    [Tooltip("Radius around player point to consider a ray hit valid")]
    [SerializeField] private float playerHitRadius = 4;
    #endregion

    #region Private Fields
    private IAstarAI ai;
    private AIPath aiPath;
    private Rigidbody2D rb;

    private GameObject player;
    private PlayerController playerScript;
    Transform playerT, listenerT;
    private StudioListener listener;

    // Waypoint list and index tracking
    private GameObject destinations;
    private readonly List<Vector2> wanderPoints = new();
    private int lastDestIndex = -1;

    // Movement direction tracking
    private Vector2 facingDir;

    // Timers
    private float delayTimer;
    private float stopChasingTime = -1f;

    // Footstep coroutine handle
    private Coroutine footstepRoutine;

    // Audio occlusion values
    private float dirOcclusion = 1f;
    private float spatOcclusion = 1f;

    // State tracking
    private bool chasingPlayer;
    private Vector2 lastKnownPlayerPos;
    private bool hasLostSight;

    #endregion

    #region Unity Callbacks
    void Start()
    {
        // Cache component references
        ai = GetComponent<IAstarAI>();
        aiPath = GetComponent<AIPath>();
        rb = GetComponent<Rigidbody2D>();

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
        playerT = player.transform;
        listener = FindFirstObjectByType<StudioListener>();
        listenerT = listener.transform;

        spatConeAngle = 360f - 360f / spatRayCount;

        // Initialize waypoint list and timers
        destinations = GameObject.FindWithTag("MonsterDestinations");
        PopulateWanderPoints();

        delayTimer = maxWaitTime;
    }

    void Update()
    {
        #if UNITY_EDITOR
            DrawDebugCircle(listenerT.position, playerHitRadius, 32, Color.cyan);
        #endif
        UpdateMoveDir(); // Updates movement/facing direction
        UpdateAIState(); // AI state decision tree
        UpdateSpatOcclusion();
        HandleFootsteps();
        UpdatePlayerEffects();
    }
    #endregion

    #region Movement Helpers
    /// <summary>Collect all child transforms under the parent as wander points.</summary>
    void PopulateWanderPoints()
    {
        wanderPoints.Clear();

        if (destinations == null || destinations.transform.childCount == 0)
        {
            Debug.LogWarning("No destinations found! Please assign a GameObject with child waypoints.");
            return;
        }

        foreach (Transform child in destinations.transform)
        {
            wanderPoints.Add(child.position);
        }
    }

    /// <summary>Update moveDir and facingDir based on the delta from last frame.</summary>
    void UpdateMoveDir()
    {
        var aiVel = ai.velocity;
        // Cache last non-zero movement direction
        if (aiVel.sqrMagnitude > 0.001f)
            facingDir = aiVel.normalized;
        Debug.DrawRay(transform.position, facingDir * 2f, Color.red, Time.deltaTime);
    }

    /// <summary>Returns true if the AI has reached its destination or is very close.</summary>
    bool AtDestination()
    {
        return ai.reachedDestination || ai.remainingDistance < 0.1f;
    }
    #endregion

    #region AI State Management
    /// <summary>
    /// Controls AI state transitions: chasing, last known location, and wandering.
    /// </summary>
    void UpdateAIState()
    {
        if (!chasingPlayer)
        {
            if (CanSeePlayer())
            {
                detectionTimer += Time.deltaTime;

                if (detectionTimer >= detectionDelay)
                {
                    HandleChasing(true);
                    detectionTimer = 0f;
                }
            }
            else
            {
                detectionTimer = 0f;
                HandleWandering();
            }

            return;
        }

        if (CanSeePlayer())
        {
            hasLostSight = false;
            lastKnownPlayerPos = playerT.position;
            ai.destination = playerT.position;
        }
        else if (!hasLostSight)
        {
            // Just lost sight of player
            hasLostSight = true;
            ai.destination = lastKnownPlayerPos;
        }
        else if (hasLostSight && AtDestination())
        {
            // Reached last seen spot, go back to wandering
            chasingPlayer = false;
            HandleWandering();
        }
        else
        {
            ai.destination = lastKnownPlayerPos;
        }

        aiPath.maxSpeed = chasingPlayer ? chaseSpeed : wanderSpeed;
    }

    /// <summary>Handle chasing state; plays screech on first sight.</summary>
    void HandleChasing(bool firstSight)
    {
        UpdateDirOcclusion();
        if (firstSight)
            PlaySound("event:/Monster/Monster_Screech");

        stopChasingTime = Time.time + 1f;
        chasingPlayer = true;
        ai.destination = playerT.position;
        aiPath.maxSpeed = chaseSpeed;
    }

    /// <summary>Handle wandering state, picking new waypoints after a delay.</summary>
    void HandleWandering()
    {
        if (AtDestination() || !ai.hasPath)
        {
            delayTimer -= Time.deltaTime * 10f;
            if (delayTimer <= 0f)
            {
                ai.destination = PickNewDestination();
                ai.SearchPath();
                delayTimer = Random.Range(0.01f, maxWaitTime);
            }
        }
        chasingPlayer = false;
        aiPath.maxSpeed = wanderSpeed;
    }

    /// <summary>Pick a new random destination, avoiding repeating the last one.</summary>
    Vector2 PickNewDestination()
    {
        if (wanderPoints.Count == 0)
            return transform.position;

        int newIndex;
        do
        {
            newIndex = Random.Range(0, wanderPoints.Count);
        } while (newIndex == lastDestIndex && wanderPoints.Count > 1);

        lastDestIndex = newIndex;
        return wanderPoints[newIndex];
    }
    #endregion

    #region Footstep Audio
    /// <summary>Start/stop footstep sounds based on movement.</summary>
    void HandleFootsteps()
    {
        if (((Vector2)ai.velocity).sqrMagnitude > 0.001f)
        {
            footstepRoutine ??= StartCoroutine(FootSteps());
        }
        else if (footstepRoutine != null)
        {
            StopCoroutine(footstepRoutine);
            footstepRoutine = null;
        }
    }

    /// <summary>Coroutine to play footsteps at intervals based on state.</summary>
    IEnumerator FootSteps()
    {
        while (((Vector2)ai.velocity).sqrMagnitude > 0.001f)
        {
            float waitTime = chasingPlayer ? Random.Range(0.3f, 0.4f) :
                                              Random.Range(0.8f, 1f);
            PlaySound("event:/Monster/Monster_Footsteps");
            yield return new WaitForSeconds(waitTime);
        }
        footstepRoutine = null;
    }
    #endregion

    #region Player Interaction
    /// <summary>Dim player's light or kill if too close.</summary>
    void UpdatePlayerEffects()
    {
        if (playerScript.isDead) return;

        float dist = Vector2.Distance(transform.position,
                                      playerT.position);
        if (!playerScript.isEnteringBox && !playerScript.isInBox)
        {
            if (dist < maxLightEffectDistance)
            {
                float pct = Mathf.Lerp(minLightPercentage, 1f,
                           (dist - playerKillDistance) /
                           (maxLightEffectDistance - playerKillDistance));
                playerScript.SetPercentage(pct);
            }
            else
            {
                playerScript.SetPercentage(1f);
            }
            if (dist <= playerKillDistance)
                playerScript.Death();
        }
    }

    /// <summary>Returns true if there is line-of-sight to the player.</summary>
    bool CanSeePlayer()
    {
        Vector2 toPlayer = playerT.position -
                           transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position,
                                             toPlayer.normalized,
                                             maxRange,
                                             detectionLayer);
        return hit && hit.collider != null && hit.collider.transform.IsChildOf(playerT);
    }
    #endregion

    #region Audio Occlusion
    /// <summary>
    /// Unified occlusion calculator.
    /// If directional==true: fires a cone of bouncing rays and returns 0(clear)→1(fully blocked).
    /// If directional==false: fires one bouncing ray toward listener and returns occlusion strength 0(clear)→1(blocked).
    /// </summary>
    float GetOcclusionValue(Vector2 source, Vector2 dir, float coneAngle, int rayCount)
    {
            int hits = 0;
            float baseAng = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            for (int i = 0; i < rayCount; i++)
            {
                float offset = Mathf.Lerp(-coneAngle / 2f, coneAngle / 2f, (float)i / (rayCount - 1));
                float ang = (baseAng + offset) * Mathf.Deg2Rad;
                Vector2 rayDir = new(Mathf.Cos(ang), Mathf.Sin(ang));
                if (CastOcclusionRay(source, rayDir))
                    hits++;
            }
            return 1f - (hits / (float)rayCount);
    }

    /// <summary>
    /// Casts a bouncing ray; returns:
    /// • for directional: 1 if any bounce hits within playerHitRadius, else 0
    /// • for spatial: occlusion strength 0(clear)→1(blocked) based on distance traveled
    /// </summary>
    bool CastOcclusionRay(Vector2 origin, Vector2 direction)
    {
        Vector2 curr = origin;
        Vector2 dir = direction.normalized;
        float traveled = 0f;
        float remaining = maxSoundDist;


        for (int i = 0; i <= maxBounces; i++)
        {
            var hit = Physics2D.Raycast(origin, dir, remaining, detectionLayer);
            if (!hit.collider)
            {
                Debug.DrawLine(curr, curr + dir * remaining, Color.red, Time.deltaTime);
                break;
            }

            float d = Vector2.Distance(curr, hit.point);
            traveled += d;
            remaining -= d;

            Debug.DrawLine(curr, hit.point, Color.cyan, Time.deltaTime);

            // radius check around listener
            if (Vector2.Distance(hit.point, listenerT.position) <= playerHitRadius)
            {
                return true;
            }

            dir = Vector2.Reflect(dir, hit.normal);
            curr = hit.point + dir * 0.01f;
        }

        // no hit within radius
        return false;
    }

    void UpdateDirOcclusion() => dirOcclusion = GetOcclusionValue(transform.position, facingDir, dirConeAngle, dirRayCount);
    void UpdateSpatOcclusion() => spatOcclusion = GetOcclusionValue(transform.position, facingDir, spatConeAngle, spatRayCount);
    #endregion

    #region Public API
    /// <summary>Called by external traps to redirect the monster and trigger a screech.</summary>
    public void ActivateTrap(Vector2 trapPos)
    {
        ai.destination = trapPos;
        chasingPlayer = false;
        stopChasingTime = -1f;
        UpdateDirOcclusion();
        PlaySound("event:/Monster/Monster_Screech");
    }
    #endregion

    #region Sound Playback
    /// <summary>Helper to play an FMOD event with current occlusion parameters.</summary>
    void PlaySound(string path)
    {
        var instance = RuntimeManager.CreateInstance(path);
        RuntimeManager.AttachInstanceToGameObject(instance,
                                                  transform,
                                                  rb);
        if (playerScript)
        {
            RuntimeManager.StudioSystem
                .setParameterByName("Directional Occlusion",
                                     dirOcclusion);
            RuntimeManager.StudioSystem
                .setParameterByName("Spatial Occlusion",
                                     spatOcclusion);
        }
        instance.start();
        instance.release();
    }
    #endregion

    #region Debug Helpers
    /// <summary>Draws a wire-circle in the Scene view for debugging.</summary>
    void DrawDebugCircle(Vector3 center,
                                 float radius,
                                 int segs,
                                 Color color)
    {
        float step = 360f / segs;
        Vector3 prev = center + Vector3.right * radius;
        for (int i = 1; i <= segs; i++)
        {
            float rad = i * step * Mathf.Deg2Rad;
            Vector3 next = center + new Vector3(
                Mathf.Cos(rad), Mathf.Sin(rad)
            ) * radius;
            Debug.DrawLine(prev, next, color);
            prev = next;
        }
    }
    #endregion
}