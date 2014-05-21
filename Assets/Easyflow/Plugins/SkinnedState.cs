using System;
using UnityEngine;

internal class SkinnedState : MotionState
{
    private int m_boneCount;
    private int[] m_boneIndices;
    private Transform[] m_bones;
    private float[] m_boneWeights;
    private Vector3[] m_currCenters;
    private Vector2[] m_motions;
    private Vector3[] m_prevCenters;
    private SkinnedMeshRenderer m_skinnedRenderer;
    private int m_vertexCount;
    private Vector2[] m_vertexMotions;

    public SkinnedState(EasyflowCamera owner, EasyflowObject obj) : base(owner, obj)
    {
    }

    internal override void OnInitialize()
    {
        base.OnInitialize();
        this.m_skinnedRenderer = base.m_obj.GetComponent<SkinnedMeshRenderer>();
        this.m_boneCount = this.m_skinnedRenderer.bones.Length;
        this.m_bones = this.m_skinnedRenderer.bones;
        this.m_motions = new Vector2[this.m_boneCount];
        this.m_currCenters = new Vector3[this.m_boneCount];
        this.m_prevCenters = new Vector3[this.m_boneCount];
        Mesh mesh2 = UnityEngine.Object.Instantiate(this.m_skinnedRenderer.sharedMesh) as Mesh;
        mesh2.name = this.GetHashCode().ToString();
        this.m_skinnedRenderer.sharedMesh = mesh2;
        this.m_vertexCount = this.m_skinnedRenderer.sharedMesh.vertexCount;
        this.m_vertexMotions = new Vector2[this.m_vertexCount];
        this.m_boneIndices = new int[this.m_vertexCount * 4];
        this.m_boneWeights = new float[this.m_vertexCount * 4];
        BoneWeight[] boneWeights = this.m_skinnedRenderer.sharedMesh.boneWeights;
        for (int i = 0; i < this.m_vertexCount; i++)
        {
            BoneWeight weight = boneWeights[i];
            this.m_boneIndices[i * 4] = weight.boneIndex0;
            this.m_boneWeights[i * 4] = weight.weight0;
            this.m_boneIndices[(i * 4) + 1] = weight.boneIndex1;
            this.m_boneWeights[(i * 4) + 1] = weight.weight1;
            this.m_boneIndices[(i * 4) + 2] = weight.boneIndex2;
            this.m_boneWeights[(i * 4) + 2] = weight.weight2;
            this.m_boneIndices[(i * 4) + 3] = weight.boneIndex3;
            this.m_boneWeights[(i * 4) + 3] = weight.weight3;
        }
        Material[] materialArray = base.m_obj.IsPartOfStaticBatch ? base.m_obj.renderer.sharedMaterials : base.m_obj.renderer.materials;
        foreach (Material material in materialArray)
        {
            material.SetFloat("_EFLOW_SKINNED", 1f);
            material.SetFloat("_EFLOW_OBJID", ((float) base.m_obj.ObjectId) / 255f);
        }
        this.OnUpdateTransform(true);
    }

    internal override void OnUpdateTransform(bool starting)
    {
        if (!base.m_initialized)
        {
            this.OnInitialize();
        }
        else if ((base.m_owner.Instance.CullingMask & (((int) 1) << base.m_obj.gameObject.layer)) != 0)
        {
            this.UpdateMesh(starting);
        }
        else
        {
            for (int i = 0; i < this.m_vertexCount; i++)
            {
                this.m_vertexMotions[i] = Vector2.zero;
            }
            this.m_skinnedRenderer.sharedMesh.uv2 = this.m_vertexMotions;
        }
    }

    private void UpdateMesh(bool starting)
    {
        bool flag = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
        Matrix4x4 prevViewProjMatrix = base.m_owner.PrevViewProjMatrix;
        Matrix4x4 viewProjMatrix = base.m_owner.ViewProjMatrix;
        for (int i = 0; i < this.m_boneCount; i++)
        {
            Vector4 vector3 = this.m_prevCenters[i];
            vector3.w = 1f;
            Vector4 position = this.m_bones[i].transform.position;
            position.w = 1f;
            this.m_prevCenters[i] = this.m_currCenters[i];
            this.m_currCenters[i] = (Vector3) position;
            Vector4 vector = (Vector4) (prevViewProjMatrix * vector3);
            Vector4 vector2 = (Vector4) (viewProjMatrix * position);
            float num2 = 1f / vector.w;
            float num3 = 1f / vector2.w;
            vector.x *= num2;
            vector2.x *= num3;
            vector.y *= flag ? -num2 : num2;
            vector2.y *= flag ? -num3 : num3;
            this.m_motions[i].x = vector2.x - vector.x;
            this.m_motions[i].y = vector2.y - vector.y;
        }
        if (starting)
        {
            for (int j = 0; j < this.m_boneCount; j++)
            {
                this.m_prevCenters[j] = this.m_currCenters[j];
            }
            for (int k = 0; k < this.m_vertexCount; k++)
            {
                this.m_vertexMotions[k] = Vector2.zero;
            }
        }
        else
        {
            for (int m = 0; m < this.m_vertexCount; m++)
            {
                Vector2 vector5;
                Vector2 vector6;
                Vector2 vector7;
                Vector2 vector8;
                int index = m * 4;
                int num8 = index + 1;
                int num9 = index + 2;
                int num10 = index + 3;
                vector5.x = this.m_motions[this.m_boneIndices[index]].x * this.m_boneWeights[index];
                vector5.y = this.m_motions[this.m_boneIndices[index]].y * this.m_boneWeights[index];
                vector6.x = this.m_motions[this.m_boneIndices[num8]].x * this.m_boneWeights[num8];
                vector6.y = this.m_motions[this.m_boneIndices[num8]].y * this.m_boneWeights[num8];
                vector7.x = this.m_motions[this.m_boneIndices[num9]].x * this.m_boneWeights[num9];
                vector7.y = this.m_motions[this.m_boneIndices[num9]].y * this.m_boneWeights[num9];
                vector8.x = this.m_motions[this.m_boneIndices[num10]].x * this.m_boneWeights[num10];
                vector8.y = this.m_motions[this.m_boneIndices[num10]].y * this.m_boneWeights[num10];
                this.m_vertexMotions[m].x = ((vector5.x + vector6.x) + vector7.x) + vector8.x;
                this.m_vertexMotions[m].y = ((vector5.y + vector6.y) + vector7.y) + vector8.y;
            }
        }
        this.m_skinnedRenderer.sharedMesh.uv2 = this.m_vertexMotions;
    }
}

