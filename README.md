# SmartLoanSystem
SmartLoanSystem is a robust, distributed backend architecture designed to handle loan applications through automated workflows and decoupled business rules. This project demonstrates advanced software engineering patterns, including Event-Driven Architecture (EDA), Workflow Orchestration, and the Decision Model and Notation (DMN) standard.

# 🏗️ Architecture & Design
The system is built on SOLID principles to ensure maintainability and extensibility:

- **Event-Driven Communication:** Services communicate asynchronously using **MassTransit** and **RabbitMQ**, ensuring high availability and loose coupling.

- **Workflow Orchestration:** Loan processing is managed by Elsa Workflows 3.0, allowing for long-running processes and complex state management.

- **Externalized Rules (DMN):** Loan approval logic is decoupled from the application code using a DMN engine. This allows business analysts to modify rules (e.g., salary thresholds) without redeploying services.

- **Idempotency & Performance:** Implements **Redis** caching to detect and prevent duplicate loan requests, ensuring system reliability.

# 🚀 Tech Stack
- **Framework:** .NET 10.0 (C#)

- **Orchestration:** Elsa Workflows 3.5

- **Messaging:** MassTransit with RabbitMQ

- **Business Rules:** DMN (Decision Model and Notation)

- **Database:** SQL Server (Entity Framework Core)

- **Caching:** Redis (StackExchange.Redis)

- **Containerization:** Docker Compose

# 📂 Project Structure
- **Loan.API:** The gateway for submitting loan requests via REST endpoints.

- **Loan.Processor:** A worker service that consumes messages, executes the ```LoanApprovalWorkflow```, and evaluates DMN rules.

- **Loan.Contracts:** Shared library containing records and message schemas to ensure type safety across services.

# 🛠️ Business Logic (DMN)
The system currently uses a DMN table to evaluate loan eligibility based on the applicant's salary:

- **Input:** ```Salary```

- **Rule:** If ```Salary > 1000```, the loan is marked as ```Approved```.

- **Rule:** If ```Salary <= 1000```, the loan is marked as ```Rejected```.

# 🚦 Getting Started
## Prerequisites
- Docker Desktop

- .NET 10 SDK

## Setup
1. **Clone the repository:**

```Bash
git clone https://github.com/absamrah/SmartLoanSystem.git
```
2. **Configure Environment:** Create a ```.env``` file in the root directory (refer to ```.env.example```):

```Code snippet
SQL_PASSWORD=YourStrongPassword!
RABBIT_USER=guest
RABBIT_PASS=guest
```
3. **Run Infrastructure:** Use Docker Compose to spin up SQL Server, RabbitMQ, and Redis:

```Bash
docker-compose up -d
```
4. **Run the Services:** Start both the API and the Processor via your IDE or CLI.
