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

### სწრაფი გაშვება — ერთი ბრძანებით

`.env`-ის ერთხელ მომზადების შემდეგ (იხ. ქვემოთ, "1. Infrastructure"), მთელი გარემო — Docker infra + 5 backend service + Angular — ერთბაშად შეგიძლია გაუშვა:

```
start-dev.bat
```

(ორჯერ-დაწკაპუნებით Explorer-ში, ან ტერმინალიდან). თითოეული service ცალკე ტერმინალის ფანჯარაში იხსნება (log-ების სანახავად), ბრაუზერიც ავტომატურად იხსნება `localhost:4200`-ზე. ერთი service-ის გასაჩერებლად უბრალოდ დახურე მისი ფანჯარა.

ყველას ერთდროულად გასაჩერებლად:

```
stop-dev.bat
```

(Docker infra ცალკე რჩება გაშვებული — `docker compose down`, თუ ისიც გინდა გაჩერდეს).

### ხელით გაშვება (დეტალურად, ეტაპობრივად)

### 1. Infrastructure (PostgreSQL + pgvector, Redis, RabbitMQ)

დაგჭირდება [Docker Desktop](https://www.docker.com/products/docker-desktop/).

```bash
cp .env.example .env
# გახსენი .env და შეცვალე POSTGRES_PASSWORD და RABBITMQ_PASSWORD

docker compose up -d
```

> PostgreSQL კონტეინერი host-ზე ნაგულისხმევად პორტ `5433`-ზეა (არა სტანდარტული `5432`) — რომ არ დაუპირისპირდეს ლოკალურად დაინსტალირებულ PostgreSQL-ს, თუ ასეთი გაქვს.

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

მონაცემთა ბაზის სქემის გასაშვებად (migrations `CareerProject.Shared`-შია):

```bash
cd backend/CareerProject.Shared
POSTGRES_HOST=localhost POSTGRES_PORT=5433 POSTGRES_DB=career_project_db \
POSTGRES_USER=career_project POSTGRES_PASSWORD=<შენი .env-დან> \
dotnet ef database update
```

### 3. Frontend

```bash
cd frontend/career-project-web
npm install
npm start
```
