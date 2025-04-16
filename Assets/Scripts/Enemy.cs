// Enemy Behavior Script
// Handles dash and projectile attackers with randomized decision-making

using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public enum State { Approaching, Charging, Attacking, Cooldown, Dying, Dead }
    public State currentState = State.Approaching;

    public Transform player;

    private float approachDistance = 1.3f;
    private float dashSpeed = 8f;
    private float dashDuration = 0.2f;
    private float chargeTime = 1.5f;
    private float attackCooldown = 1f;
    private float moveSpeed = 2f;
    private float projectileDistance = 3f;
    private float projectileChance = 0.0018f;

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Sprite walkSpriteA;
    [SerializeField] private Sprite walkSpriteB;
    [SerializeField] private Sprite chargeProjectile;
    [SerializeField] private Sprite dashSprite;
    [SerializeField] private Sprite woundedSprite;
    [SerializeField] private Sprite deadSprite;
    [SerializeField] private ParticleSystem bloodEffect;
    [SerializeField] private ParticleSystem smokeEffect;

    private float chargeTimer = 0f;
    private float cooldownTimer = 0f;
    private Vector3 dashTarget;
    private Vector3 startScale;
    private SpriteRenderer sr;
    private bool isInvincible = false;
    private bool useProjectile = false;
    private float walkAnimTimer = 0f;
    private bool usingWalkA = true;
    private float walkAnimSpeed = 0.2f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }

        if (smokeEffect != null) Instantiate(smokeEffect, transform.position, Quaternion.identity);
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Approaching:
                AnimateWalk();
                MoveTowardPlayer();
                break;
            case State.Charging:
                HandleCharge();
                break;
            case State.Attacking:
                break;
            case State.Cooldown:
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f) currentState = State.Approaching;
                break;
            case State.Dying:
                return;
            case State.Dead:
                return;
        }

        Vector3 lookDir = player.position - transform.position;
        sr.flipX = lookDir.x < 0;
    }

    void AnimateWalk()
    {
        if (walkSpriteA == null || walkSpriteB == null || sr == null) return;

        walkAnimTimer += Time.deltaTime;
        if (walkAnimTimer >= walkAnimSpeed)
        {
            walkAnimTimer = 0f;
            usingWalkA = !usingWalkA;
            sr.sprite = usingWalkA ? walkSpriteA : walkSpriteB;
        }
    }

    void MoveTowardPlayer()
    {
        if (player == null) return;
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= projectileDistance && Random.value < projectileChance)
        {
            currentState = State.Charging;
            chargeTimer = chargeTime;
            useProjectile = true;
            return;
        }

        if (dist > approachDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
        else
        {
            currentState = State.Charging;
            chargeTimer = chargeTime;
        }
    }

    void HandleCharge()
    {
        chargeTimer -= Time.deltaTime;

        sr.sprite = useProjectile ? chargeProjectile :  walkSpriteA;

        float flicker = Mathf.Sin(Time.time * 20f) * 0.5f + 0.5f;
        sr.color = Color.Lerp(Color.white, Color.red, flicker);
        float scalePulse = 1f + Mathf.Sin(Time.time * 30f) * 0.1f;
        transform.localScale = startScale * scalePulse;

        if (chargeTimer <= 0f)
        {
            sr.color = Color.white;
            transform.localScale = startScale;
            currentState = State.Attacking;

            if (useProjectile) ThrowProjectile();
            else StartCoroutine(DashAtPlayer());
        }
    }

    IEnumerator DashAtPlayer()
    {
        isInvincible = true;

        if (dashSprite != null) sr.sprite = dashSprite;

        Vector3 dir = (player.position - transform.position).normalized;
        float timer = 0f;

        while (timer < dashDuration)
        {
            timer += Time.deltaTime;
            transform.position += dir * dashSpeed * Time.deltaTime;
            yield return null;
        }

        FinishAttack();
    }

    void ThrowProjectile()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        proj.GetComponent<EnemyProjectile>().SetDirection(direction);
        useProjectile = false;
        FinishAttack();
    }

    void FinishAttack()
    {
        isInvincible = false;
        currentState = State.Cooldown;
        cooldownTimer = attackCooldown;
    }

    public void Die()
    {
        if (currentState == State.Dead || currentState == State.Dying) return;

        currentState = State.Dying;
        sr.enabled = true;
        sr.sprite = woundedSprite;
        if (bloodEffect != null) Instantiate(bloodEffect, transform.position, Quaternion.identity);
        isInvincible = true;
        GetComponent<Collider2D>().enabled = false;
        
        Invoke(nameof(BecomesDead), 1.5f);
    }

    void BecomesDead()
    {
        currentState = State.Dead;
        sr.enabled = true;
        sr.sprite = deadSprite;

        StopAllCoroutines();
        Destroy(this);
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }
}