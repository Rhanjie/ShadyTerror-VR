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
        private AudioClip reloadingSound;

        [SerializeField]
        private int ammoCapacity = 10;
        
        [SerializeField]
        private List<ImpactParticle> impactParticles;

        private int _score;
        private int _ammo;
        private bool _isReloading;
        
        private static readonly int ReloadTrigger = Animator.StringToHash("Reload");
        
#if UNITY_EDITOR
        private Vector2 _mousePosition;
        private const float RotationSensitivity = 5f;
#endif

        protected override void Start()
        {
            base.Start();

            _ammo = ammoCapacity;

            uiManager.SetHealthInfo(currentHealth, maxHealth);
            uiManager.SetAmmoInfo(_ammo);
            uiManager.SetScoreInfo(0);

            uiManager.ResizePoint(currentGunScatter);
        }

        public override void UpdateCustomBehaviour()
        {
            uiManager.ResizePoint(currentGunScatter);
            
#if UNITY_EDITOR
            //Old input system is used only for testing
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        
            if (Input.GetMouseButton(1)) {
                _mousePosition.x += Input.GetAxis("Mouse X") * RotationSensitivity;
            
                switch (_mousePosition.x)
                {
                    case <= -180:
                        _mousePosition.x += 360; break;
                    case > 180:
                        _mousePosition.x -= 360; break;
                }
            
                _mousePosition.y -= Input.GetAxis("Mouse Y") * RotationSensitivity / 2f;
                _mousePosition.y = Mathf.Clamp(_mousePosition.y, -85, 85);
            
                var rotation = Quaternion.Euler(_mousePosition.y, _mousePosition.x, 0);
            
                transform.rotation = rotation;
            }
#else
            if (Google.XR.Cardboard.Api.IsTriggerPressed)
            {
                Shoot();
            }
#endif
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

        public override void Shoot()
        {
            if (_isReloading)
            {
                return;
            }

            uiManager.SetAmmoInfo(--_ammo);
            
            base.Shoot();
            
            if (_ammo <= 0)
            {
                StartCoroutine(Reloading());
            }
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
            _score += isHead ? 10 : 1;
            
            uiManager.SetScoreInfo(_score);
        }
        
        //Reloading method is used only by player
        private IEnumerator Reloading()
        {
            _isReloading = true;
            gunAnimator.SetTrigger(ReloadTrigger);
            gunAudioSource.PlayOneShot(reloadingSound);

            yield return new WaitForSeconds(1.5f);
                
            _ammo = ammoCapacity;
            _isReloading = false;
        }
    }
}
