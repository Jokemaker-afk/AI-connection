using UnityEngine;

[DisallowMultipleComponent]
public class SceneGameplayBootstrap : MonoBehaviour
{
    [SerializeField] bool bootstrapOnAwake = false;

    void Awake()
    {
        if (!bootstrapOnAwake)
        {
            return;
        }

        GameplayFoundationBootstrap.EnsureForActiveScene();
    }
}
