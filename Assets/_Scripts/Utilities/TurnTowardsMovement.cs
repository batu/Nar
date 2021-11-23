using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTowardsMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 5f;
    PlayerCharacterController _playerCharacterController;
    void Start()
    {
        _playerCharacterController = transform.parent.GetComponent<PlayerCharacterController>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        
        Vector3 movement = _playerCharacterController.characterVelocity.normalized;
        Vector3 cameraForward = Camera.main.transform.forward;
        movement.y = 0;
        cameraForward.y = 0;

        float angle = Vector3.SignedAngle(movement, cameraForward, Vector3.up);
        transform.RotateAround(_playerCharacterController.transform.position, Vector3.up, -angle * Time.deltaTime * speed);

        
        
    }
}
