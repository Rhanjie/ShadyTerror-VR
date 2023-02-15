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
