using System;
using UnityEngine;

[Serializable]
internal class SolidState : MotionState
{
    public Matrix4x4 m_currModelViewProj;
    public Matrix4x4 m_currWorld;
    public Vector3 m_lastPosition;
    public Quaternion m_lastRotation;
    public Vector3 m_lastScale;
    public Matrix4x4 m_prevModelViewProj;
    public Matrix4x4 m_prevWorld;

    public SolidState(EasyflowCamera owner, EasyflowObject obj) : base(owner, obj)
    {
    }

    internal override void OnInitialize()
    {
        base.OnInitialize();
        Material[] materialArray = base.m_obj.IsPartOfStaticBatch ? base.m_obj.renderer.sharedMaterials : base.m_obj.renderer.materials;
        foreach (Material material in materialArray)
        {
            material.SetFloat("_EFLOW_SKINNED", 0f);
            material.SetFloat("_EFLOW_OBJID", ((float) base.m_obj.ObjectId) / 255f);
            material.SetMatrix("_EFLOW_PREV_MATRIX_MVP", Matrix4x4.identity);
            material.SetMatrix("_EFLOW_CURR_MATRIX_MVP", Matrix4x4.identity);
        }
        this.OnUpdateTransform(true);
    }

    internal override void OnUpdateTransform(bool starting)
    {
        if (!base.m_initialized)
        {
            this.OnInitialize();
        }
        else
        {
            if (!starting)
            {
                this.m_prevWorld = this.m_currWorld;
                this.m_prevModelViewProj = this.m_currModelViewProj;
            }
            if (base.m_obj.IsPartOfStaticBatch)
            {
                this.m_currModelViewProj = base.m_owner.ViewProjMatrix;
            }
            else
            {
                Transform transform = base.m_obj.transform;
                bool flag = false;
                if ((starting || (transform.position != this.m_lastPosition)) || ((transform.rotation != this.m_lastRotation) || (transform.localScale != this.m_lastScale)))
                {
                    this.m_lastPosition = transform.position;
                    this.m_lastRotation = transform.rotation;
                    this.m_lastScale = transform.localScale;
                    flag = true;
                }
                if (starting || flag)
                {
                    float num = Mathf.Abs((float) (transform.localScale.x - transform.localScale.y));
                    float num2 = Mathf.Abs((float) (transform.localScale.y - transform.localScale.z));
                    if ((num > float.Epsilon) || (num2 > float.Epsilon))
                    {
                        this.m_currWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    }
                    else
                    {
                        this.m_currWorld = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
                    }
                }
                this.m_prevModelViewProj = base.m_owner.PrevViewProjMatrix * this.m_prevWorld;
                this.m_currModelViewProj = base.m_owner.ViewProjMatrix * this.m_currWorld;
            }
            if (starting)
            {
                this.m_prevWorld = this.m_currWorld;
                this.m_prevModelViewProj = this.m_currModelViewProj;
            }
        }
    }

    internal override void OnWillRenderObject()
    {
        if ((base.m_owner.Instance.CullingMask & (((int) 1) << base.m_obj.gameObject.layer)) != 0)
        {
            Material[] materialArray = base.m_obj.IsPartOfStaticBatch ? base.m_obj.renderer.sharedMaterials : base.m_obj.renderer.materials;
            foreach (Material material in materialArray)
            {
                material.SetMatrix("_EFLOW_PREV_MATRIX_MVP", this.m_prevModelViewProj);
                material.SetMatrix("_EFLOW_CURR_MATRIX_MVP", this.m_currModelViewProj);
            }
        }
        else
        {
            Material[] materialArray2 = base.m_obj.IsPartOfStaticBatch ? base.m_obj.renderer.sharedMaterials : base.m_obj.renderer.materials;
            foreach (Material material2 in materialArray2)
            {
                material2.SetMatrix("_EFLOW_PREV_MATRIX_MVP", Matrix4x4.identity);
                material2.SetMatrix("_EFLOW_CURR_MATRIX_MVP", Matrix4x4.identity);
            }
        }
    }
}

