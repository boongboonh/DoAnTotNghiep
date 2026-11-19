using UnityEngine;

/// <summary>
/// Thong so di chuyen cua player, luu thanh asset de chinh cam giac khi dang choi
/// (sua trong Play Mode roi copy gia tri) va dung chung cho nhieu nhan vat / che do.
/// </summary>
[CreateAssetMenu(menuName = "Dang Dang/Player Movement Config", fileName = "PlayerMovementConfig")]
public class PlayerMovementConfig : ScriptableObject
{
    [Header("Chay")]
    [Tooltip("Toc do ngang toi da (don vi/giay).")]
    public float runSpeed = 7f;
    [Tooltip("Gia toc khi dang tren dat va co input (don vi/giay^2). Rat lon = doi huong tuc thi.")]
    public float groundAcceleration = 120f;
    [Tooltip("Gia toc phanh khi tren dat va khong co input.")]
    public float groundDeceleration = 120f;
    [Tooltip("Gia toc ngang khi tren khong (co input).")]
    public float airAcceleration = 90f;
    [Tooltip("Gia toc phanh ngang khi tren khong va khong co input.")]
    public float airDeceleration = 60f;

    [Header("Nhay")]
    [Tooltip("Van toc doc luc bat nhay.")]
    public float jumpVelocity = 11f;
    [Tooltip("Van toc doc cua lan nhay bo sung tren khong.")]
    [Min(0.1f)] public float airJumpVelocity = 10.5f;
    [Tooltip("Nha nut khi dang bay len: van toc doc nhan he so nay (nhay thap/cao theo luc bam).")]
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.45f;
    [Tooltip("Con cho nhay (giay) sau khi roi khoi mep.")]
    public float coyoteTime = 0.1f;
    [Tooltip("Nho lenh nhay bam som (giay) truoc khi cham dat.")]
    public float jumpBufferTime = 0.12f;
    [Tooltip("Nho lenh dash bam som truoc khi het hoi chieu / ket thuc hanh dong, giay.")]
    [Min(0f)] public float dashBufferTime = 0.15f;

    [Header("Tuong")]
    [Min(.001f)] public float wallCheckDistance = .1f;
    [Min(0f)] public float wallSlideSpeed = 2.5f;
    [Tooltip("Nhay tuong: x = day ra xa tuong, y = luc len. Nho de co the quay lai cung buc tuong va leo tiep.")]
    public Vector2 wallJumpVelocity = new Vector2(5.5f, 10f);
    [Tooltip("Thoi gian ngan bi khoa dieu khien ngang ngay sau khi bat tuong.")]
    [Min(0f)] public float wallJumpControlTime = .1f;
    [Tooltip("Con cho nhay tuong (giay) sau khi vua roi khoi tuong: bam Space tre mot chut van tinh.")]
    [Min(0f)] public float wallCoyoteTime = .12f;
    [Tooltip("Sau khi bat tuong, giu huong ve phia tuong se tang toc ngang manh de bam lai buc tuong (leo mot tuong).")]
    [Min(0f)] public float wallRegrabAcceleration = 90f;
    [Min(0f)] public float wallRegrabWindow = .5f;
    [Header("Chem xuong (Spirit Edge)")]
    [Tooltip("Van toc bat len khi chem xuong trung muc tieu (1 lan moi lan roi).")]
    public float pogoVelocity = 10f;
    [Header("Bam tuong (giu Shift / RT)")]
    [Min(0f)] public float wallClimbUpSpeed = 3.5f;
    [Min(0f)] public float wallClimbDownSpeed = 5f;
    [Tooltip("Nhay nhe qua mep khi leo het tuong: x = ve phia tuong, y = len.")]
    public Vector2 wallLedgeHop = new Vector2(3f, 7f);

    [Header("Trong luc")]
    [Tooltip("Nhan gravity khi dang roi (cam giac roi nhanh, gon hon).")]
    public float fallGravityMultiplier = 1.3f;
    [Tooltip("Toc do roi toi da (gia tri duong).")]
    public float maxFallSpeed = 25f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashTime = 0.3f;
    public float dashCooldown = 0.5f;
    [Tooltip("Gioi han van toc ngang khi thoat dash (0 = dung ngang).")]
    [Min(0f)] public float dashExitSpeed = 6.5f;

    [Header("Bi thuong")]
    [Tooltip("Thoi gian bi choang (khong dieu khien duoc), giay.")]
    public float hurtStunTime = 0.25f;
    [Tooltip("Van toc day lui: x huong ra xa nguon sat thuong, y huong len.")]
    public Vector2 hurtKnockback = new Vector2(6f, 7f);
    [Tooltip("Luc can ngang trong luc bi choang (don vi/giay^2).")]
    public float hurtDrag = 20f;
    [Tooltip("Thoi gian bat tu sau khi bi thuong, giay.")]
    public float invincibleTime = 1f;
    [Tooltip("Chu ky nhap nhay khi bat tu, giay.")]
    public float flickerInterval = 0.1f;

    [Header("Cham dat")]
    [Tooltip("Khoang cach them xuong duoi khi do mat dat.")]
    public float groundCheckExtra = 0.03f;

    [Header("Idle dac biet")]
    [Tooltip("Dung yen bao lau (giay) thi bao 'Bored' (pose nhan doi).")]
    public float boredTime = 10f;
}
