using System;
using UnityEngine;

[Serializable]
internal abstract class MotionState
{
    protected bool m_initialized;
    protected EasyflowObject m_obj;
    protected EasyflowCamera m_owner;

    public MotionState(EasyflowCamera owner, EasyflowObject obj)
    {
        this.m_owner = owner;
        this.m_obj = obj;
        this.m_initialized = false;
    }

    internal virtual void OnCameraPostRender(Camera camera)
    {
    }

    internal virtual void OnInitialize()
    {
        this.m_initialized = true;
    }

    internal abstract void OnUpdateTransform(bool starting);
    internal virtual void OnWillRenderObject()
    {
    }

    public EasyflowCamera Owner
    {
        get
        {
            return this.m_owner;
        }
    }
}

