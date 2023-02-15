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

            var redIntensity = (byte)(50 * (maxHealth - currentHealth));
            var color = new Color32(redIntensity, 0, 0, 0);
            
            FadeEyeSightTo(color, 0.5f);
        }

        public override IEnumerator DieRoutine()
        {
            yield return base.DieRoutine();
            yield return FadeEyeSightToRoutine(Color.black, 3f);
            
            SceneManager.LoadScene(0);
        }

        public void FadeEyeSightTo(Color32 newColor, float time)
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            
            _fadeCoroutine = StartCoroutine(FadeEyeSightToRoutine(newColor, time));
        }

        private IEnumerator FadeEyeSightToRoutine(Color32 newColor, float time)
        {
            var currentColor = visionLimitation.color;

            var redSpeed = (newColor.r - currentColor.r) / time;
            var greenSpeed = (newColor.g - currentColor.g) / time;
            var blueSpeed = (newColor.b - currentColor.b) / time;
            var alphaSpeed = (newColor.a - currentColor.a) / time;

            var currentTime = 0f;
            while (Math.Abs(time - currentTime) > 0.01f)
            {
                var deltaTime = Time.deltaTime;
                currentTime += deltaTime;
                
                currentColor.r += redSpeed * deltaTime;
                currentColor.g += greenSpeed * deltaTime;
                currentColor.b += blueSpeed * deltaTime;
                currentColor.a += alphaSpeed * deltaTime;

                visionLimitation.color = currentColor;
                
                yield return null;
            }

            _fadeCoroutine = null;
        }
    }
}
