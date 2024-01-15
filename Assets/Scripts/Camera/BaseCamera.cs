﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public class BaseCamera
{
    [SerializeField][Range(0.1F, 5)] protected float m_FOVSmoothTime = 0.25F;

    public bool m_IsActive;

    protected float m_FOV;

    protected float m_FOVSmoothVel;

    [HideInInspector] public Transform transform;
    [HideInInspector] public GameObject gameObject;

    protected float m_Pitch;

    protected float m_Yaw;

    protected Camera m_AttachedCamera;

    public virtual void Awake() { }
    public virtual void OnDestroy() { }

    /// <summary>
    /// This method is called in Start() on the Game Camera
    /// </summary>
    /// <param name="selfTransform"></param>
    /// <param name="initialFOV"></param>
    public virtual void Initialise(Transform selfTransform, float initialFOV)
    {
        transform = selfTransform;
        gameObject = transform.gameObject;

        m_FOV = initialFOV;

        if (m_AttachedCamera is null)
        {
            m_AttachedCamera = transform.GetComponentInChildren<Camera>();

            if (m_AttachedCamera is null) //If its still null
            {
                Debug.LogError("There is no camera attached to the child transform!");
            }
        }
    }

    protected virtual void DoUpdateFOV()
    {
        m_AttachedCamera.fieldOfView = Mathf.SmoothDamp(m_AttachedCamera.fieldOfView, m_FOV, ref m_FOVSmoothVel, m_FOVSmoothTime);
    }

    public virtual void Update()
    {
        DoUpdateFOV();
    }

    public virtual void LateUpdate()
    {
        DoUpdateRotation();
        DoUpdatePosition();
        DoUpdateSpeed();
    }

    protected virtual void DoUpdatePosition() { }

    protected virtual void DoUpdateRotation() { }

    protected virtual void DoUpdateSpeed() { }

    /// <summary>
    /// This Camera Shake method must be called with StartCoroutine(DoCameraShake(...))
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="maximumMovement"></param>
    /// <param name="intensity"></param>
    /// <returns></returns>
    public IEnumerator DoCameraShake(float duration, float maximumMovement, float intensity)
    {
        float timer = 0;

        Vector3 previousPosition = m_AttachedCamera.transform.localPosition;

        do
        {
            //Calculate a random position.
            Vector3 randomPosition = Vector3.right * Random.Range(-maximumMovement, maximumMovement) +
                    Vector3.up * Random.Range(-maximumMovement, maximumMovement);

            //Shake the camera
            m_AttachedCamera.transform.localPosition =
                Vector3.Lerp
                (
                    m_AttachedCamera.transform.localPosition,
                    randomPosition,
                    Time.deltaTime * intensity
                );

            timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();

        } while (timer < duration);

        do
        {
            m_AttachedCamera.transform.localPosition =
                Vector3.Lerp
                (
                    m_AttachedCamera.transform.localPosition,
                    previousPosition,
                    Time.deltaTime * intensity
                );

            //If the camera is extremely close to the previous position
            if ((previousPosition - m_AttachedCamera.transform.localPosition).magnitude < 0.001F)
                m_AttachedCamera.transform.localPosition = previousPosition; //snap to that position.

            yield return new WaitForEndOfFrame();

        } while (m_AttachedCamera.transform.localPosition != previousPosition);
    }
}