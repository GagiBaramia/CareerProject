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

ეტაპი 6 (Skills dictionary) დასრულებულია და დადასტურებულია: 19 skill (`SkillSeedData`, fixed GUIDs) `HasData()`-ით `CareerProject.Shared`-ის `Skill` entity-ზე, `SeedSkills` migration გატარებულია. `GET /api/skills` და `GET /api/skills?search=` `CareerProject.UserService`-ში (`EF.Functions.ILike`, case-insensitive) — დადასტურდა პირდაპირ და Gateway-ის გავლითაც (public route, ავტორიზაცია არ სჭირდება).

**შენიშვნა:** Task 5-ის Gateway routing table-ში `/api/skills` საერთოდ არ ყოფილა გათვალისწინებული — დავამატე `skills-route` (`appsettings.json`, `CareerProject.ApiGateway`), თორემ Angular ამ endpoint-ს Gateway-ის გავლით ვერასდროს მიაღწევდა.

ეტაპი 7 (Person Profile API) დასრულებულია და დადასტურებულია: `CareerProject.UserService`-ში `GET/PUT /api/profile/me`, `PUT /api/profile/me/skills` — მხოლოდ `Person` role-ისთვის (`PersonOnly` authorization policy, Company იღებს 403). Skills-ის replace ვალიდაციით (unknown skill id / არასწორი proficiency level → 400). `PersonSkill.Level` გადავიდა `int`-დან `ProficiencyLevel` enum-ზე (Beginner/Intermediate/Advanced/Expert) — `CareerProject.Shared`, ცარიელი migration (enum ისედაც `int`-ად ინახება ბაზაში).

**RabbitMQ:** `ProfileCreated`/`ProfileUpdated` მინიმალურად, პირდაპირ ქვეყნდება (`ProfileEventPublisher`, topic exchange `career_project.events`) — არა სრული Stage 13-ის publisher/consumer abstraction, რომელიც ეს მინიმალური ვერსია მომავალში ჩაანაცვლებს ყველა სერვისისთვის. `ProfileCreated` იგზავნება პირველი "რეალური" შევსებისას (`Headline` null→non-null), შემდეგ ყველა edit — `ProfileUpdated` (რადგან `PersonProfile` row უკვე რეგისტრაციისას იქმნება Stage 4-დან, "ახალი vs არსებული" გარჩევა ამ heuristic-ით გავაკეთე). დადასტურდა RabbitMQ Management API-დან (`publish_in` counter).

**რეფაქტორი:** `AuthEndpoints`-სა და `ProfileEndpoints`-ს შორის დუბლირებული `TryValidate` helper გავიტანე `CareerProject.UserService/Validation/RequestValidator.cs`-ში.

ეტაპი 8 (Angular Login/Register UI) დასრულებულია და დადასტურებულია: `/login`, `/register` (Person/Company toggle), `AuthService` (signal-based, localStorage-ში ინახავს token-ს), `authInterceptor` (Authorization header ავტომატურად), `authGuard` (protected routes). Login/register-ის შემდეგ role-ის მიხედვით redirect `/dashboard/person` ან `/dashboard/company`-ზე (ორივე ჯერ placeholder — რეალური კონტენტი Stage 16/12-ზე). დიზაინი: თეთრი/ლურჯი, „კარიერა" ბრენდი მარცხნივ ზემოთ — მაკეტების დიზაინის ენას დავეყრდენი (dedicated login მაკეტი არ არსებობდა).

**რეალურად შემოწმდა ბრაუზერში** (Playwright, headless Chromium — `chromium-cli` ამ მანქანაზე არ იყო, alternative driver დავწერე): login/register გვერდები ვიზუალურად, registration → redirect → dashboard, logout+login round-trip, არასწორი პაროლის error state. Console errors — მხოლოდ მოსალოდნელი 401 (wrong-password ტესტიდან).

