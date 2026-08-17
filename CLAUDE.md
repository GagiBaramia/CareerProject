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

ეტაპი 12 (Vacancy Creation UI) დასრულებულია და დადასტურებულია: `/jobs/new` — Company-only route (`companyGuard`, Person-ს ავტომატურად აბრუნებს `/dashboard/person`-ზე). ფორმა: სათაური (80), აღწერა (2000), ადგილმდებარეობა, სამუშაო გრაფიკი/ფორმატი dropdown-ები, skills (`SkillsAutocompleteComponent`-ის ხელახლა გამოყენებით), ხელფასის შუალედი+ვალუტა. მარჯვნივ "ბოლო კანდიდატები" static მაგალითი (Task 12-ის თავად დაშვებული გამონაკლისი — Application ფუნქციონალი ჯერ არ არსებობს).

**Backend-ის გაფართოება (გამომდინარეობს Task 12-ის ფორმის სპეციფიკაციიდან):** მაკეტს ჰქონდა `WorkFormat` (Remote/Office/Hybrid) და ხელფასის შუალედი, რაც Task 3/11-ის `Job` entity-ს არ ჰქონდა — დავამატე `Job.WorkFormat`, `SalaryMin`, `SalaryMax`, `SalaryCurrency` (`CareerProject.Shared` + migration + `CareerProject.JobService`-ის DTO/endpoint-ები).

**რეფაქტორი:** `SkillsAutocompleteComponent` გადავიტანე `features/profile-wizard/`-დან `shared/components/`-ში (ახლა job-create-საც სჭირდებოდა), და საერთო page-shell/form CSS (`brand-header`, `field`, ღილაკები) გავიტანე `shared/styles/page-form-shell.css`-ში — მესამედ აღარ გამეორდა (login/register-ს ცალკე დავტოვე, განსხვავებული layout აქვს).

**რეალურად შემოწმდა ბრაუზერში** (Playwright): company რეგისტრაცია → dashboard → "გამოაქვეყნე ვაკანსია" → ფორმის შევსება (2 skill-ით, dropdown-ებით, ხელფასით) → submit → რეალურად შეიქმნა JobService-ში (დადასტურდა `GET /api/jobs`-ით) → Person-ის მცდელობა `/jobs/new`-ზე პირდაპირი URL-ით წვდომისთვის → ავტომატური redirect `/dashboard/person`-ზე.

ეტაპი 13 (RabbitMQ Event Bus) დასრულებულია და დადასტურებულია: `CareerProject.Shared/Events/` — `EventBase` (EventId/OccurredAt/EntityId + RoutingKey) და 6 event contract (`ProfileCreated`, `ProfileUpdated`, `JobCreated`, `JobUpdated`, `ApplicationSubmitted`, `ApplicationStatusChanged` — ბოლო ორი ჯერ არსად გამოიყენება, Stage 17-მდე). `CareerProject.Shared/Messaging/`: `IEventPublisher`/`RabbitMqEventPublisher` (retry 3-jერ, exponential-ისმაგვარი backoff, publish failure request-ს არ აჩერებს — მხოლოდ log-დება) და `RabbitMqConsumerBase<TEvent>` (BackgroundService, retry + nack-without-requeue poison message-ებზე) — მომავალი კონკრეტული consumer-ებისთვის (Stage 14/18), ჯერ არცერთი არ ჩართულა production კოდში.

**UserService/JobService**-ის ძველი, task-სპეციფიური `ProfileEventPublisher`/`JobEventPublisher` წაიშალა, ორივემ ახლა საერთო `IEventPublisher`-ს იყენებს (`AddCareerProjectEventPublisher()` extension).

**რეალურად შემოწმდა:** (1) ჩვეულებრივი publish — `publish_in` counter გაიზარდა; (2) **RabbitMQ-ს დროებით გათიშვა** (`docker stop`) — request მაინც 200-ით დაბრუნდა (< 1წმ), log-ში ზუსტად ჩანს 3 მცდელობა (warn→warn→fail), caller არ დაბლოკილა; (3) **აღდგენა** — RabbitMQ-ს ჩართვის შემდეგ publish ისევ იმუშავა ავტომატურად (RabbitMQ.Client-ის built-in connection recovery); (4) **Consumer** — დროებით, ცალკე scratch პროექტში (production კოდში არ შესულა) გავუშვი `RabbitMqConsumerBase<ProfileUpdated>`-ის subclass, რეალურმა `PUT /api/profile/me`-მ გამოაქვეყნა event და consumer-მა წარმატებით მიიღო/დამუშავა — publisher-consumer მთელი ჯაჭვი დადასტურებულია.

ეტაპი 14 (CV და Job embedding) დასრულებულია და დადასტურებულია: `CareerProject.RecommendationService`-ის პირველი კოდი — `GeminiEmbeddingClient` (`gemini-embedding-001`, `outputDimensionality: 768`, `taskType: RETRIEVAL_DOCUMENT`), `PersonProfileEmbeddingService`/`JobEmbeddingService`, და 4 consumer (`ProfileCreated/Updated`, `JobCreated/Updated`) — Stage 13-ის `RabbitMqConsumerBase`-ზე აგებული, `IServiceScopeFactory`-ით (consumer singleton-ია, DbContext — scoped).

**მნიშვნელოვანი აღმოჩენა:** `text-embedding-004` (რომელსაც Stage 3-ზე ვგულისხმობდი 768-განზომილებიანი embedding-ისთვის) აღარ არსებობს ამ API key-ზე — რეალურმა API-მ აჩვენა, რომ მიმდინარე მოდელია `gemini-embedding-001`, output default 3072განზომილება. `curl`-ით პირდაპირ შემოწმებულმა API call-მა დაადასტურა, რომ `outputDimensionality: 768` პარამეტრი ზუსტად 768-ს აბრუნებს — შესაბამისად Stage 3-ის pgvector სქემა (`vector(768)`) მიგრაციის გარეშე დარჩა ვალიდური.

