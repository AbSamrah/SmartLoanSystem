using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Contracts;

public record LoanRequested(
   Guid LoanId,
   string CitizenId,
   decimal Amount,
   decimal Salary
);
