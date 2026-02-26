using UnityEngine;
using System.Collections;

namespace GameMaker
{
	public interface IObjectPool
	{
		void CreatePool();

		void ClearPool();

		PooledObject GetObject(bool isActive = true);

		void AddObject(PooledObject po);
	}

	public interface IPooledObject
	{
		void ReturnToPool();
	}
}