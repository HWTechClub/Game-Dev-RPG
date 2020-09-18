using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Vector3 cameraOffset;

    [SerializeField] Transform cameraTransform;

    [SerializeField] Transform playerTransform;

    [Range (0.1f, 10f)]
    [SerializeField] float horizontal_sens = 1;
    [Range(0.1f, 10f)]
    [SerializeField] float vertical_sens = 1;

    float xPos = 0;
    float yPos = 0;

    [Range (0.5f,3.5f)]
    [SerializeField] float zoom = 1;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        cameraTransform = Camera.main.gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        #region Input
        xPos += Input.GetAxis ("Mouse X");

        yPos += Input.GetAxis("Mouse Y");
        yPos = Mathf.Clamp(yPos, -89 / vertical_sens, 89/vertical_sens);

        zoom -= Input.GetAxis("Mouse ScrollWheel");
        zoom = Mathf.Clamp(zoom, 0.5f, 3.5f);

        #endregion

        transform.rotation = Quaternion.Euler (-yPos * vertical_sens, xPos * horizontal_sens,  0);

        cameraTransform.localPosition = cameraOffset * zoom;

        cameraTransform.LookAt(playerTransform.position + new Vector3 (0,1,0));
    }
}