ეტაპი 9 (Profile Wizard UI) დასრულებულია და დადასტურებულია: `/profile/edit` — 4-ნაბიჯიანი step indicator, **მხოლოდ ნაბიჯი 1 რეალურად ფუნქციური** (Full name, Headline, Summary, Location, Skills+level) — ნაბიჯები 2-4 (გამოცდილება/განათლება/დამატებითი) ვიზუალური placeholder-ებია, რადგან ამ მონაცემებისთვის backend-ში entity საერთოდ არ არსებობს (Task 3-ის მოდელს Experience/Education არ აქვს) — ეს პირდაპირ Task 9-ის საკუთარი ტექსტიდან გამომდინარეობს ("პირველ რეალიზაციაში აუცილებელია" სია). `SkillsAutocompleteComponent` (debounced search, chip-ისმაგვარი წაშლადი skill-ები, proficiency dropdown) და მარჯვნივ **რეალურ დროში განახლებადი Profile Preview**. `docs/mockups/profile-wizard.png` ჯერ არ აიტვირთა — დიზაინი აშენდა ადრე ნანახი screenshot-ის მეხსიერებით (თეთრი/ლურჯი, "კარიერა" ბრენდი, ორსვეტიანი layout).

**გადახრა მაკეტიდან:** მაკეტს ჰქონდა ცალკე "სახელი | გვარი" ველები — ჩვენი backend-ის `PersonProfile.FullName` კი ერთი ველია (Task 3/7-დან), ამიტომ ერთი "სრული სახელი" ველი გავაკეთე, API კონტრაქტის შესაბამისად.

**რეალურად შემოწმდა ბრაუზერში** (Playwright): registration → dashboard → "შეავსე პროფილი" ბმული → wizard-ის შევსება (headline, summary, location, 2 skill + level შეცვლა) → live preview განახლება → შენახვა (`PUT /api/profile/me` + `PUT /api/profile/me/skills`) → step 2 stub-ზე გადასვლა → გვერდის reload → მონაცემები რეალურად შენარჩუნებულია ბაზაში (headline და ორივე skill დადასტურდა).

ეტაპი 10 (Company Profile) დასრულებულია და დადასტურებულია: `CareerProject.JobService`-ის პირველი კოდი — `GET/PUT /api/company/me`, მხოლოდ Company role-ისთვის (`CompanyOnly` policy, Person იღებს 403). JWT validation setup იმეორებს ApiGateway-ს პატერნს (მხოლოდ ვალიდაცია, არა გენერაცია — Issuer/Audience `appsettings.json`-ში, secret env var-იდან).

**რეფაქტორი:** `RequestValidator` გავიტანე `CareerProject.UserService`-დან `CareerProject.Shared/Validation/`-ში (ახლა JobService-საც სჭირდებოდა იგივე ლოგიკა) — UserService-იც ახლა საერთო ვერსიას იყენებს, დუბლირების გარეშე.

**Gateway routing bugfix (ისევ):** Task 5-ის routing table-ში `/api/company/*`-იც არ ყოფილა — დავამატე `company-route`.

დადასტურდა Gateway-ის გავლით: GET/PUT company profile, 403 Person-ისთვის, 401 token-ის გარეშე.

ეტაპი 11 (Job CRUD) დასრულებულია და დადასტურებულია: `CareerProject.JobService`-ში `POST/GET/GET-by-id/PUT/DELETE /api/jobs`. `POST/PUT/DELETE` — მხოლოდ Company role, **და** მხოლოდ job-ის მფლობელი კომპანია (403 სხვა კომპანიის job-ის რედაქტირებაზე/წაშლაზე). `GET` (სია + ცალკეული) — ნებისმიერი authenticated user (Person-იც, არა მხოლოდ Company — ეს Task 11-ის ტექსტიდან გამომდინარეობს, სადაც restriction მხოლოდ create/edit-ზეა). `JobSkill.RequiredLevel` გადავიდა `ProficiencyLevel` enum-ზე (`PersonSkill.Level`-ის იგივე პატერნი). `JobEventPublisher` — `JobCreated`/`JobUpdated`, იგივე მინიმალური RabbitMQ მიდგომა, რაც Profile event-ებს ჰქონდა.

დადასტურდა Gateway-ის გავლით: create/list/get/update/delete, ownership 403, role 403 (Person-ს POST/PUT/DELETE არ შეუძლია, მაგრამ list/get კი), unknown skill id → 400, RabbitMQ publish counter.

შემდეგი: **ეტაპი 12 — Vacancy Creation UI** (`docs/mockups/job-posting.png`-ს დაეყრდნობა — ჯერ არ აიტვირთა).
