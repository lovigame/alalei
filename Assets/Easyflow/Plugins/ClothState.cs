using System;
using System.Collections.Generic;
using UnityEngine;

internal class ClothState : MotionState
{
    private Material m_clonedMat;
    private Mesh m_clonedMesh;
    private InteractiveCloth m_cloth;
    private Vector3[] m_currCenters;
    private Vector2[] m_motions;
    private Vector3[] m_prevCenters;
    private int m_vertexCount;

    public ClothState(EasyflowCamera owner, EasyflowObject obj) : base(owner, obj)
    {
    }

    internal override void OnCameraPostRender(Camera camera)
    {
        if (this.m_clonedMat.SetPass(0))
        {
            Graphics.DrawMeshNow(this.m_clonedMesh, Matrix4x4.identity);
        }
    }

    internal override void OnInitialize()
    {
        Vector3[] vectorArray2;
        int[] numArray2;
        base.OnInitialize();
        this.m_cloth = base.m_obj.GetComponent<InteractiveCloth>();
        int vertexCount = this.m_cloth.mesh.vertexCount;
        Vector3[] vertices = this.m_cloth.mesh.vertices;
        int[] triangles = this.m_cloth.mesh.triangles;
        if (this.m_cloth.vertices.Length == this.m_cloth.mesh.vertices.Length)
        {
            vectorArray2 = vertices;
            numArray2 = triangles;
        }
        else
        {
            Dictionary<Vector3, int> dictionary = new Dictionary<Vector3, int>();
            int[] numArray3 = new int[vertexCount];
            int num3 = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                int num2;
                if (dictionary.TryGetValue(vertices[i], out num2))
                {
                    numArray3[i] = num2;
                }
                else
                {
                    numArray3[i] = num3;
                    dictionary.Add(vertices[i], num3++);
                }
            }
            vectorArray2 = new Vector3[num3];
            dictionary.Keys.CopyTo(vectorArray2, 0);
            int length = triangles.Length;
            numArray2 = new int[length];
            for (int j = 0; j < length; j++)
            {
                numArray2[j] = numArray3[triangles[j]];
            }
        }
        this.m_vertexCount = vectorArray2.Length;
        this.m_prevCenters = new Vector3[this.m_vertexCount];
        this.m_currCenters = new Vector3[this.m_vertexCount];
        this.m_motions = new Vector2[this.m_vertexCount];
        this.m_clonedMesh = new Mesh();
        this.m_clonedMesh.vertices = vectorArray2;
        this.m_clonedMesh.uv = this.m_motions;
        this.m_clonedMesh.triangles = numArray2;
        string name = "Hidden/Easyflow/ClothVectors";
        if (base.m_owner.Instance.QualityLevel == Easyflow.Quality.Mobile)
        {
            name = name + "Mobile";
        }
        this.m_clonedMat = new Material(Shader.Find(name));
        this.m_clonedMat.SetFloat("_EFLOW_OBJID", ((float) base.m_obj.ObjectId) / 255f);
        this.UpdateMesh(true);
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
                this.m_motions[i] = Vector2.zero;
            }
            this.m_clonedMesh.vertices = this.m_cloth.vertices;
            this.m_clonedMesh.uv = this.m_motions;
        }
    }

    private void UpdateMesh(bool starting)
    {
        bool flag = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
        Matrix4x4 prevViewProjMatrix = base.m_owner.PrevViewProjMatrix;
        Matrix4x4 viewProjMatrix = base.m_owner.ViewProjMatrix;
        Vector3[] vertices = this.m_cloth.vertices;
        for (int i = 0; i < this.m_vertexCount; i++)
        {
            Vector4 vector3 = this.m_prevCenters[i];
            vector3.w = 1f;
            Vector4 vector4 = vertices[i];
            vector4.w = 1f;
            this.m_prevCenters[i] = this.m_currCenters[i];
            this.m_currCenters[i] = (Vector3) vector4;
            Vector4 vector = (Vector4) (prevViewProjMatrix * vector3);
            Vector4 vector2 = (Vector4) (viewProjMatrix * vector4);
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
            for (int j = 0; j < this.m_vertexCount; j++)
            {
                this.m_prevCenters[j] = this.m_currCenters[j];
                this.m_motions[j] = Vector2.zero;
            }
        }
        this.m_clonedMesh.vertices = vertices;
        this.m_clonedMesh.uv = this.m_motions;
    }
}

