using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
   public enum PlayerStates
    {
        grounded,
        inair,
        onwall,
        ledgegrab,
    }



    [Header("Physics")]
    public float MaxSpeed; //how fast we run forward
    public float BackwardsSpeed; //how fast we run backwards
    //0[Range(0,1)]
    public float InAirControl; //how much control you have over your movement direction when in air

    private float ActSpeed; //how much speed is applied to the rigidbody
    public float Acceleration; //how fast we build speed
    public float Decceleration; //how fast we slow down
    public float DirectionControl = 8; //how much control we have over changing direction
    public PlayerStates CurrentState; //the current state the player is in
    private float InAirTimer; //how long we are in the air for (this is for use when wall running or falling off the ground
    private float OnGroundTimer;
    private float AdjustmentAmt; //the amount added to our player acceleration, this is used for adjusting to new speeds such as when we slide


    [Header("Turning")]
    public float TurnSpeed; //how fast we turn when on the ground
    public float TurnSpeedInAir; //how fast we turn when in air
    public float TurnSpeedOnWalls; //how fast we turn when on the walls
    public float LookUpSpeed; //how fast we look up and down
    public Camera Head; //what will function as our players head to tilt up and down (this is a pivot point in our model that the cameras are children of
    private float YTurn; //how much we have turned left and right
    private float XTurn; //how much we have turned Up or Down
    public float MaxLookAngle = 65; //how much we can look up
    public float MinLookAngle = -30; //how much we can look down

    Vector2 currentMouseDelta = Vector2.zero;
    Vector2 currentMouseDeltaVelocity = Vector2.zero;

    [SerializeField] [Range(0, 0.5f)] float moveSmoothTime = 0.3f;
    [SerializeField] [Range(0, 0.5f)] float mouseSmoothTime = 0.3f;

    [SerializeField] Transform playerCamera = null;
    [SerializeField] float mouseSense = 3.5f;
    float cameraPitch = 0;

    [Header("WallRuns")]
    public float WallRunTime = 2f; //how long we can run on walls
    private float ActWallTime = 0; //how long we are actually on a wall
    public float TimeBeforeWallRun = 0.2f; //how long we have to be in the air before we can wallrun
    public float WallRunUpwardsMovement = 2f; //how much we move up a wall when running on it (make this 0 to just slightly move down a wall we run on
    public float WallRunSpeedAcceleration = 2f; //how quickly we build speed to run up walls


    [Header("Jumping")]
    public float JumpHeight; //how high we jump
    public float SlideSpeedLimit; //how fast we have to be traveling before a crouch will trigger a slide
    public float SlideAmt; //how much we adjust to our slide speed and regain player control

    [Header("Crouching")]
    public float CrouchSpeed = 10; //how fast we move when crouching
    public float CrouchHeight = 1.5f; //how tall our capsule will be when crouched
    private float StandingHeight = 2f; //this is how tall our capsule is
    private bool Crouch;


    [Header("LedgeGrab")]
    public float PullUpTime = 0.5f; //the time it takes to pull onto a ledge
    private float ActPullUpTime; //the actual time it takes to pull up a ledge
    private Vector3 OrigPos; //the original Position before grabbing a ledge
    private Vector3 LedgePos; //the ledge position to move to

    [Header("Sliding")]
    public float PlayerControl;

    private PlayerCollision Coli;
    private Rigidbody Rigid;
    private CapsuleCollider Cap;
    private Animator Anim;

    void Start()
    {
        Coli = GetComponent<PlayerCollision>();
        Rigid = GetComponent<Rigidbody>();
        Anim = GetComponentInChildren<Animator>();
        Cap = GetComponent<CapsuleCollider>();
        StandingHeight = Cap.height;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        AdjustmentAmt = 1;
    }

    private void Update()
    {
        float XMOV = Input.GetAxis("Horizontal");
        float YMOV = Input.GetAxis("Vertical");

        UpdateMouseLook();

        if (CurrentState == PlayerStates.grounded)
        {

            if (Input.GetButtonDown("Jump"))
            {
                //jump upwards
                JumpUp();
            }

            if(Input.GetButton("Crouching"))
            {
                if(!Crouch)
                {
                    StartCrouching();
                }
            }
            else
            {
                bool Check = Coli.CheckRoof(transform.up);
                if (!Check)
                    StopCrouching();
            }

            bool checkG = Coli.CheckFloor(-transform.up);
            if(!checkG)
            {
                InAir();
            }
        }
        else if (CurrentState == PlayerStates.inair)
        {
            
            if (Input.GetButton("Grab"))
            {
                
                Vector3 Ledge = Coli.CheckLedges();
                if (Ledge != Vector3.zero)
                {
                    LedgeGrab(Ledge);
                    return;
                }
            }

            bool Wall = CheckWall(XMOV,YMOV);
            if(Wall)
            {
                WallRun();
                return;
            }

            bool checkG = Coli.CheckFloor(-transform.up);
            if (checkG && InAirTimer > 0.2f)
            {
                OnGround();
            }
        }
        else if (CurrentState == PlayerStates.ledgegrab)
        {
            Rigid.velocity = Vector3.zero;
        }
        else if (CurrentState == PlayerStates.onwall)
        {
            bool Wall = CheckWall(XMOV, YMOV);
            if (!Wall)
            {
                InAir();
                return;
            }

            bool onGround = Coli.CheckFloor(-transform.up);
            if (onGround)
            {
                OnGround();
            }
        }
    }

    private void FixedUpdate()
    {
        float Del = Time.deltaTime;

        float XMOV = Input.GetAxis("Horizontal");
        float YMOV = Input.GetAxis("Vertical");

        //float CamX = Input.GetAxis("MouseX");
        //float CamY = Input.GetAxis("MouseY");

        //LookUpDown(CamY, Del);


        if (CurrentState == PlayerStates.grounded)
        {
            if (OnGroundTimer < 10)
                OnGroundTimer += Del;

            float InputMagnitude = new Vector2(XMOV, YMOV).normalized.magnitude;
            float TargetSpd = Mathf.Lerp(BackwardsSpeed, MaxSpeed, YMOV);
            if (Crouch)
                TargetSpd = CrouchSpeed;

            LerpSpeed(InputMagnitude, Del, TargetSpd);

            MovePlayer(XMOV, YMOV, Del, 1);
           // TurnPlayer(CamX, Del, TurnSpeed);

            if (AdjustmentAmt < 1)
                AdjustmentAmt += Del * PlayerControl;
            else
                AdjustmentAmt = 1;


        }
        else if (CurrentState == PlayerStates.inair)
        {
            Debug.Log("InAir");
            if (InAirTimer < 10)
                InAirTimer += Del;

           MovePlayer(XMOV, YMOV, Del, InAirControl );

            //turn our player with the in air modifier
           // TurnPlayer(CamX, Del, TurnSpeedInAir);
        }
        else if(CurrentState == PlayerStates.ledgegrab)
        {
            ActPullUpTime += Del;

            float pullUpLerp = ActPullUpTime / PullUpTime;

            if(pullUpLerp < 0.5)
            {
                float lamt = pullUpLerp * 2;
                Vector3 LPos = new Vector3(OrigPos.x, LedgePos.y, OrigPos.z);
                transform.position = Vector3.Lerp(OrigPos, LPos, lamt) ;
            }
            else if(pullUpLerp <= 1)
            {
                if(OrigPos.y != LedgePos.y)
                {
                    OrigPos = new Vector3(transform.position.x, LedgePos.y, transform.position.z);
                }

                float lamt = (pullUpLerp - 0.5f) * 2f;
                transform.position = Vector3.Lerp(OrigPos, LedgePos, pullUpLerp);
            }
            else
            {
                OnGround();
            }
        }
        else if (CurrentState == PlayerStates.onwall)
        {
            ActWallTime += Del;

           // TurnPlayer(CamX, Del, TurnSpeedOnWalls);

            WallRunMovement(YMOV, Del);
        }
    }



    void UpdateMouseLook()
    {
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("MouseX"), Input.GetAxis("MouseY"));

        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        cameraPitch -= currentMouseDelta.y * mouseSense;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        playerCamera.localEulerAngles = Vector3.right * cameraPitch;

        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSense);
    }
    void JumpUp()
    {
        //only jump if we are still on the ground
        if (CurrentState == PlayerStates.grounded)
        {
            //reduce our velocity on the y axis so our jump force can be added
            Vector3 VelAmt = Rigid.velocity;
            VelAmt.y = 0;
            Rigid.velocity = VelAmt;
            //add our jump force
            Rigid.AddForce(transform.up * JumpHeight, ForceMode.Impulse);
            //we are now in the air
            InAir();
        }
    }

    void LerpSpeed(float InputMag, float D, float TargetSpeed)
    {
        //multiply our speed by our input amount
        float LerpAmt = TargetSpeed * InputMag;
        //get our acceleration (if we should speed up or slow down
        float Accel = Acceleration;
        if (InputMag == 0)
            Accel = Decceleration;
        //lerp by a factor of our acceleration
        ActSpeed = Mathf.Lerp(ActSpeed, LerpAmt, D * Accel);
    }

    void MovePlayer(float Hor, float Ver, float D, float Control)
    {
        //find the direction to move in, based on the direction inputs
        Vector3 MovementDirection = (transform.forward * Ver) + (transform.right * Hor);
        MovementDirection = MovementDirection.normalized;
        //if we are no longer pressing and input, carryon moving in the last direction we were set to move in
        if (Hor == 0 && Ver == 0)
            MovementDirection = Rigid.velocity.normalized;

        MovementDirection = MovementDirection * ActSpeed;

        //apply Gravity and Y velocity to the movement direction 
        MovementDirection.y = Rigid.velocity.y;

        //apply adjustment to acceleration
        float Acel = (DirectionControl * AdjustmentAmt) * Control;
        Vector3 LerpVelocity = Vector3.Lerp(Rigid.velocity, MovementDirection, Acel * D);
        Rigid.velocity = LerpVelocity;
    }
    void TurnPlayer(float Hor, float D, float turn)
    {
        //add our inputs to our turn value
        YTurn += (Hor * D) * turn;
        //turn our character
        transform.rotation = Quaternion.Euler(0, YTurn, 0);
    }

    void LookUpDown(float Ver, float D)
    {
        //add our inputs to our look angle
        XTurn -= (Ver * D) * LookUpSpeed;
        XTurn = Mathf.Clamp(XTurn, MinLookAngle, MaxLookAngle);
        //look up and down
        Head.transform.localRotation = Quaternion.Euler(XTurn, 0, 0);
    }

    void InAir()
    {
        InAirTimer = 0;
        CurrentState = PlayerStates.inair;
        if (Crouch)
            StopCrouching();
    }
    void OnGround()
    {
        OnGroundTimer = 0;
        ActWallTime = 0;
        CurrentState = PlayerStates.grounded;
    }

    void LedgeGrab(Vector3 LPos)
    {
        LedgePos = LPos;
        OrigPos = transform.position;
        ActPullUpTime = 0;
        CurrentState = PlayerStates.ledgegrab;
    }
    bool CheckWall(float XM, float YM)
    {
        if(XM == 0 && YM == 0)
            return false;

        if(ActWallTime > WallRunTime)
        {
            return false;
        }

        Vector3 WallDirection = transform.forward * YM;
        WallDirection = WallDirection.normalized;

        bool WallCol = Coli.CheckWall(WallDirection);

        return WallCol;
    }

    void WallRun()
    {
        CurrentState = PlayerStates.onwall;
    }

    void WallRunMovement(float verticalMov, float D)
    {
        Vector3 MovDir = transform.up * verticalMov;
        MovDir = MovDir * WallRunUpwardsMovement;


        MovDir += transform.forward * ActSpeed;
        Vector3 lerpAmt = Vector3.Lerp(Rigid.velocity, MovDir, WallRunSpeedAcceleration * D);
        Rigid.velocity = lerpAmt;
    }

    void StartCrouching()
    {
        Crouch = true;
        Cap.height = CrouchHeight;

        if (ActSpeed > SlideSpeedLimit)
            SlideForwards();
    }

    void StopCrouching()
    {
        Crouch = false;
        Cap.height = StandingHeight;
        Debug.Log("Stopped crouching");
    }
    void SlideForwards()
    {
        //ActSpeed = SlideSpeedLimit;

        AdjustmentAmt = 0;

        Vector3 Dir = Rigid.velocity.normalized;

        Dir.y = 0;

     

        Rigid.AddForce(Dir * SlideAmt, ForceMode.VelocityChange);

      
    }
}
