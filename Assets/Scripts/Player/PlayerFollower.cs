using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    Transform playerOBJTransform;

    private void Start()
    {
        playerOBJTransform = transform.parent.GetChild(0).transform;
    }

    private void Update()
    {
        transform.position = playerOBJTransform.transform.position;
    }
}
