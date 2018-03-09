using System.Collections.Generic;

public delegate void NotificationEvent(object data = null);

public class NotificationCenter 
{
	private static NotificationCenter _default = new NotificationCenter();
	public static NotificationCenter Default { get { return _default; } }

	private Dictionary<string, List<NotificationEvent>> _notifies = new Dictionary<string, List<NotificationEvent>>();

	public void RemoveAllObservers()
	{
		_notifies.Clear();
	}

	public void AddObserver(string eventName, NotificationEvent notification)
	{
		if(string.IsNullOrEmpty(eventName))
			return;

		if(!_notifies.ContainsKey(eventName))
			_notifies[eventName] = new List<NotificationEvent>();

		if(!_notifies[eventName].Contains(notification))
			_notifies[eventName].Add(notification);
	}

	public void RemoveObserver(string eventName, NotificationEvent notification)
	{
		if(string.IsNullOrEmpty(eventName))
			return;

		if(!_notifies.ContainsKey(eventName))
		   return;

		_notifies[eventName].Remove(notification);

		if(_notifies[eventName].Count == 0)
			_notifies.Remove(eventName);
	}

	public void RemoveObserver(NotificationEvent notification)
	{
		foreach(List<NotificationEvent> notify in _notifies.Values)
			notify.Remove(notification);
	}

	public void PostNotification(string eventName, object data = null)
	{
		if(string.IsNullOrEmpty(eventName))
			return;

		if(_notifies.ContainsKey(eventName))
		{
			for(int i = _notifies[eventName].Count - 1; i >= 0; i--)
			{
				NotificationEvent ev = _notifies[eventName][i];
				if(ev != null)
				   ev(data);
				else
					_notifies[eventName].RemoveAt(i);
			}
		}
	}
}
