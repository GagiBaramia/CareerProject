# CareerProject

საბაკალავრო პროექტი. სრული ეტაპობრივი გეგმა — [PLAN.md](PLAN.md). ყოველთვის იმუშავე ამ გეგმის მიხედვით, ეტაპების თანმიმდევრობით (იხ. PLAN.md-ის "სამუშაოს რეალური თანმიმდევრობა").

## სამუშაო წესები

- **არსებული კოდი არ დაანგრიო.** ჯერ შეისწავლე პროექტის სტრუქტურა, შემდეგ შეასრულე მხოლოდ მიმდინარე დავალება.
- **არ შეცვალო სხვა მოდულები** საჭიროების გარეშე — თუ Task 7-ზე მუშაობ, ნუ შეეხები Job-ის ან Notification-ის კოდს, თუ პირდაპირ არ მოითხოვს.
- **ერთ ეტაპზე მეტს ერთდროულად ნუ შეასრულებ.** PLAN.md-ში ერთი Task = ერთი მოთხოვნა.
- **ბიზნეს ლოგიკის მაგიური რიცხვები** (წონები, ლიმიტები და ა.შ.) config-ში გადაიტანე, არა hardcode.
- **პაროლი/API key არასოდეს** არ ჩაწერო source code-ში — მხოლოდ environment variables / `.env` (რომელიც `.gitignore`-შია).
- დასრულებისას ყოველთვის მომეცი: (1) რომელი ფაილები შექმენი/შეცვალე, (2) როგორ გავუშვა, (3) როგორ შევამოწმო შედეგი.
- Build/compile ყოველთვის გადაამოწმე დავალების დასრულებისას, სანამ "დასრულებულად" მიიჩნევ.

## სტეკი

- **Backend:** .NET 9, ASP.NET Core Web API, EF Core, PostgreSQL + pgvector
- **Frontend:** Angular (`frontend/career-project-web`)
- **Infra:** Docker Compose, Redis, RabbitMQ
- **AI:** Gemini API (embeddings + RAG chat)

## სტრუქტურა

```text
backend/
├── CareerProject.ApiGateway/
├── CareerProject.UserService/
├── CareerProject.JobService/
├── CareerProject.RecommendationService/
├── CareerProject.NotificationService/
└── CareerProject.Shared/
frontend/career-project-web/
docker/
docker-compose.yml
```

## UI მაკეტები

`docs/mockups/`-ში დევს დამკვეთის მიერ მოწოდებული დიზაინ მაკეტები (თეთრი background, ლურჯი primary accent, „კარიერა" ბრენდი). UI ეტაპებზე (Task 8 — Login/Register, Task 9 — Profile Wizard, Task 12 — Vacancy Creation, Task 16 — Recommendations Dashboard, Task 20 — AI Chat Panel) ვიზუალურად ამ მაკეტებს დაემსგავსე ზუსტად — ფერები, spacing, კომპონენტების განლაგება.

- `profile-wizard.png` — კანდიდატის პროფილის შექმნის multi-step ფორმა (Task 9)
- `recommendations-dashboard.png` — ვაკანსიების რეკომენდაციები + AI assistant panel (Task 16, Task 20)
- `job-posting.png` — კომპანიის მიერ ვაკანსიის გამოქვეყნების ფორმა (Task 12)

## მიმდინარე სტატუსი

ეტაპი 1 დასრულებულია: 6 პროექტი + `.sln`, build succeeds, GitHub-ზეც აიტვირთა.

ეტაპი 2 (Docker) დასრულებულია და დადასტურებულია: `docker compose up -d` გაშვებულია, PostgreSQL/Redis/RabbitMQ ყველა healthy, pgvector extension ჩატვირთულია, RabbitMQ Management UI ხელმისაწვდომია (`localhost:15672`).

ეტაპი 3 (EF Core მოდელი) დასრულებულია და დადასტურებულია: 9 entity (`User`, `PersonProfile`, `Skill`, `PersonSkill`, `Company`, `Job`, `JobSkill`, `Application`, `ChatMessage`) + `CareerProjectDbContext` `CareerProject.Shared/Entities/` და `Data/`-ში, `InitialCreate` migration შექმნილია და გატარებულია რეალურ Docker Postgres-ზე (ცხრილები, FK/cascade წესები, `vector(768)` სვეტები Person/Job embedding-ისთვის — დადასტურდა `\dt`/`\d`-ით).

**შენიშვნა:** ამ მანქანაზე ცალკე ნატიური PostgreSQL 18 Windows service დგას (`postgresql-x64-18`), რომელიც პორტ 5432-ს იკავებს — ამიტომ Docker-ის PostgreSQL კონტეინერი host-პორტ `5433`-ზეა გადატანილი (`.env`/`.env.example`, `POSTGRES_PORT=5433`), რომ კონფლიქტი არ მოხდეს. `dotnet ef` ბრძანებები `CareerProject.Shared`-იდან უნდა გაეშვას, connection info მხოლოდ environment variables-იდან იკითხება (`CareerProjectDbContextFactory`), არასოდეს hardcoded.

ეტაპი 4 (ავტორიზაცია) დასრულებულია და დადასტურებულია: `CareerProject.UserService`-ში `POST /api/auth/register/person`, `POST /api/auth/register/company`, `POST /api/auth/login` — JWT (role claim-ით), `PasswordHasher<User>` პაროლის hash-ისთვის (plaintext არასოდეს), DataAnnotations validation, სწორი status code-ები (201/200/401/409). Swagger UI ხელმისაწვდომია `/swagger`-ზე (dev-ში). ყველა endpoint რეალურად გაეშვა და დატესტილია (`dotnet run` + curl) რეალურ Docker Postgres-ზე.

JWT secret `Jwt__Secret` env var-შია (`.env`, double-underscore = ASP.NET Core-ის config section syntax), Issuer/Audience/ExpiryMinutes — `appsettings.json`-ში (არასაიდუმლო). `CareerProject.Shared`-ში დაემატა `PostgresConnectionStringBuilder` — connection string აწყობის საერთო ლოგიკა, გამოიყენება migration factory-იც და UserService-იც.

ეტაპი 5 (API Gateway) დასრულებულია და დადასტურებულია: `CareerProject.ApiGateway` YARP-ით (`Yarp.ReverseProxy`) — routing `appsettings.json`-ის `ReverseProxy` სექციაში (`/api/auth/*` public, `/api/users|jobs|applications|recommendations|ai|notifications/*` მოითხოვს JWT-ს Gateway-ის დონეზე, `AuthorizationPolicy: authenticated`). CORS `localhost:4200`-ისთვის. დადასტურდა რეალურად გაშვებით: login `/api/auth/*`-ზე token-ის გარეშე გაეშვა (200), protected route token-ის გარეშე — 401 Gateway-მაც უარყო, token-ით — 502 (რადგან JobService ჯერ არ არსებობს, Stage 11-მდე), CORS preflight დადასტურდა.

**შენიშვნა:** Gateway-ს ცალკე `Jwt` config აქვს (`appsettings.json`: Issuer/Audience, იგივე მნიშვნელობები რაც UserService-ს — უნდა ემთხვეოდეს, რომ token validation იმუშაოს), იმავე `Jwt__Secret` env var-ს იყენებს. განზრახ არ გავიტანე `CareerProject.Shared`-ში, რომ Task 4-ის უკვე დასრულებული/დატესტილი UserService კოდი არ შემეხო.

შემდეგი: **ეტაპი 6 — Skills dictionary**.
