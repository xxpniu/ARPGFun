using System;
using System.Collections.Generic;
using System.Text;
using Layout.LayoutEffects;
using org.apache.zookeeper.data;
using Proto;
using XNet.Libs.Utility;

namespace GameLogic.Game
{
	public class ValueChangeEventArgs : EventArgs
	{
		public int OldValue { private set; get; }
		public int NewValue { private set; get; }
		public int FinalValue { set; get; }

		public ValueChangeEventArgs(int oldValue, int newValue, int finalValue)
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
				var args = new ValueChangeEventArgs(BaseValue, value, value);
				OnBaseValueChange(this, args);
				BaseValue = args.FinalValue;
				OnValueChange(this, EventArgs.Empty);
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
				var args = new ValueChangeEventArgs(AppendValue, value, value);
				OnAppendValueChange(this, args);
				AppendValue = args.FinalValue;
				OnValueChange(this, EventArgs.Empty);
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
				var args = new ValueChangeEventArgs(Rate, value, value);
				OnRateChange(this, args);
				Rate = args.FinalValue;
				OnValueChange(this, EventArgs.Empty);
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
				var value = (float)(BaseValue + AppendValue) * (1 + ((float)Rate / 10000f));
				return (int)value;
			}
		}

		public EventHandler<ValueChangeEventArgs> OnBaseValueChange ;
		public EventHandler<ValueChangeEventArgs> OnAppendValueChange;
		public EventHandler<ValueChangeEventArgs> OnRateChange;
		public EventHandler<EventArgs> OnValueChange;

		public static implicit operator ComplexValue(int value)
		{
			return new ComplexValue(value, 0, 0);
		}

		public static implicit operator int(ComplexValue value)
		{
			return value.FinalValue;
		}

		public static bool operator ==(ComplexValue r, ComplexValue l)
		{
			if (r == null || l == null) return false;
			return r.FinalValue == l.FinalValue;
		}

		public static bool operator !=(ComplexValue r, ComplexValue l)
		{
			if (r == null || l == null) return true;
			return r.FinalValue != l.FinalValue;
		}

		public override int GetHashCode()
		{
			return FinalValue.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is not ComplexValue complex)
				return false;
			return complex.FinalValue == this.FinalValue;
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
				default:
					//throw new ArgumentOutOfRangeException(nameof(addType), addType, null);
					Debuger.LogError($"ArgumentOutOfRangeException{nameof(addType)}");
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
	        if (Rate <= 0) return AppendValue > 0 ? $"{BaseValue}+{AppendValue}" : $"{BaseValue}";
	        var bVal = (int)(BaseValue * (1 + Rate / 10000f));
			if (AppendValue <= 0) return $"{bVal}";
			var aVal = (int)(AppendValue * (1 + Rate / 10000f));
			return $"{bVal}+{aVal}";
        }
    }
}

