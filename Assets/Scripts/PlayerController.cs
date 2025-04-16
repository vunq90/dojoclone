using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private float moveSpeed = 3f;
    private float dashSpeed = 20f;
    private float dashDistance = 2f;

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LineRenderer slashLine;
    [SerializeField] private Transform arrowIndicator;
    [SerializeField] private Sprite walkSpriteA;
    [SerializeField] private Sprite walkSpriteB;
    [SerializeField] private Sprite slashSprite;
    [SerializeField] private Sprite woundedSprite;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private ParticleSystem bloodEffect;

    private float arrowRadius = 0.3f;
    private float dashCooldown = 0.4f;
    private float walkAnimSpeed = 0.15f;

    private Vector2 moveInput;
    private Rigidbody2D rb;
    private bool isDashing = false;
    private bool dashOnCooldown = false;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;
    private Vector3 dashStart;
    private Vector3 dashTarget;
    private Vector2 lastMoveDirection = Vector2.right;
    private SpriteRenderer spriteRenderer;
    private bool restrictMovement = false;
    private bool isInvincible = false;
    private float walkAnimTimer = 0f;
    private bool usingWalkA = true;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (slashLine != null)
        {
            slashLine.positionCount = 0;
        }

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }
    }

    void Update()
    {
        if (isDead) return;

        if (!isDashing && !restrictMovement)
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            moveInput = input.normalized;

            if (input != Vector2.zero)
            {
                lastMoveDirection = input;

                if (input.x != 0)
                {
                    spriteRenderer.flipX = input.x < 0;
                }

                AnimateWalk();
            }
            else
            {
                walkAnimTimer = 0f;
                usingWalkA = true;
                if (walkSpriteA != null)
                {
                    spriteRenderer.sprite = walkSpriteA;
                }
            }

            if (Input.GetKeyDown(KeyCode.J) && !dashOnCooldown)
            {
                dashDirection = lastMoveDirection.normalized;
                if (dashDirection == Vector2.zero)
                {
                    dashDirection = Vector2.right;
                }

                dashStart = transform.position;
                dashTarget = dashStart + (Vector3)(dashDirection * dashDistance);
                isDashing = true;
                dashOnCooldown = true;
                restrictMovement = true;
                isInvincible = true;
                dashCooldownTimer = dashCooldown;

                if (slashLine != null)
                {
                    slashLine.positionCount = 2;
                    slashLine.SetPosition(0, dashStart);
                    slashLine.SetPosition(1, dashTarget);
                }

                if (slashSprite != null)
                {
                    spriteRenderer.sprite = slashSprite;
                }
            }
        }

        if (arrowIndicator != null)
        {
            Vector3 direction = new Vector3(lastMoveDirection.normalized.x, lastMoveDirection.normalized.y, 0f);
            arrowIndicator.localPosition = direction * arrowRadius;
            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                arrowIndicator.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        if (dashOnCooldown)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f)
            {
                dashOnCooldown = false;
                restrictMovement = false;
            }
        }
    }

    void AnimateWalk()
    {
        if (walkSpriteA == null || walkSpriteB == null || spriteRenderer == null) return;

        walkAnimTimer += Time.deltaTime;
        if (walkAnimTimer >= walkAnimSpeed)
        {
            walkAnimTimer = 0f;
            usingWalkA = !usingWalkA;
            spriteRenderer.sprite = usingWalkA ? walkSpriteA : walkSpriteB;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (isDashing)
        {
            Vector3 direction = (dashTarget - transform.position).normalized;
            float step = dashSpeed * Time.fixedDeltaTime;

            if (Vector3.Distance(transform.position, dashTarget) <= step)
            {
                transform.position = dashTarget;
                EndDash();
            }
            else
            {
                rb.MovePosition(transform.position + direction * step);
            }
        }
        else if (!restrictMovement)
        {
            rb.velocity = moveInput * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    void EndDash()
    {
        isDashing = false;
        isInvincible = false;
        rb.velocity = Vector2.zero;

        if (slashLine != null)
        {
            slashLine.positionCount = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDashing && ((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsInvincible())
            {
                enemy.Die();
            }
        }
        else if (!isInvincible && ((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && enemy.IsInvincible())
            {
                Die();
            }
        }
    }

    public void Die()
    {
        StartCoroutine(HandleDeathSequence());
    }

    private IEnumerator HandleDeathSequence()
    {
        isDead = true;
        isInvincible = true;
        restrictMovement = true;
        rb.velocity = Vector2.zero;

        Time.timeScale = 0.2f;

        if (woundedSprite != null)
        {
            spriteRenderer.sprite = woundedSprite;
        }

        if (bloodEffect != null)
        {
            Instantiate(bloodEffect, transform.position, Quaternion.identity);
        }

        yield return new WaitForSecondsRealtime(3f);

        Time.timeScale = 1f;

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Just in case it's still slowed
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
