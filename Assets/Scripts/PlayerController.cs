using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class PlayerController : MonoBehaviour
{
    public GameObject visualWave;
    public LayerMask floorMask;
    public float rayDistance = 500f;

    public static event Action<Vector3, float> OnPressedMouseButtons;
    public static event Action OnReleasedMouseButtons;

    private InputAction pushAction;
    private InputAction pullAction;
    private Camera mainCam;
    private Transform waveObj;


    [Header("Line")]
    public int circleSegments = 20;
    public float radius = 1.0f;

    private float currentT = 0.0f;

    private LineRenderer line;
    private void Start()
    {
        PlayerInput pi = GetComponent<PlayerInput>();
        pushAction = pi.actions["pushAction"];
        pullAction = pi.actions["pullAction"];
        mainCam = Camera.main;
        line = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        bool isPushing = pushAction.IsPressed();
        bool isPulling = pullAction.IsPressed();

        if (isPushing || isPulling)
        {
            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (waveObj) DrawCircle();

            if (Physics.Raycast(ray, out RaycastHit output, rayDistance, floorMask))
            {
                if (waveObj != null)
                {
                    waveObj.position = output.point;
                }
                else if (visualWave != null)
                {
                    waveObj = Instantiate(visualWave, output.point, Quaternion.identity).transform;
                }

                float sign = isPulling ? -1.0f : 1.0f;
                OnPressedMouseButtons?.Invoke(output.point, sign);
            }
        }
        else if (pushAction.WasReleasedThisFrame() || pullAction.WasReleasedThisFrame())
        {
            if (waveObj != null)
            {
                Destroy(waveObj.gameObject);
                line.enabled = false;
                waveObj = null;
            }

            OnReleasedMouseButtons?.Invoke();
        }
    }

    private void DrawCircle(bool outside = true)
    {
        line.enabled = true;
        line.positionCount = circleSegments + 1;
        float angleStep = 2f * Mathf.PI / circleSegments;

        for (int i = 0; i < circleSegments; i++)
        {
            float angle = i * angleStep;
            float x = waveObj.position.x + Mathf.Cos(angle) * radius;
            float y = waveObj.position.z + Mathf.Sin(angle) * radius;

            line.SetPosition(i, new Vector3(x, 0.1f, y));

        }

        line.SetPosition(circleSegments, new Vector3(waveObj.position.x + radius, 0.1f, waveObj.position.z));

    }

    
}