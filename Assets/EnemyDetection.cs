using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [HideInInspector]public bool enemyDetected;
    [HideInInspector]public Transform enemyLocation;

    int enemyCount = 0;

    Transform[] enemies = new Transform[4];

    List<Transform> enemyList = new List<Transform>();

    int levelLayer;

    private void Start() {
        levelLayer = LayerMask.NameToLayer("Level");
    }

    private void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Enemy"){
            enemyDetected = true;
            enemyCount++;
            enemyList.Add(other.gameObject.transform);
        }
    }

    private void OnTriggerStay(Collider other) {
        if(enemyDetected){
            float closestEnemyDistance = 0f;
            bool foundFirstEnemy = false;
            Transform trackEnemy = null;
            foreach (var enemy in enemyList){
                RaycastHit hit;
                Physics.Raycast(transform.parent.position, enemy.position - transform.parent.position, out hit, Mathf.Infinity);
                
                if(hit.collider!=null){
                    //print(hit.collider.gameObject.name);
                    if(hit.collider.gameObject.layer == levelLayer){
                        continue;
                    } 
                }           

                float thisEnemyDistance;
                thisEnemyDistance = Vector3.Distance(transform.parent.transform.position, enemy.position);
                if(!foundFirstEnemy){
                    closestEnemyDistance = thisEnemyDistance;
                    foundFirstEnemy = true;
                }

                if(thisEnemyDistance <= closestEnemyDistance){
                    closestEnemyDistance = Vector3.Distance(transform.parent.transform.position, enemy.position);
                    trackEnemy = enemy;
                }
            }
            
            enemyLocation = trackEnemy;
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.tag == "Enemy" && enemyCount > 0){
            enemyList.Remove(other.gameObject.transform);
            enemyCount--;
            if(enemyCount==0)
                enemyDetected = false;
        }
    }
}
