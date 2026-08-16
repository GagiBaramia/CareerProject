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
cd frontend/career-project-web
npm install
npm start
```
