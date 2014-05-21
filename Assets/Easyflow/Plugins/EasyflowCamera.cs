using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Camera)), AddComponentMenu("")]
public class EasyflowCamera : MonoBehaviour
{
    internal Easyflow Instance;
    private bool m_autoStep = true;
    private Mesh m_cube;
    private Matrix4x4 m_currBackgroundTransform;
    private Matrix4x4 m_currTerrainTransform;
    private Vector3 m_lastPosition;
    private Quaternion m_lastRotation;
    private Vector3 m_lastScale;
    private Material m_material;
    private Matrix4x4 m_prevBackgroundTransform;
    private int m_prevFrameCount;
    private Matrix4x4 m_prevTerrainTransform;
    private bool m_starting = true;
    private bool m_step;
    internal Matrix4x4 PrevViewProjMatrix;
    internal Camera Reference;
    internal Matrix4x4 ViewProjMatrix;

    internal event OnCameraPostRenderDelegate OnCameraPostRender;

    internal event OnUpdateTransformDelegate OnUpdateTransform;

    private void LateUpdate()
    {
    }

    private void OnDestroy()
    {
        this.Instance.ReleaseCamera(this.Reference);
    }

    private void OnDisable()
    {
        Easyflow.UnregisterCamera(this);
    }

    private void OnEnable()
    {
        Easyflow.RegisterCamera(this);
        if (this.Instance == null)
        {
            this.Instance = Easyflow.CurrentInstance;
        }
        if (this.Reference == null)
        {
            this.Reference = Easyflow.CurrentReference;
        }
        this.m_material = new Material(Shader.Find("Hidden/Easyflow/BackgroundVectors"));
        this.m_material.hideFlags = HideFlags.DontSave;
        int[] numArray = new int[] { 
            0, 1, 2, 2, 1, 3, 4, 0, 6, 6, 0, 2, 7, 5, 6, 6, 
            5, 4, 3, 1, 7, 7, 1, 5, 4, 5, 0, 0, 5, 1, 3, 7, 
            2, 2, 7, 6
         };
        Vector3[] vectorArray = new Vector3[] { new Vector3(-1f, 1f, -1f), new Vector3(1f, 1f, -1f), new Vector3(-1f, -1f, -1f), new Vector3(1f, -1f, -1f), new Vector3(-1f, 1f, 1f), new Vector3(1f, 1f, 1f), new Vector3(-1f, -1f, 1f), new Vector3(1f, -1f, 1f) };
        this.m_cube = new Mesh();
        this.m_cube.vertices = vectorArray;
        this.m_cube.triangles = numArray;
        this.UpdateProperties();
        this.m_step = false;
    }

    internal void OnPostRender()
    {
        if ((this.Instance != null) && (this.OnCameraPostRender != null))
        {
            this.OnCameraPostRender(base.camera);
        }
    }

    private void OnPreCull()
    {
        if ((Time.frameCount > this.m_prevFrameCount) && (this.m_autoStep || this.m_step))
        {
            bool flag = false;
            if ((this.m_starting || (base.transform.position != this.m_lastPosition)) || ((base.transform.rotation != this.m_lastRotation) || (base.transform.localScale != this.m_lastScale)))
            {
                this.m_lastPosition = base.transform.position;
                this.m_lastRotation = base.transform.rotation;
                this.m_lastScale = base.transform.localScale;
                flag = true;
            }
            if (!this.m_starting)
            {
                this.PrevViewProjMatrix = this.ViewProjMatrix;
            }
            if (this.m_starting || flag)
            {
                Matrix4x4 worldToLocalMatrix = base.camera.transform.worldToLocalMatrix;
                Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(base.camera.projectionMatrix, true);
                gPUProjectionMatrix[2, 2] = -gPUProjectionMatrix[2, 2];
                gPUProjectionMatrix[3, 2] = -gPUProjectionMatrix[3, 2];
                this.ViewProjMatrix = gPUProjectionMatrix * worldToLocalMatrix;
            }
            if (this.m_starting)
            {
                this.PrevViewProjMatrix = this.ViewProjMatrix;
            }
            this.UpdateTransform(this.m_starting);
            if (this.OnUpdateTransform != null)
            {
                this.OnUpdateTransform(this, this.m_starting);
            }
            this.m_starting = false;
            this.m_step = false;
            this.m_prevFrameCount = Time.frameCount;
        }
    }

