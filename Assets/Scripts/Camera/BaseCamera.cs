﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public class BaseCamera
{
    [Header("----------Handheld Effect----------")]
    [SerializeField] protected bool m_HandHeldEffectEnabled;
    [SerializeField][Min(0.01F)] float m_HandheldRange;
    [SerializeField][Min(0.01F)] protected float m_HandHeldSmoothing;
    Vector3 m_HandHeldVelocity;

    [Header("----------Debug----------")]
    [SerializeField] bool m_DebugHandHeldEffect;
    [SerializeField] float m_DebugHandheldPercentage;
    [SerializeField] bool m_DebugDisableCameraShake;
    public bool DebugDebugDisableCameraShake { get { return m_DebugDisableCameraShake; } }
    [Header("---------------------------")]


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
    public virtual void Start() { }
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

    protected void DoHandHeldEffect(float percent, float smoothing = 5)
    {
        if (m_DebugHandHeldEffect)
        {
            percent = m_DebugHandheldPercentage;
        }

        Vector3 handHeldPosition = new()
        {
            x = Mathf.Sin(Time.time * m_HandHeldSmoothing) * Random.Range(-m_HandheldRange, m_HandheldRange),
            y = Mathf.Cos(Time.time * m_HandHeldSmoothing) * Random.Range(-m_HandheldRange, m_HandheldRange),
        };

        handHeldPosition *= Mathf.Lerp(0, 1, Mathf.Clamp01(percent));

        m_AttachedCamera.transform.localPosition = Vector3.SmoothDamp(m_AttachedCamera.transform.localPosition, handHeldPosition, ref m_HandHeldVelocity, smoothing);
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
    public IEnumerator DoCameraShake(float duration, float magnitude)
    {
        float timer = 0;

        Vector3 currentVelocity = Vector3.zero;

        do
        {
            //Calculate a random position.
            Vector3 randomPos = new()
            {
                x = Random.Range(-1, 1) * magnitude,
                y = 0,
                z = Random.Range(-1, 1) * magnitude
            };

            //Shake the camera
            m_AttachedCamera.transform.localPosition =
                Vector3.SmoothDamp(
                    m_AttachedCamera.transform.localPosition,
                    randomPos,
                    ref currentVelocity,
                    Time.deltaTime);


            timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();

        } while (timer < duration);
    }
}