using System;
using UnityEngine;

[AddComponentMenu(""), RequireComponent(typeof(Camera))]
public sealed class EasyflowPostProcess : MonoBehaviour
{
    private Easyflow Instance;

    private void OnEnable()
    {
        if (this.Instance == null)
        {
            this.Instance = Easyflow.CurrentInstance;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (this.Instance != null)
        {
            this.Instance.InternalRenderImage(source, destination);
        }
    }

}

