using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
public class EasyflowObject : MonoBehaviour
{
    internal bool IsPartOfStaticBatch;
    internal List<MotionState> m_debugStates = new List<MotionState>();
    private Dictionary<Camera, MotionState> m_states = new Dictionary<Camera, MotionState>();
    private EasyflowObjectType m_type;
    internal int ObjectId;

    private void OnCameraPostRender(Camera camera)
    {
        MotionState state = null;
        if (this.m_states.TryGetValue(Camera.current, out state))
        {
            state.OnCameraPostRender(camera);
        }
    }

    private void OnDisable()
    {
        Easyflow.UnregisterObject(this);
    }

    private void OnEnable()
    {
        if (base.renderer.GetType() == typeof(MeshRenderer))
        {
            this.m_type = EasyflowObjectType.Solid;
        }
        else if (base.renderer.GetType() == typeof(SkinnedMeshRenderer))
        {
            this.m_type = EasyflowObjectType.Skinned;
        }
        else if (base.renderer.GetType() == typeof(ClothRenderer))
        {
            this.m_type = EasyflowObjectType.Cloth;
        }
        Easyflow.RegisterObject(this);
    }

    private void OnUpdateTransform(EasyflowCamera owner, bool starting)
    {
        MotionState state;
        if (this.m_states.TryGetValue(owner.camera, out state))
        {
            state.OnUpdateTransform(starting);
        }
    }

    private void OnWillRenderObject()
    {
        MotionState state = null;
        if (this.m_states.TryGetValue(Camera.current, out state))
        {
            state.OnWillRenderObject();
        }
    }

    internal void RegisterCamera(EasyflowCamera camera)
    {
        if (((camera.camera.cullingMask & (((int) 1) << base.gameObject.layer)) != 0) && !this.m_states.ContainsKey(camera.camera))
        {
            MotionState state = null;
            switch (this.m_type)
            {
                case EasyflowObjectType.Solid:
                    state = new SolidState(camera, this);
                    break;

                case EasyflowObjectType.Skinned:
                    state = new SkinnedState(camera, this);
                    break;

                case EasyflowObjectType.Cloth:
                    state = new ClothState(camera, this);
                    break;

                default:
                    throw new Exception("[Easyflow] Invalid object type.");
            }
            camera.OnUpdateTransform += new EasyflowCamera.OnUpdateTransformDelegate(this.OnUpdateTransform);
            if (this.m_type == EasyflowObjectType.Cloth)
            {
                camera.OnCameraPostRender += new EasyflowCamera.OnCameraPostRenderDelegate(this.OnCameraPostRender);
            }
            this.m_states.Add(camera.camera, state);
            this.m_debugStates.Add(state);
        }
    }

    private void Start()
    {
        this.IsPartOfStaticBatch = base.renderer.isPartOfStaticBatch;
        this.ObjectId = Easyflow.Instance.GenerateObjectId(base.gameObject);
        foreach (MotionState state in this.m_states.Values)
        {
            state.OnInitialize();
        }
    }

    internal void UnregisterCamera(EasyflowCamera camera)
    {
        MotionState state;
        if (this.m_states.TryGetValue(camera.camera, out state))
        {
            this.m_debugStates.Remove(state);
            camera.OnUpdateTransform -= new EasyflowCamera.OnUpdateTransformDelegate(this.OnUpdateTransform);
            if (this.m_type == EasyflowObjectType.Cloth)
            {
                camera.OnCameraPostRender -= new EasyflowCamera.OnCameraPostRenderDelegate(this.OnCameraPostRender);
            }
            this.m_states.Remove(camera.camera);
        }
    }
}

