using UnityEngine;

/// <summary>
/// Builds world targeting rays from the screen crosshair only — never from mouse pointer.
/// Prefer <see cref="AimReferenceProvider"/> for aim direction and hit queries.
/// </summary>
public static class CrosshairRayUtility
{
    public static Ray GetCrosshairRay(out Vector2 screenPoint)
    {
        Camera camera = ResolveGameplayCamera();
        if (camera == null)
        {
            screenPoint = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            return new Ray(Vector3.zero, Vector3.forward);
        }

        if (TryGetCrosshairRectScreenPoint(camera, out screenPoint))
        {
            return camera.ScreenPointToRay(screenPoint);
        }

        var cameraController = Object.FindFirstObjectByType<PlayerCameraController>();
        if (cameraController != null)
        {
            screenPoint = cameraController.GetCrosshairScreenPoint();
            return camera.ScreenPointToRay(screenPoint);
        }

        screenPoint = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        return camera.ScreenPointToRay(screenPoint);
    }

    public static Camera ResolveGameplayCamera()
    {
        var cameraController = Object.FindFirstObjectByType<PlayerCameraController>();
        if (cameraController != null)
        {
            Camera attachedCamera = cameraController.GetComponent<Camera>();
            if (attachedCamera != null)
            {
                return attachedCamera;
            }
        }

        return Camera.main;
    }

    static bool TryGetCrosshairRectScreenPoint(Camera worldCamera, out Vector2 screenPoint)
    {
        screenPoint = default;

        RectTransform crosshairRect = null;
        if (AimReferenceProvider.Instance != null)
        {
            crosshairRect = AimReferenceProvider.Instance.BoundCrosshairRect;
        }

        if (crosshairRect == null)
        {
            var crosshairHud = Object.FindFirstObjectByType<GameplayCrosshairHud>();
            crosshairRect = crosshairHud != null ? crosshairHud.CrosshairRectTransform : null;
        }

        if (crosshairRect == null || !crosshairRect.gameObject.activeInHierarchy)
        {
            return false;
        }

        Canvas canvas = crosshairRect.GetComponentInParent<Canvas>();
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, crosshairRect.position);
        return true;
    }
}
