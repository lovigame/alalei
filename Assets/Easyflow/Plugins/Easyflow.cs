using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Image Effects/Easyflow Motion Blur"), RequireComponent(typeof(Camera))]
public class Easyflow : MonoBehaviour
{
    public LayerMask CullingMask = -1;
    internal static Easyflow CurrentInstance = null;
    internal static EasyflowCamera CurrentOwner = null;
    internal static Camera CurrentReference = null;
    public static bool IgnoreMotionScaleWarning = false;
    public static Dictionary<Camera, EasyflowCamera> m_activeCameras = new Dictionary<Camera, EasyflowCamera>();
    public static Dictionary<GameObject, EasyflowObject> m_activeObjects = new Dictionary<GameObject, EasyflowObject>();
    private EasyflowCamera m_baseCamera;
    private Material m_blurMaterial;
    private RenderTexture m_combinedRenderTex;
    private Material m_combineMaterial;
    private EasyflowPostProcess m_currentPostProcess;
    private float m_deltaTime;
    private static Easyflow m_firstInstance = null;
    private int m_globalObjectId = 1;
    private bool m_hdr;
    private int m_height;
    private Dictionary<Camera, EasyflowCamera> m_linkedCameras = new Dictionary<Camera, EasyflowCamera>();
    private Material m_motionMaterial;
    private RenderTexture m_motionRenderTex;
    private float m_rcpHeight;
    private float m_rcpWidth;
    private int m_width;
    public float MotionScale = 2f;
    public Camera[] OverlayCameras;
    public Quality QualityLevel = Quality.Standard;
    public int QualitySteps = 1;

    private EasyflowCamera AddMotionCamera(Camera reference)
    {
        CurrentInstance = this;
        CurrentReference = reference;
        string auxCameraName = "Easyflow+" + reference.name;
        GameObject obj2 = this.RecursiveFindCamera(base.gameObject, auxCameraName);
        EasyflowCamera component = null;
        if (obj2 == null)
        {
            obj2 = new GameObject {
                name = auxCameraName,
                hideFlags = HideFlags.HideAndDontSave
            };
        }
        else
        {
            component = obj2.GetComponent<EasyflowCamera>();
        }
        if (component == null)
        {
            component = obj2.AddComponent<EasyflowCamera>();
            component.camera.CopyFrom(reference);
            component.camera.depth = reference.depth - 1f;
            component.camera.renderingPath = RenderingPath.VertexLit;
            component.camera.hdr = false;
            component.camera.cullingMask = reference.cullingMask;
            component.camera.depthTextureMode = DepthTextureMode.Depth;
            component.camera.enabled = true;
            component.camera.clearFlags = CameraClearFlags.Depth;
            component.camera.SetReplacementShader(this.m_motionMaterial.shader, "RenderType");
        }
        CurrentReference = null;
        CurrentInstance = null;
        return component;
    }

    private void Awake()
    {
        if (m_firstInstance == null)
        {
            m_firstInstance = this;
        }
        this.m_globalObjectId = 1;
        this.m_width = 0;
        this.m_height = 0;
        this.m_rcpWidth = this.m_rcpHeight = 0f;
    }

    private void CheckMaterialAndShader(Material material, string name)
    {
        if ((material == null) || (material.shader == null))
        {
            Debug.LogError("[Easyflow] Error creating " + name + " material");
        }
        else if (!material.shader.isSupported)
        {
            Debug.LogError("[Easyflow] " + name + " shader not supported on this platform");
        }
        else
        {
            material.hideFlags = HideFlags.DontSave;
        }
    }

    public bool CheckSupport()
    {
        if (SystemInfo.supportsImageEffects && SystemInfo.supportsRenderTextures)
        {
            return true;
        }
        Debug.LogError("[Easyflow] Initialization failed. This plugin requires support for Image Effects and Render Textures.");
        return false;
    }

    private void CreateMaterials()
    {
        if (this.QualityLevel == Quality.Mobile)
        {
            this.m_blurMaterial = new Material(Shader.Find("Hidden/Easyflow/MotionBlurMobile"));
            this.m_motionMaterial = new Material(Shader.Find("Hidden/Easyflow/VectorsMobile"));
        }
        else if (this.QualityLevel == Quality.Standard)
        {
            this.m_blurMaterial = new Material(Shader.Find("Hidden/Easyflow/MotionBlur"));
            this.m_motionMaterial = new Material(Shader.Find("Hidden/Easyflow/Vectors"));
            this.m_combineMaterial = new Material(Shader.Find("Hidden/Easyflow/Combine"));
            this.CheckMaterialAndShader(this.m_combineMaterial, "Combine");
        }
        this.CheckMaterialAndShader(this.m_blurMaterial, "Blur");
        this.CheckMaterialAndShader(this.m_motionMaterial, "Vectors");
    }

