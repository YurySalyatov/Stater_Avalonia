using System;
using System.Collections.Generic;
using System.Linq;

namespace Stater.Models;

public class EventLogic
{
    private static VariableValue GetValueByVariable(Guid guid, IDictionary<Guid, VariableValue> variables) =>
        variables[variables.Keys.First(v => v == guid)];

    public static IDictionary<Guid, VariableValue> Evaluate(Event eEvent, IDictionary<Guid, VariableValue> variables)
    {
        return eEvent switch
        {
            Event.VariableMath e => EvaluateVariableMath(e, variables),
            _ => variables
        };
    }

    private static IDictionary<Guid, VariableValue> EvaluateVariableMath(
        Event.VariableMath eEvent,
        IDictionary<Guid, VariableValue> variables
    )
    {
        var variableValue = GetValueByVariable(eEvent.VariableGuid, variables);
        _ = eEvent.MathType switch
        {
            Event.VariableMath.MathTypeEnum.Sum => variables[eEvent.VariableGuid] = variableValue + eEvent.Value,
            Event.VariableMath.MathTypeEnum.Sub => variables[eEvent.VariableGuid] = variableValue - eEvent.Value,
            Event.VariableMath.MathTypeEnum.Mul => variables[eEvent.VariableGuid] = variableValue * eEvent.Value,
            Event.VariableMath.MathTypeEnum.Div => variables[eEvent.VariableGuid] = variableValue / eEvent.Value,
            _ => null
        };
        return variables;
    }
}