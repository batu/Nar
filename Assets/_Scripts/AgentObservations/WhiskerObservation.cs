using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WhiskerObservation : MonoBehaviour, IObservation
{
    public bool visualize = false;

    [Header("Surrounding Raycasts")]
    public int numSurroundingRaycasts = 8;
    public int numVerticalRaycasts = 2;
    public float surroundingRaycastDistance = 2f;
    
    [Header("Groundmesh Raycasts")]
    public float groundMeshDistance = 0.5f;
    public float groundMeshSeperation = 1f;

    private float _agentHeight = 1.8f;
    // Start is called before the first frame update

    private NavigationAgent _navigationAgent;
    private bool _shouldRotate = false;
    
    private void Start()
    {
        _navigationAgent = GetComponent<NavigationAgent>();
        _shouldRotate = _navigationAgent.useRotation;
    }


    float[] GetSurroundingRaycasts()
    {
        float turnAngle = 360 / numSurroundingRaycasts;
        float yOffset = _agentHeight / (numVerticalRaycasts + 1);
        
        List<float> surroundingRaycasts = new List<float>();
        for (int surroundingRaycast = 0; surroundingRaycast < numSurroundingRaycasts; surroundingRaycast++)
        {
            for (int verticalRaycast = 1; verticalRaycast <= numVerticalRaycasts; verticalRaycast++)
            {
                Vector3 center = transform.position + new Vector3(0, yOffset * verticalRaycast, 0);
                Quaternion rotationQuat = Quaternion.AngleAxis(turnAngle * surroundingRaycast, Vector3.up);
                Vector3 direction = _shouldRotate ? rotationQuat * transform.forward : rotationQuat * Vector3.forward;
                Ray ray = new Ray(center, direction);
                surroundingRaycasts.Add(FireRaycasts(ray, surroundingRaycastDistance));
            }
        }

        return surroundingRaycasts.ToArray();
    }


    float[] GetGroundRaycasts()
    {
        List<float> groundCasts = new List<float>();

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Quaternion rotationQuat = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up);

                Vector3 offset = new Vector3(x * groundMeshSeperation, 0, z * groundMeshSeperation);
                offset = _shouldRotate ? rotationQuat * offset : offset;

                Vector3 center = offset + transform.position;
                Ray ray = new Ray(center, Vector3.down);
                groundCasts.Add(FireRaycasts(ray, groundMeshDistance));
            }
        }

        return groundCasts.ToArray();
    }

    float[] GetCeilingRaycast()
    {
        Ray ray = new Ray(transform.position + Vector3.up, Vector3.up);
        return new[] {FireRaycasts(ray, _agentHeight + 1f)};
    }

    private float FireRaycasts(Ray ray, float distance)
    {
        bool didHit = Physics.Raycast(ray, out RaycastHit hit, distance);
        if (didHit)
        {
            if (visualize)
                Debug.DrawRay(ray.origin, ray.direction * distance, Color.red);
            return 1f - hit.distance / distance;
        }
        if (visualize)
            Debug.DrawRay(ray.origin, ray.direction * distance, Color.blue);
        return 0f;
    }

    public float[] GetObservation()
    {
        float[] ceilingObservation = GetCeilingRaycast();
        float[] surroundingObservation = GetSurroundingRaycasts();
        float[] groundMeshObservation = GetGroundRaycasts();
        float[] localRaycastObservation = ceilingObservation.Concat(surroundingObservation).Concat(groundMeshObservation).ToArray();
        return localRaycastObservation;
    }
}
