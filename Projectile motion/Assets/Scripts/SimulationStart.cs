using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class controls what happens to start the simulation
/// </summary>
public class SimulationStart : MonoBehaviour {
    [SerializeField]
    private ProjectileLauncher canon1;
    [SerializeField]
    private ProjectileLauncher canon2;
    [SerializeField]
    private ProjectileLauncher canon3;
    [SerializeField]
    private ProjectileLauncher canon4;
    [SerializeField]
    private ProjectileLauncher canon5;

    // Upon the program starting, all of the cannons' scripts wil be disabled
    void Start(){
        canon1.enabled = false;
        canon2.enabled = false;
        canon3.enabled = false;
        canon4.enabled = false;
        canon5.enabled = false;
    }

    /// <summary>
    /// In update it will get the input of space or escape
    /// If space is pressed then the simulation setup coroutine will be triggerd
    /// If escape is pressed then all cannon scripts will be disabled again, in effect pausing the simulation
    /// </summary>
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)){
            StartCoroutine(SimulationSetup());
        }
        if (Input.GetKeyDown(KeyCode.Escape)){
            canon1.enabled = false;
            canon2.enabled = false;
            canon3.enabled = false;
            canon4.enabled = false;
            canon5.enabled = false;
        }
    }

    /// <summary>
    /// In simulation setup the cannons each get their scripts activated one by one but there is a time gap between each one so that they are not syncronised
    /// in order to make each turret generate its own random values
    /// </summary>
    /// <returns></returns>
    private IEnumerator SimulationSetup(){
        //this waits between enabling each canon just so they arent syncronised
        canon1.enabled = true;
        yield return new WaitForSeconds(1);
        canon2.enabled = true;
        yield return new WaitForSeconds(2);
        canon3.enabled = true;
        yield return new WaitForSeconds(3);
        canon4.enabled = true;
        yield return new WaitForSeconds(4);
        canon5.enabled = true;
    }
}
