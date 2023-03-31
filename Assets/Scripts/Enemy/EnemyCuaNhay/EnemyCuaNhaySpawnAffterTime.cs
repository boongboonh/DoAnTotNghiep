using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCuaNhaySpawnAffterTime : SpawnEnemyAffterTime
{
    protected override void getHealthEnemy()
    {// ko co base: base ghi them ca phuong thuc cu
        hp = enemyNeedSpawn.GetComponent<EnemyCuaNhayHealth>().healthEnemy;
    }
}
