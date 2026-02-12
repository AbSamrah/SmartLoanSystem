using Loan.Contracts;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Processor.Consumers
{
    public class LoanRequestConsumer : IConsumer<LoanRequested>
    {
        private readonly ILogger<LoanRequestConsumer> _logger;

        public LoanRequestConsumer(ILogger<LoanRequestConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<LoanRequested> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "[New Message] Loan Received: ID={LoanId}, Citizen={CitizenId}, Amount={Amount}",
                message.LoanId, message.CitizenId, message.Amount);

            return Task.CompletedTask;
        }
    }
}
