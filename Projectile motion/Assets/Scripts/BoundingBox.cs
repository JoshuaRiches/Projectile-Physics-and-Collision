using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBox 
{
    // A bounding box is always axis alligned
    private static Vec3 xAxis = new Vec3(1f, 0f, 0f);
    private static Vec3 yAxis = new Vec3(0f, 1f, 0f);
    private static Vec3 zAxis = new Vec3(0f, 0f, 1f);

    //As this box may be asked numerous times for its min and max extremes we will calculate these
    //once and store them in the following six variables to avoid the need to calculate them again.
    private float xMin = 0f;
    private float xMax = 0f;
    private float yMin = 0f;
    private float yMax = 0f;
    private float zMin = 0f;
    private float zMax = 0f;

    //Property for the position of the box
    private Vec3 v3Position = new Vec3(0f, 0f, 0f);
    public Vec3 Position{
        set{ v3Position = value; calcExtremes();}
        get{ return v3Position;}
    }

    //Property for the extents of the box
    private Vec3 v3Extents = new Vec3(0f, 0f, 0f);
    public Vec3 Extents{
        set{ v3Extents = value; calcExtremes();}
        get{ return v3Extents;}
    }

    /// <summary>
    /// Calculate the extremes of the box in the x/y/z axis, the box's pivot is at the centre of it
    /// Extents are half the bounds of the bounding box
    /// </summary>
    private void calcExtremes(){
        xMin = v3Position.x - v3Extents.x;
        xMax = v3Position.x + v3Extents.x;
        yMin = v3Position.y - v3Extents.y;
        yMax = v3Position.y + v3Extents.y;
        zMin = v3Position.z - v3Extents.z;
        zMax = v3Position.z + v3Extents.z;
    }

    /// <summary>
    /// This checks if an object is in the bounds of the box
    /// </summary>
    /// <param name="a_pos"> The object to be checked </param>
    /// <param name="a_bounds"> The bounds of the box </param>
    /// <returns> True if it is in the box, false if not</returns>
    public bool containsObject(Vec3 a_pos, Vec3 a_bounds){
        //Bounding box origin is at center of the box
        Vec3 half_a_bounds = a_bounds * 0.5f;
        if( a_pos.x - half_a_bounds.x < xMax && a_pos.x + half_a_bounds.x > xMin &&
            a_pos.y - half_a_bounds.y < yMax && a_pos.y + half_a_bounds.y > yMin &&
            a_pos.z - half_a_bounds.z < zMax && a_pos.z + half_a_bounds.z > zMin){
                return true;
            }
            return false;
    }

    /// <summary>
    /// Function to visulise box within unity editor
    /// </summary>
    public void Draw(){
        //Draw vertical lines of the box
        Debug.DrawLine(new Vector3(xMin, yMax, zMin), new Vector3(xMin, yMin, zMin), Color.red);
        Debug.DrawLine(new Vector3(xMin, yMax, zMax), new Vector3(xMin, yMin, zMax), Color.red);
        Debug.DrawLine(new Vector3(xMax, yMax, zMin), new Vector3(xMax, yMin, zMin), Color.red);
        Debug.DrawLine(new Vector3(xMax, yMax, zMax), new Vector3(xMax, yMin, zMax), Color.red);

        //Draw top lines of box
        Debug.DrawLine(new Vector3(xMin, yMax, zMin), new Vector3(xMin, yMax, zMax), Color.red);
        Debug.DrawLine(new Vector3(xMax, yMax, zMin), new Vector3(xMax, yMax, zMax), Color.red);
        Debug.DrawLine(new Vector3(xMin, yMax, zMin), new Vector3(xMax, yMax, zMin), Color.red);
        Debug.DrawLine(new Vector3(xMin, yMax, zMax), new Vector3(xMax, yMax, zMax), Color.red);

        //Draw bottom lines of the box
        Debug.DrawLine(new Vector3(xMin, yMin, zMin), new Vector3(xMin, yMin, zMax), Color.red);
        Debug.DrawLine(new Vector3(xMax, yMin, zMin), new Vector3(xMax, yMin, zMax), Color.red);
        Debug.DrawLine(new Vector3(xMin, yMin, zMin), new Vector3(xMax, yMin, zMin), Color.red);
        Debug.DrawLine(new Vector3(xMin, yMin, zMax), new Vector3(xMax, yMin, zMax), Color.red);
    }

    /// <summary>
    /// Creates the actual bounding box
    /// </summary>
    /// <param name="a_origin"> The origin point </param>
    /// <param name="a_extensts"> The size of the box </param>
    public BoundingBox(Vec3 a_origin, Vec3 a_extensts){
        v3Position = a_origin;
        //Set extents using extents property so that calcextents is called
        Extents = a_extensts;
    }
}
