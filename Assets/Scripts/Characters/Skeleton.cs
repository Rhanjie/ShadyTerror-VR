using UnityEngine;

namespace Characters
{
    public class Skeleton : Enemy
    {
        private float _rawLightLevel;
        
        public RenderTexture sourceTexture;
        public int lightLevel;
        
        public override void UpdateCustomBehaviour()
        {
            //TODO: Count light

            lightLevel = CalculateLight();
            
            if (lightLevel < 100)
                base.UpdateCustomBehaviour();
        }

        private int CalculateLight()
        {
            var tmp = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            
            Graphics.Blit(sourceTexture, tmp);
            var previous = RenderTexture.active;
            RenderTexture.active = tmp;
            
            var myTexture2D = new Texture2D(sourceTexture.width, sourceTexture.height);
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);
            
            var centerPixel = myTexture2D.GetPixel(myTexture2D.width / 2, myTexture2D.height / 2);
            Destroy(myTexture2D);
            
            _rawLightLevel = (0.2126f * centerPixel.r) + (0.7152f * centerPixel.g) + (0.0722f * centerPixel.b);

            return Mathf.RoundToInt(_rawLightLevel);
        }
    }
}
