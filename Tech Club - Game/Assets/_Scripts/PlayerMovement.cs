using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] MoveState moveState;

    [Space(10)]

    [SerializeField] CharacterController charCont;
    [SerializeField] Transform rotator;

    [Header("Walking"), Space(10)]
    [SerializeField] float currentStamina;
    [SerializeField] float maxStamina = 100;
    [SerializeField] float staminaRecoveryTimer = 100;

    [Header("Walking"), Space(10)]

    [SerializeField] float maxAcceleration;
    [SerializeField] float maxSpeed;
    [SerializeField] float runSpeedMod;
    [SerializeField] float rollHoldTimer;
    [SerializeField] float rollTimer;

    [SerializeField] bool isRunning;


    [Header("Gliding"), Space(10)]

    [SerializeField] Vector3 glideVector;

    [SerializeField] float maxGlidingAcceleration_H;
    [SerializeField] float maxGlidingAcceleration_V;
    [SerializeField] float maxGlideHorizontal;
    [SerializeField] float maxGlideVertical;
    [SerializeField] float updraftMagnitude;
    [SerializeField] float maxUpdraftSpeed;


    [SerializeField] bool isGliding;

    [SerializeField] GameObject glider;

    [Space(10)]


    [SerializeField] Vector3 velocity;


    [SerializeField] float jumpPower;
    [SerializeField] bool isGrounded;
    [SerializeField] float gravity;

    [Space(10)]


    [SerializeField] LayerMask ground;
     
    // Start is called before the first frame update
    void Start()
    {
        SetStats();
        

    }

    void SetStats ()
    {
        currentStamina = maxStamina;
        staminaRecoveryTimer = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            charCont.Move(-transform.position);

        switch (moveState)
        {
            case MoveState.Default:

                if (isGliding && !isGrounded)
                {
                    if (Input.GetKeyDown(KeyCode.G))
                    {
                        isGliding = false;
                        glider.SetActive(false);

                    }

                    GlidingUpdate();

                    CheckIfGrounded();
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
                    {
                        Jump();
                    }
                    if (!isGrounded && currentStamina > 0 && Input.GetKeyDown(KeyCode.Space))
                    {
                        CheckClimb();
                    }

                    if (Input.GetKeyDown(KeyCode.G))
                    {
                        isGliding = !isGliding;
                        glider.SetActive(isGliding);
                    }

                    if (Input.GetKey(KeyCode.LeftShift) && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) && rollHoldTimer == 25)
                        isRunning = true;
                    else
                        isRunning = false;

                    if (!isGrounded)
                        velocity.y += gravity * Time.deltaTime;

                    charCont.Move(velocity * Time.deltaTime);

                    CheckIfGrounded();

                    MovementUpdate ();

                    if (staminaRecoveryTimer >= 100)
                    {
                        RestoreStamina(100/5 * Time.deltaTime);
                    }
                    else {
                        staminaRecoveryTimer += 100/2 * Time.deltaTime;

                        staminaRecoveryTimer =  Mathf.Clamp(staminaRecoveryTimer, 0, 100);
                    }

                    updraftMagnitude = 0;

                }

                break;

            case MoveState.Climb:
                Vector2 climbInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                climbInput.Normalize();
                
                UpdateClimb(climbInput);

                if (currentStamina <= 0)
                    moveState = MoveState.Default;


                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Jump();
                    SpendStamina(25);
                    moveState = MoveState.Default;
                }



                break;
        }


    }

    void GlidingUpdate ()
    {
        Vector3 desiredVelocity = Vector3.zero;

        Vector3 rotatorDirection = rotator.forward;

        rotatorDirection.Normalize();

        transform.rotation = Quaternion.Euler(0, (Mathf.Atan2(rotatorDirection.x, rotatorDirection.z) * Mathf.Rad2Deg), 0);




        Vector3 glideAngle = transform.forward * 2 + transform.right * Input.GetAxisRaw("Horizontal");



        if (Input.GetKey(KeyCode.Space) && currentStamina > 0 && !(Input.GetAxisRaw("Vertical") < 0))
        {
            updraftMagnitude += 18.75f * Time.deltaTime;
            SpendStamina(9 * Time.deltaTime);
        }
        else
            updraftMagnitude -= 50 * Time.deltaTime;

        updraftMagnitude = Mathf.Clamp(updraftMagnitude, 0, 100);

        desiredVelocity = glideAngle.normalized * maxGlideHorizontal * (Input.GetAxisRaw("Vertical") < 0 ? 0.2f : 1);
        desiredVelocity.y = maxGlideVertical * (Input.GetAxisRaw("Vertical") < 0 ? 5f : 1) + maxUpdraftSpeed * updraftMagnitude / 100;

        float maxSpeedChangeHorizontal = maxGlidingAcceleration_H * Time.deltaTime;
        float maxSpeedChangeVertical = maxGlidingAcceleration_V * Time.deltaTime;


        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChangeHorizontal);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChangeHorizontal);
        velocity.y = Mathf.MoveTowards(velocity.y, desiredVelocity.y, maxSpeedChangeVertical);

        Vector3 displacement = velocity * Time.deltaTime;

        charCont.Move(displacement);
    }

    void MovementUpdate() {

        Vector3 desiredVelocity = Vector3.zero;

        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            Vector3 rotatorDirection = rotator.forward * Input.GetAxisRaw("Vertical") + rotator.right * Input.GetAxisRaw("Horizontal");

            rotatorDirection.Normalize();

            transform.rotation = Quaternion.Euler(0, (Mathf.Atan2(rotatorDirection.x, rotatorDirection.z) * Mathf.Rad2Deg), 0);

            desiredVelocity = transform.forward * maxSpeed * (isRunning && currentStamina > 0 ? runSpeedMod : 1);

            if (isRunning && currentStamina > 0)
                SpendStamina(20 * Time.deltaTime);

        }

        if (Input.GetKey (KeyCode.LeftShift))
        {
            rollHoldTimer += 100 * Time.deltaTime;

            rollHoldTimer = Mathf.Clamp(rollHoldTimer, 0, 25);
        } else if (Input.GetKeyUp (KeyCode.LeftShift))
        {
            if (rollHoldTimer < 25 && currentStamina >= 25)
            {
                Roll();
            }

            rollHoldTimer = 0;
        }


        float maxSpeedChange = maxAcceleration * (isRunning ? runSpeedMod : 1) * Time.deltaTime;

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange * (rollTimer > 0 ? 3 : 1));
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange * (rollTimer > 0 ? 3 : 1));


        if (rollTimer >= 0)
        {
            rollTimer -= 100 * Time.deltaTime;
            rollTimer = Mathf.Clamp(rollTimer, 0, 25);
        }

        Vector3 displacement = velocity * Time.deltaTime;

        charCont.Move(displacement);

    }

    void Roll() {
        velocity = transform.forward * 25;

        rollTimer = 25;

        SpendStamina(25);
    }

    void Jump() {
        velocity.y = Mathf.Sqrt(jumpPower * -2 * gravity);
    }

    void CheckIfGrounded() {
        if (Physics.BoxCast(transform.position, Vector3.one * 0.5f, transform.up * -1, transform.rotation, 0.75f, ground.value)) {
            isGrounded = true;
        } else {
            if (isGrounded)
            {
                velocity.y = Mathf.Clamp(velocity.y, 0, Mathf.Infinity);
            }

            isGrounded = false;
        }
    }

    Vector3 climbPoint;

    bool CheckClimb()
    {
        RaycastHit hit;

        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out hit, 1, ground))
        {
            InititializeClimb(hit.point, hit.normal);

            return true;
        }

        moveState = MoveState.Default;
        return false;
    }


    //Use two raycasts to check for surfaces
    bool UpdateClimb(Vector2 input)
    {
        Vector3 desiredMove;
        RaycastHit hit;


        for (int i = 1; i < 15; i++)
        {

            Ray ray = new Ray(transform.position, Vector3.Lerp(transform.forward, transform.right * input.x, 2 / i));


            if (Physics.Raycast(ray, out hit, 0.5f + (0.5f * i / 15), ground))
            {
                climbPoint = hit.point;

                desiredMove = transform.up * input.y + Vector3.Cross(hit.normal, Vector3.up).normalized * input.x;
                desiredMove.Normalize();

                if (desiredMove.magnitude > 0)
                    SpendStamina(15 * Time.deltaTime);

                desiredMove *= maxSpeed * Time.deltaTime;

                charCont.Move(desiredMove);
                transform.rotation = Quaternion.Euler(0, (Mathf.Atan2(-hit.normal.x, -hit.normal.z) * Mathf.Rad2Deg), 0);

                moveState = MoveState.Climb;
                isRunning = false;

                velocity = Vector3.zero;

                return true;
            }
        }

        if (input.y> 0) {
            Jump();

        }



        moveState = MoveState.Default;
        return false;
    }

    void InititializeClimb(Vector3 point, Vector3 normal) {
        Vector3 desiredPosition;
        
        climbPoint = point;

        transform.rotation = Quaternion.Euler(0, (Mathf.Atan2(-normal.x, -normal.z) * Mathf.Rad2Deg), 0);

        desiredPosition = point;
        desiredPosition += normal * 0.5f;

        charCont.Move(desiredPosition - transform.position);
        
        moveState = MoveState.Climb;
        isRunning = false;

        velocity = Vector3.zero;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(climbPoint, 0.1f);

        Gizmos.DrawLine(transform.position, transform.position + transform.forward *1);

        Gizmos.color = Color.blue;

        Gizmos.DrawLine(transform.position, transform.position + velocity.normalized * 1);

    }


    //Stat Calculations:
    public float SpendStamina (float amount)
    {
        currentStamina -= amount;

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        staminaRecoveryTimer = 0;


        return amount;
    }

    public float RestoreStamina(float amount)
    {
        currentStamina += amount;

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);


        return amount;
    }



    public float CurrentStamina
    {
        get
        {
            return currentStamina;
        }
    }

    public float MaxStamina
    {
        get
        {
            return maxStamina;
        }
    }

    public Vector3 Velocity
    {
        get
        {
            return velocity;
        }
    }

    public MoveState Move_State
    {
        get
        {
            return moveState;
        }
    }
}

public enum MoveState {
    Default,
    Climb,
    Swimming,
    Mounted
}