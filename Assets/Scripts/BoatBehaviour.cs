using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatBehaviour : MonoBehaviour
{
    public float force = 15.0f;
    public float minimalForce = 2.0f;
    public float radius = 5.0f;
    public float rotSpeed = 10.0f;

    private bool isInputActive = false;
    private bool outOfRadius = false;
    private Vector3 lastPoint = Vector3.zero;
    private float sign = 1.0f;

    private Vector3 constantForce = Vector3.zero;
    private Rigidbody rb;

    private void OnEnable()
    {
        PlayerController.OnPressedMouseButtons += ChangeMovement;
        PlayerController.OnReleasedMouseButtons += ConstMovement;
    }

    private void OnDisable()
    {
        PlayerController.OnPressedMouseButtons -= ChangeMovement;
        PlayerController.OnReleasedMouseButtons -= ConstMovement;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 finalForce = Vector3.zero;

        Vector3 flatBoatPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flatPointPos = new Vector3(lastPoint.x, 0f, lastPoint.z);

        float dist = Vector3.Distance(flatPointPos, flatBoatPos);
        Vector3 direction = (flatPointPos - flatBoatPos).normalized;

        if (isInputActive)
        {
            if (dist < radius)
            {
                outOfRadius = false;
                finalForce = direction * force * ((radius - dist) / radius);
                rb.AddForce(sign * finalForce, ForceMode.Force);
            }
            else
            {
                outOfRadius = true;
            }
        }
        else if (constantForce != Vector3.zero)
        {
            rb.AddForce(constantForce, ForceMode.Force);
        }

        if (rb.linearVelocity.magnitude > 0.1f)
        {
            Vector3 flatOurDir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 flatDestDir = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).normalized;

            if (flatDestDir != Vector3.zero)
            {
                transform.forward = Vector3.Slerp(flatOurDir, flatDestDir, rotSpeed * Time.fixedDeltaTime);
            }
        }
    }

    public void ChangeMovement(Vector3 point, float sign)
    {
        lastPoint = point;
        this.sign = sign;
        isInputActive = true;
        constantForce = Vector3.zero;
    }

    public void ConstMovement()
    {
        isInputActive = false;

        if (outOfRadius)
        {
            Vector3 flatBoatPos = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 flatPointPos = new Vector3(lastPoint.x, 0f, lastPoint.z);
            Vector3 direction = (flatPointPos - flatBoatPos).normalized;

            constantForce = direction * minimalForce * sign;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("land"))
        {
            constantForce = Vector3.zero;
        }
    }
}