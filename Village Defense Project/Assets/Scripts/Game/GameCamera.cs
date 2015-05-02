using UnityEngine;
using System.Collections;

namespace Gem
{
    public class GameCamera : MonoBehaviour
    {
        [SerializeField]
        private float m_MovementSpeed = 5.0f;
        [SerializeField]
        private float m_HeightSpeed = 30.0f;

        [SerializeField]
        private float m_Border = 5.0f;

        public Transform DebugPlayer = null;


        // Update is called once per frame
        void Update()
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 movementDirection = Vector3.zero;

            if(mousePosition.x < 0.0f + m_Border && mousePosition.x > 0.0f - m_Border)
            {
                movementDirection.x = -1.0f;
            }
            else if(mousePosition.x > Screen.width - m_Border && mousePosition.x < Screen.width + m_Border)
            {
                movementDirection.x = 1.0f;
            }

            if(mousePosition.y < 0.0 + m_Border && mousePosition.y > 0.0f - m_Border)
            {
                movementDirection.z = -1.0f;
            }
            else if(mousePosition.y > Screen.height - m_Border && mousePosition.y < Screen.height + m_Border)
            {
                movementDirection.z = 1.0f;
            }

            if(Input.GetKey(Constants.INPUT_CAMERA_RIGHT_KEY))
            {
                movementDirection.x += 1.0f;
            }
            if(Input.GetKey(Constants.INPUT_CAMERA_LEFT_KEY))
            {
                movementDirection.x -= 1.0f;
            }
            if(Input.GetKey(Constants.INPUT_CAMERA_DOWN_KEY))
            {
                movementDirection.z -= 1.0f;
            }
            if(Input.GetKey(Constants.INPUT_CAMERA_UP_KEY))
            {
                movementDirection.z += 1.0f;
            }
            
            movementDirection.x = Mathf.Clamp(movementDirection.x, -1.0f, 1.0f);
            movementDirection.z = Mathf.Clamp(movementDirection.z, -1.0f, 1.0f);
            movementDirection.y = 0.0f;

            transform.Translate(movementDirection * m_MovementSpeed * Time.deltaTime, Space.World);


            if(Input.GetKeyDown(Constants.INPUT_CAMERA_FOCUS_PLAYER))
            {
                transform.position = DebugPlayer.position + new Vector3(0.0f, 15.0f, -10.0f);

            }
        }

        void FixedUpdate()
        {
            int layerMask = 1 << LayerMask.NameToLayer("Ground");
            RaycastHit hit;
            if(Physics.Raycast(transform.position, Vector3.down, out hit, 50.0f, layerMask))
            {
                transform.position = Vector3.Lerp(transform.position, hit.point + Vector3.up * 15.0f, m_HeightSpeed * Time.fixedDeltaTime);
            }
        }
    }

}

