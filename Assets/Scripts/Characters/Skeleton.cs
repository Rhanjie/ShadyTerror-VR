using System.Collections.Generic;
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
        public Vector3 darkestDirection;

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
            CalculateLightIntensity();

            if (lightLevel >= minLightLevelToDamage)
            {
                //TODO: Jump to darkest area to avoid light
                
                HandleLightDamage();
            }
            
            else if (lightLevel >= minLightLevelToBlock)
            {
                //TODO: Check if current direction has darkest light.
                
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

        private void CalculateLightIntensity()
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
            
            lightLevel = GetIntensityFromPixel(texture2D, texture2D.width / 2, texture2D.height / 2);
            darkestDirection = GetDarkestDirection(texture2D);
            
            Destroy(texture2D);
        }

        private float GetIntensityFromPixel(Texture2D texture2D, int x, int y)
        {
            Color32 pixel = texture2D.GetPixel(x, y);
            
            return (0.2126f * pixel.r) + (0.7152f * pixel.g) + (0.0722f * pixel.b);
        }
        
        //TODO: Get all light intensities from directions

        private Vector3 GetDarkestDirection(Texture2D texture2D)
        {
            var pixelCoordinatesToCheck = new List<Vector2>
            {
                new(0, 0),
                new(texture2D.width - 1, 0),
                new(0, texture2D.height - 1),
                new(texture2D.width - 1, texture2D.height - 1),
            };
            
            //FORWARD|LEFT , FORWARD|RIGHT , BACKWARD|LEFT , BACKWARD|RIGHT
            
            //FORWARD|LEFT , FORWARD|RIGHT -> FORWARD
            //FORWARD|LEFT , BACKWARD|LEFT -> LEFT
            //FORWARD|RIGHT , BACKWARD|RIGHT -> RIGHT
            //BACKWARD|LEFT , BACKWARD|RIGHT -> BACKWARD

            foreach (var pixel in pixelCoordinatesToCheck)
            {
                var intensity = GetIntensityFromPixel(texture2D, (int)pixel.x, (int)pixel.y);
            }
            
            //TODO: Get all pixels from the corners
            //TODO: Get light intensities to check darkest diagonals
            //TODO: Get two corners and divide by 2 to get straight directions
            //TODO: Get darkest direction
            
            return Vector3.zero;
        }
    }
}
