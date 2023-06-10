using UnityEngine;

namespace BattleViews.Components
{
    [RequireComponent(typeof(Camera))]
    public class ThirdPersonCameraContollor : UnityEngine.MonoBehaviour
    {
        public static ThirdPersonCameraContollor Current { private set; get; }
        public Camera currentCamera;

        private void Awake()
        {
            Current = this;
            currentCamera = GetComponent<Camera>();
        }

        public float dampping = 25f;

        // Update is called once per frame
        void Update()
        {

            rx = Mathf.Lerp(rx, rotationX, Time.deltaTime * dampping);
            ry = Mathf.Lerp(ry, rotationY, Time.deltaTime * dampping);
            if (lookTarget)
            {
                targetPos = Vector3.Lerp(targetPos,
                    lookTarget.position + lookTarget.rotation * forwardOffset, Time.deltaTime * dampping);
            }

            this.transform.position = targetPos - (Quaternion.Euler(rx, ry, 0) * Vector3.forward) * distance;
            this.transform.LookAt(targetPos);
        }


        public float distance = 10;
        private float rx = 0;
        private float ry = 0;
        private Vector3 targetPos;
        public float rotationX = 30; //{ private set; get; } = 45;
        public float rotationY = 0; // { private set; get; } = 0;

        public Transform lookTarget;
        public Vector3 forwardOffset = Vector3.zero;

        public ThirdPersonCameraContollor SetLookAt(Transform tr, bool noDelay = false)
        {
            lookTarget = tr;
            if (noDelay) targetPos = lookTarget.position + Quaternion.Euler(0, rotationY, 0) * forwardOffset;
            return this;
        }

        public ThirdPersonCameraContollor SetForwardOffset(Vector3 offset)
        {
            this.forwardOffset = offset;
            return this;
        }

        public void SetLookAt(Vector3 tr)
        {
            targetPos = tr;
        }

        public ThirdPersonCameraContollor SetDis(float dis)
        {
            this.distance = dis;
            return this;
        }

        private Vector2 _xRange = new Vector2(5, 85);
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

        public Vector3 LookPos => targetPos;

        public Quaternion LookRotation => Quaternion.Euler(0, ry, 0);

        public bool InView(Vector3 position)
        {
            var vp = currentCamera.WorldToViewportPoint(position);
            return vp.z > 0;
        }

        public ThirdPersonCameraContollor SetXY(float x, float y)
        {
            this.rotationX = x;
            this.rotationY = y;
            return this;
        }
    }
}
