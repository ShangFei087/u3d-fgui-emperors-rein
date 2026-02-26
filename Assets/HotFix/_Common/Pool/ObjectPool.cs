using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using GameMaker;

namespace GameMaker
{
	[AddComponentMenu("GameMaker/GameObject/Object Pool/Object Pool")]
	public class ObjectPool : MonoBehaviour, IObjectPool
	{
		public GameObject prefab;
		public int poolSize;

		public List<PooledObject> availiableObjects = new List<PooledObject>();

		private void Awake()
		{
			CreatePool();
		}

		[Button]
		public virtual void ResetPool()
		{
			ClearPool();
			while (this.transform.childCount > 0)
			{
				GameObject.DestroyImmediate(this.transform.GetChild(0).gameObject);
			}
			CreatePool();
		}


		[Button]
		public virtual void CreatePool()
		{
			if (prefab == null)
				return;

			while (availiableObjects.Count < poolSize)
			{
				AddObject(CreateObject());
			}
		}

		[Button]
		public virtual void ClearPool()
		{
			for (int i = 0; i < availiableObjects.Count; ++i)
			{
				GameObject.DestroyImmediate(availiableObjects[i].gameObject);
			}
			availiableObjects.Clear();
		}

		public virtual PooledObject GetObject(bool isActive = true)
		{
			PooledObject po;
			int lastAvailiableIndex = availiableObjects.Count - 1;
			if (lastAvailiableIndex >= 0)
			{
				po = availiableObjects[lastAvailiableIndex];
				availiableObjects.RemoveAt(lastAvailiableIndex);
			}
			else
			{
				//po = CreateObject();
				po = CreateObjectWild();
            }

			if (isActive)
				po.gameObject.SetActive(isActive);

			return po;
		}

		public virtual void AddObject(PooledObject po)
		{
			po.transform.SetParent(transform, false);
			po.gameObject.SetActive(false);
			availiableObjects.Add(po);
		}

		protected PooledObject CreateObject()
		{
			var go = Instantiate(prefab) as GameObject;
			go.name = prefab.name;
			var po = go.GetComponent<PooledObject>();
			po.pool = this;
			return po;
		}

        protected PooledObject CreateObjectWild()
        {
            var go = Instantiate(prefab) as GameObject;
			go.name = $"{prefab.name} Wild";
            var po = go.GetComponent<PooledObject>();
            po.pool = null; // PooledObject.ReturnToPool() 将对其销毁
            //po.transform.SetParent(transform, false);
            return po;
        }
    }
}
