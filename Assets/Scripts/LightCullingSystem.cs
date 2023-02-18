using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightCullingSystem : MonoBehaviour
{
    private Transform _playerTransform;
    private List<TorchBehaviour> _torchBehaviours;
    
    private bool _isInit;
    
    private const int MaxActiveLights = 8;

    private void Start()
    {
        _playerTransform = GameObject.FindWithTag("Player").transform;
        _torchBehaviours = new List<TorchBehaviour>();
        
        var lightObjects = FindObjectsOfType<TorchBehaviour>() ;
        foreach (var lightObject in lightObjects)
        {
            if (!lightObject.gameObject.activeInHierarchy || lightObject.LightObject == null)
                continue;
            
            _torchBehaviours.Add(lightObject);
        }

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
            _torchBehaviours = _torchBehaviours.OrderBy(CountDistanceMagnitude).ToList();

            yield return new WaitForEndOfFrame();
        
            var activeLights = 0;
            foreach (var torchBehaviour in _torchBehaviours)
            {
                var isImportant = (torchBehaviour.IsLit && activeLights <= MaxActiveLights);
                if (isImportant)
                    activeLights += 1;
                
                torchBehaviour.LightObject.renderMode = isImportant
                    ? LightRenderMode.ForcePixel 
                    : LightRenderMode.ForceVertex;
                
                yield return new WaitForEndOfFrame();
            }
            
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    private float CountDistanceMagnitude(Component torchBehaviour)
    {
        var offset = _playerTransform.position - torchBehaviour.transform.position;

        return offset.sqrMagnitude;
    }
}
