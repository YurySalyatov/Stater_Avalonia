using System;
using System.Collections.Generic;
using System.Linq;

namespace Stater.Models;

public static class ConditionLogic
{
    private static VariableValue GetValueByVariable(Guid Guid, IDictionary<Guid, VariableValue> variables)
    {
        var variable = variables.Keys.First(v => v == Guid);
        return variables[variable];
    }

    public static bool Evaluate(Condition condition, IDictionary<Guid, VariableValue> variables)
    {
        return condition switch
        {
            Condition.VariableCondition cond => EvaluateVariableCondition(cond, variables),
            _ => false
        };
    }

    private static bool EvaluateVariableCondition(
        Condition.VariableCondition condition,
        IDictionary<Guid, VariableValue> variables
    )
    {
        var variableValue = GetValueByVariable(condition.VariableGuid, variables);
        return condition.ConditionType switch
        {
            Condition.VariableCondition.ConditionTypeEnum.Lt => variableValue < condition.Value,
            Condition.VariableCondition.ConditionTypeEnum.Le => variableValue <= condition.Value,
            Condition.VariableCondition.ConditionTypeEnum.Eq => variableValue == condition.Value,
            Condition.VariableCondition.ConditionTypeEnum.Ne => variableValue != condition.Value,
            Condition.VariableCondition.ConditionTypeEnum.Gt => variableValue > condition.Value,
            Condition.VariableCondition.ConditionTypeEnum.Ge => variableValue >= condition.Value,
            _ => false
        };
    }
}