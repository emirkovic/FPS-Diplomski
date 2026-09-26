using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

// A capsule soldier that fights like the player: it carries a pistol and a machine gun,
// picks the right one for the distance, and keeps deciding whether to push or take cover.
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
public class SoldierAI : MonoBehaviour
{
    [System.Serializable]
    public class SoldierWeapon
    {
        public string name = "Weapon";
        public GameObject model;
        public Transform muzzle;
        public AudioClip fireSFX;
        public int damage = 1;
        [Tooltip("Seconds between shots.")]
        public float fireRate = 0.6f;
        public int magazineSize = 8;
        public float reloadTime = 2f;
        [Tooltip("Seconds spent aiming before opening fire. Gives the player a chance to react.")]
        public float aimTime = 0.4f;
        [Tooltip("Shots fired before pausing. 0 = no limit.")]
        public int burstSize = 0;
        [Tooltip("Pause between bursts.")]
        public float burstPause = 0.5f;
        [Tooltip("Random spread in degrees.")]
        public float spread = 3f;
        public float minRange = 0f;
        public float maxRange = 20f;
        [Tooltip("Distance where this weapon works best.")]
        public float idealRange = 10f;
        [Tooltip("Shows a red laser while aiming, and aims again before every shot.")]
        public bool showLaserWhileAiming;
        [HideInInspector] public int ammo;
    }

    enum State { Idle, Search, Attack, TakeCover }

    [Header("Weapons")]
    [SerializeField] Transform weaponHolder;
    [SerializeField] SoldierWeapon pistol = new SoldierWeapon
    {
        name = "Pistol", damage = 1, fireRate = 0.5f, magazineSize = 8, reloadTime = 1.5f,
        aimTime = 0.3f, spread = 4f, minRange = 0f, maxRange = 18f, idealRange = 5f
    };
    [SerializeField] SoldierWeapon machineGun = new SoldierWeapon
    {
        name = "Machine Gun", damage = 1, fireRate = 0.12f, magazineSize = 25, reloadTime = 2.5f,
        aimTime = 0.4f, burstSize = 5, burstPause = 0.7f, spread = 5f, minRange = 0f, maxRange = 35f, idealRange = 15f
    };
    [SerializeField] float weaponSwapTime = 0.6f;
    [SerializeField] float minTimeBetweenSwaps = 2f;
    [SerializeField] LayerMask shootMask = ~0;
    [SerializeField] Material lineMaterial;

    [Header("Senses")]
    [SerializeField] Transform eyes;
    [SerializeField] float sightRange = 50f;
    [SerializeField] float fieldOfView = 140f;
    [Tooltip("Notices the player this close even when looking the other way.")]
    [SerializeField] float closeAwarenessRange = 6f;
    [Tooltip("How long the soldier keeps hunting the player after losing sight of them.")]
    [SerializeField] float memoryTime = 6f;

    [Header("Decisions")]
    [Tooltip("0 = careful, spends more time in cover. 1 = reckless, keeps pushing.")]
    [SerializeField] [Range(0f, 1f)] float aggression = 0.5f;
    [SerializeField] float decisionInterval = 0.25f;
    [Tooltip("After being in the open this long, the soldier starts looking for cover.")]
    [SerializeField] float maxExposureTime = 4f;
    [SerializeField] Vector2 coverWaitTime = new Vector2(1.5f, 3f);

    [Header("Cover")]
    [SerializeField] float coverSearchRadius = 15f;
    [SerializeField] int coverSamples = 24;
    [SerializeField] float coverCheckHeight = 1.2f;
    [SerializeField] float minCoverDistanceFromPlayer = 5f;

    [Header("Movement")]
    [SerializeField] float attackSpeed = 3f;
    [SerializeField] float runSpeed = 5.5f;
    [SerializeField] float turnSpeed = 540f;
    [SerializeField] float strafeRadius = 3f;

    const float TRACER_TIME = 0.05f;
    const float UNDER_FIRE_TIME = 1.5f;
    const float NAVMESH_SNAP_DISTANCE = 5f;
    const float MAX_TIME_TO_REACH_COVER = 8f;

    NavMeshAgent agent;
    EnemyHealth health;
    AudioSource audioSource;
    NavMeshPath path;
    PlayerHealth player;
    CharacterController playerController;

