using FishNet.Managing.Timing;

public class CharacterResourceAttribute : CharacterAttribute
{
	private int currentValue;

	public int CurrentValue { get { return currentValue; } }

	public override string ToString()
	{
		return Template.Name + ": " + currentValue + "/" + FinalValue;
	}

	public CharacterResourceAttribute(int templateID, int initialValue, int currentValue, int modifier) : base(templateID, initialValue, modifier)
	{
		this.currentValue = currentValue;
	}

	public void AddToCurrentValue(int value, uint applyTick)
	{
		int tmp = currentValue;
		currentValue += value;
		if (currentValue == tmp)
		{
			return;
		}
		if (currentValue > this.FinalValue)
		{
			currentValue = this.FinalValue;
		}
		Internal_OnAttributeChanged(this, applyTick);
	}

	public void SetCurrentValue(int value, uint applyTick)
	{
		currentValue = value;
		Internal_OnAttributeChanged(this, applyTick);
	}

	public void Consume(int amount, uint applyTick)
	{
		currentValue -= amount;
		if (currentValue < 0)
		{
			currentValue = 0;
		}
		Internal_OnAttributeChanged(this, applyTick);
	}

	public void Gain(int amount, uint applyTick)
	{
		currentValue += amount;
		if (currentValue >= FinalValue)
		{
			currentValue = FinalValue;
		}
		Internal_OnAttributeChanged(this, applyTick);
	}

	protected override void Internal_OnAttributeChanged(CharacterAttribute attribute, uint applyTick)
	{
		base.Internal_OnAttributeChanged(attribute, applyTick);
	}
}