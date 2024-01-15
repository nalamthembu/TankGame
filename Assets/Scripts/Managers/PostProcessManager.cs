using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessManager : MonoBehaviour
{
    [SerializeField] PostProcessVolume m_CameraShakeFX;

    public static PostProcessManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (m_CameraShakeFX == null)
            Debug.LogError("There is no camera shake fx volume assigned to the post fx manager!");

    }

    private void OnDestroy()
    {
        Instance = null;
        Debug.Log("Destroyed Post FX Manger Instance!");
    }

    public void TriggerCameraShakeFX(float duration, float intensity) => StartCoroutine(TriggerCamShakeFX(duration, intensity));

    private IEnumerator TriggerCamShakeFX(float duration, float intensity)
    {
        if (m_CameraShakeFX != null)
        {
            float timer = 0;

            float halfDuration = duration / 2;

            float intensityClamped = Mathf.Clamp01(intensity);

            //Fade in CAMERA SHAKE FX IN 
            do
            {
                //fade in 
                m_CameraShakeFX.weight = Mathf.Lerp(0, intensityClamped, timer / halfDuration - 0.1F);

                timer += Time.deltaTime;

                if (timer + 0.01F >= halfDuration)
                    timer = halfDuration;

                yield return new WaitForEndOfFrame();

            } while (timer < halfDuration);

            //Fade out camera fx

            do
            {
                //fade OUT 
                m_CameraShakeFX.weight = Mathf.Lerp(intensityClamped, 0, timer / halfDuration - 0.1F); ;

                timer += Time.deltaTime;

                if (timer + 0.01F >= halfDuration)
                    timer = halfDuration;

                yield return new WaitForEndOfFrame();

            } while (timer < halfDuration);

            m_CameraShakeFX.weight = 0;

            yield break;
        }
        else
            Debug.LogError("Camera Shake FX not assigned in Post Process Manager");
    }
}