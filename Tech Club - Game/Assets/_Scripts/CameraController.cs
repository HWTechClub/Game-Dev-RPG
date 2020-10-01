using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Vector3 cameraOffset;

    [SerializeField] Transform cameraTransform;

    [SerializeField] Transform playerTransform;

    [SerializeField] Transform desiredCamPos;

    [SerializeField] float cameraLerpTime;

    [SerializeField] LayerMask groundLayer;

    [Range(0.1f, 10f)]
    [SerializeField] float horizontal_sens = 1;
    [Range(0.1f, 10f)]
    [SerializeField] float vertical_sens = 1;

    float xPos = 0;
    float yPos = 0;

    [Range(0.5f, 3.5f)]
    [SerializeField] float zoom = 1;

    [SerializeField] bool seeingPlayer = false;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        cameraTransform = Camera.main.gameObject.transform;
    }

    [SerializeField] float zoom2 = 1;

    // Update is called once per frame
    void Update()
    {
        #region Input
        xPos += Input.GetAxis("Mouse X");

        yPos += Input.GetAxis("Mouse Y");
        yPos = Mathf.Clamp(yPos, -89 / vertical_sens, 89 / vertical_sens);

        zoom -= Input.GetAxis("Mouse ScrollWheel");
        zoom = Mathf.Clamp(zoom, 0.9f, 3.5f);

        #endregion


        //Rotate the camera rotator based on mouse position input.
        transform.rotation = Quaternion.Euler(-yPos * vertical_sens, xPos * horizontal_sens, 0);

        zoom2 = 1;

        desiredCamPos.localPosition = cameraOffset * zoom * zoom2;

        RaycastHit hit;
        for (int i = 0; i < 20; i++)
        {
            Vector3 rayDirection = playerTransform.position - desiredCamPos.position;

            Ray ray = new Ray(desiredCamPos.position, rayDirection.normalized);


            if (Physics.Raycast(ray, out hit, Vector3.Distance (desiredCamPos.position, playerTransform.position), groundLayer))
            {
                seeingPlayer = false;

                //desiredCamPos.localPosition -= rayDirection.normalized;
                zoom2 -= 0.05f;
                desiredCamPos.localPosition = cameraOffset * zoom * zoom2;
            }
            else
            {
                seeingPlayer = true;

                break;
            }
        }
        


        Vector3 smoothPosition = Vector3.Lerp(cameraTransform.position, desiredCamPos.position, cameraLerpTime * Time.deltaTime);

        cameraTransform.position = smoothPosition;

        cameraTransform.LookAt(playerTransform.position + new Vector3(0, 1.5f * zoom * zoom2, 0));




    }
}
