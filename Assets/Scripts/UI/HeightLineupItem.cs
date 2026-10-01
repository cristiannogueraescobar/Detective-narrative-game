using UnityEngine;

/// <summary>
/// Un sospechoso de la rueda con alturas reales (HeightLineup): su altura y la proporción de su figura.
/// </summary>
public class HeightLineupItem : MonoBehaviour
{
    public int heightCm = 170;
    public float aspect = 0.5f;
    public RectTransform figure;
}
