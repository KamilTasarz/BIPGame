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

	[HideInInspector] public int healthPoints = 2;

	private bool isInputActive = false;
	private bool isLaunched = false;
	private Vector3 lastPoint = Vector3.zero;
	private float sign = 1.0f;

	[Header("Resource Cargo")]
	public List<ResourceType> carriedResources = new List<ResourceType>();
	public Port originPort;

	[Header("Cargo UI Icons")]
	public Image[] cargoIcons;

	private Rigidbody rb;

	public void DisableRigidbody() { rb.isKinematic = false; }
	public void EnableRigidbody() { rb.isKinematic = true; }

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
		Vector3 flatBoatPos = new Vector3(transform.position.x, 0f, transform.position.z);
		Vector3 flatPointPos = new Vector3(lastPoint.x, 0f, lastPoint.z);
		float dist = Vector3.Distance(flatPointPos, flatBoatPos);

		if (!isLaunched)
		{
			if (isInputActive && dist < radius && dist > 0.2f)
			{
				isLaunched = true;
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
			transform.Rotate(0, baseOmega * sideSign * rotSpeed * sign * repelSmooth, 0);
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
			Destroy(gameObject);
		}
		if (collision.gameObject.CompareTag("pirate"))
		{
			healthPoints--;
			rb.AddForce(boatSpeed * transform.forward);
			if (healthPoints <= 0)
			{
				Destroy(gameObject);
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

			Debug.Log($"Sprzedano [{string.Join(", ", carriedResources)}] w porcie {hitPort.gameObject.name}. Zarobiono {finalReward}$ (baza: {baseReward}$).");
			Destroy(gameObject);
		}
	}
}