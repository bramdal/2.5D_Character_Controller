using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class ClimbDetection : MonoBehaviour
{
    public bool wallInVicinity = false;
    public bool wallTouch = false;

    private void OnTriggerEnter(Collider other){
        if(other.tag == "Climbable"){
            wallInVicinity = true;
            wallTouch = true;
        }    
    }

    private void OnTriggerStay(Collider other){
        if(wallInVicinity)
            if(other.tag == "Climbable" && !gameObject.GetComponentInParent<CharacterController>().isGrounded){
                gameObject.GetComponentInParent<PlayerController>().hanging = true;
            } 
            else if(other.tag == "Climbable" || gameObject.GetComponentInParent<CharacterController>().isGrounded){
                gameObject.GetComponentInParent<PlayerController>().hanging = false;
            }    
    }

    private void OnTriggerExit(Collider other){
        if(other.tag == "Climbable"){
            wallInVicinity = false;
            wallTouch = false;
            gameObject.GetComponentInParent<PlayerController>().hanging = false;
        }    
    }
}
