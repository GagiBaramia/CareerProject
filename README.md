# Kariera

საბაკალავრო პროექტი — კარიერული პლატფორმა კანდიდატებისა და კომპანიებისთვის, AI-ზე დაფუძნებული ვაკანსია-კანდიდატის მატჩინგითა და RAG ჩატით.

## სტრუქტურა

```text
backend/
├── Kariera.ApiGateway/          # ერთადერთი entry point frontend-ისთვის
├── Kariera.UserService/         # auth, users, profiles, skills
├── Kariera.JobService/          # companies, jobs, applications
├── Kariera.RecommendationService/ # embeddings, hybrid matching, AI chat
├── Kariera.NotificationService/ # notifications
└── Kariera.Shared/              # საერთო event contracts და utilities
frontend/
└── kariera-web/                 # Angular აპლიკაცია
docker/
docker-compose.yml
```

## გეგმა

დეტალური ეტაპობრივი გეგმა იხილეთ [PLAN.md](PLAN.md)-ში.

## გაშვება (development)

> სრული ინსტრუქცია დაემატება Stage 2 (Docker) და Stage 23 (production-like გაშვება) დასრულების შემდეგ.

### Backend

```bash
cd backend
dotnet build
```

### Frontend

```bash
cd frontend/kariera-web
npm install
npm start
```
