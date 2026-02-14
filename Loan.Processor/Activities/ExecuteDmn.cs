using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using net.adamec.lib.common.dmn.engine.parser;
using net.adamec.lib.common.dmn.engine.engine.execution.context;
using System;
using System.Collections.Generic;
using System.Text;
using Loan.Processor.Constants;

namespace Loan.Processor.Activities
{
    public class ExecuteDmn : CodeActivity<bool>
    {
        [Input]
        public Input<string> DmnFilePath { get; set; } = default!;

        [Input]
        public Input<string> DecisionId { get; set; } = default!;

        [Input]
        public Input<IDictionary<string, object>> DmnInputs { get; set; } = default!;

        [Input]
        public Input<string> ResultColumnName { get; set; } = default!;

        protected override void Execute(ActivityExecutionContext context)
        {
            var path = DmnFilePath.Get(context);
            var decisionId = DecisionId.Get(context);
            var inputs = DmnInputs.Get(context);
            var targetColumn = ResultColumnName.Get(context);

            var def = DmnParser.Parse(path);
            var dmnCtx = DmnExecutionContextFactory.CreateExecutionContext(def);

            foreach (var input in inputs)
            {
                dmnCtx.WithInputParameter(input.Key, input.Value);
            }

            var dmnResult = dmnCtx.ExecuteDecision(decisionId);

            var resultRow = dmnResult.Results.FirstOrDefault();

            if (resultRow != null)
            {
                var outputValue = resultRow.Variables.FirstOrDefault(v => v.Name == targetColumn);

                if (outputValue != null && outputValue.Value is bool resultBool)
                {
                    context.SetResult(resultBool);
                    return;
                }
            }

            context.SetResult(false);
        }
    }
}
