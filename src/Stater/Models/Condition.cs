using System;
using System.Xml.Serialization;

namespace Stater.Models;

[XmlInclude(typeof(VariableCondition))]
[Serializable]
public abstract record Condition
{
    public record VariableCondition(
        Guid VariableGuid,
        VariableCondition.ConditionTypeEnum ConditionType,
        VariableValue Value
    ) : Condition
    {
        public enum ConditionTypeEnum
        {
            Lt, // <
            Le, // <=
            Eq, // ==
            Ne, // !=
            Gt, // >
            Ge, // >=
        }

        public VariableCondition() : this(Guid.Empty, ConditionTypeEnum.Lt, null)
        {
        }
    }
}