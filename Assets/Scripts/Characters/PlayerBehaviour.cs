using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Characters
{
    public class PlayerBehaviour : Character
    {
        [Header("Custom")]

        [SerializeField]
        private Image visionLimitation;

        private string _deadSound = "DeadScream";
        private Coroutine _fadeCoroutine = null;
        
        private static readonly Color32 Transparent = new(0, 0, 0, 0);

        protected override void Start()
        {
            base.Start();

            FadeEyeSightTo(Transparent, 2f);
        }

        public override void UpdateCustomBehaviour()
        {
            //...
        }

        public override void Hit(string colliderName)
        {
            base.Hit(colliderName);

            var difference = maxHealth - currentHealth;
            var color = new Color32((byte)(difference * 40), 0, 0, (byte)(difference * 8));
            
            FadeEyeSightTo(color, 1f);
        }

        public override IEnumerator DieRoutine()
        {
            audioManager.PlaySound(_deadSound);
            
            yield return base.DieRoutine();
            yield return FadeEyeSightToRoutine(Color.black, 2f);
            
            SceneManager.LoadScene(0);
        }

        public void FadeEyeSightTo(Color32 newColor, float time)
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            
            _fadeCoroutine = StartCoroutine(FadeEyeSightToRoutine(newColor, time));
        }

        private IEnumerator FadeEyeSightToRoutine(Color newColor, float time)
        {
            var currentColor = visionLimitation.color;

            var redSpeed = (newColor.r - currentColor.r) / time;
            var greenSpeed = (newColor.g - currentColor.g) / time;
            var blueSpeed = (newColor.b - currentColor.b) / time;
            var alphaSpeed = (newColor.a - currentColor.a) / time;

            var currentTime = 0f;
            while (time - currentTime > 0f)
            {
                var deltaTime = Time.deltaTime;
                currentTime += deltaTime;
                
                currentColor.r += (redSpeed * deltaTime);
                currentColor.g += (greenSpeed * deltaTime);
                currentColor.b += (blueSpeed * deltaTime);
                currentColor.a += (alphaSpeed * deltaTime);
                
                visionLimitation.color = currentColor;

                yield return null;
            }
            
            visionLimitation.color = newColor;
            _fadeCoroutine = null;
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            DieByObstacle(collision.collider);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            DieByObstacle(other);
        }

        private void DieByObstacle(Collider other)
        {
            if (other.transform.CompareTag("Water") || other.transform.CompareTag("Fire"))
            {
                _deadSound = $"{other.transform.tag}Scream";
                
                StartCoroutine(DieRoutine());
            }
        }
    }
}
