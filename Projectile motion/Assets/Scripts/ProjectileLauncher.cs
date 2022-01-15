using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour {

    public float launchVelocity = 10f;   //the launch velocity of the projectile
    public float launchAngle    = 30f;   //the angle the projectile is fired at
    public float Gravity        = -9.8f; //the gravity that effects the porjectiles

    public Vec3 v3InitialVelocity = new Vec3(); //launch velocity as a vector
    private Vec3 v3Acceleration;                //vector quanitity for acceleration

    private float airTime = 0f;         //how long will the projectile be in the air
    private float horizontalDisplacement = 0f;   //how far in the horizontal plane will the projectile travel?

    //variables that realte to drawing the path of the projectile
    private List<Vec3> pathPoints;       //list of points along the path of the vector for drawing line of travel
    private int simulationSteps = 30;    //number of points on the path og the projectile

    public GameObject projectile;       // Game object to instantiate for our projectile
    public GameObject launchPoint;      // Game object to use as our launch point

    private int fireDelay; //The delay between each shot, it will be generated in rng script and assigned to this variable
    private bool readyToFire = false; //determines if the cannon can fire or not based on if delay has counted down

    // Start is called before the first frame update
    void Start() {
        //initialise path vector for drawing
        pathPoints = new List<Vec3>();
        calculateProjectile();
        calculatePath();

        StartCoroutine(CanonCooldown());
    }

    /// <summary>
    /// This includes the mathematical formulae to calculate the projectile's properties
    /// </summary>
    private void calculateProjectile() {
        launchAngle = transform.parent.eulerAngles.x;
        // Work out vertical offset
        float launchHeight = launchPoint.transform.position.y;
        // Work out velocity as a vector quantity
        // The velocity is calculated from the prespective of the cannon
        // My cannon faces on the -X axis
        v3InitialVelocity.x = 0f;
        v3InitialVelocity.z = launchVelocity * Mathf.Cos(launchAngle * Mathf.Deg2Rad);
        v3InitialVelocity.y = launchVelocity * Mathf.Sin(launchAngle * Mathf.Deg2Rad);
        // V3 velocity is in local space facing down the cannon's X axis
        // Transofrm that into a world space direction if this step is omitted the projectile will always move down the world's X-axis
        Vector3 txDirection = launchPoint.transform.TransformDirection(v3InitialVelocity.ToVector3());
        v3InitialVelocity = new Vec3(txDirection);
        // Gravity as a vec3
        v3Acceleration = new Vec3(0f, Gravity, 0f);
        // Calculate total air time, use quadratic formula to do so
        airTime = UseQuadraticFormula(v3Acceleration.y, v3InitialVelocity.y * 2f, launchHeight * 2f);
        // Calculate total distance travelled horizontally prior to the projectile hitting the ground
        horizontalDisplacement = airTime * v3InitialVelocity.z;
    }

    /// <summary>
    /// This is the quadratic formula
    /// </summary>
    float UseQuadraticFormula(float a, float b, float c){
        // If A is nearly 0 then the formula doesnt really hoild true
        if(0.0001f > Mathf.Abs(a)){
            return 0f;
        }

        float bb = b * b;
        float ac = a * c;
        float b4ac = Mathf.Sqrt(bb - 4f * ac);
        float t1 = (-b + b4ac)/ (2f * a);
        float t2 = (-b - b4ac)/ (2f * a);
        float t = Mathf.Max(t1, t2); // Only return the highest value as  one of these may be negative
        return t;
    }

    /// <summary>
    /// This calculates the path of the projectile
    /// </summary>
    private void calculatePath(){
        Vec3 launchPos = new Vec3(launchPoint.transform.position);
        pathPoints.Add(launchPos);

        for (int i = 0; i <= simulationSteps; i++){

            float simTime = (i / (float)simulationSteps) * airTime;
            //suvat formulae for displacement s = ut + 1/2at^2
            Vec3 displacement = v3InitialVelocity * simTime + v3Acceleration * simTime * simTime * 0.5f;
            Vec3 drawPoint = launchPos + displacement;
            pathPoints.Add(drawPoint);
        }
    }

    /// <summary>
    /// This creates a gizmo line that shows the path of the projectile's trajectory
    /// </summary>
    void drawPath(){
        for (int i = 0; i < pathPoints.Count-1; i++){
            Debug.DrawLine(pathPoints[i].ToVector3(), pathPoints[i+1].ToVector3(), Color.green);
        }
    }
    
    void Update() {
        
        drawPath();
        // If the canon is ready to fire  then it will create the projectile and apply velocity, acceleration and time to the script on the projectile
        if (readyToFire){
            pathPoints.Clear();;
            calculateProjectile();
            calculatePath();
            
            // Instantiate at the launch point position, with current rotation
            GameObject p = Instantiate(projectile, launchPoint.transform.position, launchPoint.transform.rotation);
            p.GetComponent<Projectile>().Velocity = v3InitialVelocity;
            p.GetComponent<Projectile>().Acceleration = v3Acceleration;
            p.GetComponent<Projectile>().LifeSpan = airTime;

            readyToFire = false;

            StartCoroutine(CanonCooldown());
        }
    }

    /// <summary>
    /// This will get a randomInteger using my RNG class and will set it as the fire delay,
    /// it will then wait that many seconds before setting the canon as being able to fire again
    /// </summary>
    /// <returns></returns>
    private IEnumerator CanonCooldown(){
        fireDelay = this.GetComponent<RNG>().RandomInt();
        yield return new WaitForSeconds(fireDelay);
        readyToFire = true;
    }
}