**რეალურად შემოწმდა (რეალურ Gemini API-ზე, testing key-ით):** (1) `PUT /api/profile/me` → `ProfileUpdated` → consumer-მა Gemini-ს რეალურად მიმართა (HTTP 200, ~630ms) → `PersonProfiles.Embedding` განახლდა, `vector_dims = 768`; (2) იგივე `POST /api/jobs`-ზე → `Jobs.Embedding` განახლდა; (3) **სემანტიკური ხარისხის შემოწმება** — თემატურად მსგავსი პროფილისა და ვაკანსიის (ორივე .NET/PostgreSQL/RabbitMQ) cosine similarity pgvector-ით გამოთვლილმა `0.91` აჩვენა — embedding რეალურად აზრიანად მუშაობს.

**Resilience:** ცალკე live-failure ტესტი აღარ გავიმეორე Gemini-ს გათიშვაზე — consumer-ები იმავე `RabbitMqConsumerBase`-ს იყენებენ, რომლის retry/nack-without-requeue ქცევა Stage 13-ზე უკვე საფუძვლიანად დადასტურდა. არქიტექტურულადაც გარანტირებულია, რომ Gemini-ს failure ვერასდროს შეაფერხებს თავად profile/job-ის შენახვას — embedding მთლიანად ცალკე სერვისში, event-ის მიღების შემდეგ, ცალკე async პროცესშია.

ეტაპი 15 (Hybrid Matching) დასრულებულია და დადასტურებულია: `GET /api/recommendations/jobs` `CareerProject.RecommendationService`-ში, `PersonOnly` policy (Company → 403). `HybridMatchingCalculator` — სუფთა, DB-isგან დამოუკიდებელი კლასი (`CalculateSkillOverlap`, `CalculateScore`) `Services/`-ში, 11 unit test-ით დაფარული (`CareerProject.RecommendationService.Tests`, ახალი პროექტი, დამატებულია `.sln`-ში). წონები (`StructuredWeight=0.6`, `SemanticWeight=0.4`) `appsettings.json`-ის `Recommendation` სექციაშია, არა hardcoded.

**Semantic similarity — რეალურად pgvector-ის `<=>` ოპერატორით, server-side:** `Vector.CosineDistance()` (Pgvector.EntityFrameworkCore-ის instance extension method) LINQ query-ში გამოყენებულმა წარმატებით ითარგმნა SQL-ში — ლოგში დავადასტურე: `CASE WHEN j."Embedding" IS NULL THEN 0.0 ELSE 1.0 - (j."Embedding" <=> @__personEmbedding_0) END`. embedding-ის არარსებობის შემთხვევაში (person ან job) — semanticSimilarity ნაგულისხმევად 0, request არ ინგრევა.

**Skill overlap-ის განსაზღვრება:** `matchedRequiredSkills / totalRequiredSkills`; 0 required skill → overlap = 1.0 (არაფერია რისი არშეთავსებაც, სამართლიანი დეფოლტი skill-agnostic ვაკანსიებისთვის).

**რეალურად შემოწმდა Gateway-ის გვერდის ავლით, პირდაპირ სერვისზე:** score-ები ხელით გამოთვლას ემთხვევა ორივე შემთხვევაში (embedding-იანი job: `0.6×1 + 0.4×0.913 = 0.965`; embedding-ის გარეშე: `0.6×0.5 + 0.4×0 = 0.3`), დალაგება score-ის კლებადობით სწორია, ახალ Person-ს embedding-ის გარეშეც (fallback branch) სწორად უბრუნებს მხოლოდ skill-overlap-ზე დაფუძნებულ score-ს, Company/token-ის გარეშე — 403/401.

ეტაპი 16 (Recommendations UI) დასრულებულია და დადასტურებულია: `/jobs/recommended` (Person-only, `personGuard` — ახალი, `companyGuard`-ის სარკისებური). Job card-ები (title, company, location, employment type, work format, salary, skills, matching %), location/employment-type filter-ები, sort-by-matching toggle — ყველა client-side `computed()` signal-ით, მონაცემები მთლიანად რეალური `GET /api/recommendations/jobs`-იდან. AI Assistant panel (მაკეტში ჩანდა) **განზრახ არ ავაშენე** — ეს Stage 20-ის საქმეა, არა Task 16-ისა.

**სატესტო მონაცემები დაემატა** (მომხმარებლის მოთხოვნით, დემოსა და ამ გვერდის რეალურ ტესტირებას ერთდროულად ემსახურება): 6 რეალისტური კომპანია/ვაკანსია (TBC Bank, Bank of Georgia, EPAM Systems, Datablitz Studio, CloudNine Georgia, TechHub Georgia) — სხვადასხვა employment type/work format/ლოკაცია/skill-სეტით, ცალკე Node.js seed script-ით (`docs/`-ში არ შენახულა, ერთჯერადი გამოყენებისთვის იყო).

