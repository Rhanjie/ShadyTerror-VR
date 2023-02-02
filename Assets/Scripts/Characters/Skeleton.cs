using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Characters
{
    public class Skeleton : Enemy
    {
        private RenderTexture _sourceTexture;

        public Camera lightCalculatorCamera;
        public float minLightLevelToBlock = 10f;
        public float minLightLevelToDamage = 30f;
        
        public float lightLevel;

        protected override void Start()
        {
            base.Start();

            var colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
            var depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
            
            _sourceTexture = new RenderTexture(32, 32, colorFormat, depthStencilFormat);
            lightCalculatorCamera.targetTexture = _sourceTexture;
        }

        public override void UpdateCustomBehaviour()
        {
            lightLevel = CalculateLight();

            if (lightLevel >= minLightLevelToDamage)
            {
                HandleLightDamage();
            }
            
            else if (lightLevel >= minLightLevelToBlock)
            {
                HandleIfLightBorder();
            }

            else base.UpdateCustomBehaviour();
        }

        private void HandleIfLightBorder()
        {
            //TODO: Just stay and wait for opportunity
            
            ServeGravity();
            FaceToTarget();
            animator.SetFloat(VelocityHash, 0);
        }

        private void HandleLightDamage()
        {
            //TODO: Find darkest direction and jump there
            
            ServeGravity();
            FaceToTarget();
            animator.SetFloat(VelocityHash, 0);
            
            //Hit();
        }

        private float CalculateLight()
        {
            var temporary = RenderTexture.GetTemporary(
                _sourceTexture.width, _sourceTexture.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.Linear
            );
            
            Graphics.Blit(_sourceTexture, temporary);
            var previous = RenderTexture.active;
            RenderTexture.active = temporary;
            
            var texture2D = new Texture2D(_sourceTexture.width, _sourceTexture.height);
            texture2D.ReadPixels(new Rect(0, 0, temporary.width, temporary.height), 0, 0);
            texture2D.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);
            
            Color32 centerPixel = texture2D.GetPixel(texture2D.width / 2, texture2D.height / 2);
            Destroy(texture2D);

            return (0.2126f * centerPixel.r) + (0.7152f * centerPixel.g) + (0.0722f * centerPixel.b);
        }
    }
}
