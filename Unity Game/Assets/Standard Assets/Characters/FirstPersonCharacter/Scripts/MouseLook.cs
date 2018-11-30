using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [Serializable]
    public class MouseLook
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth;
        public float smoothTime = 5f;


        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;

        private System.IO.StreamReader file; //ADDED
        private string line; //ADDED

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
            file = new System.IO.StreamReader(@"./Assets/test.txt"); //ADDED
        }


        public void LookRotation(Transform character, Transform camera)
        {

            line = file.ReadLine(); //ADDED
            string[] a = line.Split(' '); //ADDED
            //float xRot = float.Parse(a[0]); //ADDED
            //float yRot = float.Parse(a[1]); //ADDED


            //To read from textfile or server, only need to change lines lines with xRot and yRot
            float yRot = CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity;
            float xRot = CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity;

            //float xRot = 0; //ADDED
            //float yRot = 0; //ADDED

            //Debug.Log("Mouse X number: " + CrossPlatformInputManager.GetAxis("Mouse X")); //ADDED
            //Debug.Log("Mouse Y number: " + CrossPlatformInputManager.GetAxis("Mouse Y")); //ADDED


            m_CharacterTargetRot *= Quaternion.Euler(-xRot, yRot, 0f); //ADDED
            m_CameraTargetRot *= Quaternion.Euler(0f, 0f, 0f); //ADDED
            Debug.Log("Character" + m_CharacterTargetRot);
            Debug.Log("Main Camera" + camera.localRotation);
            //m_CharacterTargetRot *= Quaternion.Euler (0f, yRot, 0f);
            //m_CameraTargetRot *= Quaternion.Euler (-xRot, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis (m_CameraTargetRot);

            if(smooth)
            {
                character.localRotation = Quaternion.Slerp (character.localRotation, m_CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                //camera.localRotation = Quaternion.Slerp (camera.localRotation, m_CameraTargetRot,
                  //  smoothTime * Time.deltaTime);
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
                //camera.localRotation = m_CameraTargetRot;
            }
        }


        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

            angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}