**რეალურად შემოწმდა ბრაუზერში** (Playwright, realistic მონაცემებზე): Nino-ს (C#/.NET candidate) რეკომენდაციები სწორად დაალაგა — TBC Bank/.NET ვაკანსიები მაღლა (64-97%), React frontend ვაკანსია ყველაზე დაბლა (28%); employment-type filter-მა ზუსტად 1 job დატოვა (Internship); sort toggle-მა სწორად შეაბრუნა დალაგება. Console errors — არცერთი.

ეტაპი 17 (Application flow) დასრულებულია და დადასტურებულია: `CareerProject.JobService`-ში `POST /api/jobs/{jobId}/apply` (PersonOnly), `GET /api/company/jobs/{jobId}/applications` და `PATCH /api/applications/{id}/status` (ორივე CompanyOnly + job ownership check). `Application.Status` — Task 3-ის plain string-იდან `ApplicationStatus` enum-ზე (Submitted/InReview/Interview/Rejected/Accepted), იგივე პატერნი რაც `ProficiencyLevel`-ს ჰქონდა. დუბლირებული განაცხადის თავიდან აცილება **ორ დონეზე**: app-level check + DB-level unique index `(JobId, PersonId)`-ზე.

**Migration-ის ხელით შესწორება:** `Status`-ის string→int (enum) ცვლილებამ ავტომატური cast ვერ იპოვა (Postgres `text`→`integer` პირდაპირ ვერ იკასტება) — migration-ში ხელით ჩავანაცვლე `AlterColumn` `DropColumn`+`AddColumn`-ით (უსაფრთხო, რადგან `Applications` ცხრილი ცარიელი იყო).

**რეფაქტორი:** JobService-ში `LoadCompany` სამ სხვადასხვა endpoint ფაილში იყო თითქმის იდენტურად დუბლირებული (`CompanyEndpoints`, `JobEndpoints`, ახლა `ApplicationEndpoints`-იც დაემატებოდა) — გავიტანე `Auth/CurrentUserResolver.cs`-ში (`LoadCurrentCompanyAsync`, ახალი `LoadCurrentPersonProfileAsync`).

**Gateway routing ცვლილება არ დასჭირდა** — `/api/jobs/*`, `/api/company/*`, `/api/applications/*` უკვე Stage 5/10-დან იყო დაფარული.

**რეალურად შემოწმდა:** Nino-მ განაცხადი გაგზავნა TBC Bank-ის ვაკანსიაზე (201), მეორედ იგივეზე — 409; TBC Bank-მა (owner) დაინახა განაცხადი და შეცვალა status `InReview`-ზე (200); არასწორი status string → 400; **სხვა** კომპანია (არა owner) → 403 იმავე ვაკანსიის განაცხადებზე წვდომაზე; Person-ს Company-ის endpoint-ებზე წვდომა არ აქვს (403) და პირიქითაც (403); RabbitMQ-ს `publish_in` counter-მა დაადასტურა `ApplicationSubmitted`/`ApplicationStatusChanged` ორივეს გამოქვეყნება.

ეტაპი 18 (Notification Service) დასრულებულია და დადასტურებულია: `CareerProject.NotificationService`-ის პირველი კოდი — `ApplicationSubmittedConsumer`/`ApplicationStatusChangedConsumer`, Stage 13-ის `RabbitMqConsumerBase`-ის **პირველი production გამოყენება** (აქამდე მხოლოდ Stage 14-ის embedding consumer-ები იყენებდნენ; ესეც იმავე pattern-ს იმეორებს). `GET /api/notifications`, `PATCH /api/notifications/{id}/read` — ნებისმიერი authenticated user (Person-იც, Company-იც).

**ახალი entity:** `Notification` (Task 3-ს არ ჰქონდა, Task 18-ის საჭიროებით დაემატა) — `RecipientUserId` პირდაპირ `User`-ზეა მიბმული (არა PersonProfile ან Company ცალკე), რადგან ერთი notification-ის მექანიზმი ორივე მიმღებ ტიპს ემსახურება: `ApplicationSubmitted` → კომპანიას (job-ის მფლობელს), `ApplicationStatusChanged` → კანდიდატს.

**რეალურად შემოწმდა სრული ჯაჭვი:** Nino-მ განაცხადი გაგზავნა → NotificationService-ის consumer-მა რეალურად მიიღო event RabbitMQ-დან (log-ში ჩანს SQL INSERT) → Bank of Georgia-მ `GET /api/notifications`-ით რეალურად ნახა "Nino Giorgiashvili-მა გამოგიგზავნათ განაცხადი..."; status შეიცვალა `Interview`-ზე → Nino-მ მიიღო "თქვენი განაცხადის სტატუსი... შეიცვალა: მოწვეულია გასაუბრებაზე"; mark-as-read იმუშავა (`isRead: true`); სხვა user-ის notification-ის მონიშვნაზე — 403; token-ის გარეშე — 401.

ეტაპი 19 (AI RAG Chat) დასრულებულია და დადასტურებულია: `CareerProject.RecommendationService`-ში `POST /api/ai/chat` (`PersonOnly`). პროცესი Task 19-ის ტექსტს პირდაპირ მიჰყვება: (1) მომხმარებლის შეტყობინება ინახება `ChatMessages`-ში; (2) `GeminiEmbeddingClient.EmbedAsync(..., "RETRIEVAL_QUERY")` (Stage 14-ის იგივე client, ახლა მეორედ გამოიყენება query-ისთვის) query embedding-ს აგებს; (3) pgvector-ის `<=>` ოპერატორით (`Vector.CosineDistance()`, იგივე Stage 15-ის pattern) მოიძებნება `AiChat:TopKJobs` (config, ნაგულისხმევად 5) ყველაზე ახლო რეალური ვაკანსია; (4) ეს ვაკანსიები ჩაისმება ახალი `GeminiChatClient`-ის (`gemini-3.6-flash`, `generateContent`) system prompt-ის კონტექსტში, რომელიც პირდაპირ და კატეგორიულად კრძალავს არარსებული ვაკანსიის გამოგონებას; (5) პასუხი + გამოყენებული `jobIds`/`referencedJobs` ბრუნდება, ასისტენტის პასუხიც ინახება `ChatMessages`-ში.

ახალი ფაილები: `Services/GeminiChatClient.cs`, `Services/AiChatService.cs`, `Endpoints/AiChatEndpoints.cs`, `Dtos/AiChatRequest.cs`, `Dtos/AiChatResponse.cs`, `Config/AiChatOptions.cs`. Gateway-ის `ai-route` უკვე Stage 5-დან არსებობდა — ცვლილება არ დასჭირდა. `ChatMessage` entity/table Stage 3-დანვე მზად იყო — მიგრაცია არ დასჭირდა.

**მნიშვნელოვანი აღმოჩენა (Gemini chat-generation მოდელის სახელი):** Stage 14-ის embedding მოდელივით, ესეც არასტაბილურია დროში — `gemini-flash-latest` → 503 (high demand), `gemini-2.5-flash` → 404 "no longer available to new users, use models/gemini-3.6-flash". მუშა და დადასტურებული მოდელია `gemini-3.6-flash`, `generateContent` endpoint-ით, `systemInstruction`/`contents`/`candidates` JSON კონტრაქტით — დადასტურდა `curl`-ით პირდაპირ API-სთან საუბრით, კოდის დაწერამდე (იგივე დისციპლინა, რაც Stage 14-ზე).

**Job-ების "გამოყენების" ინტერპრეტაცია:** `jobIds`/`referencedJobs` აბრუნებს ყველა იმ ვაკანსიას, რომელიც context-ად მიეწოდა Gemini-ს (ანუ pgvector-ის top-K), არა მხოლოდ იმას, რაც პასუხის ტექსტში პირდაპირ მოიხსენია — მარტივი და საიმედო მიდგომაა structured-citation parsing-ის გარეშე. Stage 20-ის UI-მ ეს გასათვალისწინებელია: თუ პასუხი ამბობს "ვერ მოიძებნა", `referencedJobs` მაინც შეიცავს უახლოეს (მაგრამ ფაქტობრივად შეუსაბამო) ვაკანსიებს — frontend-მა ეს ცალკე უნდა დაამუშაოს (მაგ. jobs card-ები არ აჩვენოს, თუ პასუხი უარყოფითია).

**რეალურად შემოწმდა რეალურ სტეკზე** (Postgres+pgvector, RabbitMQ, Gateway→UserService→RecommendationService, ნამდვილი Gemini API): ახალი Person-ის რეგისტრაცია → პროფილის შექმნა (`ProfileCreated` → embedding რეალურ დროში დაგენერირდა, log-ით დადასტურებული) → `POST /api/ai/chat` "junior backend, დისტანციურად" კითხვით — სწორად იპოვა რეალური "Junior Backend Developer" ვაკანსია, მაგრამ **სწორად აღნიშნა, რომ ის ოფისიდანაა (OnSite), არა დისტანციური** (ფაქტობრივ მონაცემებზე ჯერადობა, არა ბრმა დამთხვევა); fabrication-საწინააღმდეგო ტესტი — გამოგონილი "ასტრონავტის ვაკანსია მთვარეზე" → მოდელმა სწორად თქვა, რომ ასეთი ვაკანსია არ არსებობს, არ გამოიგონა ალტერნატივა; ორივე შეტყობინება (user+assistant) რეალურად შენახულა `ChatMessages`-ში (`psql`-ით დადასტურდა); token-ის გარეშე → 401; ცარიელი `message` → 400 validation. Log-ებში errors/warnings არ ყოფილა.

ეტაპი 20 (AI Chat UI) დასრულებულია და დადასტურებულია: `/jobs/recommended` გვერდი ორსვეტიან grid-ად გარდაიქმნა (Stage 9-ის wizard-grid-ის იგივე პატერნი, 1.5fr/1fr, sticky მარჯვენა პანელი) — მარცხნივ არსებული job-card-ების სია უცვლელია, მარჯვნივ ახალი AI Assistant chat panel. `docs/mockups/recommendations-dashboard.png` ფაილურად ისევ არ არსებობს (`docs/mockups/`-ში მხოლოდ `.gitkeep`) — დიზაინი აშენდა აპლიკაციაში უკვე დამკვიდრებული ვიზუალური ენით (თეთრი/ლურჯი, chat bubble-ები, არსებული CSS token-ები).

Panel-ს აქვს Task 20-ის ყველა მოთხოვნილი ელემენტი: conversation history (user/assistant bubble-ები), send ღილაკი+input, loading state (three-dot animation), error state (წითელი inline შეტყობინება). Real `POST /api/ai/chat`-ს იყენებს (`AiChatService`, `core/services/`) — mock პასუხი არსად არის. ახალი ფაილები: `core/models/ai-chat.models.ts`, `core/services/ai-chat.service.ts`; შეიცვალა მხოლოდ `job-recommendations.component.{ts,html,css}` (Stage 16-ის სხვა ნაწილი — ფილტრები, სორტირება — ხელუხლებელია).

**"ვაკანსიის გახსნის" ინტერპრეტაცია:** ცალკე job-detail გვერდი აპლიკაციაში არ არსებობს (Task 20-ის სფეროს გარეთაა ახლის აშენება) — ამიტომ პასუხში დაბრუნებული `referencedJobs` chip-ების დაწკაპუნება (`focusJob()`) filter-ебს წმენდს და უკვე იმავე გვერდზე ნაჩვენებ შესაბამის job-card-თან საშუალებას scroll+highlight-ს უკეთებს (2წმ ლურჯი border pulse). ეს პრაქტიკული, არადამატებითი გადაწყვეტაა, რადგან `GET /api/recommendations/jobs` ისედაც ყველა რეალურ ვაკანსიას აბრუნებს.

**რეალურად შემოწმდა ბრაუზერში** (Playwright, headless Chromium — ისევ ცალკე დაინსტალირდა ამ სესიაში, `chromium-cli` აქაც არ იყო): ახალი Person-ის რეგისტრაცია → პროფილი → `/jobs/recommended` გახსნა → chat panel ვიზუალურად ემთხვევა დანარჩენ აპს → შეტყობინების გაგზავნა "junior backend, დისტანციურად" → loading state რეალურად გამოჩნდა → რეალურმა Gemini-მ უპასუხა (იგივე პატიოსანი "OnSite, არა Remote" დაზუსტება, რაც Stage 19-ზე) → 5 job-chip გამოჩნდა პასუხის ქვეშ → chip-ზე დაწკაპუნებამ ზუსტად ის job-card ჰაილაითი გაუკეთა და scroll-გაუკეთა, რომელიც chip-ს შეესაბამებოდა → ცარიელი input-ით send ღილაკი disabled-ია → console errors — არცერთი.

ეტაპი 21 (Redis) დასრულებულია და დადასტურებულია: `CareerProject.RecommendationService`-ში `StackExchange.Redis` ორი დანიშნულებით — (1) `IRecommendationCache`/`RedisRecommendationCache` (`Caching/`) `GET /api/recommendations/jobs`-ის შედეგს პირადად თითო Person-ისთვის ინახავს (`Recommendation:CacheTtlSeconds`, ნაგულისხმევად 120), invalidate-დება `ProfileUpdatedConsumer`-იდან (მხოლოდ ის Person) და `JobUpdatedConsumer`-იდან (ყველა cache-ჩანაწერი, `SCAN`-ით `recommendations:*` pattern-ზე — რადგან ერთი job-ის ცვლილება ყველას რეკომენდაციაზე მოქმედებს); (2) `IAiChatRateLimiter`/`RedisAiChatRateLimiter` fixed-window counter-ით `POST /api/ai/chat`-ს ზღუდავს (`AiChat:RateLimit:MaxRequests`/`WindowSeconds`, ნაგულისხმევად 10/60წმ) — ლიმიტის გადაჭარბებაზე 429.

**Resilience (Task 21-ის ცენტრალური მოთხოვნა):** ორივე caching კლასი ყველა Redis ოპერაციას try/catch-ში წევს და failure-ზე მხოლოდ warning-ს წერს (log-დება, caller არ ინგრევა) — cache read/write failure → საერთოდ recompute ხდება database-იდან (cache miss-ის იდენტურად), rate-limit check failure → request fail-open-ით უშვებს (infra-ს ჩავარდნამ არ უნდა დაბლოკოს legitimate feature). `RedisConnectionFactory`-ს `abortConnect=false` აქვს, რომ სერვისი Redis-ის გარეშეც აეშვას; `connectTimeout=1000,syncTimeout=1000,connectRetry=1`, რომ downed Redis-ზე ერთი მოთხოვნა წამებით კი არ დაიკეტოს (default 5წმ), 1-2წმ-ში fail-ავდეს.

**რეალურად შემოწმდა:** (1) cache hit — მეორე identical მოთხოვნა 15x სწრაფი (712ms→48ms); (2) TTL — 120წმ-დან 111წმ დარჩენილი დადასტურდა `TTL`-ით; (3) `ProfileUpdated` invalidation — profile-ის განახლების შემდეგ cache key რეალურად წაიშალა; (4) `JobUpdated` invalidation — job-ის განახლების შემდეგ **ყველა** cache key წაიშალა (არა მხოლოდ ერთი Person-ის); (5) rate limit — Redis counter-ის ხელით 10-ზე დაყენებით 11-ე მოთხოვნამ ზუსტად 429 დააბრუნა, counter 11-მდე გაიზარდა; (6) **Redis-ის რეალურად გათიშვა** (`docker stop`) — service ისევ ჩვეულებრივად ავარდა, `GET /api/recommendations/jobs` მაინც 200-ით დაბრუნდა (cache-ის გარეშე, DB-დან), `POST /api/ai/chat` მაინც იმუშავა (rate limiter fail-open); (7) **აღდგენა** — Redis-ის ჩართვის შემდეგ caching ავტომატურად განახლდა, ხელახალი restart არ დასჭირდა.

**გვერდითი, ამ ტესტირებისას აღმოჩენილი bug-ის გასწორება (Stage 19-ის კოდში):** rapid-fire AI chat request-ებმა (rate-limit ტესტისას) გამოავლინა, რომ Gemini-ს **საკუთარი** (external) rate limit-ის დარტყმისას `GeminiChatClient.EnsureSuccessStatusCode()`-მა unhandled `HttpRequestException` ისროდა → უსამართლო 500 raw stack trace-ით. დავამატე try/catch `AiChatEndpoints.Chat`-ში — Gemini-ის ნებისმიერი HTTP failure-ზე ახლა სუფთა 503 ქართული შეტყობინებით ბრუნდება, არა 500. ეს პირდაპირ Task 21-ის საერთო resilience-თემას ემსახურება, თუმცა მექანიზმი (Gemini quota) განსხვავდება Redis-ისგან.

**რეფაქტორი:** არ დასჭირდა — ახალი `Caching/` namespace მთლიანად RecommendationService-ის შიგნითაა, `CareerProject.Shared`-ში არ გატანილა (ჯერ მხოლოდ ერთ სერვისს სჭირდება, DRY-on-third-use პრინციპის მიხედვით).

ეტაპი 22 (ტესტირება) დასრულებულია და დადასტურებულია — 42 .NET ტესტი (4 ახალი/გაფართოებული პროექტი) + 6 Playwright E2E სცენარი, ყველა რეალურად გაშვებული და გამწვანებული.

**Unit Tests** (სწრაფი, infrastructure არ სჭირდება):
- Skill overlap + hybrid score — უკვე დაფარული იყო Stage 15-ის `HybridMatchingCalculatorTests`-ით (11 ტესტი).
- **Role validation** — ახალი `CareerProject.Shared.Tests/Unit/RequestValidatorTests.cs` (generic) + `CareerProject.UserService.Tests/Unit/RegistrationRoleValidationTests.cs` (`RegisterPersonRequest`/`RegisterCompanyRequest`/`LoginRequest` კონკრეტულად — ამოწმებს, რომ არასწორი მოთხოვნა ვერასდროს მიაღწევს User-ის+role-ის შექმნამდე).
- **Application duplicate prevention** — ახალი, სუფთა `CareerProject.JobService/Services/ApplicationRules.cs` (`IsDuplicate`), ამოღებული `ApplicationEndpoints.SubmitApplication`-ის inline query-იდან (იგივე Stage 15-ის `HybridMatchingCalculator`-ის ამოღების პატერნი) — 3 unit ტესტი `CareerProject.JobService.Tests/Unit/`-ში.

**Integration Tests** (რეალურ dev Postgres/RabbitMQ-ზე, `docker compose up -d` საჭირო):
- **PostgreSQL** — ახალი `CareerProject.Shared.Tests/Integration/PostgresIntegrationTests.cs`: insert real user → ახალი `DbContext` instance-ით readback.
- **RabbitMQ** — ახალი `CareerProject.Shared.Tests/Integration/RabbitMqIntegrationTests.cs`: `RabbitMqEventPublisher` → real exchange/queue → `RabbitMqConsumerBase<T>` subclass — Stage 13-ის scratch harness-ის ფორმალიზებული ვერსია, ახლა commit-ში.
- **Auth flow** — ახალი `CareerProject.UserService.Tests/Integration/AuthFlowIntegrationTests.cs`, `WebApplicationFactory<Program>`-ით: register→login, დუბლირებული email → 409, არასწორი პაროლი/უცნობი email → 401.
- **Job CRUD** — ახალი `CareerProject.JobService.Tests/Integration/JobCrudIntegrationTests.cs`: create/get/list/update/delete, role 403 (Person), ownership 403 (სხვა კომპანია), token-ის გარეშე 401.
- `WebApplicationFactory<Program>`-ისთვის საჭირო გახდა `public partial class Program {}` მარკერის დამატება `UserService`/`JobService`-ის `Program.cs`-ების ბოლოში (სტანდარტული .NET pattern top-level statements-ის მქონე პროექტისთვის, runtime ქცევას არ ცვლის).
- ყველა integration ტესტი თავად ასუფთავებს შექმნილ მონაცემებს (`IAsyncLifetime.DisposeAsync` / `finally`) — გადამოწმდა, არცერთი ტესტის შემდეგ ბაზაში/RabbitMQ-ში ნაშთი არ დარჩენილა.

**E2E (Playwright, Angular)**: `frontend/career-project-web/e2e/` + `playwright.config.ts`, `npm run test:e2e`. 6 სცენარი (ყველა Task 22-ის სიიდან): person registration, profile creation, company registration, vacancy creation, recommendation viewing, apply. Setup-ისთვის (რეგისტრაცია/პროფილი) პირდაპირ API-ს იყენებს (`e2e/helpers.ts`), რომ თითო test ფოკუსირებული დარჩეს საკუთარ სცენარზე; auth localStorage-ში `addInitScript`-ით injected-ია (იგივე მექანიზმი, რასაც `AuthService` თავად წერს).

**აღმოჩენა — "Apply" E2E-ს ვერ დავწერდი UI-ს გარეშე:** Stage 17-ის Application flow მხოლოდ backend-ად დარჩა (curl-ით ტესტილი), frontend-ში "განაცხადის გაგზავნა" ღილაკი არსად არსებობდა. Task 22 კი პირდაპირ ითხოვს "Apply" E2E სცენარს — ამიტომ დავამატე მინიმალური apply ღილაკი `/jobs/recommended`-ის job-card-ებზე (`JobService.applyToJob()`, ღილაკის loading/applied/error state-ები), იგივე პრინციპით რაც Stage 12-ის `Job.WorkFormat`/`Salary` ველების დამატებას ჰქონდა — მომდევნო ტასკის საკუთარი მოთხოვნიდან გამომდინარეობს, არა თვითნებური scope-ის გაფართოება.

**რეალურად შემოწმდა:** ყველა 4 .NET ტესტ-პროექტი ერთად გაეშვა (`dotnet test backend/CareerProject.sln`) — 42/42 გამწვანდა; Playwright-ის 6/6 სცენარი რეალურ სრულ სტეკზე (Docker + 5 backend + `ng serve`) გამწვანდა. გზაში ერთი საკუთარი ტესტის ბაგი ვიპოვე და გავასწორე — Playwright-ის strict-mode-მა ზუსტად აჩვენა მიზეზი ("C#" ტექსტი გვერდზე ორჯერ ჩნდება, skill-სიაში და live preview-ში) — ეს არის ზუსტად Task 22-ის "თითო failing test-ის მიზეზი მკაფიოდ უნდა ჩანდეს" მოთხოვნის რეალური დემონსტრაცია.

**precision@k/recall@k** (ნაშრომში ნახსენები, PLAN.md-ის შენიშვნის მიხედვით) — ამ ეტაპზე არ აშენებულა: მოითხოვს ground-truth რელევანტობის judgment-ебს რეალურ candidate-vacancy წყვილებზე, რაც სცილდება ამ პროექტის სატესტო მონაცემების (6 დემო ვაკანსია) მასშტაბს — თუ საჭირო გახდება, ცალკე task-ად უნდა მოთხოვო.

შემდეგი: **ეტაპი 23 — Production-like Docker Compose გაშვება**.

---

## PLAN.md-გარეთა დიდი დავალება: Audit + Chat + Uploads + Dashboard-ების redesign

ეს არ არის PLAN.md-ის ერთ-ერთი Stage — ცალკე, დიდი, მომხმარებლის მოთხოვნით, ერთბაშად შესრულებული დავალება (Stage 22-სა და Stage 23-ს შორის), რომელმაც შეგნებულად დაარღვია "ერთ ეტაპზე მეტს ერთდროულად ნუ შეასრულებ" წესი — მომხმარებელმა ეს პირდაპირ დაადასტურა კითხვაზე პასუხით ("ერთიანად, დიდი ცვლილებით"), მას შემდეგ რაც გავაფრთხილე რისკებზე.

**აუდიტის შედეგი (დავალების საწყისი პრემისა ნაწილობრივ არასწორი იყო):** Vacancy Creation, Apply და Company Application Management **უკვე მუშაობდა** (Stage 11/12/17-დან) — გადამოწმდა ხელახლა curl-ითაც და ახალი E2E ტესტებითაც. რეალურად აკლდა: Logout-ის ხილვადი UI ღილაკი (`AuthService.logout()` არსებობდა, მაგრამ არსად არ იძახებოდა), Company-ს candidates-ის UI გვერდი (backend Stage 17-დან იყო, frontend არასდროს აშენებულა), employer↔candidate chat (საერთოდ არ არსებობდა), photo/logo upload (არ არსებობდა), და dashboard-ები იყო თითქმის ცარიელი placeholder.

### ახალი Entities და migration
`PersonProfile.PhotoUrl`, `Conversation` (`ApplicationId` UNIQUE index — 1 Application → მაქს. 1 Conversation), `DirectMessage` — ერთი migration (`AddChatUploadsFeatures`), გატარებული რეალურ dev Postgres-ზე, არსებული migrations ხელუხლებელია.

### ახალი/შეცვლილი Backend endpoints
- `POST /api/profile/me/photo`, `POST /api/company/me/logo` — multipart upload, MIME whitelist (jpeg/png/webp), 5MB ლიმიტი (`Upload:MaxImageBytes` config), random filename (`Guid`), `LocalFileStorage : IFileStorage` აბსტრაქციით (მომავალში S3-ზე გადასართველად).
- `GET /api/applications/mine`, `GET /api/company/applications` — ორივე აკლდა (candidate-ს არ ჰქონდა საკუთარი განაცხადების ნახვის, company-ს კი ყველა-ვაკანსიაზე-ერთად ნახვის გზა).
- `GET /api/conversations`, `GET /api/conversations/{id}/messages`, `POST /api/conversations/{id}/messages` — ორივე მხარისთვის, ownership check (მონაწილე არ ხარ → 403).
- `PATCH /api/applications/{id}/status`-ს დაემატა: `Accepted`-ზე გადასვლისას ავტომატურად, idempotently იქმნება `Conversation` (`ConversationService.EnsureConversationForAcceptedApplicationAsync`) — მეორედ Accepted-ზე გადაყვანა დუბლიკატს არ ქმნის.
- `ApplicationResponse` გამდიდრდა (`CompanyName`, `PersonHeadline`, `PersonPhotoUrl`, `PersonSkills`), `CompanyProfileResponse`-ს დაემატა `Id` (frontend-ს სჭირდებოდა "ჩემი ვაკანსიები" გასაფილტრად).
- **SignalR** (`ChatHub`, `/hub/chat`) — realtime მხოლოდ delivery-ზეა პასუხისმგებელი, history/persistence REST-ზეა. JWT browser-ის WebSocket-ს Authorization header ვერ ატანს → query-string token (`?access_token=`) დამატებულია **ორივეს**, JobService-ის და Gateway-ის JWT bearer config-ს.

### აღმოჩენილი და გასწორებული 2 რეალური ბაგი (ორივე ამ ახალი ფუნქციონალის ტესტირებისას)
1. **SignalR race condition:** `joinConversation()` invoke-ს იძახებდა კავშირის `start()`-ის დასრულებამდე, შეცდომას `.catch(() => {})` ჩუმად ყლაპავდა — group-ში join საერთოდ არ ხდებოდა. გასწორდა `connectPromise`-ის გაზიარებით.
2. **Gateway CORS:** `AllowCredentials()` აკლდა Gateway-ის CORS policy-ს — SignalR-ის browser client ყოველთვის `credentials: include`-ს იყენებს negotiate request-ზე, რაც უჩუმრად ბლოკავდა კავშირს. დადასტურდა ორივე ბაგი რეალურ ბრაუზერში, browser console-ის ჩაწერით, არა ვარაუდით.

### ახალი Frontend pages/components
`SidebarNavComponent` (როლის მიხედვით სხვადასხვა ბმულები + გასვლა), `StatCardComponent`, `StatusBadgeComponent`, `AvatarComponent`, `EmptyStateComponent`; `PersonDashboardComponent`/`CompanyDashboardComponent` — სრული redesign, ყველა მონაცემი რეალური API-დან; ახალი გვერდები `/applications`, `/company/applicants`, `/company/profile`, `/messages`; `ChatHubService` (`@microsoft/signalr`), `SessionService` (logout ორკესტრაცია — SignalR disconnect + auth clear + redirect, გამოძახებადი როგორც sidebar-ის ღილაკიდან, ისე 401 interceptor-იდან).

**შეგნებული scope-შეზღუდვები:** Profile Wizard/Job Create/Recommendations გვერდებს sidebar არ დავამატე (რომ Stage 9/12/16/20-ის უკვე მუშა, დატესტილი layout არ დამენგრია) — მათი არსებული "← უკან" navigation საკმარისია. Toast/confirmation-dialog infrastructure არ ავაშენე — არსებული inline success/error pattern გამოვიყენე ყველგან, დავალების საკუთარი "არ გააკეთო overengineering" წესის მიხედვით. "პარამეტრები" sidebar-item არცერთ როლს არ დავამატე — არცერთგან არ იყო კონკრეტული მოთხოვნა, ცარიელი placeholder გვერდი აზრს მოკლებული იქნებოდა.

### რეალურად შემოწმდა
**Backend:** 59 .NET ტესტი (4 პროექტი) — ახალი: `ApplicationChatFlowIntegrationTests` (5 ტესტი: duplicate apply, ownership, **Accepted → ზუსტად 1 Conversation თუნდაც ორჯერ Accept-ის შემდეგ**, მონაწილეებს შორის message გაცვლა + outsider 403, Rejected-ზე Conversation არ იქმნება), `PhotoUploadIntegrationTests` (3 ტესტი: არასწორი MIME → 400, ვალიდური PNG → 200 + URL, token-ის გარეშე → 401), `ImageUploadValidatorTests` (6 unit ტესტი).

**E2E (Playwright):** ახალი `full-hire-flow.spec.ts` — **ორი დამოუკიდებელი browser context/SignalR კავშირით** (რეალური realtime დადასტურებისთვის, არა mock-ით): company რეგისტრაცია → ვაკანსია → person რეგისტრაცია → apply → company ხედავს კანდიდატს → Accept → **ორივე მხარეს ცალკე ჩნდება chat, ცალკე SignalR connection-ით realtime message-ები მიდის ორივე მიმართულებით (< 10წმ, გვერდის reload-ის გარეშე)** → ორივე Logout → person ხელახლა login → application/chat history კვლავ არსებობს. + `logout.spec.ts` (2 ტესტი: ღილაკით logout + browser-back დაცვა, invalid token → ავტომატური logout 401 interceptor-იდან). სულ 9/9 E2E სცენარი გამწვანდა.

**ვიზუალური QA:** რეალური photo/logo upload (ნამდვილი PNG ფაილით, curl-ის ნაცვლად Playwright multipart-ით) → screenshot-ებით დადასტურდა, რომ ატვირთული სურათი რეალურად render-დება (არა მხოლოდ fallback initial) `AvatarComponent`-ში ყველგან — dashboard-ებზე, wizard-ში, applicants-გვერდზე, company profile-ზე. Console errors — არცერთგან.

**Migration/data უსაფრთხოება:** ყველა ტესტმა თავად გაასუფთავა შექმნილი მონაცემები (`e2e-*`/`*-test-*` prefix კონვენციით) — არსებული დემო მონაცემები (TBC Bank, Nino და ა.შ.) ხელუხლებელია.

### შემდგომი მცირე გასწორებები (იმავე დიდი დავალების ფარგლებში, მომხმარებლის რეალური გამოცდის შემდეგ)

- **ჩატის discoverability:** `company-applicants` გვერდზე `Accepted`-ზე გადაყვანის შემდეგ არსად არ ჩანდა პირდაპირი ბმული ახლადშექმნილ chat-ზე — მომხმარებელმა თავად ვერ იპოვა. დაემატა "💬 ჩატის გახსნა" ბმული (`/messages?conversationId=...`) სტატუსის select-ის გვერდით, `ConversationService`-ით.
- **ლოგოს კლიკადობა:** ზედა-მარცხენა "კარიერა" ლოგო (`brand`/`sidebar-brand`) ყველგან `<div>` იყო, არა ბმული — გადაკეთდა `<a [routerLink]="auth.dashboardRouteForCurrentRole()">`-ად ყველა გვერდზე (sidebar + auth-shared + profile-wizard + job-create + job-recommendations).
- **Responsive/mobile:** `SidebarNavComponent` გახდა hamburger-ით გახსნადი off-canvas drawer (`@media max-width:768px`, backdrop, `position:fixed`+`transform`), dashboard/form გვერდებზე დაემატა mobile breakpoint-ები (768px სტრუქტურული, 480-560px card-stacking). `messages` გვერდზე single-pane mobile toggle (სია ↔ chat, back ღილაკით). **ტესტირებისას ნაპოვნი 2 ბაგი:** (1) `.sidebar-backdrop`-ს ჰქონდა default `display:none`, mobile media query-ში display აღარ განესაზღვრა თავიდან — backdrop DOM-ში იყო, მაგრამ უხილავი/დაუწკაპუნებელი, drawer ვერასდროს დაიხურებოდა მასზე დაჭერით; (2) messages გვერდი ავტომატურად ირჩევდა პირველ conversation-ს page load-ზე (desktop-ზე გამართლებული — ორივე panel ერთად ჩანს), მაგრამ mobile-ზე ეს პირდაპირ chat-ში აგდებდა, სია დამალული, უკან დაბრუნების ღილაკის გარეშე ეს ტუპიკად გამოიყურებოდა — გასწორდა: mobile viewport-ზე auto-select არ ხდება (მხოლოდ პირდაპირი `?conversationId=` deep link მუშაობს), დესკტოპზე ძველებურად რჩება. ორივე ბაგი რეალურ iPhone SE ზომაზე (Playwright) დადასტურდა/გასწორდა, screenshot-ებით — არცერთი გვერდი არ ახდენს ჰორიზონტალურ overflow-ს (`scrollWidth`-ის შემოწმებით 13 გვერდზე).
