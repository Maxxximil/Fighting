using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public float smoothTime = 0.2f;
    public Transform target;

    private Vector3 _velocity = Vector3.zero;

    private void LateUpdate()
    {
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, smoothTime);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
