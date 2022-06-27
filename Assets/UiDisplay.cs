using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiDisplay : MonoBehaviour
{
    public Slider volumeSlider;
    public TextMeshProUGUI hpText;
    private void Start()
    {
        var audioVolume = PlayerPrefs.GetFloat("GlobalVolume", 0.5f);
        AudioListener.volume = audioVolume;

        volumeSlider.SetValueWithoutNotify(audioVolume);
        volumeSlider.onValueChanged.AddListener (UpdateVolume);
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetFloat("GlobalVolume", AudioListener.volume);
    }

    private static void UpdateVolume(float audioVolume)
    {
        AudioListener.volume = audioVolume;
    }

    public void SetHp(int currentHp, int maxHp)
    {
        hpText.text = $"HP: {currentHp}/{maxHp}";
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
