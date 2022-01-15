using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeNode 
{
    private int targetsTotal = 0;

    public BoundingBox BBox;

    private OctreeNode _Parent;
    public OctreeNode Parent{
        set{ Debug.Assert(_Parent == null, "Node already has a parent!"); _Parent = value;}
        get{ return _Parent;}
    }

    private List<OctreeNode> Children;
    private List<GameObject> Objects;

    /// <summary>
    /// Constructs the node of the octree
    /// </summary>
    public OctreeNode (Vec3 origin, Vec3 extents){
        //Create the bounding box for the node
        BBox = new BoundingBox(origin, extents);
        //Lazy initialisation, we will only allocate these things as they are required
        _Parent = null;
        Children = null;
        Objects = null;
    }

    /// <summary>
    /// This function creates the children nodes of the root node
    /// </summary>
    public void MakeChildren(){
        Debug.Assert(Children == null, "Children already present on this OctreeNode");
        Children = new List<OctreeNode>();
        //Create the 8 child objects for this Octree node
        //Calculate the bounfs of the child objects (simply multiply by 0.5 in each dimension)
        Vec3 extents = BBox.Extents * 0.5f;
        //Origin is in the center of the bounding boc offset is half new bounds in each dimension
        Vec3 origin = BBox.Position;
        Children.Add(new OctreeNode(new Vec3(origin.x - extents.x, origin.y - extents.y, origin.z - extents.z), extents));
        Children.Add(new OctreeNode(new Vec3(origin.x - extents.x, origin.y - extents.y, origin.z + extents.z), extents));
        Children.Add(new OctreeNode(new Vec3(origin.x - extents.x, origin.y + extents.y, origin.z - extents.z), extents));
        Children.Add(new OctreeNode(new Vec3(origin.x - extents.x, origin.y + extents.y, origin.z + extents.z), extents));
        Children.Add(new OctreeNode(new Vec3(origin.x + extents.x, origin.y - extents.y, origin.z - extents.z), extents));
        Children.Add(new OctreeNode(new Vec3(origin.x + extents.x, origin.y - extents.y, origin.z + extents.z), extents));
        Children.Add(new OctreeNode(new Vec3(origin.x + extents.x, origin.y + extents.y, origin.z - extents.z), extents));
        Children.Add(new OctreeNode(new Vec3(origin.x + extents.x, origin.y + extents.y, origin.z + extents.z), extents));

        //Note to self: you need to make it so each child is assigned this node as a parent
    }

    /// <summary>
    /// This sends objects in the scene to the children nodes dependant on if the object is in the child or not
    /// </summary>
    void SendObjectsToChildren(){
        List<GameObject> delist = new List<GameObject>();
        //Iterate through the objects in this list and see which child they belong to
        foreach (GameObject go in Objects){
            Bounds b = go.GetComponent<Renderer>().bounds;
            Vec3 goCentre = new Vec3(b.center);
            Vec3 goExtents = new Vec3(b.extents);
            foreach (OctreeNode child in Children){
                if (child.BBox.containsObject(goCentre, goExtents)){
                    if (child.AddObject(go)){
                        //object successfully added to child object
                        delist.Add(go);
                        break; //break out of this for to go back to the go for loop
                    }
                }
            }
        }
        //Remove the objects from the list in the parent node
        foreach (GameObject go in delist){
            Objects.Remove(go);
        }
    }

    /// <summary>
    /// This adds an object to the node of the octree
    /// </summary>
    /// <param name="a_object"> The object to be added </param>
    public bool AddObject (GameObject a_object){
        bool objectAdded = false;
        //if node has children attempt to add object to child of node
        if(Children != null && Children.Count > 0){
            foreach(OctreeNode child in Children){
                objectAdded = child.AddObject(a_object);
                if(objectAdded) break;
            }
        }

        //If object not added then continue to attempt to add object to this node
        if (objectAdded != true){
            if (Objects == null){
                Objects = new List<GameObject>();
            }

            Bounds b = a_object.GetComponent<Renderer>().bounds;
            Vec3 goCentre = new Vec3(b.center);
            Vec3 goExtents = new Vec3(b.extents);
            if (BBox.containsObject(goCentre, goExtents)){
                Objects.Add(a_object);
                objectAdded = true;
                //if our object count exceeds a certain amount then we initialise children and redistribute objects
                if (Objects.Count >= 4 && Children == null){
                    MakeChildren();
                    SendObjectsToChildren();
                }
            }
        }
        return objectAdded;
    }

    /// <summary>
    /// This function will remove and object from the node and/or any child nodes
    /// </summary>
    /// <param name="a_object"> The object to be removed </param>
    /// <returns></returns>
    private bool RemoveObject(GameObject a_object){
        if (Objects != null){
            if (Objects.Remove(a_object)){
                return true;
            }
        }
        if (Children != null){
            foreach (OctreeNode child in Children){
                if (child.RemoveObject(a_object)){
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// This will draw the bound box for the node using the unity gizmos
    /// </summary>
    public void Draw(){
        //Draw the parent bound inbox
        BBox.Draw();
        if (Children != null){
            //draw each child bounds
            foreach (OctreeNode child in Children){
                child.Draw();
            }
        }
    }

    /// <summary>
    /// This will check for a collision between two projectiles in the scene,
    /// it does this by getting two projectiles in the same node and then checking the bounds of each projectile
    /// to see if they intersect, if they do intersect then it will add them to the delist 
    /// it then goes through the delist and will remove each object from the list and destroy the game object in the scene
    /// </summary>
    public void PerformCollisionTest(){
        //step through children
        if (Children != null && Children.Count > 0){
            foreach (OctreeNode child in Children){
                child.PerformCollisionTest();
            }
        }
        if (Objects != null && Objects.Count > 1){
            List<GameObject> delist = new List<GameObject>();

            for (int i = 0; i < Objects.Count; i++){
                //test bounds against next object in list
                Bounds obj1 = Objects[i].GetComponent<Renderer>().bounds;
                Vec3 obj1Centre = new Vec3(obj1.center);
                Vec3 obj1Extents = new Vec3(obj1.extents);
                for (int j = 0; j < Objects.Count; j++){
                    if (Objects[i] != Objects[j]){
                        Bounds obj2 = Objects[j].GetComponent<Renderer>().bounds;
                        Vec3 obj2Centre = new Vec3(obj2.center);
                        Vec3 obj2Extents = new Vec3(obj2.extents);
                        //test two objects for collision
                        if (obj1Centre.x - obj1Extents.x < obj2Centre.x + obj2Extents.x && obj1Centre.x + obj1Extents.x > obj2Centre.x - obj2Extents.x &&
                            obj1Centre.y - obj1Extents.y < obj2Centre.y + obj2Extents.y && obj1Centre.y + obj1Extents.y > obj2Centre.y - obj2Extents.y &&
                            obj1Centre.z - obj1Extents.z < obj2Centre.z + obj2Extents.z && obj1Centre.z + obj1Extents.z > obj2Centre.z - obj2Extents.z)
                        {
                            //object in collision with another object, add them to delist
                            delist.Add(Objects[i]);
                            delist.Add(Objects[j]);
                        }
                    }
                }
            }
            //Go through delist and remove each object and delete the object in scene
            if (delist.Count > 0){
                foreach (GameObject go in delist){
                    Objects.Remove(go);
                    GameObject.Destroy(go);
                    
                }
                SceneManager.targetsHit += 1;
            }
        }
        

    }
}
