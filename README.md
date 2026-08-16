# CareerProject

საბაკალავრო პროექტი — კარიერული პლატფორმა კანდიდატებისა და კომპანიებისთვის, AI-ზე დაფუძნებული ვაკანსია-კანდიდატის მატჩინგითა და RAG ჩატით.

## სტრუქტურა

```text
backend/
├── CareerProject.ApiGateway/          # ერთადერთი entry point frontend-ისთვის
├── CareerProject.UserService/         # auth, users, profiles, skills
├── CareerProject.JobService/          # companies, jobs, applications
├── CareerProject.RecommendationService/ # embeddings, hybrid matching, AI chat
├── CareerProject.NotificationService/ # notifications
└── CareerProject.Shared/              # საერთო event contracts და utilities
frontend/
└── career-project-web/                 # Angular აპლიკაცია
docker/
│   └── postgres/init.sql               # ქმნის pgvector extension-ს
docker-compose.yml
.env.example
```

## გეგმა

დეტალური ეტაპობრივი გეგმა იხილეთ [PLAN.md](PLAN.md)-ში.

## გაშვება (development)

### 1. Infrastructure (PostgreSQL + pgvector, Redis, RabbitMQ)

დაგჭირდება [Docker Desktop](https://www.docker.com/products/docker-desktop/).

```bash
cp .env.example .env
# გახსენი .env და შეცვალე POSTGRES_PASSWORD და RABBITMQ_PASSWORD

docker compose up -d
```

**შემოწმება:**

```bash
docker compose ps
# ყველა service-ს "healthy" სტატუსი უნდა ჰქონდეს

docker exec -it careerproject-postgres psql -U career_project -d career_project_db -c "\dx"
# ჩამონათვალში უნდა გამოჩნდეს "vector" extension
```

RabbitMQ Management UI: [http://localhost:15672](http://localhost:15672) (login — `.env`-ში მითითებული `RABBITMQ_USER` / `RABBITMQ_PASSWORD`)

გაჩერება: `docker compose down` (მონაცემების წასაშლელადაც: `docker compose down -v`)

### 2. Backend

```bash
cd backend
dotnet build
```

### 3. Frontend

```bash
cd frontend/career-project-web
npm install
npm start
```
