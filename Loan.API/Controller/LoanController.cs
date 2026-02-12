using Loan.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Loan.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public LoanController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitLoanRequest(LoanRequestDto request)
        {
            var message = new LoanRequested(
                Guid.NewGuid(),
                request.CitizenId,
                request.Amount,
                request.Salary
            );

            await _publishEndpoint.Publish(message);

            return Accepted(new
            {
                RequestId = message.LoanId,
                Status = "Pending",
                Message = "Your task has been recieved successfully."
            });
        }
    }
    public record LoanRequestDto(string CitizenId, decimal Amount, decimal Salary);
}
