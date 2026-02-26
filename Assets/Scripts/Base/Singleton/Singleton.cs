//namespace GameMaker { }

public class Singleton<T> where T : new()
{
	private static T _instance;
	private static object _mutex = new object();
		
	public static T Instance
	{
		get
		{
			lock(_mutex)
			{
				if (_instance == null)
				{
					_instance = new T();
				}
					
				return _instance;
			}
		}
	}
}
