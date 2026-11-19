using System.Collections.Generic;
using UnityEngine;

/// <summary>Do lech cua 1 bone trong 1 pose, cong them len goc nghi (rest) cua bone.</summary>
[System.Serializable]
public class BonePose
{
    [Tooltip("Trung voi Id trong danh sach Bones cua PosePlayer.")]
    public string boneId;

    [Header("Tu the tinh")]
    public Vector2 position;
    [Tooltip("Do, quanh truc Z.")]
    public float rotation;
    [Tooltip("Cong them vao scale (0 = giu nguyen, 0.1 = to hon 10%).")]
    public Vector2 scaleAdd;

    [Header("Chu ky (chay, tho)")]
    [Tooltip("Bien do xoay (do) theo chu ky.")]
    public float swingRotation;
    [Tooltip("Bien do dich chuyen theo chu ky (vd nhun len xuong).")]
    public Vector2 swingPosition;
    [Tooltip("Lech pha (0..1 chu ky). Chan trai/phai lech 0.5.")]
    public float swingPhase;
    [Tooltip("Foot lift in local units during the forward half of the cycle.")]
    public float stepLift;
}

/// <summary>Mot pose co ten: danh sach lech cua cac bone, toc do chuyen vao va nhip chu ky.</summary>
[System.Serializable]
public class PoseEntry
{
    public string name = "Idle";
    [Tooltip("Toc do chuyen vao pose (lon = nhanh). 12 ~ 0.2 giay.")]
    public float blendSpeed = 12f;
    [Tooltip("So chu ky moi giay cua phan swing. 0 = khong lac.")]
    public float cycleHz = 0f;
    public List<BonePose> bones = new List<BonePose>();
}

/// <summary>
/// Du lieu pose cho PosePlayer. Chinh cam giac (dan nguoi khi chem, ha thap khi ngam, nhip chay)
/// ngay trong asset, khong phai sua code hay prefab. Dung chung duoc cho nhieu nhan vat co cung Id bone.
/// </summary>
[CreateAssetMenu(menuName = "Dang Dang/Pose Set", fileName = "PoseSet")]
public class PoseSet : ScriptableObject
{
    [SerializeField] private List<PoseEntry> poses = new List<PoseEntry>();

    public PoseEntry Find(string poseName)
    {
        for (int i = 0; i < poses.Count; i++)
            if (poses[i].name == poseName) return poses[i];
        return null;
    }
}
