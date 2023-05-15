﻿using System;
using System.Collections.Generic;

public class CharacterAttribute
{
	public int templateID;
	public CharacterAttributeTemplate Template { get { return CharacterAttributeTemplate.Cache[templateID]; } }

	private int baseValue;
	private int modifier;
	private int finalValue;
	private Dictionary<string, CharacterAttribute> parents = new Dictionary<string, CharacterAttribute>();
	private Dictionary<string, CharacterAttribute> children = new Dictionary<string, CharacterAttribute>();
	private Dictionary<string, CharacterAttribute> dependencies = new Dictionary<string, CharacterAttribute>();

	public delegate void AttributeUpdated(CharacterAttribute item, uint applyTick);
	public event AttributeUpdated OnAttributeUpdated;

	protected virtual void Internal_OnAttributeChanged(CharacterAttribute item, uint applyTick)
	{
		OnAttributeUpdated?.Invoke(item, applyTick);
	}

	public int BaseValue { get { return baseValue; } }
	public void SetValue(int newValue, uint applyTick)
	{
		SetValue(newValue, false, applyTick);
	}

	public void SetValue(int newValue, bool skipUpdate, uint applyTick)
	{
		if (baseValue != newValue)
		{
			baseValue = newValue;
			if (!skipUpdate)
			{
				UpdateValues(true, applyTick);
			}
		}
	}
	/// <summary>
	/// Used to add or subtract an amount from the base value of the attribute. Addition: AddValue(123) | Subtraction: AddValue(-123)
	/// </summary>
	/// <param name="amount"></param>
	public void AddValue(int amount, uint applyTick)
	{
		AddValue(amount, false, applyTick);
	}
	public void AddValue(int amount, bool skipUpdate, uint applyTick)
	{
		int tmp = baseValue + amount;
		if (baseValue != tmp)
		{
			baseValue = tmp;
			if (!skipUpdate)
			{
				UpdateValues(true, applyTick);
			}
		}
	}
	public void SetModifier(int newValue)
	{
		if (modifier != newValue)
		{
			modifier = newValue;
			finalValue = CalculateFinalValue();
		}
	}
	public void AddModifier(int amount)
	{
		int tmp = modifier + amount;
		if (modifier != tmp)
		{
			modifier = tmp;
			finalValue = CalculateFinalValue();
		}
	}

	public int Modifier { get { return modifier; } }
	public int FinalValue { get { return finalValue; } }
	/// <summary>
	/// Returns the value as a float. 
	/// </summary>
	public float FinalValueAsFloat { get { return finalValue; } }
	/// <summary>
	/// Returns the value as a percentage instead. Value*0.01f
	/// </summary>
	public float FinalValueAsPct { get { return finalValue * 0.01f; } }

	public Dictionary<string, CharacterAttribute> Parents { get { return parents; } }
	public Dictionary<string, CharacterAttribute> Children { get { return children; } }
	public Dictionary<string, CharacterAttribute> Dependencies { get { return dependencies; } }

	public override string ToString()
	{
		return Template.Name + ": " + FinalValue;
	}

	public CharacterAttribute(int templateID, int initialValue, int initialModifier)
	{
		this.templateID = templateID;
		baseValue = initialValue;
		modifier = initialModifier;
		finalValue = CalculateFinalValue();
	}

	public void AddParent(CharacterAttribute parent)
	{
		if (!parents.ContainsKey(parent.Template.Name))
		{
			parents.Add(parent.Template.Name, parent);
		}
	}

	public void RemoveParent(CharacterAttribute parent)
	{
		parents.Remove(parent.Template.Name);
	}

	public void AddChild(CharacterAttribute child, uint applyTick)
	{
		if (!children.ContainsKey(child.Template.Name))
		{
			children.Add(child.Template.Name, child);
			child.AddParent(this);
			UpdateValues(applyTick);
		}
	}

	public void RemoveChild(CharacterAttribute child, uint applyTick)
	{
		children.Remove(child.Template.Name);
		child.RemoveParent(this);
		UpdateValues(applyTick);
	}

	public void AddDependant(CharacterAttribute dependency)
	{
		Type dependencyType = dependency.GetType();
		if (!dependencies.ContainsKey(dependencyType.Name))
		{
			dependencies.Add(dependencyType.Name, dependency);
		}
	}

	public void RemoveDependant(CharacterAttribute dependency)
	{
		dependencies.Remove(dependency.GetType().Name);
	}

	public CharacterAttribute GetDependant(string name)
	{
		CharacterAttribute result;
		dependencies.TryGetValue(name, out result);
		return result;
	}

	public int GetDependantBaseValue(string name)
	{
		CharacterAttribute attribute;
		return (!dependencies.TryGetValue(name, out attribute)) ? 0 : attribute.BaseValue;
	}

	public int GetDependantMinValue(string name)
	{
		CharacterAttribute attribute;
		return (!dependencies.TryGetValue(name, out attribute)) ? 0 : attribute.Template.MinValue;
	}

	public int GetDependantMaxValue(string name)
	{
		CharacterAttribute attribute;
		return (!dependencies.TryGetValue(name, out attribute)) ? 0 : attribute.Template.MaxValue;
	}

	public int GetDependantModifier(string name)
	{
		CharacterAttribute attribute;
		return (!dependencies.TryGetValue(name, out attribute)) ? 0 : attribute.Modifier;
	}

	public int GetDependantFinalValue(string name)
	{
		CharacterAttribute attribute;
		return (!dependencies.TryGetValue(name, out attribute)) ? 0 : attribute.FinalValue;
	}

	public void UpdateValues(uint applyTick)
	{
		UpdateValues(false, applyTick);
	}
	public void UpdateValues(bool forceUpdate, uint applyTick)
	{
		int oldFinalValue = finalValue;

		ApplyChildren(applyTick);

		if (forceUpdate || finalValue != oldFinalValue)
		{
			foreach (CharacterAttribute parent in parents.Values)
			{
				parent.UpdateValues(applyTick);
			}
		}
	}

	private void ApplyChildren(uint applyTick)
	{
		modifier = 0;
		if (Template.Formulas != null)
		{
			foreach (KeyValuePair<CharacterAttributeTemplate, CharacterAttributeFormulaTemplate> pair in Template.Formulas)
			{
				CharacterAttribute child;
				if (children.TryGetValue(pair.Key.Name, out child))
				{
					modifier += pair.Value.CalculateBonus(this, child);
				}
			}
		}
		finalValue = CalculateFinalValue();
		OnAttributeUpdated?.Invoke(this, applyTick);
	}

	private int CalculateFinalValue()
	{
		if (Template.ClampFinalValue)
		{
			return (baseValue + modifier).Clamp(Template.MinValue, Template.MaxValue);
		}
		return baseValue + modifier;
	}
}