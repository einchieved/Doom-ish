using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BotControl : MonoBehaviour
{
    private const string BULLET_TAG = "Bullet";
    private const string PLAYER_TAG = "Player";

    public MainControl mainControl;

    public Transform player;
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundLayer;

    public GameObject flame;

    private NavMeshAgent agent;
    private Animator animator;

    private Vector3 previousPosition;
    private float previousRotation;

    public int damagePerBullet = 25;
    public GameObject playerObject;
    public LayerMask playerLayer;

    private int health;
    private bool isAlive = true;

    private float attackDelay = 10f;
    private float remainingAttackDelay;
    private float attackRange = 100f;

    // Called before Start
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        previousPosition = transform.position;
        previousRotation = transform.rotation.eulerAngles.y;

        health = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive)
        {
            return;
        }

        animator.SetBool("isHit", false);
        animator.SetBool("isShooting", false);

        if (remainingAttackDelay > 0f)
        {
            remainingAttackDelay -= Time.deltaTime;
        }

        // calculate speed for animation
        Vector3 curMove = transform.position - previousPosition;
        float curSpeed = curMove.magnitude / Time.deltaTime;
        previousPosition = transform.position;

        // calculate rotation for animation
        float curRotation = transform.rotation.eulerAngles.y - previousRotation;
        float curAngle = curRotation / 100 / Time.deltaTime;
        previousRotation = transform.rotation.eulerAngles.y;

        // set speed and rotation for this frame
        animator.SetFloat("Forward", Mathf.Clamp(curSpeed, 0, 1));
        animator.SetFloat("Turn", Mathf.Clamp(curAngle, -1f, 1f));

        animator.SetBool("OnGround", Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer));

        DetermineAction();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals(BULLET_TAG))
        {
            //DealDamage();
            Destroy(other.gameObject);
        }
    }

    public void DealDamage()
    {
        health -= damagePerBullet;
        if (health <= 0 && isAlive)
        {
            isAlive = false;
            animator.applyRootMotion = true;
            animator.SetBool("isDead", true);
            Destroy(GetComponent<CapsuleCollider>());
            Destroy(GetComponent<NavMeshAgent>());
            mainControl.ReportDestruction();
            Invoke(nameof(SelfDestruction), 10f);
        }
        else
        {
            animator.SetBool("isHit", true);
        }
    }

    private void SelfDestruction()
    {
        Destroy(gameObject);
    }

    private void DetermineAction()
    {
        Vector3 sightDirection;
        bool los = HasLineOfSight(out sightDirection);
        if (los && IsFacedToPlayer(sightDirection, transform.forward))
        {
            StopMoving();
            if (animator.GetCurrentAnimatorStateInfo(1).IsName("aiming"))
            {
                Attack();
            }
        }
        else
        {
            // Move towards the player
            agent.SetDestination(player.position);
        }
    }

    private bool HasLineOfSight(out Vector3 sightDirection)
    {
        Vector3 dir = player.position - transform.position;
        sightDirection = dir.normalized;

        return Physics.Raycast(transform.position, sightDirection, out RaycastHit hit, attackRange, playerLayer) && hit.collider.CompareTag(PLAYER_TAG);
    }

    private void StopMoving()
    {
        agent.SetDestination(transform.position);
        animator.SetFloat("Forward", 0f);
        animator.SetFloat("Turn", 0f);
    }

    private void Attack()
    {
        if (remainingAttackDelay <= 0f)
        {
            animator.SetBool("isShooting", true);
            remainingAttackDelay = attackDelay;
            EnableFlame();
            Invoke(nameof(DisableFlame), 0.2f);
            int rnd = Random.Range(1, 6);
            if (rnd == 1)
            {
                Invoke(nameof(AttackDamage), 0.1f);
            }
        }
    }

    private void AttackDamage()
    {
        playerObject.GetComponent<PlayerMovement>().DealDamage();
    }

    private void EnableFlame()
    {
        flame.SetActive(true);
    }

    private void DisableFlame()
    {
        flame.SetActive(false);
    }

    private bool IsFacedToPlayer(Vector3 sightDirection, Vector3 forward)
    {
        Vector3 norm = Vector3.zero;
        norm.x = sightDirection.x;
        norm.z = sightDirection.z;
        norm = norm.normalized;
        float diffX = norm.x - forward.x;
        float diffZ = norm.z - forward.z;
        // Requiring these values to be equal would result in Ethan getting way to close to the player
        return diffX > -0.1f && diffX < 0.1f && diffZ > -0.1f && diffZ < 0.1f;
    }
}
