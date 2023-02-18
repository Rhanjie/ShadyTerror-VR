using System.Collections;
using System.Collections.Generic;
using Characters;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameLogic : MonoBehaviour
{
    //Simple and temporary class to serve game final
    
    [Header("References")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private MeshRenderer lampRenderer;
    [SerializeField] private Light lampLight;
    [SerializeField] private Transform neckTransform;
    [SerializeField] private PlayerBehaviour playerBehaviour;
    [SerializeField] private Transform headVRTransform;
    [SerializeField] private TextMeshProUGUI canvasText1;
    [SerializeField] private TextMeshProUGUI canvasText2;

    [Header("Values")]
    [SerializeField] public Material redMaterial;
    [SerializeField] public Color32 color;

    public void Trigger()
    {
        StartCoroutine(TriggerRoutine());
    }

    private IEnumerator TriggerRoutine()
    {
        //yield return new WaitForSeconds(0.5f);
        
        lampRenderer.materials[1] = redMaterial;
        lampLight.color = color;

        var direction = (headVRTransform.position - neckTransform.position).normalized;
        neckTransform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(15f, -15f, 0f);
        
        audioManager.PlaySound("Scream");
        yield return new WaitForSeconds(2);
        
        audioManager.PlaySound("Laughter");
        playerBehaviour.FadeEyeSightTo(Color.black, 2f);
        yield return new WaitForSeconds(1);
        
        audioManager.PlaySound("Monster");
        yield return new WaitForSeconds(1);
        
        yield return TextFadeIn(canvasText1, "To be continued...", 2f);
        yield return TextFadeIn(canvasText2, "Thank you for playing this game!", 1f);
        
        yield return new WaitForSeconds(5);
        
        SceneManager.LoadScene(0);
    }

    private IEnumerator TextFadeIn(TMP_Text textObject, string message, float time)
    {
        textObject.enabled = true;
        textObject.text = message;

        var currentColor = textObject.color = new Color(1f, 1f, 1f, 0f);

        var alphaSpeed = 255 / time;

        var currentTime = 0f;
        while (time - currentTime > 0f)
        {
            var deltaTime = Time.deltaTime;
            currentTime += deltaTime;

            currentColor.a += (alphaSpeed * deltaTime);
                
            textObject.color = currentColor;

            yield return null;
        }
    }
}
