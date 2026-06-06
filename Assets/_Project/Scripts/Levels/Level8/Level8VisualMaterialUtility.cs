using UnityEngine;

/// <summary>Shared URP material helpers for Level8 placeholder visuals.</summary>
public static class Level8VisualMaterialUtility
{
    public static Material CreateLit(Color color, bool transparent = false)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = color;

        if (transparent)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.renderQueue = 3000;
        }

        return material;
    }

    public static void ApplyColor(Renderer renderer, Color color, bool transparent = false)
    {
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = CreateLit(color, transparent);
    }
}
