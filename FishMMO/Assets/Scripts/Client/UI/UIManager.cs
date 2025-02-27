﻿using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace FishMMO.Client
{
	/// <summary>
	/// Helper class for our UI
	/// </summary>
	public static class UIManager
	{
		private static Dictionary<string, UIControl> controls = new Dictionary<string, UIControl>();

		internal static void Register(UIControl control)
		{
			if (control == null)
			{
				return;
			}
			if (controls.ContainsKey(control.Name))
			{
				return;
			}
			//Debug.Log("UIManager: Registered " + control.Name);
			controls.Add(control.Name, control);
		}

		internal static void Unregister(UIControl control)
		{
			if (control == null)
			{
				return;
			}
			else
			{
				//Debug.Log("UIManager: Unregistered " + control.Name);
				controls.Remove(control.Name);
			}
		}

		public static bool TryGet<T>(string name, out T control) where T : UIControl
		{
			if (controls.TryGetValue(name, out UIControl result))
			{
				if ((control = result as T) != null)
				{
					return true;
				}
			}
			control = null;
			return false;
		}

		public static bool Exists(string name)
		{
			if (controls.ContainsKey(name))
			{
				return true;
			}
			return false;
		}

		public static void Show(string name)
		{
			UIControl result = null;
			if (controls.TryGetValue(name, out result))
			{
				result.OnShow();
			}
		}

		public static void Hide(string name)
		{
			UIControl result = null;
			if (controls.TryGetValue(name, out result) && result.visible)
			{
				result.OnHide();
			}
		}

		public static void HideAll()
		{
			foreach (KeyValuePair<string, UIControl> p in controls)
			{
				p.Value.OnHide();
			}
		}

		public static void ShowAll()
		{
			foreach (KeyValuePair<string, UIControl> p in controls)
			{
				p.Value.OnShow();
			}
		}

		public static bool ControlHasFocus()
		{
			if (EventSystem.current.currentSelectedGameObject != null)
			{
				return true;
			}
			foreach (UIControl control in controls.Values)
			{
				if (control.visible && control.hasFocus)
				{
					return true;
				}
			}
			return false;
		}

		public static bool InputControlHasFocus()
		{
			foreach (UIControl control in controls.Values)
			{
				if (control.visible && control.inputField != null && control.inputField.isFocused)
				{
					return true;
				}
			}
			return false;
		}
	}
}