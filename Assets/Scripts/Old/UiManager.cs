using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI healthText;
    
    [SerializeField]
    private TextMeshProUGUI ammoText;
    
    [SerializeField]
    private TextMeshProUGUI scoreText;
    
    [SerializeField]
    private Image point;

    private Animator _healthAnimator;
    private Animator _ammoAnimator;
    private Animator _scoreAnimator;
    
    private static readonly int ChangeTrigger = Animator.StringToHash("Change");

    private void Awake()
    {
        _healthAnimator = healthText.GetComponent<Animator>();
        _ammoAnimator = ammoText.GetComponent<Animator>();
        _scoreAnimator = scoreText.GetComponent<Animator>();
    }

    public void ResizePoint(float gunScatter)
    {
        var size = point.rectTransform.sizeDelta;
        
        size.x = 10 + (gunScatter * 1000);
        size.y = 10 + (gunScatter * 1000);

        point.rectTransform.sizeDelta = size;
    }

    public void SetHealthInfo(int health, int maxHealth)
    {
        healthText.text = $"Health {health}/{maxHealth}";
        
        _healthAnimator.SetTrigger(ChangeTrigger);
    }
    
    public void SetAmmoInfo(int ammo)
    {
        ammoText.text = $"Ammo {ammo}";
        
        _ammoAnimator.SetTrigger(ChangeTrigger);
    }
    
    public void SetScoreInfo(int score)
    {
        scoreText.text = $"Score {score}";
        
        _scoreAnimator.SetTrigger(ChangeTrigger);
    }
}
