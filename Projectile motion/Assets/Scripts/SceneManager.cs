using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour {
    
    public static int targetsHit = 0;
    [SerializeField]
    private Text targetsHitText;

    // Update is called once per frame
    void Update() {
        //Create a new octree root node that encompases the entire scene
        //My scene is 40 x 50 x 55
        OctreeNode rootNode = new OctreeNode(new Vec3(20f, 25f, -27.5f), new Vec3(20f, 25f, 27.5f));
        //Find all projectiles that are launched into the scene
        GameObject[] allActiveProjectiles = GameObject.FindGameObjectsWithTag("Projectile");
        foreach (GameObject projectile in allActiveProjectiles){
            //add the projectiles to the root node
            rootNode.AddObject(projectile);
        }
        //draw the octree
        rootNode.Draw();

        //iterate through each node in the octree and perform collision tests
        //from the root node step inot each child node and test collisions for each object in node
        rootNode.PerformCollisionTest();

        // In the PerformCollisionTest() the value of targetsHit is incremented by the octree node script,
        // this then displays that value in the text object I have within the scene for the targets hit.
        targetsHitText.text = "Targets Hit: " + targetsHit;
    }
}
