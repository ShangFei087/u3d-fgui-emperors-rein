using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace GameMaker
{
	public class ReturnToPool : MonoBehaviour
	{
		public float delay = 1.0f;
		public IEnumerator Start()
		{
			yield return new WaitForSeconds(delay);
			GetComponent<PooledObject>().ReturnToPool();
		}
	}
}