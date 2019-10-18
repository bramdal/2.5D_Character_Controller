using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Lateral translation")]
    public float movementVelocity = 5f;
    float verticalVelocity;
    
    [Header("Standard Verticals")]
    //verticals
    public float gravity;
    public float jumpForce = 10f;
    public int allowedJumps = 2;
    int jumps;
    
    [Header("Climb Verticals")]
    //climb verticals
    public float climbGravity = 5f;
    public float wallPull = 5f;
    float horizontalVelocity;
    public float wallHopForce = 5f;

    [Header("Leap Variables")]
    //leap
    public float burstForce = 20f;
    public float leapEffectTime;
    public float leapLockInputTime;
    Vector3 burstDirection;
    float leapTime;
    float leapFinishTime;
    float leapLockInputFinishTime;
    float lerpStep;

    [Header("Slope Variables")]
    //slope slide variables
    public float slopeGravity = 15f;
    float slopeActingGravity;
    RaycastHit checkSlope;
    Vector3 slopeMomentum = Vector3.zero;
    float slopeLeaveCounter;
    public float slopeLeaveTimer = 0.3f;
    float slopeLeaveTimerFinish;
    bool leftSlope = false;
   
    
    //references
    Transform objectDetector;

    CharacterController charController;

    [Header("States")]
    //states
    [HideInInspector]public bool canInput = true;
    bool canRotate = true;
    bool leaped;
    [HideInInspector]public bool hanging;
    [HideInInspector]public bool moving;

    public float speed;

    //input values
    GUIInput inputScript;
    [HideInInspector]public bool jumpInput;
    [HideInInspector]public float inputX;
    
    //animation
    //Animator anim;
    public bool touchInput;


    private void Start() {
        charController = GetComponent<CharacterController>();
        objectDetector = transform.Find("Object Detector");
        //anim = GetComponent<Animator>();
        inputScript = GetComponent<GUIInput>();

        canInput = true;
        hanging = false;
        moving = false;
    }

    private void Update(){
        GetInputs();
        if(hanging){
            DoLadderPhysics();
        }    
        else
        {
            DoMovementPhysics(); 
        }

        WallLeapPhysics();
        DoSlopePhysics();

        SetStates();

        //print(charController.isGrounded);
    }

    void GetInputs(){
        if(canInput){
            //"touch" input
            if(touchInput){
                inputX = inputScript.horizontalInput;

                jumpInput = inputScript.jumpActionInput;
            }
            else{
            // keyboard input
            inputX = Input.GetAxis("Horizontal");
            
            if(Input.GetButtonDown("Jump"))
                jumpInput = true;
            else
                jumpInput = false;

            }
        }        
        else{
            inputX = 0f;
            jumpInput = false;
        } 

    }

    void SetStates(){
        if(inputX != 0 || !charController.isGrounded)
            moving = true;
        else 
            moving = false;     

        if(moving)
            speed = Mathf.Abs(inputX) * movementVelocity;     

        //wrap up state changes
        if(!hanging)
            horizontalVelocity = 0f;       
    }

    void DoMovementPhysics(){
        //verticals
        if(charController.isGrounded){
            verticalVelocity = -gravity * Time.deltaTime;
            if(jumpInput){
                verticalVelocity = jumpForce;
            }
            jumps = 0;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
            if(jumpInput && jumps < allowedJumps-1){
                verticalVelocity = jumpForce;
                jumps++;

                //anim.SetTrigger("Double Jump");
            }
        }
           
        Vector3 movementVector = new Vector3(inputX * movementVelocity, verticalVelocity, 0f);

        //amimations
        // if(charController.isGrounded){
        //     anim.SetBool("Grounded", true);
        //     if(movementVector.x != 0){
        //         anim.SetBool("Hanging", false);
        //         anim.SetBool("Airtime", false);
        //         anim.SetBool("Running", true);
        //     }
        //     else{
        //         anim.SetBool("Hanging", false);
        //         anim.SetBool("Airtime", false);
        //         anim.SetBool("Running", false);
        //     }
        // }
        // else{
        //     anim.SetBool("Hanging", false);
        //     anim.SetBool("Running", false);
        //     anim.SetBool("Grounded", false);
        //     anim.SetBool("Airtime", true);
        // }

        charController.Move(movementVector * Time.deltaTime);

        if(canRotate)
            if(movementVector != Vector3.zero){
                    if(movementVector.x != 0f){
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(movementVector.x, 0f, 0f)), 1f);
                    }    
            }
    }

    void DoLadderPhysics(){
        //animations
        // anim.SetBool("Running", false);
        // anim.SetBool("Airtime", false);
        // anim.SetBool("Grounded", false);
        // anim.SetBool("Hanging", true);


        //verticals
        verticalVelocity -= climbGravity * Time.deltaTime;
        verticalVelocity = Mathf.Clamp(verticalVelocity, -10f, wallHopForce);
        if(jumpInput && (inputX < 0.2f || inputX > -0.2f)){
            verticalVelocity = wallHopForce;
            //anim.SetTrigger("Climbing");
        }
           
        //horizontals
        if(objectDetector.position.x < transform.position.x){
            horizontalVelocity -= wallPull * Time.deltaTime;
            if(jumpInput && inputX > 0.2f){
                DoWallLeap();
            }
        }    
        else if(objectDetector.position.x > transform.position.x){
            horizontalVelocity += wallPull * Time.deltaTime; 
            if(jumpInput && inputX < -0.2f){
                DoWallLeap();
            }
        }
        
        if (leaped)
        {
            horizontalVelocity = 0f; 
        }
         
        Vector3 movementVector = new Vector3(horizontalVelocity, verticalVelocity, 0f);
        charController.Move(movementVector * Time.deltaTime);      

        jumps = 0;
    }

    void DoWallLeap(){
       leapTime = Time.time;
       leapFinishTime = leapTime + leapEffectTime;
       leapLockInputFinishTime = leapTime + leapLockInputTime;

       if(objectDetector.position.x < transform.position.x){
           burstDirection = Vector3.right;
       }
       else{
           burstDirection = -Vector3.right;
       }

       if(burstDirection.magnitude > 0f)
           burstDirection *= burstForce;

       leaped = true;  
       canInput = false;  
       lerpStep = 0f;
    }

    void WallLeapPhysics(){
        if(leaped){
            if(burstDirection.magnitude > 0.2f){
                charController.Move(burstDirection * Time.deltaTime);
            }

            burstDirection = Vector3.Lerp(burstDirection, Vector3.zero, lerpStep / leapFinishTime);
            lerpStep += Time.deltaTime;
            lerpStep = Mathf.Clamp(lerpStep, 0f, 1f);

            leapTime += Time.deltaTime;
            if(leapTime > leapFinishTime){
                //effects
                leaped = false;
            }
            if(leapTime > leapLockInputFinishTime){
                //effects
                canInput = true;
            }
        }
    }

    void DoSlopePhysics(){
        Physics.Raycast(transform.position, Vector3.down, out checkSlope, 1.5f);
        if(checkSlope.collider != null){
            if(checkSlope.collider.tag == "Slope"){
                Vector3 slopeDirection = Vector3.zero;
                if(checkSlope.normal.x > 0){
                    slopeDirection = Vector3.Cross(checkSlope.normal, new Vector3(0f, 0f, 1f));
                }
                else if(checkSlope.normal.x < 0)
                {
                    slopeDirection = Vector3.Cross(checkSlope.normal, new Vector3(0f, 0f, -1f));
                }

                //Debug.DrawRay(transform.position, slopeDirection, Color.black, 0.1f);
                slopeActingGravity = slopeGravity * (-10f * slopeDirection.y);  
                slopeDirection *= slopeGravity;  
                
                canRotate = false;
                charController.Move(slopeDirection * Time.deltaTime);

                canRotate = false;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(slopeDirection.x, 0f, 0f)), 0.5f);

                slopeMomentum = slopeDirection;
                slopeLeaveCounter = Time.time;
                slopeLeaveTimerFinish = slopeLeaveCounter + slopeLeaveTimer;
                lerpStep = 0;
                leftSlope = true;
                

                
                jumps = 0;
            }
            else{
                //print("not slope");
                canRotate = true;
                
                
                slopeLeaveCounter = lerpStep += Time.deltaTime;

                if(slopeLeaveCounter>slopeLeaveTimerFinish){
                    leftSlope = false;
                }    

                if(leftSlope){
                    //print(leftSlope);
                    slopeMomentum = Vector3.Lerp(slopeMomentum, Vector3.zero, lerpStep/slopeLeaveTimer);
                    Vector3 actingSlopeMomentum = new Vector3(slopeMomentum.x, 0f, 0f);
                    charController.Move(actingSlopeMomentum * Time.deltaTime);
                }
            }
        }
        else{
            leftSlope = false;
            canRotate = true;
            slopeMomentum = Vector3.zero;
        }
    }

}
