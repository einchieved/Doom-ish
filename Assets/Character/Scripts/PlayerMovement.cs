using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private const int MOVE_GROUND = 0;
    private const int MOVE_CLIMB = 1;
    private const int MOVE_MIXED = 2;

    private const string STATUS_READY = "Ready";
    private const string STATUS_USED = "Used";

    public GameObject gun;

    public float speed = 12f;
    public float gravity = -9.81f;
    public float wallJumpXAndZEffect = 1f;
    public float jumpHeight = 2f;

    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundDistance = 0.1f;

    public float dashCooldown = 10f;
    public float maxDashDistance = 10f;
    public float dashSpeed = 20f;

    public LayerMask climbLayer;
    public Transform climbCheck;
    public float climbDistance;

    public LayerMask jumpLayer;
    public float jumpPadHeightMultiplyer = 2f;

    public float grapplingSpeed = 5f;
    public Transform cam;
    public LayerMask grapplingLayer;

    public UnityEngine.UI.Text dashStatus;
    public UnityEngine.UI.Text doubleJumpStatus;
    public UnityEngine.UI.Text hp;

    private CharacterController controller;
    private bool isGrounded;
    private bool hasDoubleJump = false;
    private bool isDashing = false;
    private float dashCooldownRemain;
    private float dashedDistance;
    private Vector3 velocity;

    private bool isClimbing;
    private float remaingwallJumpTime;
    private Vector3 grapplePoint;
    private GameObject grappledTo;
    private Vector3 grappleDiff;

    private LineRenderer lineRenderer;
    private PlayerLook playerLook;

    private bool isAlive = true;
    private float health = 100f;
    private float damagePerHit = 10f;
    public GameObject damageFedback;

    public GameObject bombPrefab;
    public float throwForce = 10f;

    // Start is called before the first frame update
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        lineRenderer = GetComponent<LineRenderer>();

        playerLook = cam.GetComponent<PlayerLook>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive)
        {
            return;
        }

        if (dashCooldownRemain > 0f)
        {
            dashCooldownRemain -= Time.deltaTime;
        }

        if (Input.GetButton("Fire2"))
        {
            GrapplingMovement();
        }
        else
        {
            TriMovement();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            GameObject bomb = Instantiate(bombPrefab, gun.transform.position, Quaternion.identity);
            bomb.GetComponent<Rigidbody>().AddForce(cam.forward * throwForce, ForceMode.Impulse);
        }

        if (Input.GetButtonUp("Fire2"))
        {
            grappledTo = null;
        }
    }

    // Called after Update
    private void LateUpdate()
    {
        // draw grappling line
        if (!gun.activeSelf)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, gun.transform.position);
            lineRenderer.SetPosition(1, grapplePoint);
        }
        else
        {
            lineRenderer.positionCount = 0;
        }

        UpdateUI();
    }

    private void GrapplingMovement()
    {
        //standard motion
        float x = Input.GetAxis("Horizontal");
        // float z = Input.GetAxis("Vertical"); not needed -> grappling pulls toward grappling point

        RaycastHit hit;
        Debug.DrawRay(cam.position, cam.forward, Color.red, 1f);
        if (Physics.Raycast(cam.position, cam.forward, out hit, 125f, grapplingLayer) || grappledTo != null)
        {
            Vector3 move;
            if (grappledTo == null)
            {
                move = cam.forward;
                grappledTo = hit.collider.gameObject;
                grapplePoint = hit.point;
                grappleDiff = grapplePoint - grappledTo.transform.position;
            }
            else
            {
                move = (grappledTo.transform.position - transform.position + grappleDiff).normalized;
                grapplePoint = grappledTo.transform.position + grappleDiff;
            }
            controller.Move(move * grapplingSpeed * Time.deltaTime);
            gun.SetActive(false);
            // disable player look
            playerLook.SetLookControlActive(false);
        }
        else
        {
            // if missed, the player should still be able to move
            TriMovement();
        }
    }

    private void TriMovement()
    {
        gun.SetActive(true);
        playerLook.SetLookControlActive(true);

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

        if (isGrounded)
        {
            hasDoubleJump = true;
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -1f;
        }

        if (remaingwallJumpTime > 0)
        {
            remaingwallJumpTime -= Time.deltaTime;
        }
        else
        {
            velocity.x = 0f;
            velocity.z = 0f;
        }

        if (Physics.CheckSphere(groundCheck.position, groundDistance, jumpLayer))
        {
            PrepareJumpPad();
        }

        //standard motion
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        isClimbing = Physics.CheckSphere(climbCheck.position, climbDistance, climbLayer);

        if (!isClimbing)
        {
            // Moving
            PrepareMove(z, x, MOVE_GROUND);
            //Jumping
            PrepareJump();
            // Dashing
            PrepareDash();
        }
        else if (isGrounded && isClimbing)
        {
            // Moving
            PrepareMove(z, x, MOVE_MIXED);
            // Jumping
            PrepareJump();
        }
        else
        {
            // Moving
            PrepareMove(z, x, MOVE_CLIMB);
            // Jumping
            PrepareWallJump();
        }
    }

    private void PrepareMove(float upAndForward, float side, int mode)
    {
        Vector3 move = Vector3.zero;
        switch (mode)
        {
            case MOVE_GROUND:
                move = transform.right * side + transform.forward * upAndForward;
                break;

            case MOVE_CLIMB:
                move = transform.up * upAndForward + transform.right * side;
                break;

            case MOVE_MIXED:
                if (upAndForward > 0)
                {
                    move += transform.up * upAndForward;
                }
                else
                {
                    move += transform.forward * upAndForward;
                }
                move += transform.right * side;
                break;
        }
        controller.Move(move * speed * Time.deltaTime);
    }

    private void PrepareJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else if (Input.GetButtonDown("Jump") && hasDoubleJump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            hasDoubleJump = false;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void PrepareDash()
    {
        Vector3 dashDirection = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownRemain <= 0f && !isDashing)
        {
            dashDirection = transform.forward * maxDashDistance * Time.deltaTime * dashSpeed;
            dashedDistance = maxDashDistance * Time.deltaTime * dashSpeed;
            isDashing = true;
            dashCooldownRemain = dashCooldown;
        }
        else if (isDashing)
        {
            dashDirection = transform.forward * maxDashDistance * Time.deltaTime * dashSpeed;
            dashedDistance += maxDashDistance * Time.deltaTime * dashSpeed;
        }

        if (dashedDistance >= maxDashDistance)
        {
            isDashing = false;
        }

        controller.Move(dashDirection);
    }

    private void PrepareJumpPad()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity) * jumpPadHeightMultiplyer;
        controller.Move(velocity * Time.deltaTime);
    }

    private void PrepareWallJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            Vector3 dir = transform.forward * -20f;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            velocity.x = dir.x;
            velocity.z = dir.z;

            remaingwallJumpTime = wallJumpXAndZEffect;
        }

        Vector3 v = new Vector3(velocity.x, 0, velocity.z); // with velocity.y the character would keep falling down from the wall
        controller.Move(v * Time.deltaTime);
    }

    private void UpdateUI()
    {
        if (hasDoubleJump)
        {
            doubleJumpStatus.text = STATUS_READY;
        }
        else
        {
            doubleJumpStatus.text = STATUS_USED;
        }

        if (dashCooldownRemain > 0f)
        {
            float remain = Mathf.Round(dashCooldownRemain * 100f) / 100f;
            dashStatus.text = remain + "s";
        }
        else
        {
            dashStatus.text = STATUS_READY;
        }
    }

    public void DealDamage()
    {
        health -= damagePerHit;
        EnableHitFeedback();
        Invoke(nameof(DisableHitFeedback), 0.1f);
        if (health <= 0f)
        {
            hp.text = "0";
            isAlive = false;
        }
        else
        {
            hp.text = Mathf.Round(health).ToString();
        }
    }

    private void EnableHitFeedback()
    {
        damageFedback.SetActive(true);
    }

    private void DisableHitFeedback()
    {
        damageFedback.SetActive(false);
    }

    public float GetHealth()
    {
        return health;
    }

    public void Revive()
    {
        health = 100f;
        isAlive = true;
        dashCooldownRemain = 0f;
        velocity.y = -1f;
        isDashing = false;
        isClimbing = false;
        remaingwallJumpTime = 0f;
        grappledTo = null;
    }
}
