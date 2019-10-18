using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEnemy : MonoBehaviour
{
    [HideInInspector]public Vector3 enemyLocation;
    [HideInInspector]public Vector3 startLocation;
    [HideInInspector]public Vector3 QBCPoint;

    public float projectileVelocity = 10f;

    //bezier curve
    float time = 0f;

    private void Update(){
        TrackEnemy();
    }

    void TrackEnemy(){
        if(time < 1f){
            transform.position = GetBezierQCCurvePoint(time, startLocation, QBCPoint, enemyLocation);
            time += Time.deltaTime;
        }
    }
    
    private void OnTriggerEnter(Collider other){
        if(other.tag == "Enemy"){
            Destroy(gameObject);
        }
    }

    public Vector3 GetBezierQCCurvePoint(float t, Vector3 p0, Vector3 p1, Vector3 p2){
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        return (uu * p0) + (2 * u * t * p1) + (tt * p2);
    }
}
