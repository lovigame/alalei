/* *
 * FUNCTION: Update the Check Points position in PatchLineDrawer
 * of CheckPoints_Curve or CheckPoints_Straight.
 * 
 * INFO: This script needs to be run from "Custom > Patches CP Generater"
 * every time the spline/ CPs are rearranged.
 * 
 * */

using UnityEngine;
using UnityEditor;
using System.Collections;
 
public class Patches_CPGenerator : ScriptableObject
{
        //private static Vector3 position;
        //private static Quaternion rotation;
        //private static Vector3 scale;
        //private static string myName; 
 
    [MenuItem ("Custom/Patches CP Generator &p")]
    static void DoApply()
    {        
        ParameterizeCPs();
    }
    
    static void ParameterizeCPs()
	{
		//set the checkpoints values for the PathLineDrawer.js script
		//remove this code if you are using C#
		PathLineDrawer PLDS_H;
		PLDS_H = (PathLineDrawer)Selection.activeGameObject.GetComponent("PathLineDrawer");		
		PLDS_H.SetCPValues();
		
		//set the checkpoints values for the PathLineDrawer.cs script
		//remove this code if you are using javacript
		PathLineDrawerCS pathLineDrawerCS;
		pathLineDrawerCS = (PathLineDrawerCS)Selection.activeGameObject.GetComponent(typeof(PathLineDrawerCS));
		pathLineDrawerCS.SetCPValues();
	}//end of parameterize CPs function
}