    internal void OnPreRender()
    {
        if ((this.Instance != null) && (this.Reference == this.Instance.camera))
        {
            Shader.SetGlobalMatrix("_EFLOW_PREV_MATRIX_MVP", this.m_prevBackgroundTransform);
            Shader.SetGlobalMatrix("_EFLOW_CURR_MATRIX_MVP", this.m_currBackgroundTransform);
            Shader.SetGlobalFloat("_EFLOW_SKINNED", 0f);
            Shader.SetGlobalFloat("_EFLOW_OBJID", 0f);
            RenderTexture.active = base.camera.targetTexture;
            float x = base.camera.near * 2f;
            if (this.m_material.SetPass(0))
            {
                Graphics.DrawMeshNow(this.m_cube, Matrix4x4.TRS(base.camera.transform.position, Quaternion.identity, new Vector3(x, x, x)));
            }
            if ((Terrain.activeTerrain != null) && ((this.Instance.CullingMask & (((int) 1) << Terrain.activeTerrain.gameObject.layer)) != 0))
            {
                Shader.SetGlobalMatrix("_EFLOW_PREV_MATRIX_MVP", this.m_prevTerrainTransform);
                Shader.SetGlobalMatrix("_EFLOW_CURR_MATRIX_MVP", this.m_currTerrainTransform);
            }
            else
            {
                Shader.SetGlobalMatrix("_EFLOW_PREV_MATRIX_MVP", Matrix4x4.identity);
                Shader.SetGlobalMatrix("_EFLOW_CURR_MATRIX_MVP", Matrix4x4.identity);
            }
        }
    }

    private void Start()
    {
    }

    internal void StartAutoStep()
    {
        this.m_autoStep = true;
    }

    internal void Step()
    {
        this.m_step = true;
    }

    internal void StopAutoStep()
    {
        if (this.m_autoStep)
        {
            this.m_autoStep = false;
            this.m_step = true;
        }
    }

    internal void UpdateProperties()
    {
        base.camera.cullingMask = this.Reference.cullingMask;
        base.camera.pixelRect = this.Reference.pixelRect;
        base.camera.rect = this.Reference.rect;
        base.camera.orthographic = this.Reference.orthographic;
        base.camera.orthographicSize = this.Reference.orthographicSize;
        base.camera.aspect = this.Reference.aspect;
        base.camera.fov = this.Reference.fov;
        base.camera.near = this.Reference.near;
        base.camera.far = this.Reference.far;
        base.camera.depth = this.Reference.depth - 1f;
    }

    private void UpdateTransform(bool starting)
    {
        if (!starting)
        {
            this.m_prevBackgroundTransform = this.m_currBackgroundTransform;
            this.m_prevTerrainTransform = this.m_currTerrainTransform;
        }
        this.m_currBackgroundTransform = this.ViewProjMatrix * Matrix4x4.TRS(base.camera.transform.position, Quaternion.identity, Vector3.one);
        if (Terrain.activeTerrain != null)
        {
            this.m_currTerrainTransform = this.ViewProjMatrix * Matrix4x4.TRS(Terrain.activeTerrain.GetPosition(), Quaternion.identity, Vector3.one);
        }
        else
        {
            this.m_currTerrainTransform = this.ViewProjMatrix;
        }
        if (starting)
        {
            this.m_prevBackgroundTransform = this.m_currBackgroundTransform;
            this.m_prevTerrainTransform = this.m_currTerrainTransform;
        }
    }

    public bool AutoStep
    {
        get
        {
            return this.m_autoStep;
        }
    }

    internal delegate void OnCameraPostRenderDelegate(Camera camera);

    internal delegate void OnUpdateTransformDelegate(EasyflowCamera owner, bool starting);
}

