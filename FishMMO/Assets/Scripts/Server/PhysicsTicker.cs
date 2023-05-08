using FishNet;
using UnityEngine;

public class PhysicsTicker : MonoBehaviour
{
	private PhysicsScene _physicsScene;

	public void Initialize(PhysicsScene scene)
	{
		if (InstanceFinder.TimeManager != null)
		{
			InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
			_physicsScene = scene;
		}
	}

	void OnDestroy()
	{
		if (InstanceFinder.TimeManager != null)
		{
			InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
		}
	}

	void TimeManager_OnTick()
	{
		_physicsScene.Simulate((float)InstanceFinder.TimeManager.TickDelta);
	}
}