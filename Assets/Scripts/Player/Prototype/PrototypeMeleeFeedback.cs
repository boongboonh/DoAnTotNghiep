using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(PlayerCombat))]
public sealed class PrototypeMeleeFeedback : MonoBehaviour
{
    [SerializeField] private Sprite square;
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private float hitStopDuration = .045f;
    private PlayerCombat combat;
    private SpriteRenderer region;
    private LineRenderer outline;
    private float lastHitFrame = -1f;
    private TextMesh comboLabel;

    private void Awake()
    {
        combat = GetComponent<PlayerCombat>();
        var g = new GameObject("Attack Region");
        g.transform.SetParent(transform, false);
        region = g.AddComponent<SpriteRenderer>();
        region.sprite = square;
        region.sortingOrder = 5;
        outline = g.AddComponent<LineRenderer>();
        outline.useWorldSpace = false;
        outline.loop = true;
        outline.positionCount = 4;
        outline.SetPositions(new[] { new Vector3(-.5f,-.5f,0), new Vector3(.5f,-.5f,0), new Vector3(.5f,.5f,0), new Vector3(-.5f,.5f,0) });
        outline.sharedMaterial = outlineMaterial;
        outline.widthMultiplier = .025f;
        outline.sortingOrder = 6;
        var label = new GameObject("Combo Status");
        label.transform.SetParent(transform, false);
        label.transform.localPosition = new Vector3(0f, 1.05f, 0f);
        comboLabel = label.AddComponent<TextMesh>();
        comboLabel.anchor = TextAnchor.MiddleCenter;
        comboLabel.fontSize = 48;
        comboLabel.characterSize = .065f;
        ShowRegion();
    }

    private void OnEnable()
    {
        combat.HitLanded += OnHit;
        combat.Ended += ShowRegion;
    }

    private void OnDisable()
    {
        combat.HitLanded -= OnHit;
        combat.Ended -= ShowRegion;
        region.enabled = outline.enabled = false;
        comboLabel.text = string.Empty;
    }

    private void LateUpdate() => ShowRegion();

    private void ShowRegion()
    {
        bool visible = combat.Active;
        region.enabled = outline.enabled = visible;
        comboLabel.text = visible ? (combat.StepIndex + 1) + "/" + combat.ComboStepCount
            + " " + combat.CurrentStep.name + (combat.Upward ? " UP" : combat.Downward ? " DOWN" : combat.Long ? " LONG" : "")
            + (combat.ChainQueued ? " > NEXT" : combat.CanQueueNext ? " [LMB]" : "") : string.Empty;
        // TextMesh giu chieu doc khi player quay mat sang trai.
        comboLabel.transform.rotation = Quaternion.identity;
        if (!visible) return;
        var step = combat.CurrentStep;
        region.transform.localPosition = combat.HitOffset;
        region.transform.localScale = new Vector3(combat.HitSize.x, combat.HitSize.y, 1f);
        bool last = combat.StepIndex == combat.ComboStepCount - 1 && combat.ComboStepCount > 1;
        Color activeColor = last ? new Color(1f,.45f,1f)
            : combat.StepIndex % 2 == 0 ? new Color(1f,.3f,.1f) : new Color(.25f,.9f,1f);
        Color color = combat.Phase == MeleeAttackPhase.Windup ? new Color(1f,.75f,.2f)
            : combat.Phase == MeleeAttackPhase.Active ? activeColor : new Color(.65f,.7f,.75f);
        outline.startColor = outline.endColor = color;
        color.a = combat.Phase == MeleeAttackPhase.Active ? .42f : .12f;
        region.color = color;
    }

    private void OnHit(AttackStep step, Collider2D target)
    {
        var dummy = target.GetComponentInParent<PrototypeTrainingDummy>();
        bool finisher = combat.StepIndex == combat.ComboStepCount - 1 && combat.ComboStepCount > 1;
        if (dummy != null) dummy.PushAwayFrom(GetComponent<PlayerMotor>().Position, finisher ? 6f : 3.5f);
        var enemy = target.GetComponentInParent<PrototypeMeleeEnemy>();
        if (enemy != null) enemy.PushAwayFrom(GetComponent<PlayerMotor>().Position, finisher ? 6f : 3.5f);
        var special = target.GetComponentInParent<PrototypeSpecialEnemy>();
        if (special != null) special.PushAwayFrom(GetComponent<PlayerMotor>().Position, finisher ? 6f : 3.5f);
        Vector2 center = combat.HitCenter;
        Vector2 point = target.ClosestPoint(center);
        int rays = finisher ? 6 : 4;
        for (int i = 0; i < rays; i++)
        {
            var g = new GameObject("Impact");
            g.transform.position = point;
            g.transform.rotation = Quaternion.Euler(0, 0, 45f + i * (360f / rays));
            g.transform.localScale = new Vector3(finisher ? .4f : .25f, .06f, 1);
            var r = g.AddComponent<SpriteRenderer>();
            r.sprite = square;
            r.color = new Color(1, .9f, .5f);
            r.sortingOrder = 10;
            Vector3 destination = g.transform.position + g.transform.right * .4f;
            DOTween.Sequence().Append(g.transform.DOMove(destination, .14f))
                .Join(g.transform.DOScale(Vector3.zero, .14f))
                .SetLink(g).OnComplete(() => Destroy(g));
        }
        // Nhieu muc tieu trong cung frame cung chi yeu cau mot hitstop.
        if (lastHitFrame != Time.frameCount)
        {
            lastHitFrame = Time.frameCount;
            DangDang.Combat.HitStop.Request(hitStopDuration * (finisher ? 1.5f : 1f));
        }
    }
}
