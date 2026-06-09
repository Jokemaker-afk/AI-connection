/// <summary>Scene-specific objective HUD panel on persistent GameplayHUD.</summary>
public interface ILevelObjectiveHudPanel
{
    int ObjectiveLevel { get; }

    void ResetForSceneTransition();

    void RefreshObjectiveDisplay();
}
