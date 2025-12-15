using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class Character : Singleton<Character>
{
    private State _currentState;

    [Header("Spine Animation")]
    public SkeletonAnimation skeletonAnimation;

    [Header("Movement")]
    public Rigidbody2D rb;
    public float moveSpeed = 10f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;
    public CapsuleCollider2D boxCollider;
    public int hp = 100;
    public Enemy targetEnemy;
    public bool IsDead => hp <= 0;
    public int dir;
    public float horizontalInput;
    public float verticalInput;
    public List<Skill> skills;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<CapsuleCollider2D>();
    }
    private void Start()
    {
        _currentState = new IdleState(this);
    }

    // private void Update()
    // {

    // }

    public void SetAnimation(string animationName, bool loop = false)
    {
        if (skeletonAnimation == null) return;

        skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
    }

    public void FlipCharacter(int direction)
    {
        if (direction == 0) return;
        skeletonAnimation.skeleton.ScaleX = direction;

    }

    private void FixedUpdate()
    {
        _currentState.Update(horizontalInput, verticalInput);
        horizontalInput = 0;
        verticalInput = 0;
    }

    public void SetState(State newState)
    {
        Debug.Log($"Changing state from {_currentState?.GetType().Name} to {newState.GetType().Name}");
        if (_currentState != null && _currentState.GetType() == newState.GetType())
            return;

        if (_currentState != null)
            _currentState.Exit();

        _currentState = newState;
        _currentState.Enter();
    }
    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0)
        {
            hp = 100;
            Debug.Log("Player died!");
        }
        else
        {
            Debug.Log($"Player took {dmg} damage. HP left: {hp}");
        }
    }
    public bool IsGrounded()
    {
        Bounds bounds = boxCollider.bounds;

        Vector2 left = new Vector2(bounds.min.x, bounds.min.y);
        Vector2 center = new Vector2(bounds.center.x, bounds.min.y);
        Vector2 right = new Vector2(bounds.max.x, bounds.min.y);

        // Debug để xem trong Scene
        Debug.DrawRay(left, Vector2.down * groundCheckDistance, Color.red);
        Debug.DrawRay(center, Vector2.down * groundCheckDistance, Color.green);
        Debug.DrawRay(right, Vector2.down * groundCheckDistance, Color.blue);

        return Physics2D.Raycast(left, Vector2.down, groundCheckDistance, groundLayer) ||
               Physics2D.Raycast(center, Vector2.down, groundCheckDistance, groundLayer) ||
               Physics2D.Raycast(right, Vector2.down, groundCheckDistance, groundLayer);
    }

    public void Initialize(CharacterData data)
    {
        if (data == null) return;
        this.hp = data.hp;
        this.moveSpeed = data.moveSpeed;
        this.skills = data.skills;
        this.dir = 1;
        FlipCharacter(this.dir);
        transform.position = new Vector2(data.x, data.y);
    }
    public void FindTagetEnemyInRange(float range)
    {
        Enemy enemy = FindObjectOfType<Enemy>();
        if (enemy != null)
        {
            SetTargetEnemy(enemy);
        }
    }
    public void Move(int dir)
    {
        this.dir = dir;
        horizontalInput = dir;
        FlipCharacter(dir);
    }

    public void AddVelocityY(int vy)
    {
        verticalInput = vy;
    }

    public void Teleport(Vector3 vector3)
    {
        StartCoroutine(TeleportFadeEffect(vector3));
    }

    private IEnumerator TeleportFadeEffect(Vector3 pos)
    {
        yield return StartCoroutine(FadeManager.Instance.FadeOut(0.2f));
        yield return new WaitForSeconds(2f);
        transform.position = pos;
        rb.velocity = Vector2.zero;
        SetState(new IdleState(this));
        yield return StartCoroutine(FadeManager.Instance.FadeIn(0.2f));
    }

    public void SetTargetEnemy(Enemy enemy)
    {
        if (enemy == null || enemy.IsDead)
        {
            targetEnemy = null;
            return;
        }

        targetEnemy = enemy;
    }
    public void ResetState()
    {
        SetState(new IdleState(this));
    }
}
