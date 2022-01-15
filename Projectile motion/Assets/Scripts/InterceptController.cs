using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterceptController : MonoBehaviour
{
    public float fVisionRadius = 10f;
    private List<Projectile> projectiles;

    public InterceptLauncher launcher;

    [SerializeField]
    private Text targetSpeed;

    private float coolDownTime = 0.1f;
    private float coolDownCounter = 0f;

    private bool isRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        projectiles = new List<Projectile>();
        isRunning = true;
    }

    /// <summary>
    /// In update the processes for the intercept controller is carried out, here it calculates where it needs to shoot at to intercept its target
    /// </summary>
    void Update()
    {
        if (isRunning){
            coolDownCounter -= Time.deltaTime * 0.1f;
        
            projectiles.Clear();
            //Use Unity's FindObjectsOfType function to find all the projectiles within the scene
            GameObject[] allActiveProjectiles = GameObject.FindGameObjectsWithTag("Projectile");
            // Convert it from unity's vector 3 to my custom Vec3 class
            Vec3 position = new Vec3(transform.position);
            float radSqrd = fVisionRadius * fVisionRadius;

            foreach (GameObject projectile in allActiveProjectiles)
            {
                //Get the distance to this projectile
                Vec3 vecToProjectile = new Vec3(projectile.transform.position) - position;
                float distanceToProjectile = vecToProjectile.MagnitudeSquared(); //use Mag squared to avoid sqrt calc. 
                // If it is in range of the intercept controller add it to the list of target projectiles
                if (distanceToProjectile < radSqrd)
                {
                    Projectile p = projectile.GetComponent<Projectile>();
                    projectiles.Add(p);
                }
            }

            //Now that we have all projectiles in the detection range
            //calculate the time to intercept for all of the projectiles in range
            //store these in a tuple with the index of the projectile in the projectiles array and time to intercept
            List<(int id, float time)> interceptTimes = new List<(int id, float time)>();
            for( int i = 0; i < projectiles.Count; ++i )
            {
                interceptTimes.Add((i, findTimeToIntercept(launcher.launchVelocity * Mathf.Cos(launcher.launchAngle * Mathf.Deg2Rad), projectiles[i].Position, projectiles[i].Velocity)));
            }
            //get the index in the tuple with the shortest time to intercept
            int index = -1;
            float fiTime = float.MaxValue;
            foreach( var intercep in interceptTimes )
            {
                if( intercep.time < fiTime ){
                    fiTime = intercep.time;
                    index = intercep.id;
                }
            }

            if( index != -1 )
            {
                if( coolDownCounter < 0f )
                {
                    //calculate the position of the projectile at this time interval
                    Projectile p = projectiles[index];
                    //get future position using s = ut + 1/2at^2
                    Vec3 predictedPos = p.Position + (p.Velocity * fiTime + p.Acceleration * 0.5f * fiTime * fiTime);

                    //display the speed of the target projectile in my UI text object by using the square root of the veloctiy x^2,y^2,z^2
                    targetSpeed.text = "Target Speed: " + Mathf.Sqrt(p.Velocity.x * p.Velocity.x + p.Velocity.y * p.Velocity.y + p.Velocity.z * p.Velocity.z) + " m/s";

                    Vec3 dirToPPos = predictedPos - position;
                    float distToTarget = dirToPPos.Normalize();
                    //launcher vector velocity 
                    Vec3 v3Vel = new Vec3(0f, launcher.launchVelocity * Mathf.Sin(launcher.launchAngle * Mathf.Deg2Rad),
                                        launcher.launchVelocity * Mathf.Cos(launcher.launchAngle * Mathf.Deg2Rad));
                    Transform tx = launcher.transform;
                    tx.rotation = Quaternion.LookRotation(dirToPPos.ToVector3()); //Rotate the turret to look at the target
                    v3Vel = new Vec3(tx.transform.TransformDirection(v3Vel.ToVector3()));
                    Vec3 impactPos = position + ( v3Vel * fiTime + p.Acceleration * 0.5f * fiTime * fiTime); // calculate impact position using s = ut + 1/2at^2

                    //fire the projectile with the calculated parameters
                    launcher.FireProjectile(dirToPPos, fiTime);
                    coolDownCounter = coolDownTime;
                }
            }
        }
        
        // If the user presses escape then it will halt the previous section from working as it will only work if isRunning is true
        // this enables the simulation to be paused
        if (Input.GetKeyDown(KeyCode.Escape)){
            isRunning = false;
        }
        // If the user presses space then the simulation of the intercept controller will be resumed
        if (Input.GetKeyDown(KeyCode.Space)){
            isRunning = true;
        }

    }

    float findTimeToIntercept( float launcherVelocity, Vec3 projectilePos, Vec3 projectileVel )
    {
        //law of cosines formula c^2 = a^2 + b^2 - 2ab*cos(phi);
        //re-arrange to look like quadratic
        // x = (lv - pv)t^2 + (2 ab* cos(phi))t - a^2;  
        //for quadratic x = ax^2 + bx + c
        // a = (lv - pv)^2
        // b = (2ab * A.B)
        // c = a^2

        //get direction to projectile for A.B dot product (want direction from projectile towards gun)
        Vec3 directionShooterToProjectile = new Vec3(transform.position) - projectilePos;
        float distanceToProjectileSquared = directionShooterToProjectile.MagnitudeSquared();
        //As the projectile is only accelerating in the Y we can remove this part of the veclocity as it will not 
        //creates a complexity in the equation we can avoid by ignoring it and making this a problem that only exists in the 
        //X/Z Plane.
        Vec3 horizontalProjectileVelocity = new Vec3( projectileVel.x, 0f, projectileVel.z);

        //for the quadratic 
        float c = -(distanceToProjectileSquared);
         //abcos(phi) from the formula can be calculated as the dot product
        //of the projectile velocity and the direction to projectile
        // A.B = |A||B|cos(phi), |A| = a, |B| = b 
        float b = 2 * Vec3.DotProduct(directionShooterToProjectile, horizontalProjectileVelocity);
        //Fun fact the dot product of a vector with itself provides you with the lenght of the vector squared.
        float a = launcherVelocity * launcherVelocity - horizontalProjectileVelocity.DotProduct(horizontalProjectileVelocity);

        float timeToIntercept = UseQuadraticFormula(a, b, c);
        return timeToIntercept;
    }

    float UseQuadraticFormula( float a, float b, float c)
    {
        //if A is nearly 0 then the formula doesn't really hold true
        if( 0.0001f > Mathf.Abs(a) )
        {
            return 0f;
        }

        float bb = b * b;
        float ac = a * c;
        float b4ac = Mathf.Sqrt(bb - 4f * ac);
        float t1 = (-b + b4ac)/ (2f * a);
        float t2 = (-b - b4ac)/ (2f * a);
        float t = Mathf.Max(t1,t2); //only return the highest value as one of these may be negative
        return t;

    }
}
