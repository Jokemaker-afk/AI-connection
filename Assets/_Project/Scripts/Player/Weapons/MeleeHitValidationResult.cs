public struct MeleeHitValidationResult
{
    public bool HasTarget;
    public bool InRange;
    public bool InAngle;
    public bool InVertical;
    public bool IsValid;
    public float Distance;
    public float Angle;
    public float MaxRange;
    public float MaxAngle;
    public string MissReason;
}
