using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public float distance = 10;
    public float xSpeed = 120;
    public float ySpeed = 120;
    public float yMin = 15;
    public float yMax = 80;

    private float x = 0;
    private float y = 0;

    private void Start()
    {
        Vector3 euler = transform.eulerAngles;
        x = euler.y;
        y = euler.x;
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            Cursor.visible = false;
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            x += mouseX * xSpeed * Time.deltaTime;
            y -= mouseY * ySpeed * Time.deltaTime;
            y = Mathf.Clamp(y, yMin, yMax);
        }
        else
        {
            Cursor.visible = true;
        }
        transform.rotation = Quaternion.Euler(y, x, 0);
        transform.position = -transform.forward * distance;
    }
}
