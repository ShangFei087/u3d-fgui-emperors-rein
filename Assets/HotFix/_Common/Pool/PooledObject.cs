using UnityEngine;
using System.Collections;
namespace GameMaker
{
	/// <summary>
	/// 池里的子对象
	/// </summary>
	[AddComponentMenu("GameMaker/GameObject/Object Pool/Pooled Object")]
	public class PooledObject : MonoBehaviour, IPooledObject
	{
		public ObjectPool pool;

		public virtual void ReturnToPool()
		{
			if (pool)
			{
				pool.AddObject(this);
			}
			else
			{
				DebugUtils.LogWarning("PooledObject has not pool.");
				Destroy(gameObject);
			}
		}

		public virtual void WaitAndReturnToPool(float time)
		{
			Invoke("ReturnToPool", time);
		}
	}
}