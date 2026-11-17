using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float force;
    public float radius = 5.0f;
    public float rotSpeed = 10.0f;
    public GameObject visualWave;

    public LayerMask floorMask;

    private Vector3 constantForce = Vector3.zero;

    private InputAction pushAction;
    private InputAction pullAction;

    private Camera mainCam;
    private Rigidbody rb;

    private Transform waveObj;

    void Start()
    {
        pushAction = GetComponent<PlayerInput>().actions["pushAction"];
        pullAction = GetComponent<PlayerInput>().actions["pullAction"];
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody>();
    }



    void Update()
    {
        Vector3 finalForce = Vector3.zero;


        rb.AddForce(constantForce, ForceMode.Force);

        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit output;

        if (Physics.Raycast(ray, out output, 100f, floorMask))
        {
            float dist = Vector3.Distance(output.point, transform.position);
            if (dist < radius)
            {
                Vector3 direction = output.point - transform.position;
                direction = new Vector3(direction.x, 0, direction.z);
                direction.Normalize();
                finalForce = direction * force * ((radius - dist) / radius);
            }

        }

        if (pushAction.IsPressed())
        {
            if (waveObj)
            {
                waveObj.position = output.point;
            } else
            {
                waveObj = Instantiate(visualWave, output.point, Quaternion.identity).transform;
            }
            constantForce = finalForce.normalized;
            rb.AddForce(finalForce, ForceMode.Force);
        }
        if (pullAction.IsPressed())
        {
            if (waveObj)
            {
                waveObj.position = output.point;
            }
            else
            {
                waveObj = Instantiate(visualWave, output.point, Quaternion.identity).transform;
            }
            constantForce = finalForce.normalized;
            rb.AddForce(-finalForce, ForceMode.Force);
        }

        if (rb.linearVelocity.magnitude > 0.1f)
        {
            Vector3 flatOurDir = new Vector3(transform.forward.x, 0, transform.forward.z);
            Vector3 flatDestDir = new Vector3(rb.linearVelocity.normalized.x, 0, rb.linearVelocity.normalized.z);
            transform.forward = Vector3.Slerp(flatOurDir, flatDestDir, rotSpeed * Time.deltaTime);
            //Quaternion.RotateTowards();
            //rb.AddTorque(0, Vector3.Dot(transform.forward, rb.linearVelocity.normalized), 0);
        }

        if (!pullAction.IsPressed() && !pushAction.IsPressed())
        {
            if (waveObj) { 
                Destroy(waveObj.gameObject);
                waveObj = null;
            }
        }
    }
}