    private bool FindValidTag(Material[] materials)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null)
            {
                switch (materials[i].GetTag("RenderType", false))
                {
                    case "Opaque":
                    case "TransparentCutout":
                        return true;
                }
            }
        }
        return false;
    }

    public int GenerateObjectId(GameObject obj)
    {
        if (obj.isStatic)
        {
            return 0;
        }
        if (this.m_globalObjectId > 0xff)
        {
            return 0;
        }
        return this.m_globalObjectId++;
    }

    private void InitializeCameras()
    {
        Camera[] cameraArray = new Camera[this.OverlayCameras.Length + 1];
        cameraArray[0] = base.camera;
        for (int i = 0; i < this.OverlayCameras.Length; i++)
        {
            cameraArray[i + 1] = this.OverlayCameras[i];
        }
        this.m_linkedCameras.Clear();
        for (int j = 0; j < cameraArray.Length; j++)
        {
            Camera key = cameraArray[j];
            if (!this.m_linkedCameras.ContainsKey(key))
            {
                EasyflowCamera camera2 = this.AddMotionCamera(key);
                this.m_linkedCameras.Add(key, camera2);
            }
        }
    }

    internal void InternalRenderImage(RenderTexture source, RenderTexture destination)
    {
        this.UpdateRenderTextures();
        float num = 1f / Mathf.Clamp(this.m_deltaTime, 0.005f, 0.06666667f);
        Vector4 zero = Vector4.zero;
        zero.x = ((this.MotionScale * this.m_rcpWidth) * num) * 0.125f;
        zero.y = ((this.MotionScale * this.m_rcpHeight) * num) * 0.125f;
        if (this.QualityLevel == Quality.Mobile)
        {
            this.RenderMobile(source, destination, zero);
        }
        else if (this.QualityLevel == Quality.Standard)
        {
            this.RenderStandard(source, destination, zero);
        }
    }

    private void LateUpdate()
    {
        if (this.m_baseCamera.AutoStep)
        {
            this.m_deltaTime = Time.deltaTime;
        }
        this.MotionScale = Mathf.Max(this.MotionScale, 0f);
        foreach (EasyflowCamera camera in this.m_linkedCameras.Values)
        {
            camera.UpdateProperties();
            if (!camera.gameObject.activeInHierarchy)
            {
                camera.gameObject.SetActive(true);
            }
        }
        this.UpdatePostProcess();
    }

    private void LinkCameras()
    {
        foreach (KeyValuePair<Camera, EasyflowCamera> pair in this.m_linkedCameras)
        {
            pair.Value.transform.parent = pair.Key.transform;
            pair.Value.gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        foreach (EasyflowCamera camera in this.m_linkedCameras.Values)
        {
            UnityEngine.Object.Destroy(camera.gameObject);
        }
    }

    private void OnDisable()
    {
        if (this.m_currentPostProcess != null)
        {
            this.m_currentPostProcess.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (!this.CheckSupport())
        {
            base.enabled = false;
        }
        else
        {
            this.CreateMaterials();
            this.UpdateActiveObjects();
            this.InitializeCameras();
            this.m_linkedCameras.TryGetValue(base.camera, out this.m_baseCamera);
            if (this.m_currentPostProcess != null)
            {
                this.m_currentPostProcess.enabled = true;
            }
        }
    }

    private GameObject RecursiveFindCamera(GameObject obj, string auxCameraName)
    {
        GameObject obj2 = null;
        if (obj.name == auxCameraName)
        {
            return obj;
        }
        foreach (Transform transform in obj.transform)
        {
            obj2 = this.RecursiveFindCamera(transform.gameObject, auxCameraName);
            if (obj2 != null)
            {
                return obj2;
            }
        }
        return obj2;
    }

    public void Register(GameObject gameObj)
    {
        if (!m_activeObjects.ContainsKey(gameObj))
        {
            this.TryRegister(gameObj);
        }
    }

    internal static void RegisterCamera(EasyflowCamera cam)
    {
        m_activeCameras.Add(cam.camera, cam);
        foreach (EasyflowObject obj2 in m_activeObjects.Values)
        {
            obj2.RegisterCamera(cam);
        }
    }

    internal static void RegisterObject(EasyflowObject obj)
    {
        m_activeObjects.Add(obj.gameObject, obj);
        foreach (EasyflowCamera camera in m_activeCameras.Values)
        {
            obj.RegisterCamera(camera);
        }
    }

    public void RegisterRecursively(GameObject gameObj)
    {
        if (!m_activeObjects.ContainsKey(gameObj))
        {
            this.TryRegister(gameObj);
        }
        foreach (Transform transform in gameObj.transform)
        {
            this.RegisterRecursively(transform.gameObject);
        }
    }

    internal void ReleaseCamera(Camera reference)
    {
        this.m_linkedCameras.Remove(reference);
    }

    private void RenderMobile(RenderTexture source, RenderTexture destination, Vector4 blurStep)
    {
        this.m_blurMaterial.SetTexture("_MotionTex", this.m_motionRenderTex);
        this.m_blurMaterial.SetVector("_BlurStep", blurStep);
        Graphics.Blit(source, destination, this.m_blurMaterial);
    }

    private void RenderStandard(RenderTexture source, RenderTexture destination, Vector4 blurStep)
    {
        this.m_combineMaterial.SetTexture("_MotionTex", this.m_motionRenderTex);
        this.m_blurMaterial.SetTexture("_MotionTex", this.m_motionRenderTex);
        Graphics.Blit(source, this.m_combinedRenderTex, this.m_combineMaterial);
        if (this.QualitySteps > 1)
        {
            RenderTexture temp = RenderTexture.GetTemporary(this.m_width, this.m_height, 0, this.m_hdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
            float num = 1f / ((float) this.QualitySteps);
            float num2 = 1f;
            RenderTexture combinedRenderTex = this.m_combinedRenderTex;
            RenderTexture dest = temp;
            for (int i = 0; i < this.QualitySteps; i++)
            {
                this.m_blurMaterial.SetVector("_BlurStep", (Vector4) (blurStep * num2));
                Graphics.Blit(combinedRenderTex, dest, this.m_blurMaterial);
                if (i < (this.QualitySteps - 2))
                {
                    RenderTexture texture4 = dest;
                    dest = combinedRenderTex;
                    combinedRenderTex = texture4;
                }
                else
                {
                    combinedRenderTex = dest;
                    dest = destination;
                }
                num2 -= num;
            }
            RenderTexture.ReleaseTemporary(temp);
        }
        else
        {
            this.m_blurMaterial.SetVector("_BlurStep", blurStep);
            Graphics.Blit(this.m_combinedRenderTex, destination, this.m_blurMaterial);
        }
    }

    private void Start()
    {
        this.UpdatePostProcess();
        this.UpdateRenderTextures();
        this.LinkCameras();
        if ((this.MotionScale > 20f) && !IgnoreMotionScaleWarning)
        {
            Debug.LogWarning("[Easyflow] MotionScale range has been changed. Default is now 2, and max recommended is 10. Please adjust.\nNOTE: To get rid of this warning, please set 'Easyflow.IgnoreMotionScaleWarning = true' via script, e.g. on Awake(). ");
        }
    }

    public void StartAutoStep()
    {
        foreach (EasyflowCamera camera in this.m_linkedCameras.Values)
        {
            camera.StartAutoStep();
        }
    }

    public void Step(float delta)
    {
        this.m_deltaTime = delta;
        foreach (EasyflowCamera camera in this.m_linkedCameras.Values)
        {
            camera.Step();
        }
    }

    public void StopAutoStep()
    {
        foreach (EasyflowCamera camera in this.m_linkedCameras.Values)
        {
            camera.StopAutoStep();
        }
    }

    private void TryRegister(GameObject gameObj)
    {
        UnityEngine.Renderer renderer = gameObj.renderer;
        if (((renderer != null) && (renderer.sharedMaterials != null)) && (renderer.enabled && this.FindValidTag(renderer.sharedMaterials)))
        {
            bool flag = false;
            if (((renderer.GetType() == typeof(MeshRenderer)) || (renderer.GetType() == typeof(SkinnedMeshRenderer))) || (flag = renderer.GetType() == typeof(ClothRenderer)))
            {
                if (flag && (gameObj.GetComponent<InteractiveCloth>().tearFactor != 0f))
                {
                    Debug.LogWarning("[Easyflow] Tearable cloth objects are not supported at this time. Ignoring cloth object \"" + renderer.name + "\"");
                }
                else if (gameObj.GetComponent<EasyflowObject>() == null)
                {
                    gameObj.AddComponent<EasyflowObject>();
                }
            }
        }
    }

    internal static void UnregisterCamera(EasyflowCamera cam)
    {
        foreach (EasyflowObject obj2 in m_activeObjects.Values)
        {
            obj2.UnregisterCamera(cam);
        }
        m_activeCameras.Remove(cam.camera);
    }

    internal static void UnregisterObject(EasyflowObject obj)
    {
        foreach (EasyflowCamera camera in m_activeCameras.Values)
        {
            obj.UnregisterCamera(camera);
        }
        m_activeObjects.Remove(obj.gameObject);
    }

    public void UpdateActiveObjects()
    {
        GameObject[] objArray = UnityEngine.Object.FindSceneObjectsOfType(typeof(GameObject)) as GameObject[];
        for (int i = 0; i < objArray.Length; i++)
        {
            if (!m_activeObjects.ContainsKey(objArray[i]))
            {
                this.TryRegister(objArray[i]);
            }
        }
    }

    private void UpdatePostProcess()
    {
        Camera camera = null;
        float minValue = float.MinValue;
        foreach (Camera camera2 in this.m_linkedCameras.Keys)
        {
            if (camera2.depth > minValue)
            {
                camera = camera2;
                minValue = camera2.depth;
            }
        }
        if ((this.m_currentPostProcess != null) && (this.m_currentPostProcess.gameObject != camera.gameObject))
        {
            UnityEngine.Object.Destroy(this.m_currentPostProcess);
            this.m_currentPostProcess = null;
        }
        if ((this.m_currentPostProcess == null) && (camera != null))
        {
            CurrentInstance = this;
            this.m_currentPostProcess = camera.gameObject.AddComponent<EasyflowPostProcess>();
            CurrentInstance = null;
        }
    }

    private void UpdateRenderTextures()
    {
        if (((this.m_width != ((int) base.camera.pixelWidth)) || (this.m_height != ((int) base.camera.pixelHeight))) || (this.m_hdr != base.camera.hdr))
        {
            this.m_width = (int) base.camera.pixelWidth;
            this.m_height = (int) base.camera.pixelHeight;
            this.m_rcpWidth = 1f / ((float) this.m_width);
            this.m_rcpHeight = 1f / ((float) this.m_height);
            this.m_hdr = base.camera.hdr;
            if (this.m_motionRenderTex != null)
            {
                UnityEngine.Object.Destroy(this.m_motionRenderTex);
                this.m_motionRenderTex = null;
            }
            if (this.m_combinedRenderTex != null)
            {
                UnityEngine.Object.Destroy(this.m_combinedRenderTex);
                this.m_combinedRenderTex = null;
            }
        }
        if (this.m_motionRenderTex == null)
        {
            this.m_motionRenderTex = new RenderTexture(this.m_width, this.m_height, 0x10, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            this.m_motionRenderTex.name = "EasyflowMotionRT+" + base.name;
            this.m_motionRenderTex.wrapMode = TextureWrapMode.Clamp;
            this.m_motionRenderTex.hideFlags = HideFlags.DontSave;
            this.m_motionRenderTex.Create();
            foreach (EasyflowCamera camera in this.m_linkedCameras.Values)
            {
                camera.camera.targetTexture = this.m_motionRenderTex;
            }
        }
        if ((this.m_combinedRenderTex == null) && (this.QualityLevel == Quality.Standard))
        {
            this.m_combinedRenderTex = new RenderTexture(this.m_width, this.m_height, 0, this.m_hdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
            this.m_combinedRenderTex.name = "EasyflowCombinedRT+" + base.name;
            this.m_combinedRenderTex.wrapMode = TextureWrapMode.Clamp;
            this.m_combinedRenderTex.hideFlags = HideFlags.DontSave;
            this.m_combinedRenderTex.Create();
        }
    }

    public EasyflowCamera BaseCamera
    {
        get
        {
            return this.m_baseCamera;
        }
    }

    public static Easyflow FirstInstance
    {
        get
        {
            return m_firstInstance;
        }
    }

    public static Easyflow Instance
    {
        get
        {
            return m_firstInstance;
        }
    }

    public enum Quality
    {
        Mobile,
        Standard
    }
}

