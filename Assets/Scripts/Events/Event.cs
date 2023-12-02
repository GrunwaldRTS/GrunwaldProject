using System;

public class Event
{
	Action listeners = delegate { };

	public void AddListener(Action listener)
	{
		listeners -= listener;
		listeners += listener;
	}
	public void RemoveListener(Action listener)
	{
		listeners -= listener;
	}
	public void Invoke()
	{
		listeners();
	}
}
public class Event<T>
{
	Action<T> listeners = delegate { } ;

	public void AddListener(Action<T> listener)
	{
		listeners -= listener;
		listeners += listener;
	}
	public void RemoveListener(Action<T> listener)
	{
		listeners -= listener;
	}
	public void Invoke(T value)
	{
		listeners(value);
	}
}
public class Event<T, T1>
{
	Action<T, T1> listeners = delegate { };

	public void AddListener(Action<T, T1> listener)
	{
		listeners -= listener;
		listeners += listener;
	}
	public void RemoveListener(Action<T, T1> listener)
	{
		listeners -= listener;
	}
	public void Invoke(T value, T1 value1)
	{
		listeners(value, value1);
	}
}
public class Event<T, T1, T2>
{
	Action<T, T1, T2> listeners = delegate { };

	public void AddListener(Action<T, T1, T2> listener)
	{
		listeners -= listener;
		listeners += listener;
	}
	public void RemoveListener(Action<T, T1, T2> listener)
	{
		listeners -= listener;
	}
	public void Invoke(T value, T1 value1, T2 value2)
	{
		listeners(value, value1, value2);
	}
}
