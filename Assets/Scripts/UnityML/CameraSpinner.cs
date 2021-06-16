using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSpinner : MonoBehaviour
{
    public Transform target;
    public Transform agent;
    public Transform goal;
    

    public float speed;

    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    { 
    
        transform.RotateAround(target.position, Vector3.up, speed * Time.deltaTime);
        transform.LookAt(target.position, Vector3.up);
        
        // MoveGoalToMouse();
        // MovePlayerToMouse();
    }

    void MovePlayerToMouse()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            Cursor.lockState = CursorLockMode.None;
            Vector3? hitPos = GetClickPosition();
            if (hitPos != null)
            {
                agent.position = (Vector3) (hitPos + new Vector3(0, 1f, 1));
            }
        }
    }

    void MoveGoalToMouse()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            Vector3? hitPos = GetClickPosition();
            if (hitPos != null)
            {
                goal.position = (Vector3) (hitPos + new Vector3(0, 2f, 1));
            }
        }
    }
    
    
    Vector3? GetClickPosition()
    {
        Vector3? hitPos = null;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hitData, 1000) && (!hitData.transform.CompareTag("Goal") && !hitData.transform.CompareTag("Player")))
        {
            hitPos = hitData.point;
        }
        return hitPos;
    }
}
