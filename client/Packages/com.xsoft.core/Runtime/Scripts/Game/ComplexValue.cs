using System;
using System.Collections.Generic;
using System.Text;
using Layout.LayoutEffects;
using Proto;

namespace GameLogic.Game
{
	public class ValueChanageEventArgs : EventArgs
	{
		public int OldValue { private set; get; }
		public int NewValue { private set; get; }
		public int FinalValue { set; get; }

		public ValueChanageEventArgs(int oldValue, int newValue, int finalValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
			FinalValue = finalValue;
		}
	}

	public sealed class ComplexValue
	{
		public int Max { set; get; } = int.MaxValue;

		public ComplexValue() : this(0, 0, 0)
		{

		}

		public ComplexValue(int baseValue, int appendValue, int rate)
		{
			BaseValue = baseValue;
			AppendValue = appendValue;
			Rate = rate;
		}
		public int BaseValue { private set; get; }
		public int AppendValue { private set; get; }
		public int Rate { private set; get; }
		public void SetBaseValue(int value)
		{
			if (OnBaseValueChange != null)
			{
				var args = new ValueChanageEventArgs(BaseValue, value, value);
				OnBaseValueChange(this, args);
				BaseValue = args.FinalValue;
				OnValueChange(this, new EventArgs());
			}
			else
			{
				BaseValue = value;
			}
		}
		public void SetAppendValue(int value)
		{
			if (OnAppendValueChange != null)
			{
				var args = new ValueChanageEventArgs(AppendValue, value, value);
				OnAppendValueChange(this, args);
				AppendValue = args.FinalValue;
				OnValueChange(this, new EventArgs());
			}
			else
			{
				AppendValue = value;
			}
		}
		public void SetRate(int value)
		{
			if (OnRateChange != null)
			{
				var args = new ValueChanageEventArgs(Rate, value, value);
				OnRateChange(this, args);
				Rate = args.FinalValue;
				OnValueChange(this, new EventArgs());
			}
			else
			{
				Rate = value;
			}
		}
		public int FinalValue
		{
			get
			{
				float value = (float)(BaseValue + AppendValue) * (1 + ((float)Rate / 10000f));
				return (int)value;
			}
		}

		public EventHandler<ValueChanageEventArgs> OnBaseValueChange;
		public EventHandler<ValueChanageEventArgs> OnAppendValueChange;
		public EventHandler<ValueChanageEventArgs> OnRateChange;
		public EventHandler<EventArgs> OnValueChange;

		static public implicit operator ComplexValue(int value)
		{
			return new ComplexValue(value, 0, 0);
		}

		static public implicit operator int(ComplexValue value)
		{
			return value.FinalValue;
		}

		static public bool operator ==(ComplexValue r, ComplexValue l)
		{
			return r.FinalValue == l.FinalValue;
		}

		static public bool operator !=(ComplexValue r, ComplexValue l)
		{
			return r.FinalValue != l.FinalValue;
		}

		public override int GetHashCode()
		{
			return FinalValue.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ComplexValue))
				return false;
			var temp = obj as ComplexValue;
			return temp.FinalValue == this.FinalValue;
		}

		public void ModifyValueAdd(AddType addType, float add)
		{
			switch (addType)
			{
				case AddType.Append:
					{
						SetAppendValue((int)add + AppendValue);
					}
					break;
				case AddType.Base:
					{
						SetBaseValue((int)add+BaseValue);
					}
					break;
				case AddType.Rate:
					{
						SetRate((int)add+Rate);
					}
					break;
			}
		}

		public void ModifyValue(AddType addType, float resultValue)
		{
			switch (addType)
			{
				case AddType.Append:
					{
						SetAppendValue((int)resultValue);
					}
					break;
				case AddType.Base:
					{
						SetBaseValue((int)resultValue);
					}
					break;
				case AddType.Rate:
					{
						SetRate((int)resultValue);
					}
					break;
			}
		}

        internal void ModifyValueMinutes(AddType miType, float mi)
        {
            
			switch (miType)
			{
				case AddType.Append:
					{
						SetAppendValue( AppendValue-(int)mi);
					}
					break;
				case AddType.Base:
					{
						SetBaseValue(BaseValue - (int)mi);
					}
					break;
				case AddType.Rate:
					{
						SetRate( Rate - (int)mi);
					}
					break;
			}
		}

        public override string ToString()
        {
			//var str = string.Empty;

			if (Rate > 0)
			{
				var basev = (int)(BaseValue * (1 + Rate / 10000f));
				if (AppendValue > 0)
				{
					var appv = (int)(AppendValue * (1 + Rate / 10000f));
					return $"{basev}+{appv}";
				}
				else {
					return $"{basev}";
				}
			}
			else
			{
				if (AppendValue > 0)
				{
					return $"{BaseValue}+{AppendValue}";

				}
				else {
					return $"{BaseValue}";
                }

			}
			
        }
    }
}

