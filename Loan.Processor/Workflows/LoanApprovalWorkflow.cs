using Elsa.Extensions; 
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Models;
using Loan.Contracts;
using Loan.Processor.Activities;
using Loan.Processor.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Processor.Workflows
{
    public class LoanApprovalWorkflow : WorkflowBase
    {
        public Input<LoanRequested> LoanRequestInput { get; set; } = default!;

        protected override void Build(IWorkflowBuilder builder)
        {
            builder.Name = "LoanApprovalWorkflow";
            builder.DefinitionId = nameof(LoanApprovalWorkflow);

            var loanVariable = builder.WithVariable<LoanRequested>();
            var isApprovedVariable = builder.WithVariable<bool>();

            builder.Root = new Sequence
            {
                Activities =
                {
                    new SetVariable
                    {
                        Variable = loanVariable,
                        Value = new(context => context.GetInput<LoanRequested>(nameof(LoanRequestInput)))
                    },

                    new WriteLine(ctx =>
                    {
                        var loan = loanVariable.Get(ctx);
                        return $"Processing Loan {loan!.LoanId} for Citizen {loan.CitizenId}. Amount: {loan.Amount:C}";
                    }),


                    new ExecuteDmn
                    {
                        DmnFilePath = new(LoanRules.FilePath),

                        DecisionId = new(LoanRules.DecisionId),

                        ResultColumnName = new(LoanRules.Outputs.IsApproved),

                        DmnInputs = new(context => new Dictionary<string, object>
                        {
                            { LoanRules.Inputs.Salary, loanVariable!.Get(context)!.Salary }
                        }),

                        

                        Result = new Output<bool>(isApprovedVariable)
                    },

                    new If
                    {
                        Condition = new(context => isApprovedVariable.Get(context)),

                        Then = new Sequence
                        {
                            Activities =
                            {
                                new WriteLine("Loan Approved by DMN Logic!"),
                            }
                        },

                        Else = new Sequence
                        {
                            Activities =
                            {
                                new WriteLine("Loan Rejected by DMN Logic."),
                            }
                        }
                    },



                    new WriteLine("Processing Finished.")
                }
            };
        }
    }
}
