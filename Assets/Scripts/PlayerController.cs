using System;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private void Start()
    {
        PlayerInput pi = GetComponent<PlayerInput>();
        pushAction = pi.actions["pushAction"];
        pullAction = pi.actions["pullAction"];
        mainCam = Camera.main;
    }

    private void Update()
    {
        bool isPushing = pushAction.IsPressed();
        bool isPulling = pullAction.IsPressed();

        if (isPushing || isPulling)
        {
            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

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
                waveObj = null;
            }

            OnReleasedMouseButtons?.Invoke();
        }
    }
}