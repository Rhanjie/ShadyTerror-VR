using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightCullingSystem : MonoBehaviour
{
    private Transform _cameraVR;
    private List<Light> _lights;
    
    private bool _isInit;
    
    private const int MaxActiveLights = 8;

    private void Start()
    {
        _cameraVR = GameObject.FindWithTag("MainCamera").transform;
        _lights = FindObjectsOfType<Light>(true).ToList();

        StartCoroutine(LazySortRoutine());
        _isInit = true;
    }

    private void OnEnable()
    {
        if (!_isInit)
            return;
        
        StartCoroutine(LazySortRoutine());
    }

    private IEnumerator LazySortRoutine()
    {
        while (gameObject.activeInHierarchy)
        {
            _lights = _lights.OrderBy(CountDistanceMagnitude).ToList();

            yield return new WaitForEndOfFrame();
        
            var activeLights = 0;
            foreach (var lightObject in _lights)
            {
                //TODO: Ignore far lights behind player
                
                var isImportant = (lightObject.isActiveAndEnabled && activeLights <= MaxActiveLights);
                if (isImportant)
                    activeLights += 1;
                
                lightObject.renderMode = isImportant
                    ? LightRenderMode.ForcePixel 
                    : LightRenderMode.ForceVertex;

                //lightObject.enabled = isImportant;
                yield return new WaitForEndOfFrame();
            }
            
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    private float CountDistanceMagnitude(Component torchBehaviour)
    {
        var offset = _cameraVR.position - torchBehaviour.transform.position;

        return offset.sqrMagnitude;
    }
}
