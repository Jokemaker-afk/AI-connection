using UnityEngine;

public struct WeaponTargetCandidate
{
    public IDamageable Damageable;
    public EnemyController Enemy;
    public Collider Collider;
    public Vector3 HitPoint;
    public float AngleDegrees;
    public float Distance;
    public float ScreenDistance;
    public bool IsDirectRayHit;

    public string DisplayName
    {
        get
        {
            if (Enemy != null && Enemy.Data.IsValid)
            {
                return Enemy.Data.DisplayNameChinese;
            }

            return Damageable is MonoBehaviour mono ? mono.name : "目标";
        }
    }
}

public struct WeaponTargetingResult
{
    public WeaponTargetCandidate Primary;
    public WeaponTargetCandidate[] DamagedTargets;
    public WeaponTargetCandidate[] CandidatesInArea;
    public bool HasPrimary => Primary.Damageable != null;
    public int DamagedCount => DamagedTargets != null ? DamagedTargets.Length : 0;
    public int CandidateCount => CandidatesInArea != null ? CandidatesInArea.Length : 0;
}
