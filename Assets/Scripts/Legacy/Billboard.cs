using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// currently defunct but might be brought back to use if i ever decide I need to billboard tree sprites instead of rendering tree models in full,
/// same with some other objects
/// </summary>
public class Billboard : MonoBehaviour
{
    public Camera m_Camera;

    private void Start()
    {
        m_Camera = Camera.main;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward,
            m_Camera.transform.rotation * Vector3.up);

        // The next three lines make this work only on the horizontal axis
        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.x = 0;
        transform.eulerAngles = eulerAngles;

    }
}
