using System.Threading.Tasks;
using UnityEngine;

namespace UApp
{
	public abstract class UGate: UnityEngine.MonoBehaviour
	{
		protected virtual async Task JoinGate(params object[] args)
		{
			await Task.CompletedTask;
		}

		protected virtual async Task ExitGate()
		{
			await Task.CompletedTask;
		}

		protected virtual void Tick()
		{
		}

		public static void DoTick(UGate gate)
		{
			gate.Tick();
		}

		public static async Task DoJoinGate(UGate gate, params object[] args)
		{
			await gate.JoinGate(args);
		}

		public static async Task DoExitGate(UGate gate)
		{
			await gate.ExitGate();
		}
	}
}

 

