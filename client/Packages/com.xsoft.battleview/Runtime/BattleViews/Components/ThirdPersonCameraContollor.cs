using UnityEngine;

namespace BattleViews.Components
{
    [RequireComponent(typeof(Camera))]
    public class ThirdPersonCameraContollor : MonoBehaviour
    {
        public Camera currentCamera;

        public float dampping = 25f;


        public float distance = 10;
        public float rotationX = 30; //{ private set; get; } = 45;
        public float rotationY; // { private set; get; } = 0;

        public Transform lookTarget;
        public Vector3 forwardOffset = Vector3.zero;

        private Vector2 _xRange = new(5, 85);
        private float rx;
        private float ry;
        public static ThirdPersonCameraContollor Current { private set; get; }

        public Vector3 LookPos { get; private set; }

        public Quaternion LookRotation => Quaternion.Euler(0, ry, 0);

        private void Awake()
        {
            Current = this;
            currentCamera = GetComponent<Camera>();
        }

        // Update is called once per frame
        private void Update()
        {
            rx = Mathf.Lerp(rx, rotationX, Time.deltaTime * dampping);
            ry = Mathf.Lerp(ry, rotationY, Time.deltaTime * dampping);
            if (lookTarget)
                LookPos = Vector3.Lerp(LookPos,
                    lookTarget.position + lookTarget.rotation * forwardOffset, Time.deltaTime * dampping);

            transform.position = LookPos - Quaternion.Euler(rx, ry, 0) * Vector3.forward * distance;
            transform.LookAt(LookPos);
        }

        public ThirdPersonCameraContollor SetLookAt(Transform tr, bool noDelay = false)
        {
            lookTarget = tr;
            if (noDelay) LookPos = lookTarget.position + Quaternion.Euler(0, rotationY, 0) * forwardOffset;
            return this;
        }

        public ThirdPersonCameraContollor SetForwardOffset(Vector3 offset)
        {
            forwardOffset = offset;
            return this;
        }

        public void SetLookAt(Vector3 tr)
        {
            LookPos = tr;
        }

        public ThirdPersonCameraContollor SetDis(float dis)
        {
            distance = dis;
            return this;
        }

        public ThirdPersonCameraContollor SetClampX(float min, float max)
        {
            _xRange = new Vector2(min, max);
            return this;
        }

        public ThirdPersonCameraContollor RotationByX(float x)
        {
            rotationX += x;
            rotationX = Mathf.Clamp(rotationX, _xRange.x, _xRange.y);
            return this;
        }

        public ThirdPersonCameraContollor RotationByY(float y)
        {
            rotationY -= y;
            return this;
        }

        public bool InView(Vector3 position)
        {
            var vp = currentCamera.WorldToViewportPoint(position);
            return vp.z > 0;
        }

        public ThirdPersonCameraContollor SetXY(float x, float y)
        {
            rotationX = x;
            rotationY = y;
            return this;
        }
    }
}