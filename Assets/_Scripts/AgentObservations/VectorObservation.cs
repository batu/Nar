using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorObservation : MonoBehaviour, IObservation
{
    // Start is called before the first frame update
    private GameObject _goal;

    private NavigationAgent _navigationAgent;
    private PlayerCharacterController _playerCharacterController;

    private float _maxDistance;
    private bool _useAbsolutePositions;
    
    void Awake()
    {
        _navigationAgent = GetComponent<NavigationAgent>();
        _playerCharacterController = GetComponent<PlayerCharacterController>();

        _useAbsolutePositions = _navigationAgent.useAbsolutePositions;
        _goal = _navigationAgent.goal;
        _maxDistance = _navigationAgent.maxDistance;
    }
    


    public float[] GetObservation()
    {
        List<float> vectorResults = new List<float>();
        
        var localPosition = transform.localPosition;
        var goalPosition = _goal.transform.localPosition;
        
        Vector3 dir = goalPosition - localPosition;
        vectorResults.Add(CanSeeGoal(dir.normalized)); //0
        vectorResults.Add(dir / _maxDistance); // 3
        vectorResults.Add(dir.normalized);    // 6
        vectorResults.Add(dir.magnitude / _maxDistance); // 7

        int isGroundedInt = _playerCharacterController.isGrounded ? 1 : 0; 
        vectorResults.Add((_playerCharacterController.m_remainingJumpCount + isGroundedInt) / 2f); // 8

        vectorResults.Add(_playerCharacterController.characterVelocity.x /  _playerCharacterController.maxSpeedOnGround); // 9
        vectorResults.Add(Mathf.Clamp(_playerCharacterController.characterVelocity.y / (_playerCharacterController.maxSpeedInAir * 5), -1, 1)); // 10
        vectorResults.Add(_playerCharacterController.characterVelocity.z / _playerCharacterController.maxSpeedOnGround); // 11
        
        vectorResults.Add(_playerCharacterController.isGrounded); // 12

        if (_useAbsolutePositions)
        {
            vectorResults.Add(localPosition / _maxDistance);  // 15
            vectorResults.Add(goalPosition / _maxDistance);  // 18
        }

        return vectorResults.ToArray();
    }
    
    bool CanSeeGoal(Vector3 direction)
    {
        Ray ray = new Ray(transform.position, direction);
        bool didHit = Physics.Raycast(ray, out RaycastHit hit, 1000f);
        return didHit && hit.transform.CompareTag("Goal");
    }
    

    
}
