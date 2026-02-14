using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using Elsa.Workflows.Runtime.Parameters;
using Loan.Contracts;
using Loan.Processor.Workflows;
using MassTransit;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Processor.Consumers
{
    public class LoanRequestConsumer : IConsumer<LoanRequested>
    {
        private readonly ILogger<LoanRequestConsumer> _logger;
        private readonly IWorkflowRuntime _workflowRuntime;
        private readonly IDistributedCache _cache;

        public LoanRequestConsumer(ILogger<LoanRequestConsumer> logger, IWorkflowRuntime workflowRuntime, IDistributedCache distributedCache)
        {
            _logger = logger;
            _workflowRuntime = workflowRuntime;
            _cache = distributedCache;
        }

        public async Task Consume(ConsumeContext<LoanRequested> context)
        {
            var message = context.Message;
            string cacheKey = $"processed-loan:{message.LoanId}";

            var cachedValue = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogWarning("Duplicate Request Detected! Loan {LoanId} was already processed.", message.LoanId);
                return; 
            }

            _logger.LogInformation(
                "[New Message] Loan Received: ID={LoanId}, Citizen={CitizenId}, Amount={Amount}",
                message.LoanId, message.CitizenId, message.Amount);

            await _cache.SetStringAsync(
                cacheKey,
                "Processed",
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            var client = await _workflowRuntime.CreateClientAsync();

            var request = new CreateAndRunWorkflowInstanceRequest
            {
                WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId(nameof(LoanApprovalWorkflow)),
                Input = new Dictionary<string, object>
                {
                    { nameof(LoanApprovalWorkflow.LoanRequestInput), message }
                },
                CorrelationId = message.LoanId.ToString()
            };

            await client.CreateAndRunInstanceAsync(request);

            _logger.LogInformation("Dispatched workflow for Loan {LoanId}", message.LoanId);
        }
    }
}
