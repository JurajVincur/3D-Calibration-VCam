using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSphere : MonoBehaviour
{
    public float distance = 2f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float dir = 1;

    void Start()
    {
        startPos = transform.position;
        targetPos = transform.up * distance * dir;
    }

    void Update()
    {
        if (targetPos != transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.01f);
        }
        else
        {
            dir = -dir;
            targetPos = transform.up * distance * dir;
        }
    }
}
