
using UnityEngine;

public class TorchBehaviour : MonoBehaviour
{
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private Transform bulbEmpty;
    [SerializeField] private MeshRenderer bulbRenderer;
    [SerializeField] private BulbDetector bulbDetector;
    [SerializeField] private Material lightMaterial;
    [SerializeField] private Material unlightMaterial;
    
    [SerializeField] private bool enabledOnStart;

    private Light _light;

    private float _remainingValue = 100;

    private float _totalVerticalBulbScale;

    public bool IsLit { get; private set; } = true;

    private void Start()
    {
        _light = bulbRenderer.GetComponentInChildren<Light>();
        _totalVerticalBulbScale = bulbEmpty.localScale.y;

        IsLit = !enabledOnStart;
        if (enabledOnStart)
            Light();
        else Unlight();
        
        bulbDetector.Init(this);
    }
    
    private void Update()
    {
        if (!IsLit)
            return;
        
        fireParticles.transform.localRotation = Quaternion.Inverse(transform.localRotation) * Quaternion.Euler(-90, 0f, 0f);

        _remainingValue -= Time.deltaTime;
        if (_remainingValue <= 0f)
        {
            _remainingValue = 0f;
            bulbEmpty.gameObject.SetActive(false);
            
            Unlight();
        }
        
        var newScale = bulbEmpty.localScale;
        newScale.y = _totalVerticalBulbScale * (_remainingValue / 100f);
        bulbEmpty.localScale = newScale;
    }

    public void Light()
    {
        if (IsLit || _remainingValue <= 0)
            return;
        
        ToggleLight(true);
    }
    
    public void Unlight()
    {
        if (!IsLit)
            return;

        ToggleLight(false);
    }

    private void ToggleLight(bool isActive)
    {
        IsLit = isActive;
        
        _light.enabled = IsLit;
        bulbRenderer.material = IsLit
            ? lightMaterial
            : unlightMaterial;
        
        if (IsLit)
            fireParticles.Play();
        else fireParticles.Stop();
    }
}
