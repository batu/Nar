using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Unity.MLAgents;
using UnityEngine;

[RequireComponent(typeof(NavigationAgent))]
public class DepthMapObservation : MonoBehaviour, IObservation
{
    // Start is called before the first frame update
    private PlayerCharacterController _playerCharacterController;
    private NavigationAgent _navigationAgent;
    public bool visualize = true;
    [Range(1,50)]
    public float maxLen = 50f;
    public int rayCount = 2;
    public Transform aimPoint;
    private bool _agentCanRotate = false;
    public Vector3 _eyeOffset = new Vector3(0, 1.5f, 0);

    [Range(1,120)]
    public float FOV = 80f;
    private void Start()
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
        Vector3 agentEyePosition = transform.localPosition + _eyeOffset;
        float sideDistance = maxLen * Mathf.Tan(FOV / 2f * Mathf.Deg2Rad);
        float raySeparation = 2 * sideDistance / (rayCount + 1);
        for (float xPos = -sideDistance; xPos <= sideDistance; xPos += raySeparation)
        {
            for (float yPos = -sideDistance; yPos <= sideDistance; yPos += raySeparation)
            {
                Vector3 rayTarget = new Vector3(xPos, yPos, maxLen) + agentEyePosition;
                Vector3 rayVector = rayTarget - agentEyePosition;
                Ray ray = new Ray(agentEyePosition, rayVector.normalized);
                
                bool didHit = Physics.Raycast(ray, out  RaycastHit hit, rayVector.magnitude);
                if (didHit)
                {
                    depthMaskResults.Add(1f - hit.distance / rayVector.magnitude);
                    if (visualize)
                        Debug.DrawRay(transform.position + _eyeOffset, hit.point - (transform.position + _eyeOffset), Color.red, 0.2f);
                }
                else
                {
                    depthMaskResults.Add(0f);
                    if (visualize)
                        Debug.DrawRay(transform.position + _eyeOffset, rayVector, Color.blue, 0.2f);
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
