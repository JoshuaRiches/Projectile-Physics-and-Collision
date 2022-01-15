using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    
    private Vec3 v3CurrentVeolcity; // Launch velocity as a vector
    private Vec3 v3Acceleration; // Vector quantity for acceleration
    private float fLifeSpan = 0f;    // Lifespan of the gameobject

    public Vec3 Velocity{
        set{ v3CurrentVeolcity = value;}
        get{ return v3CurrentVeolcity;}
    }

    public Vec3 Acceleration{
        set{v3Acceleration = value;}
        get{return v3Acceleration;}
    }

    public float LifeSpan{
        set{fLifeSpan = value;}
        get{return fLifeSpan;}
    }

    public Vec3 Position{
        set{transform.position = value.ToVector3();}
        get{return new Vec3(transform.position);}
    }

    /// <summary>
    /// In fixed update the life span will be decreased by 1 tenth of a second each time it loops back round 
    /// It then calculates the movement it should be moving, according to projectile motion
    /// </summary>
    private void FixedUpdate(){
        float microTimeStep = Time.deltaTime * 0.1f;
        fLifeSpan -= microTimeStep;

        Vec3 currentPos = new Vec3(transform.position);
        // Work out current velocity
        v3CurrentVeolcity += v3Acceleration * microTimeStep;
        // Work out displacement
        Vec3 displacement = v3CurrentVeolcity * microTimeStep;
        currentPos += displacement;
        transform.position = currentPos.ToVector3();

        // If it reaches the end of its lifespan then it destroys itself, this is so that entities dont linger around and lag the simulation
        if(fLifeSpan < 0f){
            Destroy(gameObject);
        }
    }
}
