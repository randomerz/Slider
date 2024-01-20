using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	[AddComponentMenu("More Mountains/Tools/Gyroscope/MMGyroscope")]
	public class MMGyroscope : MonoBehaviour
	{
		public enum TimeScales { Scaled, Unscaled }

		public static bool GyroscopeActive = true;
		public static TimeScales TimeScale = TimeScales.Scaled;
		public static Vector2 Clamps = new Vector2(-1f, 1f);
		public static float LerpSpeed = 1f;
		public static bool TestMode;
        
		[Header("Debug")]
		/// turn this on if you want to use the inspector to test this camera
		public bool _TestMode = false;
		/// the rotation to apply on the x axiswhen in test mode
		[Range(-1f, 1f)]
		public float TestXAcceleration = 0f;
		/// the rotation to apply on the y axis while in test mode
		[Range(-1f, 1f)]
		public float TestYAcceleration = 0f;
		/// the rotation to apply on the y axis while in test mode
		[Range(-1f, 1f)]
		public float TestZAcceleration = 0f;

		public static Quaternion GyroscopeAttitude { get { GetValues(); return m_GyroscopeAttitude; } }
		public static Vector3 GyroscopeRotationRate { get { GetValues(); return m_GyroscopeRotationRate; } }
		public static Vector3 GyroscopeAcceleration { get { GetValues(); return m_GyroscopeAcceleration; } }
		public static Vector3 InputAcceleration { get { GetValues(); return m_InputAcceleration; } }
		public static Vector3 GyroscopeGravity { get { GetValues(); return m_GyroscopeGravity; } }

		public static Quaternion InitialGyroscopeAttitude { get { GetValues(); return m_InitialGyroscopeAttitude; } }
		public static Vector3 InitialGyroscopeRotationRate { get { GetValues(); return m_InitialGyroscopeRotationRate; } }
		public static Vector3 InitialGyroscopeAcceleration { get { GetValues(); return m_InitialGyroscopeAcceleration; } }
		public static Vector3 InitialInputAcceleration { get { GetValues(); return m_InitialInputAcceleration; } }
		public static Vector3 InitialGyroscopeGravity { get { GetValues(); return m_InitialGyroscopeGravity; } }

		public static Vector3 CalibratedInputAcceleration { get { GetValues(); return m_CalibratedInputAcceleration; } }
		public static Vector3 CalibratedGyroscopeGravity { get { GetValues(); return m_CalibratedGyroscopeGravity; } }

		public static Vector3 LerpedCalibratedInputAcceleration { get { GetValues(); return m_LerpedCalibratedInputAcceleration; } }
		public static Vector3 LerpedCalibratedGyroscopeGravity { get { GetValues(); return m_LerpedCalibratedGyroscopeGravity; } }
        
		private static Quaternion m_GyroscopeAttitude;
		private static Vector3 m_GyroscopeRotationRate;
		private static Vector3 m_GyroscopeAcceleration;
		private static Vector3 m_InputAcceleration;
		private static Vector3 m_GyroscopeGravity;
		private static Quaternion m_InitialGyroscopeAttitude;
		private static Vector3 m_InitialGyroscopeRotationRate;
		private static Vector3 m_InitialGyroscopeAcceleration;
		private static Vector3 m_InitialInputAcceleration;
		private static Vector3 m_InitialGyroscopeGravity;
		private static Vector3 m_CalibratedInputAcceleration;
		private static Vector3 m_CalibratedGyroscopeGravity;
		private static Vector3 m_LerpedCalibratedInputAcceleration;
		private static Vector3 m_LerpedCalibratedGyroscopeGravity;

		[Header("Settings")]
		/// whether this rig should move in scaled or unscaled time
		[SerializeField]
		private TimeScales _TimeScale = TimeScales.Scaled;
		/// the clamps to apply to the values
		[SerializeField]
		private Vector2 _Clamps = new Vector2(-1f, 1f);
		/// the speed at which to move towards the new position
		[SerializeField]
		private float _LerpSpeed = 1f;

		[Header("Raw Values")]
		[MMReadOnly]
		[SerializeField]
		private Quaternion _GyroscopeAttitude;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _GyroscopeRotationRate;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _GyroscopeAcceleration;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _InputAcceleration;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _GyroscopeGravity;

		[Header("AutoCalibration Values")]
		[MMReadOnly]
		[SerializeField]
		private Quaternion _InitialGyroscopeAttitude;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _InitialGyroscopeRotationRate;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _InitialGyroscopeAcceleration;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _InitialInputAcceleration;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _InitialGyroscopeGravity;

		[Header("Relative Values")]
		[MMReadOnly]
		[SerializeField]
		private Vector3 _CalibratedInputAcceleration;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _CalibratedGyroscopeGravity;

		[Header("Lerped Values")]
		[MMReadOnly]
		[SerializeField]
		private Vector3 _LerpedCalibratedInputAcceleration;
		[MMReadOnly]
		[SerializeField]
		private Vector3 _LerpedCalibratedGyroscopeGravity;

		[MMInspectorButton("Calibrate")]
		public bool CalibrateButton;

		private static Gyroscope _gyroscope;
		protected static Vector3 _testVector = Vector3.zero;
		private static bool _initialized = false;
		private static Matrix4x4 _accelerationMatrix;
		private static Matrix4x4 _gravityMatrix;
		private static float _lastGetValuesAt = 0f;
        
		protected virtual void Start()
		{
			TimeScale = _TimeScale;
			Clamps = _Clamps;
			LerpSpeed = _LerpSpeed;
			TestMode = _TestMode;
			GyroscopeInitialization();
		}

		public static void GyroscopeInitialization()
		{
			_gyroscope = Input.gyro;
			_gyroscope.enabled = true;
		}

		protected virtual void Update()
		{
			if (!GyroscopeActive)
			{
				return;
			}
			HandleTestMode();
			GetValues();
			_GyroscopeAttitude = m_GyroscopeAttitude;
			_GyroscopeRotationRate = m_GyroscopeRotationRate;
			_GyroscopeAcceleration = m_GyroscopeAcceleration;
			_InputAcceleration = m_InputAcceleration;
			_GyroscopeGravity = m_GyroscopeGravity;
			_InitialGyroscopeAttitude = m_InitialGyroscopeAttitude;
			_InitialGyroscopeRotationRate = m_InitialGyroscopeRotationRate;
			_InitialGyroscopeAcceleration = m_InitialGyroscopeAcceleration;
			_InitialInputAcceleration = m_InitialInputAcceleration;
			_InitialGyroscopeGravity = m_InitialGyroscopeGravity;
			_CalibratedInputAcceleration = m_CalibratedInputAcceleration;
			_CalibratedGyroscopeGravity = m_CalibratedGyroscopeGravity;
			_LerpedCalibratedInputAcceleration = m_LerpedCalibratedInputAcceleration;
			_LerpedCalibratedGyroscopeGravity = m_LerpedCalibratedGyroscopeGravity;
		}

		public static void GetValues()
		{
			if (Time.frameCount == _lastGetValuesAt)
			{
				return;
			}
			AutoCalibration();
			GetGyroValues();
			_lastGetValuesAt = Time.frameCount;
		}

		private static void GetGyroValues()
		{
			float deltaTime = (TimeScale == TimeScales.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime;

			m_GyroscopeAttitude = GyroscopeToUnity(Input.gyro.attitude);
			m_GyroscopeRotationRate = Input.gyro.rotationRateUnbiased;
			m_GyroscopeAcceleration = Input.gyro.userAcceleration;
            
			GetAccelerationAndGravity();
			ClampAcceleration();

			m_CalibratedInputAcceleration = CalibratedAcceleration(m_InputAcceleration, _accelerationMatrix);
			m_CalibratedGyroscopeGravity = CalibratedAcceleration(m_GyroscopeGravity, _gravityMatrix);

			m_LerpedCalibratedInputAcceleration = Vector3.Lerp(m_LerpedCalibratedInputAcceleration, m_CalibratedInputAcceleration, deltaTime * LerpSpeed);
			m_LerpedCalibratedGyroscopeGravity = Vector3.Lerp(m_LerpedCalibratedGyroscopeGravity, m_CalibratedGyroscopeGravity, deltaTime * LerpSpeed);
		}

		private static void AutoCalibration()
		{
			if (!_initialized && Time.time > 0.5f)
			{
				m_InitialGyroscopeAttitude = GyroscopeToUnity(Input.gyro.attitude);
				m_InitialGyroscopeRotationRate = Input.gyro.rotationRateUnbiased;
				m_InitialGyroscopeAcceleration = Input.gyro.userAcceleration;
				m_InitialInputAcceleration = Input.acceleration;
				m_InitialGyroscopeGravity = Input.gyro.gravity; 

				Calibrate();

				_initialized = true;
			}
		}

		protected static Quaternion GyroscopeToUnity(Quaternion q)
		{
			return new Quaternion(q.x, q.y, -q.z, -q.w);
		}

		private static void ClampAcceleration()
		{
			m_InputAcceleration.x = Mathf.Clamp(m_InputAcceleration.x, Clamps.x, Clamps.y);
			m_InputAcceleration.y = Mathf.Clamp(m_InputAcceleration.y, Clamps.x, Clamps.y);
			m_InputAcceleration.z = Mathf.Clamp(m_InputAcceleration.z, Clamps.x, Clamps.y);

			m_GyroscopeGravity.x = Mathf.Clamp(m_GyroscopeGravity.x, Clamps.x, Clamps.y);
			m_GyroscopeGravity.y = Mathf.Clamp(m_GyroscopeGravity.y, Clamps.x, Clamps.y);
			m_GyroscopeGravity.z = Mathf.Clamp(m_GyroscopeGravity.z, Clamps.x, Clamps.y);
		}

		protected virtual void HandleTestMode()
		{
			if (TestMode)
			{
				_testVector.x = TestXAcceleration;
				_testVector.y = TestYAcceleration;
				_testVector.z = TestZAcceleration;
				m_InputAcceleration = _testVector;
				m_GyroscopeGravity = _testVector;
			}
			else
			{
				GetAccelerationAndGravity();
			}
		}

		private static void GetAccelerationAndGravity()
		{
			if (!TestMode)
			{
				m_InputAcceleration = Input.acceleration;
				m_GyroscopeGravity = Input.gyro.gravity;
			}            
		}

		private static void Calibrate()
		{
			_accelerationMatrix = CalibrateAcceleration(m_InputAcceleration);
			_gravityMatrix = CalibrateAcceleration(Input.gyro.gravity);
		}

		private static Matrix4x4 CalibrateAcceleration(Vector3 initialAcceleration)
		{
			Quaternion rotationQuaternion = Quaternion.FromToRotation(-Vector3.forward, initialAcceleration);
			Matrix4x4 newMatrix = Matrix4x4.TRS(Vector3.zero, rotationQuaternion, Vector3.one);
			return newMatrix.inverse;
		}

		private static Vector3 CalibratedAcceleration(Vector3 accelerator, Matrix4x4 matrix)
		{
			Vector3 fixedAcceleration = matrix.MultiplyVector(accelerator);
			return fixedAcceleration;
		}
	}
}