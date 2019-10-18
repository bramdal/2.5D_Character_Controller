using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactReceiver : MonoBehaviour
{

    Vector3 impactDirection = Vector3.zero;
    public float impactForce = 20f;
    
    CharacterController charController;
    PlayerController playerController;

    float impactTime;
    float impactFinishTime;
    public float impactEffectTime = 0.4f;

    [Range(10f, 45f)]
    public float knockbackAngle = 45f;

    bool knockedBack = false;

    private void Start() {
        charController = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
    }


    void Update(){
        if(knockedBack){
            if(impactDirection.magnitude > 0.2f){
                charController.Move(impactDirection * Time.deltaTime);
            }
    
            impactDirection = Vector3.Lerp(impactDirection, Vector3.zero, 5 * Time.deltaTime);

            impactTime += Time.deltaTime;
            if(impactTime > impactFinishTime){
                playerController.canInput = true;
                knockedBack = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Projectile" || other.gameObject.tag == "Weapon"){
          
            impactTime = Time.time;
            impactFinishTime = impactTime + impactEffectTime;

            if(transform.position.x < other.transform.position.x){
                impactDirection = -Vector3.right + (Vector3.up * Mathf.Sin(knockbackAngle));
            }    
            else{
                impactDirection = Vector3.right + (Vector3.up * Mathf.Sin(knockbackAngle)); 
            }    

            if(impactDirection.magnitude > 0f){
                impactDirection *= impactForce;
            }     

            playerController.canInput = false;
            knockedBack = true;
        }
    }
}
