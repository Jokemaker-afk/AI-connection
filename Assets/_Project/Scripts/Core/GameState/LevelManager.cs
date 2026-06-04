using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum LevelWinCondition
{
    ScoreThreshold,
    ReachGoal
}

public class LevelManager : MonoBehaviour
{
    [SerializeField] LevelWinCondition winCondition = LevelWinCondition.ScoreThreshold;
    [SerializeField] int scoreToAdvance = 100;
    [SerializeField] string nextSceneName = "Level2";
    [SerializeField] float transitionDelay = 1.5f;
    [SerializeField] string levelCompleteMessage = "恭喜通关！";
    [SerializeField] string scoreAdvanceMessage = "分数达标！";
    [SerializeField] string scoreAdvanceHint = "按 Y 键进入第二关";
    [SerializeField] string goalAdvanceHint = "按 Y 键进入下一关";

    bool transitioning;
    GameScore gameScore;

    public static LevelManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        LevelTransitionOverlay.Hide();
        gameScore = FindFirstObjectByType<GameScore>();
        if (winCondition == LevelWinCondition.ScoreThreshold && gameScore != null)
        {
            gameScore.OnScoreChanged += HandleScoreChanged;
            HandleScoreChanged(gameScore.Score);
        }
    }

    void OnDisable()
    {
        if (gameScore != null)
        {
            gameScore.OnScoreChanged -= HandleScoreChanged;
        }
    }

    void HandleScoreChanged(int score)
    {
        if (winCondition != LevelWinCondition.ScoreThreshold || transitioning)
        {
            return;
        }

        if (score >= scoreToAdvance)
        {
            BeginTransition(nextSceneName);
        }
    }

    public void NotifyGoalReached()
    {
        if (winCondition != LevelWinCondition.ReachGoal || transitioning)
        {
            return;
        }

        BeginTransition(string.IsNullOrEmpty(nextSceneName) ? null : nextSceneName);
    }

    public void ConfigureGoalTransition(string sceneName, string completeMessage, string advanceHint)
    {
        winCondition = LevelWinCondition.ReachGoal;
        nextSceneName = sceneName;
        if (!string.IsNullOrEmpty(completeMessage))
        {
            levelCompleteMessage = completeMessage;
        }

        if (!string.IsNullOrEmpty(advanceHint))
        {
            goalAdvanceHint = advanceHint;
        }
    }

    void BeginTransition(string sceneName)
    {
        if (transitioning)
        {
            return;
        }

        transitioning = true;
        StartCoroutine(TransitionRoutine(sceneName));
    }

    IEnumerator TransitionRoutine(string sceneName)
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = false;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            LevelTransitionOverlay.Show(levelCompleteMessage);
            yield return new WaitForSeconds(transitionDelay + 1f);
            LevelTransitionOverlay.Hide();
            transitioning = false;

            if (player != null)
            {
                player.enabled = true;
            }

            yield break;
        }

        string hint = winCondition == LevelWinCondition.ScoreThreshold
            ? scoreAdvanceHint
            : goalAdvanceHint;

        if (winCondition == LevelWinCondition.ScoreThreshold)
        {
            int currentScore = gameScore != null ? gameScore.Score : scoreToAdvance;
            LevelTransitionOverlay.ShowScoreAdvancePrompt(
                scoreAdvanceMessage,
                currentScore,
                scoreToAdvance,
                hint);
        }
        else
        {
            LevelTransitionOverlay.ShowAdvancePrompt(levelCompleteMessage, hint);
        }

        yield return WaitForConfirmKey();

        LevelTransitionOverlay.Hide();
        GameplayFoundationBootstrap.PrepareSceneTransition(sceneName);
        SceneManager.LoadScene(sceneName);
    }

    static IEnumerator WaitForConfirmKey()
    {
        while (true)
        {
            if (Keyboard.current != null && Keyboard.current.yKey.wasPressedThisFrame)
            {
                yield break;
            }

            yield return null;
        }
    }
}
