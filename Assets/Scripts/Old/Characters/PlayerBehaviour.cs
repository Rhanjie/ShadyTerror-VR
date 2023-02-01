using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Characters
{
    [Serializable]
    internal struct ImpactParticle
    {
        public LayerMask layerMask;
        public ParticleSystem particleSystem;
    }
    
    public class PlayerBehaviour : Character
    {
        [Header("Custom")]

        [SerializeField]
        private UiManager uiManager;
        
        [SerializeField]
        private AudioClip hitSound;

        [SerializeField]
        private List<ImpactParticle> impactParticles;
        
        protected override void Start()
        {
            base.Start();
        }

        public override void UpdateCustomBehaviour()
        {
            //...
        }

        protected override IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint, RaycastHit hit = default)
        {
            if (hit.collider != null)
            {
                var hitObjectLayer = hit.collider.gameObject.layer;
                var index = impactParticles.FindIndex(it => it.layerMask == (it.layerMask | (1 << hitObjectLayer)));
                if (index != -1)
                {
                    Instantiate(impactParticles[index].particleSystem, hitPoint, Quaternion.LookRotation(hit.normal), hit.transform);
                }
            }

            yield return base.SpawnTrail(trail, hitPoint, hit);
        }

        public override void Attack()
        {
            base.Attack();
            
            //...
        }

        public override void Hit(string colliderName)
        {
            base.Hit(colliderName);

            uiManager.SetHealthInfo(currentHealth, maxHealth);
        }

        public override IEnumerator DieRoutine()
        {
            yield return base.DieRoutine();
            
            //TODO: Show summary panel
            SceneManager.LoadScene(0);
        }

        public void SendDeathMessage(bool isHead)
        {
            //uiManager.SetScoreInfo(_score);
        }
    }
}
