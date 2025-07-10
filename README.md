# EventFlow

EventFlow is a production-grade **event-driven task management system** built using **.NET 9**, **Kafka**, and **SQL Server**, following clean architecture principles. It demonstrates the power and performance of **event sourcing** and **snapshot-based rehydration** in a distributed system.

> 📊 Performance-tested using **K6**, **Grafana**, and **InfluxDB** — with detailed observability metrics and load test dashboards.

---

## 🧩 Architecture Overview

EventFlow uses a **layered clean architecture** approach with the following projects:

- `API`: Exposes REST endpoints to interact with tasks (Create, Update, Complete).
- `Application`: Contains MediatR command/query handlers and business logic.
- `Domain`: Defines aggregates, entities, and encapsulates domain logic.
- `Infrastructure`: Handles Kafka, SQL persistence, and external dependencies.

### 🔁 Event Sourcing & Rehydration

The core domain logic is implemented through event sourcing:

- **Rehydration**: Aggregates are re-created by replaying events from an event stream.
- **Snapshots**: Periodic snapshots optimize performance by allowing rehydration from the last saved state.

Supported rehydration strategies:
- 🧠 *Full Event Replay*
- ⚡ *Snapshot + Partial Replay*

---

## ⚙️ Tech Stack

| Layer           | Tool / Library                        |
|----------------|----------------------------------------|
| Framework      | [.NET 9](https://dotnet.microsoft.com/)             |
| Messaging      | [Apache Kafka](https://kafka.apache.org/) via Docker |
| Persistence    | Entity Framework Core + SQL Server     |
| Mediation      | [MediatR](https://github.com/jbogard/MediatR)       |
| Resilience     | [Polly](https://github.com/App-vNext/Polly)         |
| Observability  | [K6](https://k6.io/), InfluxDB, Grafana |
| DevOps         | Docker, Docker Compose                 |

---

## 📦 Kafka Integration

- **Producer**: Publishes domain events when task operations occur (e.g., `TaskCreated`, `TaskCompleted`).
- **Consumer**: Background hosted service (`ReadModelConsumer`) processes and persists events for querying.

Both are isolated behind interfaces to allow easy replacement (e.g., Kafka → RabbitMQ).

---

## 🧠 Domain Modeling

- Aggregates are implemented as **immutable** objects.
- Domain events are used to represent all state changes.
- Rehydration via constructor ensures no bypassing of domain logic.

Key concepts:
- `TaskAggregate` is the central domain model.
- **Records** (C# 9+) are used for event immutability.
- Event replay and snapshot logic is encapsulated inside the aggregate.

---

## 📊 Load Testing & Performance

Load tests simulate real-world concurrent usage:
- Tasks being **created**, **updated**, and **completed** by multiple users.
- Performed using **K6 + InfluxDB + Grafana** stack.

K6 test result metrics:
- 📈 Average response time: ~114 ms
- 🧱 Max blocked duration: 89 ms
- ✅ No failed requests under full load

### K6 Testing Example

```bash
# Run with JSON output
k6 run --out json=result.json loadtest.js

# Run with InfluxDB for Grafana integration
k6 run --out influxdb=http://localhost:8086/k6 loadtest.js

docker run -d \
  --name influxdb \
  -p 8086:8086 \
  -e INFLUXDB_DB=k6 \
  -e INFLUXDB_ADMIN_USER=admin \
  -e INFLUXDB_ADMIN_PASSWORD=admin123 \
  influxdb:1.8

EventFlow/
├── API/                 # REST endpoints + DI
├── Application/         # MediatR handlers, interfaces
├── Domain/              # Aggregates, domain events
├── Infrastructure/      # Kafka, SQL Repos
└── docker-compose.yml   # Dev environment (Kafka, SQL Server)

