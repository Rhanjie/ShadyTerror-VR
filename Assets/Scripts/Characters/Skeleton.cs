using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Characters
{
    public class Skeleton : Enemy
    {
        private RenderTexture _sourceTexture;

        public Camera lightCalculatorCamera;
        public float minLightLevelToBlock = 10f;
        public float minLightLevelToBack = 15f;
        public float minLightLevelToDamage = 30f;
        
        public float lightIntensityLevel;
        public Vector3 darkestDirection;

        private Coroutine _getHitCoroutine;
        private readonly List<(Vector3 direction, float intensity)> _previousDarkestDirections = new();
        private readonly List<(Vector3 direction, float intensity)> _directionsAround = new()
        {
            (Vector3.forward + Vector3.left, 255),
            (Vector3.forward + Vector3.right, 255),
            (Vector3.back + Vector3.left, 255),
            (Vector3.back + Vector3.right, 255),

            (Vector3.forward, 255),
            (Vector3.back, 255),
            (Vector3.left, 255),
            (Vector3.right, 255),
        };

        protected override void Start()
        {
            base.Start();

            var colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
            var depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
            
            _sourceTexture = new RenderTexture(32, 32, colorFormat, depthStencilFormat);
            lightCalculatorCamera.targetTexture = _sourceTexture;

            SetRandomDataForPreviousDarkestDirections();
        }

        public override void UpdateCustomBehaviour()
        {
            ServeGravity();
            CalculateLightIntensity();
            
            if (foundPlayer || TryToFindTarget())
                UpdateAttackRoutine();

            if (!foundPlayer && _waypoints.Count != 0)
            {
                targetToReach = _waypoints[_currentWaypointIndex];
            }

            var foundLightBug = CheckIfFoundLightBug();
            if (lightIntensityLevel >= minLightLevelToDamage)
            {
                _getHitCoroutine ??= StartCoroutine(HandleLightDamage());
            }

            else if (lightIntensityLevel >= minLightLevelToBack)
            {
                if (!foundLightBug) 
                    HandleBackingAwayFromLight();
            }

            else if (lightIntensityLevel >= minLightLevelToBlock && CheckIfPlayerHasMoreLight())
            {
                StopRunOperation();
            }

            else if (_attackCoroutineObject == null && targetToReach != Vector2.zero)
            {
                //if (!foundLightBug)
                UpdateWalkRoutine();
            }
        }

        private void StopRunOperation()
        {
            //TODO: Just stay and wait for opportunity
            //TODO: Run scream and special animation

            _currentSpeed = 0f;
            
            animator.SetFloat(VelocityHash, _currentSpeed);
        }

        private void HandleBackingAwayFromLight()
        {
            GoToDirection(darkestDirection);
        }

        private IEnumerator HandleLightDamage()
        {
            AddImpact(darkestDirection, 50f);
            
            animator.SetFloat(VelocityHash, 0);
            
            Hit("Body");

            yield return new WaitForSeconds(1f);
            _getHitCoroutine = null;
        }

        private bool CheckIfPlayerHasMoreLight()
        {
            var direction = GetDirectionToTarget();
            var intensityInDirection = GetDirectionPairFrom(direction).Item2;

            return lightIntensityLevel < intensityInDirection;
        }
        
        private (Vector3, float) GetDirectionPairFrom(Vector2 direction2D)
        {
            var normalizedX = (int)Math.Round(direction2D.x);
            var normalizedZ = (int)Math.Round(direction2D.y);

            foreach (var tuple in _directionsAround)
            {
                var directionX = (int)tuple.direction.x;
                var directionZ = (int)tuple.direction.z;
                
                if (directionX == normalizedX && directionZ == normalizedZ)
                {
                    return tuple;
                }
            }
            
            return _directionsAround[0];
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
            
            CalculateLightIntensityAround(texture2D);
            
            Destroy(texture2D);
        }

        private void CalculateLightIntensityAround(Texture2D texture2D)
        {
            const int offset = 4;
            var width = texture2D.width;
            var height = texture2D.height;
            
            //(FORWARD + LEFT) | (FORWARD + RIGHT) | (BACK + LEFT) | (BACK + RIGHT)
            var pixelCoordinatesToCheck = new List<Vector2>
            {
                new(offset, offset),
                new(width - offset - 1, offset),
                new(offset, height - offset - 1),
                new(width - offset - 1, height - offset - 1),
            };
            
            //Main light level
            lightIntensityLevel = GetIntensityFromPixel(texture2D, texture2D.width / 2, texture2D.height / 2);
            
            for(var i = 0; i < pixelCoordinatesToCheck.Count; i++)
            {
                var pixel = pixelCoordinatesToCheck[i];
                var intensity = GetIntensityFromPixel(texture2D, (int)pixel.x, (int)pixel.y);
                
                SetLightIntensity(i, intensity);
            }

            //  ((FORWARD + LEFT)  , (FORWARD + RIGHT))  -> FORWARD
            SetLightIntensity(4, GetAverageIntensityFrom(0, 1)); 
            //  ((FORWARD + LEFT)  , (BACKWARD + LEFT))  -> LEFT
            SetLightIntensity(5, GetAverageIntensityFrom(0, 2));
            //  ((FORWARD + RIGHT) , (BACKWARD + RIGHT)) -> RIGHT
            SetLightIntensity(6, GetAverageIntensityFrom(1, 3));
            //  ((BACKWARD + LEFT) , (BACKWARD + RIGHT)) -> BACKWARD
            SetLightIntensity(7, GetAverageIntensityFrom(2, 3));
            
            darkestDirection = GetDarkestDirection();
        }
        
        private static float GetIntensityFromPixel(Texture2D texture2D, int x, int y)
        {
            Color32 pixel = texture2D.GetPixel(x, y);
            
            return (0.2126f * pixel.r) + (0.7152f * pixel.g) + (0.0722f * pixel.b);
        }

        private void SetLightIntensity(int index, float intensity)
        {
            var tuple = _directionsAround[index];
            tuple.intensity = intensity;

            _directionsAround[index] = tuple;
        }

        private float GetAverageIntensityFrom(int index1, int index2)
        {
            return (_directionsAround[index1].intensity + _directionsAround[index2].intensity) / 2f;
        }

        private void SetRandomDataForPreviousDarkestDirections()
        {
            for (var i = 0; i < 5; i++)
                _previousDarkestDirections.Add((Vector3.zero, i));
        }

        private Vector3 GetDarkestDirection()
        {
            var min = _directionsAround.OrderBy(it => it.intensity).First();

            _previousDarkestDirections.Remove(_previousDarkestDirections.First());
            _previousDarkestDirections.Add(min);
            
            return min.direction;
        }

        private bool CheckIfFoundLightBug()
        {
            var lastIndex = _previousDarkestDirections.Count - 1;
            var foundBug = 
                _previousDarkestDirections[lastIndex] == _previousDarkestDirections[lastIndex - 2] &&
                _previousDarkestDirections[lastIndex - 1] == _previousDarkestDirections[lastIndex - 3];

            return foundBug;
        }
    }
}
