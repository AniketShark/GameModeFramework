using System;
using UnityEngine;

namespace GameModules.Shooting
{
	public enum RecoilStatus
	{
		Rest,
		Recoil,
		Decay
	}

	public class RecoilFunctionApex
	{
		private Vector3 _accumulatedRecoil;
		private Vector3 _accumulatedInputDuringRecoil;
		private Vector3 _accumulatedInputDuringDecay;
		private int _invertedPitch = -1;
		private float _targetYaw = 0;
		private float _targetPitch = 0;
		private RecoilStatus _status = RecoilStatus.Rest;
		private Vector3 _restValue = Vector3.zero;

		public float RecoiledPitch { get; private set; }
		public float RecoiledYaw { get; private set; }	
		public float RecoiledRoll { get; private set; }
		public int Inverted { get { return _invertedPitch; } 
			set { _invertedPitch = value > 0 ? 1 : -1; } }
		public RecoilStatus Status{get{ return _status;}}
		public Vector3 RecoilRestValue {get{ return _restValue;} }
		public Vector3 AccumulatedRecoil {get{ return _accumulatedRecoil;}}
		public Vector3 InputDuringRecoil {get{ return _accumulatedInputDuringRecoil;} }
		public Vector3 InputDuringDecay {
			get{ return _accumulatedInputDuringDecay;} 
			set{_accumulatedInputDuringDecay = value;}
		}

		#region Events
		public event Action<float> setPitch;
		public event Action<Vector3> addTargetRotationDelta;
		public event Action<float> setYaw;
		public event Action<RecoilStatus> onStatusChange; 
		#endregion

		#region Public Methods
		public void AddRecoil(Vector3 original, Vector3 input, Vector3 recoilForce)
		{
			if (_status != RecoilStatus.Recoil)
			{
				_status = RecoilStatus.Recoil;
				onStatusChange?.Invoke(_status);
			}

			ApplyForce(-recoilForce.x, recoilForce.y, 0);

			_accumulatedInputDuringRecoil.x -= input.x;
			_accumulatedInputDuringRecoil.y += input.y;


			RecoiledPitch = original.x + _accumulatedRecoil.x + _accumulatedInputDuringRecoil.x;
			RecoiledYaw   = original.y + _accumulatedRecoil.y + _accumulatedInputDuringRecoil.y;

			if (RecoiledPitch > original.x)
			{
				setPitch?.Invoke(RecoiledPitch);
				_accumulatedRecoil.x = 0;
				_accumulatedInputDuringRecoil.x = 0;
			}
		}

		public void ApplyDecay(Vector3 original, Vector3 input, float decaySpeed)
		{
			_accumulatedInputDuringDecay.x += input.x * _invertedPitch;
			_accumulatedInputDuringDecay.y += input.y;

			if (_status == RecoilStatus.Recoil)
			{
				_status = RecoilStatus.Decay;
				onStatusChange?.Invoke(_status);
				if (RecoiledPitch < original.x)
				{
					_targetYaw = original.y + _accumulatedInputDuringRecoil.y;
					if (_accumulatedInputDuringRecoil.x < 0)
					{
						_targetPitch = original.x + _accumulatedInputDuringRecoil.x;
					}
				}
			}
			Decay(decaySpeed, original);
		}

		#endregion

		#region Private Methods
		private void ApplyForce(float pitchForce, float yawForce, float rollForce)
		{
			_accumulatedRecoil.x += pitchForce;
			_accumulatedRecoil.y += yawForce;
			_accumulatedRecoil.z += rollForce;
		}

		private void Decay(float decaySpeed, Vector3 original)
		{
			// Resetting accumulated recoil
			_accumulatedRecoil.x = Mathf.SmoothStep(_accumulatedRecoil.x, _restValue.x, decaySpeed);
			_accumulatedRecoil.y = Mathf.SmoothStep(_accumulatedRecoil.y, _restValue.y, decaySpeed);

			// Resetting accumulated Input only over pitch as we want to keep horizontal input intact
			_accumulatedInputDuringRecoil.x = Mathf.SmoothStep(_accumulatedInputDuringRecoil.x, 0, decaySpeed);
			_accumulatedInputDuringRecoil.y = Mathf.SmoothStep(_accumulatedInputDuringRecoil.y, 0, decaySpeed);

			var diffAdd = Vector3.zero;
			// Keeping horizontal input intact by resetting to target yaw
			if (Mathf.Abs(_targetYaw) > 0)
			{
				float diff = _targetYaw - original.y;
				diffAdd.y = Mathf.SmoothStep(0, diff, decaySpeed);
			}

			if (Mathf.Abs(_targetPitch) > 0)
			{
				float diff = _targetPitch - original.x;
				diffAdd.x = Mathf.SmoothStep(0, diff, decaySpeed);
			}

			if (diffAdd != Vector3.zero)
				addTargetRotationDelta?.Invoke(diffAdd);

			if (Mathf.Abs(_accumulatedRecoil.x - _restValue.x) <= 0.001f && Mathf.Abs(_accumulatedRecoil.y - _restValue.y) <= 0.001f)
			{
				_targetYaw = 0;
				_targetPitch = 0;
				if (_status != RecoilStatus.Rest)
				{
					_status = RecoilStatus.Rest;
					onStatusChange?.Invoke(_status);
				}
			}
		} 

		public void Reset()
		{
			_accumulatedInputDuringDecay = Vector3.zero;
			_accumulatedInputDuringRecoil = Vector3.zero;
			_accumulatedRecoil = Vector3.zero;
			_status = RecoilStatus.Rest;
		}

		#endregion
	}
}
