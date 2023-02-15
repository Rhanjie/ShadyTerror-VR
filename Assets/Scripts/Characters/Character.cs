using System;
using System.Collections;
using UnityEngine;

namespace Characters
{
    public abstract class Character : MonoBehaviour, ICharacter
    {
        [Header("References")]
        [SerializeField] protected AudioManager audioManager;

        protected int maxHealth = 5;
        protected int currentHealth;
        protected bool isHeadshot = false;
        protected bool isDead;

        protected virtual void Start()
        {
            currentHealth = maxHealth;
        }

        protected virtual void Update()
        {
            if (isDead)
                return;

            UpdateCustomBehaviour();
            
            if (currentHealth <= 0)
            {
                StartCoroutine(DieRoutine());
            }
        }

        public abstract void UpdateCustomBehaviour();

        public virtual void Hit(string colliderName)
        {
            //If it is a headshot, then kill the enemy immediately
            isHeadshot = colliderName.Contains("head", StringComparison.CurrentCultureIgnoreCase);
            
            currentHealth -= (isHeadshot) ? maxHealth : 1;
        }

        public virtual IEnumerator DieRoutine()
        {
            isDead = true;

            yield return null;
        }
    }
}
