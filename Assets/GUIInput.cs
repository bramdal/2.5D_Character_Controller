using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIInput : MonoBehaviour
{
    public Joystick joystick;
    [HideInInspector]public float horizontalInput;
    [HideInInspector]public bool jumpActionInput;
    float jumpDisableCounter;
    float jumpDisableCounterFinish;
    
    
    private void Update() {
        horizontalInput = joystick.Horizontal;
        
        jumpDisableCounter += Time.deltaTime;
        if(jumpDisableCounter > jumpDisableCounterFinish)
            jumpActionInput = false;
    }

    public void GiveJumpInput(){
        jumpActionInput = true;
        jumpDisableCounter = Time.time;
        jumpDisableCounterFinish = jumpDisableCounter + Time.deltaTime/4;
    }


}
