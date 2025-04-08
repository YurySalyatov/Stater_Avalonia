using System;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace Stater.Models;

public record Variable(
    Guid Guid,
    string Name,
    VariableValue StartValue
)
{
    public Variable() : this(
        Guid.NewGuid(),
        "Variable",
        VariableValueBuilder.fromString("0")
    )
    {
    }
}

public class VariableOperatorException(string message) : Exception(message);

public class VariableMathException(string message) : Exception(message);

public static class VariableValueBuilder
{
    public static VariableValue fromString(string input)
    {
        switch (input)
        {
            case "true":
                return new VariableValue.BoolVariable(true);
            case "false":
                return new VariableValue.BoolVariable(false);
        }

        try
        {
            return new VariableValue.FloatVariable(float.Parse(input));
        }
        catch (Exception _)
        {
            // ignored
        }

        try
        {
            return new VariableValue.IntVariable(int.Parse(input));
        }
        catch (Exception _)
        {
            // ignored
        }

        return new VariableValue.StringVariable(input);
    }
}

[XmlInclude(typeof(IntVariable))]
[XmlInclude(typeof(BoolVariable))]
[XmlInclude(typeof(FloatVariable))]
[XmlInclude(typeof(StringVariable))]
[Serializable]
public abstract record VariableValue
{
    public record IntVariable(int Value) : VariableValue
    {
        public IntVariable() : this(0)
        {
        }

        public override string ToString() => Value.ToString();

        protected override VariableValue Default() => new IntVariable(0);

        protected override bool Greater(VariableValue otherVariableValue) => otherVariableValue switch
        {
            IntVariable v => Value > v.Value,
            FloatVariable v => Value > v.Value,
            _ => throw new VariableOperatorException("Integer must compare only with integer or float")
        };

        protected override VariableValue Sum(VariableValue otherVariableValue) => otherVariableValue switch
        {
            IntVariable v => new IntVariable(Value + v.Value),
            FloatVariable v => new IntVariable(Value + (int)v.Value),
            _ => throw new VariableMathException("Integer must sum only with integer or float")
        };

        protected override VariableValue Sub(VariableValue otherVariableValue) => otherVariableValue switch
        {
            IntVariable v => new IntVariable(Value - v.Value),
            FloatVariable v => new IntVariable(Value - (int)v.Value),
            _ => throw new VariableMathException("Integer must sub only with integer or float")
        };

        protected override VariableValue Mul(VariableValue otherVariableValue) => otherVariableValue switch
        {
            IntVariable v => new IntVariable(Value * v.Value),
            FloatVariable v => new IntVariable(Value * (int)v.Value),
            StringVariable v => new StringVariable(string.Concat(Enumerable.Repeat(v.Value, Value))),
            _ => throw new VariableMathException("Integer must mul only with integer, float or string")
        };

        protected override VariableValue Div(VariableValue otherVariableValue) => otherVariableValue switch
        {
            IntVariable v => new IntVariable(Value / v.Value),
            FloatVariable v => new IntVariable(Value / (int)v.Value),
            _ => throw new VariableMathException("Integer must div only with integer or float")
        };
    };

    public record BoolVariable(bool Value) : VariableValue
    {
        public BoolVariable() : this(false)
        {
        }
        
        public override string ToString() => Value.ToString();
        protected override VariableValue Default() => new BoolVariable(false);

        protected override bool Greater(VariableValue otherVariableValue)
        {
            throw new VariableOperatorException("Bool variables not support operations: >, <, <=, >=");
        }

        protected override VariableValue Sum(VariableValue otherVariableValue)
        {
            throw new VariableOperatorException("Bool variables not support operations: +, -, *, /");
        }

        protected override VariableValue Sub(VariableValue otherVariableValue)
        {
            throw new VariableOperatorException("Bool variables not support operations: +, -, *, /");
        }

        protected override VariableValue Mul(VariableValue otherVariableValue)
        {
            throw new VariableOperatorException("Bool variables not support operations: +, -, *, /");
        }

        protected override VariableValue Div(VariableValue otherVariableValue)
        {
            throw new VariableOperatorException("Bool variables not support operations: +, -, *, /");
        }
    }

