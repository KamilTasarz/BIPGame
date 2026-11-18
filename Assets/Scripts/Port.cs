using System.Collections.Generic;
using UnityEngine;

public class Port : MonoBehaviour
{
	public ResourceType producedResource;
	public List<ResourceType> acceptedResources = new List<ResourceType>();
	public Transform boatSpawnPoint;
}