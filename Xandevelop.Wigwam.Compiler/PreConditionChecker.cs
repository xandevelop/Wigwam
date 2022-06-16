using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xandevelop.Wigwam.Ast;

namespace Xandevelop.Wigwam.Compiler
{
    public class PreConditionChecker
    {
        public bool PreConditionsMet(List<AstPreCondition> preConditions, Dictionary<string, string> currentConditions)
        {
            return preConditions.All(x => PreConditionMet(x, currentConditions));
        }

        public bool PreConditionMet(AstPreCondition preCondition, Dictionary<string, string> currentConditions)
        {
            string expected = preCondition.Value;
            string actual = currentConditions.ContainsKey(preCondition.Variable) ? currentConditions[preCondition.Variable] : null;

            switch (preCondition.Comparison)
            {
                case PreConditionComparisonType.Equals: return expected == actual;
                case PreConditionComparisonType.NotEquals: return expected != actual;
                case PreConditionComparisonType.Contains: return actual.Contains(expected);
                case PreConditionComparisonType.NotContains: return !actual.Contains(expected);
                case PreConditionComparisonType.Regex: return Regex.IsMatch(actual, expected);
                default: throw new NotImplementedException();
            }
        }
    }
}