    public record FloatVariable(float Value) : VariableValue
    {
        public FloatVariable() : this(0f)
        {
        }
        
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        protected override VariableValue Default() => new FloatVariable(0);

        protected override bool Greater(VariableValue otherVariableValue) => otherVariableValue switch
        {
            FloatVariable v => Value > v.Value,
            IntVariable v => Value > v.Value,
            _ => throw new VariableOperatorException("Float must compare only with integer or float")
        };

        protected override VariableValue Sum(VariableValue otherVariableValue) => otherVariableValue switch
        {
            IntVariable v => new FloatVariable(Value + v.Value),
            FloatVariable v => new FloatVariable(Value + v.Value),
            _ => throw new VariableMathException("Float must sum only with integer or float")
        };

        protected override VariableValue Sub(VariableValue otherVariableValue) => otherVariableValue switch
        {
            IntVariable v => new FloatVariable(Value - v.Value),
            FloatVariable v => new FloatVariable(Value - v.Value),
            _ => throw new VariableMathException("Float must sub only with integer or float")
        };

        protected override VariableValue Mul(VariableValue otherVariableValue) => otherVariableValue switch
        {
            IntVariable v => new FloatVariable(Value * v.Value),
            FloatVariable v => new FloatVariable(Value * v.Value),
            _ => throw new VariableMathException("Float must mul only with integer or float")
        };

        protected override VariableValue Div(VariableValue otherVariableValue) => otherVariableValue switch
        {
            IntVariable v => new FloatVariable(Value / v.Value),
            FloatVariable v => new FloatVariable(Value / v.Value),
            _ => throw new VariableMathException("Float must div only with integer or float")
        };
    };

    public record StringVariable(string Value) : VariableValue
    {
        public StringVariable() : this("")
        {
        }
        
        public override string ToString() => Value;
        protected override VariableValue Default() => new StringVariable("");

        protected override bool Greater(VariableValue otherVariableValue) => otherVariableValue switch
        {
            StringVariable v => string.CompareOrdinal(Value, v.Value) > 0,
            _ => throw new VariableOperatorException("String must compare only with string")
        };

        protected override VariableValue Sum(VariableValue otherVariableValue) => otherVariableValue switch
        {
            StringVariable v => new StringVariable(Value + v.Value),
            _ => throw new VariableMathException("String must sum only with string")
        };

        protected override VariableValue Sub(VariableValue otherVariableValue)
        {
            throw new VariableOperatorException("String variables not support operations: +, -, *, /");
        }

        protected override VariableValue Mul(VariableValue otherVariableValue) => otherVariableValue switch
        {
            IntVariable v => new StringVariable(string.Concat(Enumerable.Repeat(Value, v.Value))),
            _ => throw new VariableMathException("String must sum only with string")
        };

        protected override VariableValue Div(VariableValue otherVariableValue)
        {
            throw new VariableOperatorException("String variables not support operations /");
        }
    };

    protected abstract VariableValue Default();

    protected abstract bool Greater(VariableValue otherVariableValue); // >

    public static bool operator >(VariableValue a, VariableValue b) => a.Greater(b);
    public static bool operator >=(VariableValue a, VariableValue b) => a.Greater(b) || a == b;
    public static bool operator <(VariableValue a, VariableValue b) => !(a.Greater(b) || a == b);
    public static bool operator <=(VariableValue a, VariableValue b) => !a.Greater(b);


    protected abstract VariableValue Sum(VariableValue otherVariableValue);
    protected abstract VariableValue Sub(VariableValue otherVariableValue);
    protected abstract VariableValue Mul(VariableValue otherVariableValue);
    protected abstract VariableValue Div(VariableValue otherVariableValue);

    public static VariableValue operator +(VariableValue a, VariableValue b) => a.Sum(b);
    public static VariableValue operator -(VariableValue a, VariableValue b) => a.Sub(b);
    public static VariableValue operator *(VariableValue a, VariableValue b) => a.Mul(b);
    public static VariableValue operator /(VariableValue a, VariableValue b) => a.Div(b);

    public override string ToString()
    {
        return base.ToString();
    }
}