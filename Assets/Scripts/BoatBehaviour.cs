using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatBehaviour : MonoBehaviour
{
    public float boatSpeed = 1.5f;
    public float radius = 5.0f;
    public float rotSpeed = 10.0f;

    private bool isInputActive = false;
    private Vector3 lastPoint = Vector3.zero;
    private float sign = 1.0f;

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
        rb.linearVelocity = boatSpeed * transform.forward;

        if (!isInputActive)
        {
            rb.angularVelocity = Vector3.zero;
            return;
        }

        Vector3 flatBoatPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flatPointPos = new Vector3(lastPoint.x, 0f, lastPoint.z);

        float dist = Vector3.Distance(flatPointPos, flatBoatPos);

        Vector3 toPoint = (flatPointPos - flatBoatPos).normalized;

        if (dist < radius && dist > 0.2f)
        {

            float crossY = Vector3.Cross(transform.forward, toPoint).y;
            float sideSign = Mathf.Sign(crossY);

            float repelSmooth = (sign < 0f) ? Mathf.Clamp01(Mathf.Abs(crossY) * 4f + Mathf.Max(0f, Vector3.Dot(transform.forward, toPoint))) : 1f;

            float baseOmega = rb.linearVelocity.magnitude / dist;

            baseOmega = (dist < 1.0f) ? baseOmega * (1.0f + dist / radius) : baseOmega;

            rb.angularVelocity = new Vector3(0f, sideSign * rotSpeed * sign * repelSmooth * baseOmega, 0f);
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ChangeMovement(Vector3 point, float sign)
    {
        lastPoint = point;
        this.sign = sign;
        isInputActive = true;
    }

    public void ConstMovement()
    {
        isInputActive = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("land"))
        {
            // Destroy(gameObject);
        }
    }
}