    State state = State.Idle;
    SoldierWeapon currentWeapon;
    SoldierWeapon pendingWeapon;
    float swapDoneTime;
    float lastSwapTime = -100f;
    float reloadDoneTime = -1f;
    float nextShotTime;
    float aimReadyTime = -1f;
    int shotsInBurst;

    bool alerted;
    bool canSeePlayer;
    float lastSeenTime = -100f;
    float lastDamagedTime = -100f;
    Vector3 lastKnownPlayerPosition;

    float nextDecisionTime;
    float exposedTime;
    float nextStrafeTime;
    bool hasCover;
    bool atCover;
    float coverUntilTime;
    float coverGiveUpTime;
    Vector3 coverPosition;

    LineRenderer laser;
    LineRenderer tracer;
    float tracerHideTime;

    bool IsSwapping => pendingWeapon != null;
    bool IsReloading => reloadDoneTime > 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<EnemyHealth>();
        audioSource = GetComponent<AudioSource>();
        path = new NavMeshPath();
        agent.updateRotation = false;

        pistol.ammo = pistol.magazineSize;
        machineGun.ammo = machineGun.magazineSize;
        Equip(pistol);

        laser = CreateLine("Laser", 0.015f);
        tracer = CreateLine("Tracer", 0.025f);
    }

    void OnEnable()
    {
        health.Damaged += OnDamaged;
    }

    void OnDisable()
    {
        health.Damaged -= OnDamaged;
    }

    void Start()
    {
        player = FindFirstObjectByType<PlayerHealth>();
        if (player) playerController = player.GetComponent<CharacterController>();

        // The agent starts disabled so the soldier can be dropped anywhere near the NavMesh.
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, NAVMESH_SNAP_DISTANCE, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            agent.enabled = true;
        }
        else
        {
            Debug.LogWarning("SoldierAI could not find the NavMesh near its position.", this);
        }
    }

    void Update()
    {
        if (tracer.enabled && Time.time >= tracerHideTime) tracer.enabled = false;
        if (!agent.enabled) return;

        if (!player)
        {
            // The player is dead.
            agent.isStopped = true;
            laser.enabled = false;
            return;
        }

        UpdateWeaponTimers();

        if (Time.time >= nextDecisionTime)
        {
            nextDecisionTime = Time.time + decisionInterval;
            Perceive();
            Decide();
        }

        Act();
        UpdateRotation();
    }

    void OnDamaged()
    {
        // Getting shot always gives away where the player is.
        lastDamagedTime = Time.time;
        alerted = true;
        if (player)
        {
            lastSeenTime = Time.time;
            lastKnownPlayerPosition = player.transform.position;
        }
        nextDecisionTime = 0f;
    }

    // ---------- Senses ----------

    void Perceive()
    {
        canSeePlayer = CanSee(PlayerAimPoint());
        if (!canSeePlayer) return;

        alerted = true;
        lastSeenTime = Time.time;
        lastKnownPlayerPosition = player.transform.position;
    }

    bool CanSee(Vector3 target)
    {
        Vector3 toTarget = target - eyes.position;
        float distance = toTarget.magnitude;
        if (distance > sightRange) return false;

        bool inView = alerted || distance < closeAwarenessRange || Vector3.Angle(transform.forward, toTarget) < fieldOfView * 0.5f;
        return inView && HasClearShot(eyes.position, target);
    }

    bool HasClearShot(Vector3 from, Vector3 target)
    {
        Vector3 direction = target - from;
        if (Physics.Raycast(from, direction.normalized, out RaycastHit hit, direction.magnitude + 0.5f, shootMask, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.GetComponentInParent<PlayerHealth>() != null;
        }
        return true;
    }

    Vector3 PlayerAimPoint()
    {
        if (playerController)
        {
            Bounds bounds = playerController.bounds;
            return bounds.center + Vector3.up * bounds.extents.y * 0.5f;
        }
        return player.transform.position + Vector3.up * 1.5f;
    }

    // ---------- Decisions ----------

    void Decide()
    {
        // Stop hunting eventually if the player can't be found.
        if (alerted && !canSeePlayer && Time.time - lastSeenTime > memoryTime * 3f) alerted = false;

        bool isAware = canSeePlayer || (alerted && Time.time - lastSeenTime < memoryTime);
        if (!isAware)
        {
            state = alerted ? State.Search : State.Idle;
            return;
        }

        ChooseWeapon(Vector3.Distance(transform.position, lastKnownPlayerPosition));

        float healthPercent = health.HealthPercent;
        bool underFire = Time.time - lastDamagedTime < UNDER_FIRE_TIME;
        bool needsReload = IsReloading || currentWeapon.ammo <= 0;

        if (state == State.TakeCover && hasCover)
        {
            bool coverBlown = atCover && canSeePlayer;
            if (coverBlown)
            {
                if (!FindCover()) EnterAttack();
                return;
            }
            // Couldn't reach the cover spot in time: fight instead of running forever.
            if (!atCover && Time.time > coverGiveUpTime)
            {
                EnterAttack();
                return;
            }
            // Stay hidden until the wait is over and the gun is loaded again.
            if (!atCover || Time.time < coverUntilTime || needsReload) return;
            EnterAttack();
            return;
        }

        // Utility scores: the soldier does whichever makes more sense right now.
        float coverScore = (1f - healthPercent) * 0.8f
                         + (underFire ? 0.6f : 0f)
                         + (needsReload ? 1.2f : 0f)
                         + (exposedTime > maxExposureTime ? 0.8f : 0f)
                         + (1f - aggression) * 0.3f;
        float attackScore = healthPercent * 0.5f
                          + aggression * 0.6f
                          + (canSeePlayer ? 0.2f : 0f)
                          + AmmoPercent(currentWeapon) * 0.3f;

        if (coverScore > attackScore + 0.15f && FindCover())
        {
            state = State.TakeCover;
            return;
        }

        if (state != State.Attack) EnterAttack();
    }

    void EnterAttack()
    {
        state = State.Attack;
        hasCover = false;
        atCover = false;
        exposedTime = 0f;
        nextStrafeTime = 0f;
    }

    void ChooseWeapon(float distance)
    {
        if (IsSwapping || Time.time - lastSwapTime < minTimeBetweenSwaps) return;

        SoldierWeapon other = OtherWeapon(currentWeapon);
        if (WeaponScore(other, distance) > WeaponScore(currentWeapon, distance) + 0.2f)
        {
            StartSwap(other);
        }
    }

    float WeaponScore(SoldierWeapon weapon, float distance)
    {
        float score = 1f - Mathf.Clamp01(Mathf.Abs(distance - weapon.idealRange) / weapon.maxRange);
        if (distance < weapon.minRange || distance > weapon.maxRange) score -= 1f;
        // A loaded gun beats one that needs reloading.
        score += weapon.ammo > 0 ? 0.3f : -0.6f;
        return score;
    }

    bool FindCover()
    {
        Vector3 threatEyes = lastKnownPlayerPosition + Vector3.up * 1.6f;
        Vector3 toThreat = (lastKnownPlayerPosition - transform.position).normalized;
        float bestScore = float.MinValue;
        bool found = false;

        for (int i = 0; i < coverSamples; i++)
        {
            float angle = 360f / coverSamples * i + Random.Range(-10f, 10f);
            float radius = Random.Range(2f, coverSearchRadius);
            Vector3 candidate = transform.position + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas)) continue;

            Vector3 point = hit.position;
            float distanceToThreat = Vector3.Distance(point, lastKnownPlayerPosition);
            if (distanceToThreat < minCoverDistanceFromPlayer) continue;
            if (!IsHiddenFrom(threatEyes, point)) continue;

            float distanceToMe = Vector3.Distance(transform.position, point);
            float score = -distanceToMe + Mathf.Min(distanceToThreat, 25f) * 0.25f;
            // Running towards the player to reach cover is a bad idea.
            if (Vector3.Dot((point - transform.position).normalized, toThreat) > 0.5f) score -= 3f;
            if (score <= bestScore) continue;
            if (!agent.CalculatePath(point, path) || path.status != NavMeshPathStatus.PathComplete) continue;

            bestScore = score;
            coverPosition = point;
            found = true;
        }

        if (found)
        {
            hasCover = true;
            atCover = false;
            coverGiveUpTime = Time.time + MAX_TIME_TO_REACH_COVER;
        }
        return found;
    }

    bool IsHiddenFrom(Vector3 threatEyes, Vector3 groundPoint)
    {
        Vector3 chest = groundPoint + Vector3.up * coverCheckHeight;
        if (!Physics.Linecast(threatEyes, chest, out RaycastHit hit, shootMask, QueryTriggerInteraction.Ignore)) return false;

        // Only level geometry counts as cover, not ourselves, other enemies or the player.
        if (hit.collider.transform.IsChildOf(transform)) return false;
        if (hit.collider.GetComponentInParent<EnemyHealth>()) return false;
        if (hit.collider.GetComponentInParent<PlayerHealth>()) return false;
        return true;
    }

    // ---------- Actions ----------

    void Act()
    {
        switch (state)
        {
            case State.Idle:
                agent.isStopped = true;
                laser.enabled = false;
                break;

            case State.Search:
                laser.enabled = false;
                MoveTo(lastKnownPlayerPosition, attackSpeed);
                if (HasArrived())
                {
                    // Lost them. Give up and wait.
                    alerted = false;
                    state = State.Idle;
                }
                break;

            case State.TakeCover:
                laser.enabled = false;
                MoveTo(coverPosition, runSpeed);
                if (!atCover && HasArrived())
                {
                    atCover = true;
                    float wait = Random.Range(coverWaitTime.x, coverWaitTime.y);
                    // Hurt soldiers stay down longer.
                    coverUntilTime = Time.time + wait * (1.5f - health.HealthPercent * 0.5f);
                }
                if (atCover && !IsReloading && !IsSwapping && currentWeapon.ammo < currentWeapon.magazineSize)
                {
                    StartReload();
                }
                break;

            case State.Attack:
                Attack();
                break;
        }
    }

    void Attack()
    {
        if (canSeePlayer) exposedTime += Time.deltaTime;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool inRange = distance >= currentWeapon.minRange && distance <= currentWeapon.maxRange;

        if (!canSeePlayer || !inRange)
        {
            // Close in (or peek out) until there is a clear shot.
            MoveTo(lastKnownPlayerPosition, attackSpeed);
        }
        else if (currentWeapon.showLaserWhileAiming)
        {
            // Hold still while aiming a laser-sighted weapon.
            agent.isStopped = true;
        }
        else if (Time.time >= nextStrafeTime)
        {
            // Side-step so the soldier isn't an easy target, but only to spots that keep the shot.
            nextStrafeTime = Time.time + Random.Range(1.5f, 3f);
            Vector3 strafePoint = transform.position + transform.right * Random.Range(-strafeRadius, strafeRadius);
            if (NavMesh.SamplePosition(strafePoint, out NavMeshHit hit, 1f, NavMesh.AllAreas)
                && HasClearShot(hit.position + Vector3.up * eyes.localPosition.y, PlayerAimPoint()))
            {
                MoveTo(hit.position, attackSpeed);
            }
            else
            {
                agent.isStopped = true;
            }
        }

        TryShoot();
    }

    void MoveTo(Vector3 destination, float speed)
    {
        agent.isStopped = false;
        agent.speed = speed;
        if ((agent.destination - destination).sqrMagnitude > 0.25f)
        {
            agent.SetDestination(destination);
        }
    }

    bool HasArrived()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f;
    }

    void UpdateRotation()
    {
        bool facePlayer = canSeePlayer || (state == State.Attack && alerted);
        Vector3 lookDirection = facePlayer ? lastKnownPlayerPosition - transform.position : agent.velocity;
        if (facePlayer && canSeePlayer) lookDirection = player.transform.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        if (!weaponHolder) return;
        Quaternion holderTarget = canSeePlayer
            ? Quaternion.LookRotation(PlayerAimPoint() - weaponHolder.position)
            : transform.rotation;
        weaponHolder.rotation = Quaternion.Slerp(weaponHolder.rotation, holderTarget, 10f * Time.deltaTime);
    }

    // ---------- Weapons ----------

    void TryShoot()
    {
        bool canShoot = canSeePlayer && !IsSwapping && !IsReloading && currentWeapon.ammo > 0;
        if (!canShoot)
        {
            aimReadyTime = -1f;
            shotsInBurst = 0;
            laser.enabled = false;
            return;
        }

        if (aimReadyTime < 0f) aimReadyTime = Time.time + currentWeapon.aimTime;

        Vector3 target = PlayerAimPoint();
        if (currentWeapon.showLaserWhileAiming)
        {
            laser.enabled = true;
            laser.SetPosition(0, MuzzlePosition());
            laser.SetPosition(1, target);
        }

        if (Time.time < aimReadyTime || Time.time < nextShotTime) return;
        Fire(target);
    }

    void Fire(Vector3 target)
    {
        currentWeapon.ammo--;
        nextShotTime = Time.time + currentWeapon.fireRate;
        if (currentWeapon.burstSize > 0 && ++shotsInBurst >= currentWeapon.burstSize)
        {
            shotsInBurst = 0;
            nextShotTime = Time.time + currentWeapon.burstPause;
        }
        if (currentWeapon.showLaserWhileAiming)
        {
            aimReadyTime = Time.time + currentWeapon.aimTime;
            laser.enabled = false;
        }

        // A moving player is harder to hit.
        float playerSpeed = playerController ? playerController.velocity.magnitude : 0f;
        float spread = currentWeapon.spread * (1f + playerSpeed * 0.15f);

        Vector3 origin = eyes.position;
        Vector3 direction = (target - origin).normalized;
        direction = Quaternion.AngleAxis(Random.Range(-spread, spread), Vector3.up)
                  * Quaternion.AngleAxis(Random.Range(-spread, spread), transform.right)
                  * direction;

        float range = currentWeapon.maxRange * 1.5f;
        Vector3 end = origin + direction * range;
        if (Physics.Raycast(origin, direction, out RaycastHit hit, range, shootMask, QueryTriggerInteraction.Ignore))
        {
            end = hit.point;
            PlayerHealth hitPlayer = hit.collider.GetComponentInParent<PlayerHealth>();
            if (hitPlayer) hitPlayer.TakeDamage(currentWeapon.damage);
        }

        tracer.SetPosition(0, MuzzlePosition());
        tracer.SetPosition(1, end);
        tracer.enabled = true;
        tracerHideTime = Time.time + TRACER_TIME;

        if (audioSource && currentWeapon.fireSFX) audioSource.PlayOneShot(currentWeapon.fireSFX);
    }

    void UpdateWeaponTimers()
    {
        if (IsSwapping && Time.time >= swapDoneTime)
        {
            Equip(pendingWeapon);
        }

        if (IsReloading && Time.time >= reloadDoneTime)
        {
            currentWeapon.ammo = currentWeapon.magazineSize;
            reloadDoneTime = -1f;
        }

        if (IsSwapping || IsReloading || currentWeapon.ammo > 0) return;

        // Empty gun: switching is faster than reloading if the other gun is loaded and usable.
        SoldierWeapon other = OtherWeapon(currentWeapon);
        float distance = player ? Vector3.Distance(transform.position, player.transform.position) : 0f;
        bool otherUsable = other.ammo > 0 && distance >= other.minRange && distance <= other.maxRange;
        if (otherUsable) StartSwap(other);
        else StartReload();
    }

    void StartSwap(SoldierWeapon weapon)
    {
        pendingWeapon = weapon;
        swapDoneTime = Time.time + weaponSwapTime;
        lastSwapTime = Time.time;
        reloadDoneTime = -1f;
        aimReadyTime = -1f;
        shotsInBurst = 0;
        laser.enabled = false;
        if (currentWeapon != null && currentWeapon.model) currentWeapon.model.SetActive(false);
    }

    void StartReload()
    {
        reloadDoneTime = Time.time + currentWeapon.reloadTime;
        aimReadyTime = -1f;
        shotsInBurst = 0;
        laser.enabled = false;
    }

    void Equip(SoldierWeapon weapon)
    {
        currentWeapon = weapon;
        pendingWeapon = null;
        if (pistol.model) pistol.model.SetActive(weapon == pistol);
        if (machineGun.model) machineGun.model.SetActive(weapon == machineGun);
    }

    SoldierWeapon OtherWeapon(SoldierWeapon weapon)
    {
        return weapon == pistol ? machineGun : pistol;
    }

    float AmmoPercent(SoldierWeapon weapon)
    {
        return weapon.magazineSize > 0 ? (float)weapon.ammo / weapon.magazineSize : 0f;
    }

    Vector3 MuzzlePosition()
    {
        return currentWeapon.muzzle ? currentWeapon.muzzle.position : eyes.position;
    }

    LineRenderer CreateLine(string lineName, float width)
    {
        GameObject lineObject = new GameObject(lineName);
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.startWidth = width;
        line.endWidth = width;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        if (lineMaterial)
        {
            line.sharedMaterial = lineMaterial;
        }
        else
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader) line.material = new Material(shader) { color = Color.red };
        }
        line.enabled = false;
        return line;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, closeAwarenessRange);
        if (hasCover)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(coverPosition + Vector3.up, new Vector3(0.6f, 2f, 0.6f));
        }
    }
}
