using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightCullingSystem : MonoBehaviour
{
    private Transform _cameraVR;
    private List<TorchBehaviour> _torchBehaviours;
    
    private bool _isInit;
    
    private const int MaxActiveLights = 8;

    private void Start()
    {
        _cameraVR = GameObject.FindWithTag("MainCamera").transform;
        _torchBehaviours = new List<TorchBehaviour>();
        
        var foundTorches = FindObjectsOfType<TorchBehaviour>() ;
        foreach (var torch in foundTorches)
        {
            if (!torch.gameObject.activeInHierarchy || torch.LightObject == null)
                continue;
            
            _torchBehaviours.Add(torch);
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
                //TODO: Ignore far lights behind player
                
                var isImportant = (torchBehaviour.IsLit && activeLights <= MaxActiveLights);
                if (isImportant)
                    activeLights += 1;
                
                torchBehaviour.LightObject.renderMode = isImportant
                    ? LightRenderMode.ForcePixel 
                    : LightRenderMode.ForceVertex;

                //torchBehaviour.LightObject.enabled = isImportant;
                
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
