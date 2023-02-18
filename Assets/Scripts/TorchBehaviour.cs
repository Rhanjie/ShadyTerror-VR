
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchBehaviour : MonoBehaviour
{
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private Transform bulbEmpty;
    [SerializeField] private MeshRenderer bulbRenderer;
    
    [SerializeField] private Material lightMaterial;
    [SerializeField] private Material unlightMaterial;
    
    [SerializeField] private bool enabledOnStart;
    [SerializeField] private bool ignoreWickBurning;

    public Light LightObject { get; private set; }
    public UniversalAdditionalLightData LightData { get; private set; }
    
    private float _remainingValue = 100;
    private float _totalVerticalBulbScale;

    public bool IsLit { get; private set; } = true;

    private void Start()
    {
        _totalVerticalBulbScale = bulbEmpty.localScale.y;
        
        LightObject = bulbRenderer.GetComponentInChildren<Light>();
        LightData = bulbRenderer.GetComponentInChildren<UniversalAdditionalLightData>();

        var bulbDetector = bulbRenderer.GetComponent<BulbDetector>();
        if (bulbDetector != null)
            bulbDetector.Init(this);
        
        ToggleLight(enabledOnStart);
    }
    
    private void Update()
    {
        if (!IsLit)
            return;

        ResetParticlesRotation();
        
        if (!ignoreWickBurning)
            BurningOut();
    }

    private void ResetParticlesRotation()
    {
        fireParticles.transform.localRotation = Quaternion.Inverse(transform.localRotation) * Quaternion.Euler(-90, 0f, 0f);
    }

    private void BurningOut()
    {
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
        
        if (LightObject != null)
            LightObject.enabled = IsLit;
        
        bulbRenderer.material = IsLit
            ? lightMaterial
            : unlightMaterial;
        
        if (IsLit)
            fireParticles.Play();
        else fireParticles.Stop();
    }
}
