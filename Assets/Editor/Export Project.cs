/*
 * FUNCTION:	Export the complete project including editor specific items
 * 				such as tags, physics, quality and script execution order.
 * */

using UnityEngine;
using UnityEditor;
using System.Collections;

public class ExportProject : ScriptableObject {
	
	[MenuItem ("Custom/Export Project")]
	static void DoApply()
	{
		string[] projectContent = AssetDatabase.GetAllAssetPaths();
		AssetDatabase.ExportPackage(projectContent, "UltimateTemplate.unitypackage", ExportPackageOptions.Recurse | ExportPackageOptions.IncludeLibraryAssets );
		Debug.Log("Project Exported");
	}
}
