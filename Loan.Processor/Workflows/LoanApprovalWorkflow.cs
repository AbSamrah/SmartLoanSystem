using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Models;
using Loan.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
// ... other imports
using Elsa.Extensions; // Ensure this is present for GetInput

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
                        return $"Processing Loan {loan.LoanId} for Citizen {loan.CitizenId}. Amount: {loan.Amount:C}";
                    }),

                    new If
                    {
                        Condition = new(context => loanVariable.Get(context).Salary > 1000),

                        Then = new Sequence
                        {
                            Activities =
                            {
                                new WriteLine("Loan Approved! Salary is good.")
                            }
                        },

                        Else = new Sequence
                        {
                            Activities =
                            {
                                new WriteLine("Loan Rejected. Salary is too low.")
                            }
                        }
                    },

                    new WriteLine("Processing Finished.")
                }
            };
        }
    }
}
