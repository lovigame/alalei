About

  Easyflow (c) Insidious Technologies. All rights reserved.

  Easyflow is a Motion Blur Plugin for Unity Pro
  
  Current Features:

    * Full-scene vector motion blur
    * Works on all surfaces
    * Supports camera movement
    * Variable quality steps

  Redistribution of Easyflow is frowned upon. If you want to share the 
  software, please refer others to the official download page:

    http://www.insidious.pt/#easyflow

Minimum Requirements

  Software

    Unity 3.5.7f6

Description

  Easyflow is a complete and fast full-scene vector motion blur effect 
  that works with all opaque and coverage/alpha-test surfaces. It supports 
  both stationary and moving objects as well as Skinned and Cloth meshes.
  Handles translation as well as rotation on all objects and camera using 
  per-pixel vectors.

Workflow Overview

  1) Apply a "Easyflow Motion Blur" Image Effect to your main camera
    
  2) Adjust "Quality Steps" and "Motion Scale" to fit your needs

  3) $$$ profit
    
Technical Artistic Considerations

  A) "Motion Scale" defines the maximum distance for the directional blur

  B) Transparent/Alpha-blended are not directly supported. Transparent surfaces will
     be blurred according to the vectors defined by the opaque or coverage/alpha-test
	 surfaces below them.

Technical Considerations

  A) To enable Motion Blur on objects instantiated at runtime, simply call 
     Easyflow.Instance.Register( obj ) on each new object:

       // Instantiates 10 copies of prefab each 2 units apart from each other
       for ( var i : int = 0; i < 10; i++ )
       {
           GameObject obj = Instantiate( prefab, Vector3( i * 2f, 0, 0 ), Quaternion.identity );

           Easyflow.Instance.Register( obj ); // same for C#
       }

     An easier, yet more expensive, alternative is also available. Simply call
     Easyflow.Instance.UpdateActiveObjects() after instantiating all the objects.

       // Instantiates 10 copies of prefab each 2 units apart from each other
       for ( var i : int = 0; i < 10; i++ )
           GameObject obj = Instantiate( prefab, Vector3( i * 2f, 0, 0 ), Quaternion.identity );	
           
       Easyflow.Instance.UpdateActiveObjects(); // same for C#

Feedback

  To file error reports, questions or suggestions, you may use 
  our feedback form online:
	
    http://www.insidious.pt/#feedback

  Or contact us directly:

    For general inquiries: info@insidious.pt
    For technical support: support@insidious.pt (customers only)