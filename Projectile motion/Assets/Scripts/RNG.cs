using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class generates a random number based on the run time of the game as the seed
/// </summary>
public class RNG : MonoBehaviour {
    
    private int startSeed;
    private int maxValue = 15;
    private int minValue = 1;

    /// <summary>
    /// The randomInt function generates a random number by performing a series of mathmatical functions on the seed number,
    /// it is not true random but should generate a different number as long as the seed isnt the same
    /// </summary>
    /// <returns></returns>
    public int RandomInt(){
        int randomNumber = startSeed * startSeed;
        randomNumber = randomNumber << 2;
        randomNumber = randomNumber * randomNumber;
        randomNumber = randomNumber / 11;
        randomNumber = Mathf.Abs(randomNumber);

        // This acts as a catch for if the number is 0 or 1 as these two values produce an infinte loop of the same number
        // so i have it catch if its these and changes it to be the time instead, this will cause it to break the loop
        if (randomNumber == 0 || randomNumber == 1){
            randomNumber = (int)Time.time;
        }

        startSeed = randomNumber;
        
        //modulus the number to be within range so it can be used as seconds between shots
        randomNumber = (randomNumber % (maxValue - minValue)) + minValue;
        return randomNumber;
    }
}
