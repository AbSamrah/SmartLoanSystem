using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Processor.Constants
{
    public class LoanRules
    {
        public const string FilePath = "Rules/loan-approval.dmn";

        public const string DecisionId = "approve-loan";

        public static class Inputs
        {
            public const string Salary = "Salary";
        }

        public static class Outputs
        {
            public const string IsApproved = "IsApproved";
        }
    }
}
