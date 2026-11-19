using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class PirateBehaviour : MonoBehaviour
{
    [Header("Pirate Properties")]
    public float boatSpeed = 1.5f;
    public float radius = 1.0f;
    public float rotSpeed = 10.0f;
    public float howOftenCheckBoats = 5.0f;

    [Header("Destroy properties")]
    public GameObject ps;
    public float timeToRotate = 0.5f;
    public float timeToSink = 1.5f;

    private bool isInputActive = false;
    private bool isLaunched = false;
    private Vector3 lastPoint = Vector3.zero;
    private float sign = 1.0f;

    private bool isControlledByPlayer = false;
    private float timerToCheckBoats = 0.0f;

    private Rigidbody rb;
    public BoatBehaviour targetBoat = null;

    private bool isDead = false;

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        timerToCheckBoats = howOftenCheckBoats;
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        if (timerToCheckBoats <= 0.0f)
        {
            FindClosestBoat();
            timerToCheckBoats = howOftenCheckBoats;
        } else
        {
            timerToCheckBoats -= Time.fixedDeltaTime;
        }


        isControlledByPlayer = false;

        Vector3 flatBoatPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flatPointPos = new Vector3(lastPoint.x, 0f, lastPoint.z);
        float dist = Vector3.Distance(flatPointPos, flatBoatPos);

        rb.linearVelocity = boatSpeed * transform.forward;

        Vector3 toPoint = (flatPointPos - flatBoatPos).normalized;

        if (isInputActive)
        {
            if (dist < radius && dist > 0.2f)
            {
                isControlledByPlayer = true;
                timerToCheckBoats = howOftenCheckBoats;
                targetBoat = null;
            }
        }

        if (isControlledByPlayer)
        {
            float crossY = Vector3.Cross(transform.forward, toPoint).y;
            float sideSign = Mathf.Sign(crossY);
            float repelSmooth = (sign < 0f) ? Mathf.Clamp01(Mathf.Abs(crossY) * 4f + Mathf.Max(0f, Vector3.Dot(transform.forward, toPoint))) : 1f;

            float baseOmega = boatSpeed / radius;
            transform.Rotate(0, baseOmega * sideSign * rotSpeed * sign * repelSmooth, 0);

        } else
        {
            if (targetBoat)
            {
                Vector3 direction = targetBoat.transform.position - transform.position;
                direction.Normalize();
                if (Vector3.Dot(transform.forward, direction) < 0.99f)
                {
                    transform.forward = Vector3.RotateTowards(transform.forward, direction, Mathf.Deg2Rad * 5.0f, rotSpeed * Time.fixedDeltaTime);
                }
            }
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

    public void FindClosestBoat()
    {
        BoatBehaviour closestBoat = null;
        float smallestDist = Mathf.Infinity; // Nieskończoność zamiast sztywnego 1000f

        BoatBehaviour[] boats = GameManager.Instance.boatParent.GetComponentsInChildren<BoatBehaviour>();

        foreach (BoatBehaviour boat in boats)
        {
            float distSqr = (boat.transform.position - transform.position).sqrMagnitude;

            if (distSqr < smallestDist)
            {
                smallestDist = distSqr;
                closestBoat = boat;
            }
        }

        targetBoat = closestBoat;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("land"))
        {
            isDead = true;
            GameObject o = Instantiate(ps, Vector3.zero, Quaternion.identity, transform);
            o.transform.localPosition = Vector3.zero;
            transform.DORotate(new Vector3(-50f, 0f, 0f), timeToRotate).SetRelative(true).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                float y_off = 2.0f;
                float z_off = 2.0f;
                Vector3 sinkDirection = (-Vector3.up * y_off) + (-transform.forward * z_off);
                Vector3 targetPosition = transform.position + sinkDirection;

                transform.DOMove(targetPosition, timeToSink).SetEase(Ease.Linear).OnComplete(() =>
                {
                    Destroy(gameObject);
                });
            });
        }

        if (collision.gameObject.CompareTag("pirate"))
        {
            isDead = true;
            Destroy(gameObject);
        }
    }

    
}
