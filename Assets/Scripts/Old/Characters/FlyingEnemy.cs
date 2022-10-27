using System;
using System.Collections;
using UnityEngine;

namespace Characters
{
    public class FlyingEnemy : Enemy
    {
        public GameObject balloon;

        public override void Hit(string colliderName)
        {
            var hitBalloon = colliderName.Contains("balloon", StringComparison.CurrentCultureIgnoreCase);
            if (hitBalloon)
            {
                RemoveBalloon();

                PlaySoundWithRandomPitch(scream, 0.9f, 1.1f);

                return;
            }
        
            base.Hit(colliderName);
        }
        
        public override IEnumerator DieRoutine()
        {
            RemoveBalloon();
            
            yield return base.DieRoutine();
        }

        private void RemoveBalloon()
        {
            rigidbody.useGravity = true;
            randomPositionMethod = null;
            targetToReach = targetToShoot.transform.position;
                
            Destroy(balloon);
        }
    }
}
