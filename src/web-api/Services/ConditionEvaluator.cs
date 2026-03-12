using System.Text.RegularExpressions;
using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public sealed class ConditionEvaluator
{
    public bool Evaluate(IEnumerable<Condition> conditions, TriggerContext context)
    {
        foreach (var condition in conditions)
        {
            var fieldValue = context.Fields.TryGetValue(condition.Field, out var v) ? v : string.Empty;
            if (!EvaluateOne(condition.Operator, fieldValue, condition.Value))
                return false;
        }
        return true;
    }

    private static bool EvaluateOne(string op, string fieldValue, string conditionValue) =>
        op switch
        {
            "equals"       => string.Equals(fieldValue, conditionValue, StringComparison.OrdinalIgnoreCase),
            "not_equals"   => !string.Equals(fieldValue, conditionValue, StringComparison.OrdinalIgnoreCase),
            "contains"     => fieldValue.Contains(conditionValue, StringComparison.OrdinalIgnoreCase),
            "not_contains" => !fieldValue.Contains(conditionValue, StringComparison.OrdinalIgnoreCase),
            "starts_with"  => fieldValue.StartsWith(conditionValue, StringComparison.OrdinalIgnoreCase),
            "ends_with"    => fieldValue.EndsWith(conditionValue, StringComparison.OrdinalIgnoreCase),
            "gt"           => TryParseDouble(fieldValue, out var a) && TryParseDouble(conditionValue, out var b) && a > b,
            "lt"           => TryParseDouble(fieldValue, out var a) && TryParseDouble(conditionValue, out var b) && a < b,
            "gte"          => TryParseDouble(fieldValue, out var a) && TryParseDouble(conditionValue, out var b) && a >= b,
            "lte"          => TryParseDouble(fieldValue, out var a) && TryParseDouble(conditionValue, out var b) && a <= b,
            "regex"        => TryRegex(fieldValue, conditionValue),
            _              => false,
        };

    private static bool TryParseDouble(string s, out double result) =>
        double.TryParse(s, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out result);

    private static bool TryRegex(string input, string pattern)
    {
        try { return Regex.IsMatch(input, pattern); }
        catch { return false; }
    }
}
