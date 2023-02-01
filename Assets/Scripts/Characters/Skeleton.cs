using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Characters
{
    public class Skeleton : Enemy
    {
        private RenderTexture _sourceTexture;

        public Camera lightCalculatorCamera;
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
            //TODO: Count light

            lightLevel = CalculateLight();
            
            if (lightLevel < 0.1f)
                base.UpdateCustomBehaviour();
        }

        private float CalculateLight()
        {
            var tmp = RenderTexture.GetTemporary(
                _sourceTexture.width, _sourceTexture.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.Linear
            );
            
            Graphics.Blit(_sourceTexture, tmp);
            var previous = RenderTexture.active;
            RenderTexture.active = tmp;
            
            var myTexture2D = new Texture2D(_sourceTexture.width, _sourceTexture.height);
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);
            
            Color32 centerPixel = myTexture2D.GetPixel(myTexture2D.width / 2, myTexture2D.height / 2);
            Destroy(myTexture2D);

            return (0.2126f * centerPixel.r) + (0.7152f * centerPixel.g) + (0.0722f * centerPixel.b);
        }
    }
}
