using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

[RequireComponent(typeof(NavigationAgent))]
public class DepthMaskObservation : MonoBehaviour, IObservation
{
    // Start is called before the first frame update
    private PlayerCharacterController _playerCharacterController;
    private NavigationAgent _navigationAgent;
    public bool visualize = true;
    public float maxLen = 50f;
    public int rayCount = 2;
    public float rayDistance = 1f;
    public Transform aimPoint;
    private bool _agentCanRotate = false;

    private void Awake()
    {
        _playerCharacterController = GetComponent<PlayerCharacterController>();
        _navigationAgent = GetComponent<NavigationAgent>();
        _agentCanRotate = _navigationAgent.useRotation;
    }

    public float[] GetObservation()
    {
        float[] depthObservation = GetDepthMaskObservation();
        float[] rotationObservation = GetRotationObservation();
        float[] combined = rotationObservation.Concat(depthObservation).ToArray();
        return combined;
    }

    private float[] GetRotationObservation()
    {

        Transform rotationTransform = _agentCanRotate ? transform : aimPoint;
        float[] rotations =
        {
            Mathf.Sin(rotationTransform.rotation.eulerAngles.x * Mathf.Deg2Rad),
            Mathf.Sin(rotationTransform.rotation.eulerAngles.y * Mathf.Deg2Rad),
            Mathf.Sin(rotationTransform.rotation.eulerAngles.z * Mathf.Deg2Rad),
            
            Mathf.Cos(rotationTransform.rotation.eulerAngles.x * Mathf.Deg2Rad),
            Mathf.Cos(rotationTransform.rotation.eulerAngles.y * Mathf.Deg2Rad),
            Mathf.Cos(rotationTransform.rotation.eulerAngles.z * Mathf.Deg2Rad),
        };
        return rotations;
    }


    private float[] GetDepthMaskObservation()
    {
        List<float> depthMaskResults = new List<float>();
        Vector3 direction = _playerCharacterController.characterVelocity.normalized;
        direction = direction == Vector3.zero ? Vector3.right : direction;
        aimPoint.rotation = Quaternion.LookRotation(direction);
        Debug.DrawRay(aimPoint.position, direction, Color.red);
        for (int x = -rayCount; x <= rayCount; x++)
        {
            for (int y = -rayCount; y <= rayCount; y++)
            {
                //Vector3 offset = Quaternion.AngleAxis(angle, Vector3.up) * new Vector3(x * rayDistance, y * rayDistance, 0);
                Vector3 offset = new Vector3(x * rayDistance, y * rayDistance, 0);
                
                // If the agent is not controlling the rotation, then we rotate the depthMask to the direction of movement
                Vector3 rotationPivot = _agentCanRotate ? transform.rotation.eulerAngles : aimPoint.rotation.eulerAngles;
                Vector3 rayPosition = RotatePointAroundPivot(transform.position + offset, transform.position, rotationPivot);
                Ray ray = new Ray(rayPosition, direction);
                
                bool didHit = Physics.Raycast(ray, out  RaycastHit hit, maxLen);
                if (didHit)
                {
                    depthMaskResults.Add(1f - hit.distance / maxLen);
                    if (visualize)
                        Debug.DrawRay(rayPosition, direction * maxLen, Color.red, 0.2f);
                }
                else
                {
                    depthMaskResults.Add(0f);
                    if (visualize)
                        Debug.DrawRay(rayPosition, direction * maxLen, Color.blue, 0.2f);
                }
            }
        }
        return depthMaskResults.ToArray();
    }

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }
}
