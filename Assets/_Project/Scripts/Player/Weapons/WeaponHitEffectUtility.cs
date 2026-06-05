using UnityEngine;

public static class WeaponHitEffectUtility
{
    public static void SpawnHitEffect(WeaponProfile profile, Vector3 position, Vector3 normal)
    {
        if (profile.HitEffectPrefab != null)
        {
            Object.Instantiate(profile.HitEffectPrefab, position, Quaternion.LookRotation(normal));
            return;
        }

        var flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "WeaponHitFlash";
        flash.transform.position = position;
        flash.transform.localScale = Vector3.one * 0.22f;
        Object.Destroy(flash.GetComponent<Collider>());

        var renderer = flash.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(1f, 0.75f, 0.35f, 0.85f);
            renderer.sharedMaterial = material;
        }

        Object.Destroy(flash, 0.18f);
    }
}
