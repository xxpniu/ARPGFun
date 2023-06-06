using Layout.LayoutElements;
using UnityEngine;

namespace BattleViews.Components
{
	public class MissileFollowPath : MonoBehaviour
	{

		public MovementType MType = MovementType.Line;

		public enum MissileMoveState
		{
			NoStart,
			Actived,
			Moveing,
			Death
		}

		public MissileMoveState state = MissileMoveState.NoStart;

		public float Speed;
		public Transform Target;
		public Transform Actived;
		public Transform Moveing;
		public Transform Death;
		void Start()
		{
			if (Actived)
				Actived.gameObject.SetActive(false);
			if (Moveing)
				Moveing.gameObject.SetActive(false);
			if (Death)
				Death.gameObject.SetActive(false);
		}


		void Update()
		{
			if (!Target && !forward.HasValue)
			{
				return;
			}
			switch (state)
			{
				case MissileMoveState.NoStart:
					break;
				case MissileMoveState.Actived:
					if (Actived) Actived.gameObject.SetActive(true);
					state = MissileMoveState.Moveing;
					if (Moveing)
					{
						Moveing.gameObject.SetActive(true);
						switch (MType)
						{
							case MovementType.Follow:
							case MovementType.AutoTarget:
								Moveing.LookAt(Target);
								break;
							case MovementType.Line:
								Moveing.forward = forward.Value;
								break;
						
						}
					}
					break;
				case MissileMoveState.Moveing:
					if (Moveing)
					{
						switch (MType)
						{
							case MovementType.Follow:
							case MovementType.AutoTarget:
							{
								Moveing.LookAt(Target);
								Moveing.localPosition += (Moveing.forward * Speed * Time.deltaTime);

								if (Vector3.Distance(Target.position, this.Moveing.position) <= Speed / 20)
								{
									Moveing.position = Target.position;
									state = MissileMoveState.Death;
								}
							}
								break;
							case MovementType.Line:
							{
								Moveing.forward = forward.Value;
								Moveing.localPosition += (Moveing.forward * Speed * Time.deltaTime);
								time -= Time.deltaTime;
								if (time <= 0)
								{
									//Moveing.position = Target.position;
									state = MissileMoveState.Death;
								}
							}
								break;
						}

					
					}
					break;
				case MissileMoveState.Death:
					if (Death)
					{
						Death.gameObject.SetActive(true);
						Death.localPosition = Moveing.localPosition;
					}
					state = MissileMoveState.NoStart;
					break;
			}
		}

		private void SetTarget(Transform target, float speed)
		{
			Target = target;
			this.Speed = speed;
		
		}

		public MissileFollowPath BeginAutoTarget(Transform target, float speed)
		{
			MType = MovementType.AutoTarget;
			SetTarget(target, speed);
			state = MissileMoveState.Actived;
			return this;
		}

		public MissileFollowPath BeginMove(Transform target, float speed)
		{
			MType = MovementType.Follow;
			SetTarget(target, speed);
			state = MissileMoveState.Actived;
			return this;
		}


		public Vector3? forward;
		public float time = 0;

		public MissileFollowPath BeginMove(Vector3 forward, float speed, float time)
		{
			MType = MovementType.Line;
			this.Speed = speed;
			this.time = time;
			this.forward = forward;
			state = MissileMoveState.Actived;
			return this;
		}
	}
}
