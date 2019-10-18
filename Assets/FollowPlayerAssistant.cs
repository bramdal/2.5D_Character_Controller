using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerAssistant : MonoBehaviour
{
    //references
    Transform player;
    EnemyDetection enemyDetection;
    //chase variables
    [Header("Chase Variables")]
    public float chaseOffsetY;
    public float stillOffsetY;
    public float flowHeight;
    public float chaseSpeed = 3f;
    float playerSpeed;
    float speed;
    public float lagLimit = 3f;
    //for smooth damp
    Vector3 temp = new Vector3(1f, 1f, 1f);
    //line function variables
    float lineFunction = 0f;
    bool goingUp = false;
    bool goingDown = false;

    [Header("Hover Variables")]
    //hover variables
    //leminiscate variables
    public float lemniscateRadiusParameter = 2f;
    
    
    [Header("Projectile Variables")]
    //projectile
    public GameObject projectile;
    public float coolDown = 0.75f;
    float coolDownCounter;
    float coolDownCounterFinish;
    public int maxCombo = 3;
    int currentCombo = 0;
    public float comboCoolDown = 1f;
    float comboCoolDownCounter;
    float comboCoolDownCounterFinish;

    private void Start(){
        player = GameObject.FindWithTag("Player").transform;
        playerSpeed = player.gameObject.GetComponent<PlayerController>().speed;
        enemyDetection = player.gameObject.GetComponentInChildren<EnemyDetection>();
    }

    private void Update(){
        playerSpeed = player.gameObject.GetComponent<PlayerController>().speed;
        coolDownCounter += Time.deltaTime;
        comboCoolDownCounter += Time.deltaTime;
        
        if(currentCombo == maxCombo && comboCoolDownCounter>comboCoolDownCounterFinish)
            currentCombo = 0;

        if(!enemyDetection.enemyDetected){
            if(player.gameObject.GetComponent<PlayerController>().moving){
                ChasePlayer();
            }
            else{
                HoverPlayer();
            }
            currentCombo = 0;
        }
        else{
            ProtectPlayer();
            FireProjectile();
        }
    }

    void ChasePlayer(){
        float xPosition = player.position.x;
        float yPosition = player.position.y + chaseOffsetY;
        Vector3 moveTarget = Vector3.zero;

        Vector3 currentPosition = transform.position;
        if(player.gameObject.GetComponent<PlayerController>().hanging){
            currentPosition.y = yPosition;
            lineFunction = player.position.y;
        }    
        else{ 
            currentPosition.y = GetLine(yPosition);
        }    
        //transform.position = currentPosition; 
        transform.position = Vector3.SmoothDamp(transform.position, currentPosition, ref temp, Time.deltaTime * 3);
        
        moveTarget = new Vector3(xPosition, yPosition, 0f);
        
        if (Vector3.Distance(transform.position, moveTarget)> lagLimit*4){
            speed = chaseSpeed * 10f;
        }
        else if(Vector3.Distance(transform.position, moveTarget)>lagLimit)
            speed = 7.5f;    //hard coded to player velocity, make runtime dynamic later
        else 
            speed = chaseSpeed;    

        transform.position = Vector3.MoveTowards(transform.position, moveTarget, Time.deltaTime * speed); 
        print("chasing player");
    }

    void HoverPlayer(){
        Vector3 moveTarget = Vector3.zero;
       
        float xPosition = 0f;
        float yPosition = 0f;

        xPosition = player.position.x;
        yPosition = player.position.y + stillOffsetY;
        moveTarget = new Vector3(xPosition, yPosition, 0f);
        moveTarget = GetLemniscate(lemniscateRadiusParameter, Time.time, moveTarget);

        transform.position = Vector3.MoveTowards(transform.position, moveTarget, Time.deltaTime * chaseSpeed);

        lineFunction = player.position.y;
        print("hovering player");
    }

    void ProtectPlayer(){
        if(enemyDetection.enemyLocation != null){
            Vector3 moveTarget = enemyDetection.enemyLocation.position - player.position;
            moveTarget *= 0.5f;
            moveTarget += player.position;
            speed = playerSpeed * 2f;

            transform.position = Vector3.MoveTowards(transform.position, moveTarget, Time.deltaTime * 15f);
            
            lineFunction = player.position.y;
            print("protecting");
        }
        else{
            if(player.gameObject.GetComponent<PlayerController>().moving){
                ChasePlayer();
            }
            else{
                HoverPlayer();
            }
        }
    }

    void FireProjectile(){
        if(Input.GetButtonDown("Fire2") && coolDownCounter>coolDownCounterFinish && currentCombo <= maxCombo-1){
            coolDownCounter = Time.time;
            coolDownCounterFinish = coolDownCounter + coolDown;
            currentCombo ++;
            if(currentCombo == maxCombo){
                comboCoolDownCounter = Time.time;
                comboCoolDownCounterFinish = comboCoolDownCounter + comboCoolDown;
            }    

            var createdProjectile = Instantiate(projectile, transform);
            createdProjectile.GetComponent<HitEnemy>().enemyLocation = enemyDetection.enemyLocation.position;
            createdProjectile.GetComponent<HitEnemy>().startLocation = transform.position;
            createdProjectile.GetComponent<HitEnemy>().QBCPoint = new Vector3(transform.position.x, transform.position.y + Random.Range(-2f, 2f), 0f);
            createdProjectile.transform.parent = null;
        }    
    }

    float GetLine(float currentOrigin){
        if(lineFunction >= currentOrigin + flowHeight){
            goingDown = true;
            goingUp = false;
        }    
        else if (lineFunction <= currentOrigin){
            goingUp = true;
            goingDown = false;
        }

        if(goingUp)
            lineFunction += Time.deltaTime * 2;
        else if(goingDown) 
            lineFunction -= Time.deltaTime * 2;    

        currentOrigin = lineFunction;
        return currentOrigin;
    }

   Vector3 GetLemniscate(float a, float t, Vector3 yPosition){
       float denominator = Mathf.Sin(t) * Mathf.Sin(t) + 1;
       float term1 = a * 1.41f * Mathf.Cos(t);
       float x = (term1) / denominator;
       float y = (term1 * Mathf.Sin(t)) / denominator;

       Vector3 lemniscatePoint = new Vector3(x, y, 0f);
       lemniscatePoint += yPosition;

       return lemniscatePoint;
   }
    //lemniscate function
    //x={\frac {a{\sqrt {2}}\cos(t)}{\sin ^{2}(t)+1}};\qquad y={\frac {a{\sqrt {2}}\cos(t)\sin(t)}{\sin ^{2}(t)+1}}
}
