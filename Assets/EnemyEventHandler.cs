using System.Collections;
using System.Collections.Generic;
using Characters;
using UnityEngine;

public class EnemyEventHandler : MonoBehaviour
{
    [SerializeField] private Enemy enemy;

    public void ShowAttackCollider()
    {
        enemy.ToggleAttackCollider(true);
    }
    
    public void HideAttackCollider()
    {
        enemy.ToggleAttackCollider(false);
    }
}
