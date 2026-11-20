using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class BoatBehaviour : MonoBehaviour
{
	[Header("Boat Properties")]
	public float boatSpeed = 1.5f;
	public float radius = 5.0f;
	public float rotSpeed = 10.0f;

    [Header("Destroy properties")]
    public GameObject ps;
    public float timeToRotate = 0.5f;
    public float timeToSink = 1.5f;
    [Header("Deliver properties")]
    public GameObject ps1;

    [HideInInspector] public float healthPoints = 1.1f;

	private bool isInputActive = false;
	private bool isLaunched = false;
	private Vector3 lastPoint = Vector3.zero;
	private float sign = 1.0f;

	[Header("Resource Cargo")]
	public List<ResourceType> carriedResources = new List<ResourceType>();
	public Port originPort;

	[Header("Cargo UI Icons")]
	public Image[] cargoIcons;

	private bool isDead = false;
	private bool isAnimation = false;
	private Rigidbody rb;

	public void DisableRigidbody() { rb.isKinematic = false; }
	public void EnableRigidbody() { rb.isKinematic = true; }

	private void OnEnable()
	{
		healthPoints = healthPoints * UpgradesManager.Instance.ProvideCurrentModifierValue(UpgradesManager.Instance.modifiers[2]);
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
		StartAnimation();
		
	}

    public void SetCargo(List<ResourceType> resources)
	{
		carriedResources = resources;
		UpdateCargoUI();
	}

	private void UpdateCargoUI()
	{
		if (cargoIcons == null || GameManager.Instance == null) return;

		for (int i = 0; i < cargoIcons.Length; i++)
		{
			if (cargoIcons[i] == null) continue;

			if (i < carriedResources.Count)
			{
				cargoIcons[i].sprite = GameManager.Instance.GetResourceIcon(carriedResources[i]);
				cargoIcons[i].gameObject.SetActive(cargoIcons[i].sprite != null);
			} else
			{
				cargoIcons[i].gameObject.SetActive(false);
			}
		}
	}

	private void FixedUpdate()
	{
		if (isDead) return;
        if (isAnimation) return;


        Vector3 flatBoatPos = new Vector3(transform.position.x, 0f, transform.position.z);
		Vector3 flatPointPos = new Vector3(lastPoint.x, 0f, lastPoint.z);
		float dist = Vector3.Distance(flatPointPos, flatBoatPos);

		if (!isLaunched)
		{
			if (isInputActive && dist < radius && dist > 0.2f)
			{
				isLaunched = true;
				TurnCanvases(true);
				if (originPort != null)
				{
					originPort.OnBoatDeparted();
				}
			} else
			{
				rb.linearVelocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
				return;
			}
		}

		rb.linearVelocity = boatSpeed * transform.forward;

		if (!isInputActive)
		{
			rb.angularVelocity = Vector3.zero;
			return;
		}

		Vector3 toPoint = (flatPointPos - flatBoatPos).normalized;

		if (dist < radius && dist > 0.2f)
		{
			float crossY = Vector3.Cross(transform.forward, toPoint).y;
			float sideSign = Mathf.Sign(crossY);
			float repelSmooth = (sign < 0f) ? Mathf.Clamp01(Mathf.Abs(crossY) * 4f + Mathf.Max(0f, Vector3.Dot(transform.forward, toPoint))) : 1f;

			float baseOmega = boatSpeed / radius;

            float multi = UpgradesManager.Instance.ProvideCurrentModifierValue(UpgradesManager.Instance.modifiers[1]);

            transform.Rotate(0, baseOmega * sideSign * rotSpeed * sign * repelSmooth * multi, 0);
		} else
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
            isDead = true;
			DeathAnimation();
        }
		if (collision.gameObject.CompareTag("pirate"))
		{
			isAnimation = true;
			FightAnimation(collision.transform.position);
			
		}

        if (collision.gameObject.CompareTag("rock"))
        {
            healthPoints -= 0.5f;
            rb.AddForce(5.0f * boatSpeed * transform.forward);
            if (healthPoints <= 0.0f)
            {
                isDead = true;
                DeathAnimation();
            }
        }
    }

	private void OnTriggerEnter(Collider other)
	{
		Port hitPort = other.GetComponent<Port>();
		if (hitPort != null)
		{
			if (hitPort == originPort)
			{
				return;
			}

			int baseReward = hitPort.CalculateCargoValue(carriedResources);
			int finalReward = GameManager.Instance != null ? GameManager.Instance.AddWaveEarnings(baseReward) : baseReward;

			hitPort.RollNewDemand();

			Debug.Log($"Sprzedano [{string.Join(", ", carriedResources)}] w porcie {hitPort.gameObject.name}. Zarobiono {finalReward}$ (baza: {baseReward}$).");
			DeliverAnimation(other.transform.position);
		}
	}

    private void DeathAnimation()
    {

        TurnCanvases(false);
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

	private void FightAnimation(Vector3 pos)
	{
		rb.isKinematic = true;

		TurnCanvases(false);

        transform.DOMove(transform.position, 1.5f).OnComplete(() =>
		{
			rb.isKinematic = false;
            healthPoints -= 1.0f;
			isAnimation = false;
			TurnCanvases(true);
            if (healthPoints <= 0.0f)
            {
                isDead = true;
                DeathAnimation();
            } else
			{
				Vector3 dir = (transform.position - pos).normalized;
				transform.forward = dir;
				transform.position += transform.forward * 0.02f;
            }
        });
	}

	private void StartAnimation()
	{
		isAnimation = true;
		TurnCanvases(false);
		rb.isKinematic = true;
		transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		//transform.position.Set(transform.position.x, 0.2f, transform.position.z);
		transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
		{
			isAnimation = false;
            rb.isKinematic = false;

        });
	}
	private void DeliverAnimation(Vector3 portPos)
	{
		
		isAnimation = true;

		//GameObject g = Instantiate(ps1, transform);
		//Vector3 dir = portPos - transform.position;
		//dir.Normalize();
		//g.transform.forward = dir;

		//transform.DOMove(transform.position, 1.0f).OnComplete(() =>
		//{
			//transform.localScale = Vector3.zero;
			transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
			{
				isAnimation = false;
				Destroy(gameObject);
			});
		//});
	}

	private void TurnCanvases(bool on = true)
	{
        for (int i = 0; i < cargoIcons.Length; i++)
        {
            if (cargoIcons[i] == null) continue;
            cargoIcons[i].gameObject.SetActive(true);

			cargoIcons[i].DOFade((on ? 1.0f : 0.0f), 0.5f);
			cargoIcons[i].transform.DOScale((on ? 0.08f : 0.0f), 0.5f);
        }
    }
}