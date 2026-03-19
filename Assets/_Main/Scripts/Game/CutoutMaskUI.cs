using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class CutoutMaskUI : Image
{
    private Material runtimeMaterial;
    private Material sourceMaterial;

    public override Material materialForRendering
    {
        get
        {
            var baseMaterial = base.materialForRendering;
            if (!baseMaterial)
            {
                return null;
            }

            if (!runtimeMaterial || sourceMaterial != baseMaterial)
            {
                ReleaseRuntimeMaterial();
                sourceMaterial = baseMaterial;
                runtimeMaterial = new Material(baseMaterial)
                {
                    name = $"{baseMaterial.name} (CutoutMaskUI)"
                };
            }

            runtimeMaterial.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
            return runtimeMaterial;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ReleaseRuntimeMaterial();
    }

    protected override void OnDestroy()
    {
        ReleaseRuntimeMaterial();
        base.OnDestroy();
    }

    private void ReleaseRuntimeMaterial()
    {
        if (!runtimeMaterial)
        {
            sourceMaterial = null;
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(runtimeMaterial);
        }
        else
        {
            DestroyImmediate(runtimeMaterial);
        }

        runtimeMaterial = null;
        sourceMaterial = null;
    }
}