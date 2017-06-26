using UnityEngine;

public class SingletonBehavior<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	public static T Instance
	{
		get
		{	
			return _instance;
		}
	}

	protected void Awake()
	{
		if (_instance != null)
		{
			Destroy(gameObject);
			return;
		}

		_instance = GetComponent<T>();
		Init();
	}

	protected virtual void Init()
	{
	}
}