using System;
using UnityEngine;

public class GameScore : MonoBehaviour
{
    [SerializeField] int score = 0;

    public int Score => score;

    public event Action<int> OnScoreChanged;

    public void AddScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    public void ResetScore()
    {
        score = 0;
        OnScoreChanged?.Invoke(score);
    }
}
