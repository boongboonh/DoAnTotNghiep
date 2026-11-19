using System.Collections.Generic;
using UnityEngine;

/// <summary>Three-wave combat test on an extension to the left of the playground.</summary>
public sealed class PrototypeCombatRoom : MonoBehaviour
{
    private PlayerController player;
    private PrototypePlayerHealth health;
    private Sprite sprite;
    private readonly List<MonoBehaviour> enemies = new List<MonoBehaviour>();
    private readonly HashSet<PrototypeProjectile> projectiles = new HashSet<PrototypeProjectile>();
    private TextMesh label;
    private float nextWaveDelay;
    public int WaveIndex { get; private set; }
    public bool Active { get; private set; }
    public bool Completed { get; private set; }
    public int ProjectileCount => projectiles.Count;

    public void Initialize(PlayerController controller, PrototypePlayerHealth vitality, Sprite square)
    {
        player=controller;health=vitality;sprite=square;
        var floor=PrototypeGeometry.Rectangle(transform,sprite,"Combat Room Floor",new Vector2(16f,1f),new Vector2(-15f,-.65f),new Color(.25f,.28f,.3f),0);
        floor.gameObject.AddComponent<BoxCollider2D>().size=Vector2.one;
        label=PrototypeGeometry.Label(transform,"Wave Status",new Vector2(-15f,4.6f),.11f);
        var entrance=PrototypeGeometry.Label(transform,"Combat Entrance",new Vector2(-8f,2.6f),.085f);
        entrance.text="< COMBAT TRIAL\n3 WAVES | W / Up + LMB: Up attack";
        AddMelee(new Vector2(-16,.55f));
        AddSpecial(new Vector2(-17,.55f),PrototypeEnemyKind.Shooter);
        AddMelee(new Vector2(-18,.55f));
        AddSpecial(new Vector2(-12,.55f),PrototypeEnemyKind.Shooter);
        AddSpecial(new Vector2(-15,2.4f),PrototypeEnemyKind.Flyer);
        AddMelee(new Vector2(-20.5f,.55f),true);
        ResetRoom(true);
    }

    private GameObject Root(string name,Vector2 position)
    {
        var root=new GameObject(name);root.transform.SetParent(transform,false);root.transform.position=position;root.tag="Enemy";
        root.AddComponent<Rigidbody2D>();root.AddComponent<BoxCollider2D>();return root;
    }
    private void AddMelee(Vector2 position,bool charger=false)
    {
        var enemy=Root(charger?"Wave Charger":"Wave Melee",position).AddComponent<PrototypeMeleeEnemy>();
        enemy.Initialize(player,health,sprite);
        if(charger)enemy.MakeCharger();
        enemies.Add(enemy);
    }
    private void AddSpecial(Vector2 position,PrototypeEnemyKind kind)
    {
        var enemy=Root("Wave "+kind,position).AddComponent<PrototypeSpecialEnemy>();
        enemy.Initialize(player,health,sprite,kind,this);enemies.Add(enemy);
    }
    private bool InRoom => player.Motor.Position.x < -9f && player.Motor.Position.x > -22f
        && player.Motor.Position.y > -2f && player.Motor.Position.y < 6f;
    private void Update() => Advance(Time.deltaTime);

    public void Advance(float dt)
    {
        if(dt<=0 || player.IsDead || Completed)return;
        if(!Active)
        {
            if(InRoom)StartWave(WaveIndex);
            return;
        }
        // Do not allow AI to attack the player after leaving the trial.
        if(!InRoom){ResetRoom(false);return;}
        foreach(var enemy in enemies)
        {
            if(!enemy.gameObject.activeSelf)continue;
            int hp=enemy is PrototypeMeleeEnemy melee?melee.Health:((PrototypeSpecialEnemy)enemy).Health;
            if(hp>0){nextWaveDelay=0;return;}
        }
        nextWaveDelay+=dt;
        if(nextWaveDelay<.8f)return;
        ClearProjectiles();
        if(WaveIndex>=2)
        {
            Active=false;Completed=true;label.text="COMBAT TRIAL COMPLETE\nF5: Reset | Ctrl+F5: Restart";
        }
        else StartWave(WaveIndex+1);
    }

    private void StartWave(int index)
    {
        ClearProjectiles();WaveIndex=index;Active=true;Completed=false;nextWaveDelay=0;
        for(int i=0;i<enemies.Count;i++)
        {
            bool enabled=index==0?i==0:index==1?i==1:i>=2;
            if(enemies[i] is PrototypeMeleeEnemy melee)melee.ResetEnemy();
            else ((PrototypeSpecialEnemy)enemies[i]).ResetEnemy();
            enemies[i].gameObject.SetActive(enabled);
        }
        label.text="WAVE "+(index+1)+" / 3\n"+(index==0?"MELEE":index==1?"SHOOTER":"MELEE + SHOOTER + FLYER + CHARGER");
    }
    public void ResetRoom(bool fullRestart)
    {
        ClearProjectiles();
        if(fullRestart||Completed)WaveIndex=0;
        Active=false;Completed=false;nextWaveDelay=0;
        foreach(var enemy in enemies)
        {
            if(enemy is PrototypeMeleeEnemy melee)melee.ResetEnemy();
            else ((PrototypeSpecialEnemy)enemy).ResetEnemy();
            enemy.gameObject.SetActive(false);
        }
        if(label!=null)label.text="COMBAT TRIAL\nEnter from right | Next: Wave "+(WaveIndex+1);
    }
    public void Register(PrototypeProjectile shot)=>projectiles.Add(shot);
    public void Unregister(PrototypeProjectile shot)=>projectiles.Remove(shot);
    public void ClearProjectiles()
    {
        var snapshot=new PrototypeProjectile[projectiles.Count];projectiles.CopyTo(snapshot);
        foreach(var shot in snapshot)if(shot!=null)shot.Despawn();
        projectiles.Clear();
    }
    private void OnDestroy()=>ClearProjectiles();
}
