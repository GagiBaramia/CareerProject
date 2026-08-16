# კარიერა — AI-ით რეალიზაციის ტექნიკური დავალებების გეგმა

## როგორ ვიმუშაოთ AI-სთან

პროექტი ააწყვე ეტაპობრივად. ერთდროულად მთელი სისტემის დაწერა არ მოსთხოვო AI-ს.

ყოველი დავალების შემდეგ:

* [ ] გაუშვი პროექტი
* [ ] დარწმუნდი, რომ build წარმატებულია
* [ ] შეამოწმე კონკრეტული ფუნქცია
* [ ] მხოლოდ ამის შემდეგ გადადი შემდეგ დავალებაზე
* [ ] ყოველი დასრულებული ეტაპის შემდეგ გააკეთე Git commit

AI-ს ყოველთვის უთხარი:

> არსებული კოდი არ დაანგრიო. ჯერ შეისწავლე პროექტის სტრუქტურა, შემდეგ შეასრულე მხოლოდ მოცემული დავალება. არ შეცვალო სხვა მოდულები საჭიროების გარეშე. დასრულებისას ჩამომიწერე რომელი ფაილები შექმენი ან შეცვალე, როგორ გავუშვა და როგორ შევამოწმო შედეგი.

---

# ეტაპი 1 — პროექტის საწყისი სტრუქტურა

## Task 1 — Solution-ისა და პროექტების შექმნა

AI-ს მიეცი:

> ვიწყებ საბაკალავრო პროექტს სახელად **CareerProject**.
>
> Backend უნდა იყოს .NET/C#-ზე, frontend Angular-ზე.
>
> შექმენი საწყისი repository structure:
>
> ```text
> CareerProject/
> ├── backend/
> │   ├── CareerProject.ApiGateway/
> │   ├── CareerProject.UserService/
> │   ├── CareerProject.JobService/
> │   ├── CareerProject.RecommendationService/
> │   ├── CareerProject.NotificationService/
> │   └── CareerProject.Shared/
> ├── frontend/
> │   └── career-project-web/
> ├── docker/
> ├── docker-compose.yml
> ├── .gitignore
> └── README.md
> ```
>
> თითოეული .NET პროექტი ამ ეტაპზე იყოს მინიმალური ASP.NET Core Web API.
>
> შექმენი საერთო `.sln` ფაილიც.
>
> ჯერ არ დაამატო ბიზნეს ლოგიკა.
>
> საბოლოოდ ყველა backend პროექტი წარმატებით უნდა build-დებოდეს.

**შედეგი:** გვაქვს ცარიელი, მაგრამ სწორად ორგანიზებული პროექტი.

---

# ეტაპი 2 — Docker ინფრასტრუქტურა

## Task 2 — PostgreSQL, pgvector, Redis და RabbitMQ

AI prompt:

> პროექტში დაამატე development გარემოსთვის `docker-compose.yml`.
>
> საჭიროა:
>
> * PostgreSQL pgvector მხარდაჭერით
> * Redis
> * RabbitMQ
> * RabbitMQ Management UI
>
> გამოიყენე environment variables და `.env.example`.
>
> მონაცემთა ბაზის სახელი იყოს:
>
> `career_project_db`
>
> არ ჩაწერო პაროლები source code-ში.
>
> Docker Compose-ის გაშვების შემდეგ მინდა მუშაობდეს:
>
> * PostgreSQL
> * pgvector extension
> * Redis
> * RabbitMQ
>
> README-ში დაამატე გაშვების ინსტრუქცია.
>
> ბრძანება უნდა იყოს დაახლოებით:
>
> `docker compose up -d`
>
> ბოლოს მომეცი შემოწმების ნაბიჯები.

**Acceptance criteria:**

* [x] PostgreSQL მუშაობს
* [x] pgvector ჩაიტვირთა
* [x] Redis მუშაობს
* [x] RabbitMQ მუშაობს
* [x] RabbitMQ Management UI იხსნება

---

# ეტაპი 3 — მონაცემთა ბაზის მოდელი

## Task 3 — Entity-ების შექმნა

AI prompt:

> შექმენი CareerProject პროექტის მონაცემთა მოდელი Entity Framework Core-ით.
>
> საჭიროა შემდეგი entity-ები:
>
> ### User
>
> * Id
> * Email
> * PasswordHash
> * Role
> * CreatedAt
>
> Role:
>
> * Person
> * Company
>
> ### PersonProfile
>
> * Id
> * UserId
> * FullName
> * Headline
> * CvSummary
> * Location
> * Embedding
>
> ### Skill
>
> * Id
> * Name
>
> ### PersonSkill
>
> * PersonId
> * SkillId
> * Level
>
> ### Company
>
> * Id
> * UserId
> * Name
> * Description
> * LogoUrl
> * Industry
>
> ### Job
>
> * Id
> * CompanyId
> * Title
> * Description
> * EmploymentType
> * Location
> * Embedding
> * CreatedAt
>
> ### JobSkill
>
> * JobId
> * SkillId
> * RequiredLevel
>
> ### Application
>
> * Id
> * JobId
> * PersonId
> * Status
> * AppliedAt
>
> ### ChatMessage
>
> * Id
> * PersonId
> * Role
> * Content
> * CreatedAt
>
> PostgreSQL-ის `vector` ტიპისთვის გამოიყენე pgvector-ის შესაბამისი .NET integration.
>
> გააკეთე foreign key-ები და relationships.
>
> `PersonSkill` და `JobSkill` იყოს many-to-many დამაკავშირებელი entity-ები.
>
> შექმენი პირველი EF Core migration.
>
> ამ ეტაპზე API endpoint-ები არ შექმნა.

ეს მოდელი პირდაპირ შეესაბამება ნაშრომში განსაზღვრულ მონაცემთა ბაზას.

---

# ეტაპი 4 — ავტორიზაცია

## Task 4 — Person / Company რეგისტრაცია

AI prompt:

> CareerProject.UserService-ში ააწყე Authentication.
>
> საჭიროა:
>
> `POST /api/auth/register/person`
>
> `POST /api/auth/register/company`
>
> `POST /api/auth/login`
>
> გამოიყენე:
>
> * JWT
> * password hashing
> * role claim
>
> ორი როლი:
>
> `Person`
>
> `Company`
>
> წარმატებული login-ის დროს დააბრუნე JWT access token და მომხმარებლის ძირითადი ინფორმაცია.
>
> გააკეთე DTO-ები, validation და შესაბამისი HTTP status code-ები.
>
> password არასოდეს შეინახო plaintext ფორმით.
>
> დაამატე Swagger-იდან endpoint-ების შემოწმების შესაძლებლობა.
>
> ბოლოს მომეცი სამი სატესტო request:
>
> 1. Person registration
> 2. Company registration
> 3. Login

---

# ეტაპი 5 — API Gateway

## Task 5 — Gateway

AI prompt:

> ააწყე `CareerProject.ApiGateway`.
>
> გამოიყენე .NET-ისთვის შესაფერისი reverse proxy/gateway გადაწყვეტა.
>
> Gateway უნდა იყოს Angular frontend-ის ერთადერთი backend entry point.
>
> გააკეთე routing:
>
> `/api/auth/*` → UserService
>
> `/api/users/*` → UserService
>
> `/api/jobs/*` → JobService
>
> `/api/applications/*` → JobService
>
> `/api/recommendations/*` → RecommendationService
>
> `/api/ai/*` → RecommendationService
>
> `/api/notifications/*` → NotificationService
>
> JWT authentication გაატარე Gateway-ის დონეზეც.
>
> დაამატე CORS Angular development URL-ისთვის.
>
> ჯერ rate limiting არ დაამატო.

---

# ეტაპი 6 — Skills

## Task 6 — Skills dictionary

AI prompt:

> UserService-ში შექმენი საერთო Skills Dictionary.
>
> დაამატე seed მონაცემები მინიმუმ:
>
> * C#
> * .NET
> * Java
> * Spring Boot
> * JavaScript
> * TypeScript
> * Angular
> * React
> * Node.js
> * HTML
> * CSS
> * PostgreSQL
> * SQL
> * Redis
> * RabbitMQ
> * Docker
> * Git
> * REST API
> * AWS
>
> API:
>
> `GET /api/skills`
>
> `GET /api/skills?search=rea`
>
> search უნდა იყოს case-insensitive.
>
> საბოლოოდ endpoint შესაძლებელი უნდა იყოს Angular autocomplete-იდან გამოსაყენებლად.

---

# ეტაპი 7 — Candidate Profile / CV

## Task 7 — Person Profile API

AI prompt:

> UserService-ში ააწყე ავტორიზებული Person-ის პროფილის ფუნქციონალი.
>
> Endpoint-ები:
>
> `GET /api/profile/me`
>
> `PUT /api/profile/me`
>
> `PUT /api/profile/me/skills`
>
> პროფილი შეიცავდეს:
>
> * fullName
> * headline
> * cvSummary
> * location
> * skills
>
> თითო skill-ს ჰქონდეს proficiency level:
>
> * Beginner
> * Intermediate
> * Advanced
> * Expert
>
> Company მომხმარებელი Person profile endpoint-ებს ვერ უნდა იყენებდეს.
>
> update-ის შემდეგ გამოაქვეყნე RabbitMQ event:
>
> `ProfileUpdated`
>
> ახალი პროფილის შემთხვევაში:
>
> `ProfileCreated`

---

# ეტაპი 8 — Angular-ის პირველი რეალური გვერდი

## Task 8 — Login/Register UI

AI prompt:

> Angular frontend-ში შექმენი CareerProject-ს authentication UI.
>
> დიზაინი იყოს იმავე სტილში, როგორც ჩვენი mockup-ები:
>
> * თეთრი background
> * ლურჯი primary accent
> * მარცხნივ „კარიერა“ logo/brand
> * სუფთა თანამედროვე ფორმები
>
> შექმენი:
>
> `/login`
>
> `/register`
>
> რეგისტრაციისას მომხმარებელი ირჩევდეს:
>
> * კანდიდატი
> * კომპანია
>
> API requests წავიდეს მხოლოდ API Gateway-ზე.
>
> JWT შეინახე უსაფრთხო client-side მიდგომით პროექტის ამ development ეტაპისთვის.
>
> დაამატე Angular HTTP interceptor Authorization header-ისთვის.
>
> login-ის შემდეგ role-ის მიხედვით გადაამისამართე შესაბამის dashboard-ზე.

---

# ეტაპი 9 — CV-ის UI

## Task 9 — Profile Wizard

ეს უკვე უნდა დაემსგავსოს ჩვენს გაკეთებულ სქრინს.

AI prompt:

> Angular-ში ააწყე კანდიდატის პროფილის მრავალსაფეხურიანი ფორმა.
>
> Route:
>
> `/profile/edit`
>
> Step-ები:
>
> 1. პირადი ინფორმაცია
> 2. გამოცდილება
> 3. განათლება
> 4. დამატებითი ინფორმაცია
>
> პირველ რეალიზაციაში აუცილებელია:
>
> * Full name
> * Headline
> * Summary
> * Location
> * Skills autocomplete
> * Skill proficiency
>
> მარჯვენა მხარეს რეალურ დროში აჩვენე Profile Preview.
>
> ვიზუალურად მაქსიმალურად დაემსგავსოს CareerProject-ს არსებულ დიზაინ mockup-ს.
>
> ფორმა დააკავშირე რეალურ backend API-სთან.
>
> mock data არ გამოიყენო იქ, სადაც backend endpoint უკვე არსებობს.

---

# ეტაპი 10 — Company

## Task 10 — Company Profile

AI prompt:

> JobService-ში შექმენი Company profile.
>
> API:
>
> `GET /api/company/me`
>
> `PUT /api/company/me`
>
> ველები:
>
> * name
> * description
> * industry
> * logoUrl
>
> მხოლოდ Company role-ს ჰქონდეს წვდომა.

---

# ეტაპი 11 — ვაკანსიები

## Task 11 — Job CRUD

AI prompt:

> JobService-ში ააწყე ვაკანსიების CRUD.
>
> საჭიროა:
>
> `POST /api/jobs`
>
> `GET /api/jobs`
>
> `GET /api/jobs/{id}`
>
> `PUT /api/jobs/{id}`
>
> `DELETE /api/jobs/{id}`
>
> Job-ის ველები:
>
> * title
> * description
> * employmentType
> * location
> * requiredSkills
>
> თითო required skill-ს ჰქონდეს requiredLevel.
>
> Job შექმნა/რედაქტირება მხოლოდ Company role-ს შეეძლოს.
>
> შექმნის შემდეგ გამოაქვეყნე:
>
> `JobCreated`
>
> განახლების შემდეგ:
>
> `JobUpdated`

---

# ეტაპი 12 — Company UI

## Task 12 — Vacancy Creation Page

AI prompt:

> Angular-ში ააწყე Company-ის „ახალი ვაკანსიის გამოქვეყნება“ გვერდი.
>
> ზუსტად გამოიყენე CareerProject-ს ჩვენ მიერ შექმნილი დიზაინის სტილი.
>
> ფორმა:
>
> * ვაკანსიის სათაური
> * აღწერა
> * ადგილმდებარეობა
> * სამუშაო გრაფიკი
> * სამუშაო ფორმატი
> * საჭირო უნარები
> * ხელფასის შუალედი
>
> skills უნდა მოდიოდეს Skills API-დან.
>
> Submit-ზე შეიქმნას რეალური Job backend-ში.
>
> გვერდზე აღარ გამოიყენო სტატიკური mock data, გარდა კანდიდატების გვერდითი მაგალითებისა, სანამ Application ფუნქციონალი არ გაკეთდება.

---

# ეტაპი 13 — RabbitMQ

## Task 13 — Event Bus

AI prompt:

> პროექტში შექმენი RabbitMQ-ზე დაფუძნებული საერთო event infrastructure.
>
> CareerProject.Shared-ში შექმენი event contracts:
>
> * ProfileCreated
> * ProfileUpdated
> * JobCreated
> * JobUpdated
> * ApplicationSubmitted
> * ApplicationStatusChanged
>
> შექმენი publisher და consumer abstraction.
>
> ყველა event-ს ჰქონდეს:
>
> * EventId
> * OccurredAt
> * EntityId
>
> RabbitMQ კავშირის პარამეტრები environment variables-იდან წამოიღე.
>
> retries და basic error handling დაამატე.
>
> ჯერ email notification არ გააკეთო.

---

# ეტაპი 14 — Embedding

## Task 14 — CV და Job embedding

AI prompt:

> RecommendationService-ში შექმენი embedding worker.
>
> `ProfileCreated`, `ProfileUpdated`, `JobCreated`, `JobUpdated` event-ების მიღებისას შესაბამისი ტექსტი გარდაქმენი embedding ვექტორად.
>
> Person-ის embedding ტექსტში გაერთიანდეს:
>
> * headline
> * cvSummary
> * skills
>
> Job embedding ტექსტში:
>
> * title
> * description
> * requiredSkills
>
> embedding-ის გენერაციისთვის გამოიყენე Gemini API.
>
> API key მხოლოდ environment variable-ში უნდა იყოს.
>
> მიღებული embedding შეინახე PostgreSQL pgvector ველში.
>
> დაამატე logging და error handling.
>
> Gemini API-ის წარუმატებლობამ მომხმარებლის profile/job შენახვა არ უნდა გააჩეროს, რადგან embedding ფონური პროცესია.

---

# ეტაპი 15 — Hybrid Matching

## Task 15 — Recommendation Algorithm

ნაშრომში საწყისი ფორმულა უკვე გვაქვს: `0.6 × skill_overlap + 0.4 × semantic_similarity`.

AI prompt:

> RecommendationService-ში შექმენი Hybrid Job Matching.
>
> საბოლოო ქულა:
>
> `score = 0.6 * skillOverlap + 0.4 * semanticSimilarity`
>
> `skillOverlap` გამოითვალოს Person skills-ისა და Job required skills-ის შედარებით.
>
> `semanticSimilarity` გამოითვალოს Person და Job embedding-ების cosine similarity-ით PostgreSQL pgvector-ის გამოყენებით.
>
> 0.6 და 0.4 არ ჩაწერო hardcoded business logic-ში.
>
> გადაიტანე configuration-ში:
>
> `StructuredWeight`
>
> `SemanticWeight`
>
> Endpoint:
>
> `GET /api/recommendations/jobs`
>
> შედეგი დაალაგე score descending.
>
> თითო შედეგი აბრუნებდეს:
>
> * job
> * company
> * score
> * skillOverlap
> * semanticSimilarity
>
> score frontend-ს პროცენტად უნდა შეეძლოს ჩვენება.
>
> დაამატე unit tests ალგორითმისთვის.

---

# ეტაპი 16 — Recommendation UI

## Task 16 — მთავარი Dashboard

AI prompt:

> Angular-ში ააწყე CareerProject კანდიდატის მთავარი რეკომენდაციების გვერდი.
>
> Route:
>
> `/jobs/recommended`
>
> გამოიყენე backend-ის:
>
> `GET /api/recommendations/jobs`
>
> თითო Job card-ზე აჩვენე:
>
> * title
> * company
> * location
> * employment type
> * skills
> * matching percentage
>
> დაამატე:
>
> * location filter
> * employment type filter
> * sorting by matching
>
> ვიზუალურად დაემსგავსოს CareerProject-ს უკვე შექმნილ mockup-ს.
>
> ამ ეტაპზე გვერდზე არსებული ვაკანსიების სია მთლიანად რეალური API-დან უნდა მოდიოდეს.

---

# ეტაპი 17 — Apply

## Task 17 — განაცხადის გაგზავნა

AI prompt:

> JobService-ში დაამატე Application flow.
>
> `POST /api/jobs/{jobId}/apply`
>
> Person-ს ერთსა და იმავე ვაკანსიაზე ორჯერ განაცხადის გაგზავნა არ შეეძლოს.
>
> საწყისი status:
>
> `Submitted`
>
> შესაძლო status-ები:
>
> * Submitted
> * InReview
> * Interview
> * Rejected
> * Accepted
>
> Company API:
>
> `GET /api/company/jobs/{jobId}/applications`
>
> `PATCH /api/applications/{id}/status`
>
> განაცხადის შექმნისას გამოაქვეყნე:
>
> `ApplicationSubmitted`
>
> სტატუსის შეცვლისას:
>
> `ApplicationStatusChanged`

---

# ეტაპი 18 — Notifications

## Task 18 — Notification Service

AI prompt:

> NotificationService-ში დაამატე RabbitMQ consumer-ები.
>
> დაამუშავე:
>
> * ApplicationSubmitted
> * ApplicationStatusChanged
>
> საწყის ეტაპზე ნოტიფიკაციები შეინახე database-ში.
>
> API:
>
> `GET /api/notifications`
>
> `PATCH /api/notifications/{id}/read`
>
> შემდეგ ეტაპზე შესაძლებელი უნდა იყოს email provider-ის დამატება.
>
> ამ ეტაპზე რეალური SMS არ გვჭირდება.

---

# ეტაპი 19 — AI ჩატი

## Task 19 — RAG Chat

AI prompt:

> RecommendationService-ში ააწყე CareerProject AI Job Assistant.
>
> Endpoint:
>
> `POST /api/ai/chat`
>
> request:
>
> ```json
> {
>   "message": "მინდა junior backend პოზიცია დისტანციურად"
> }
> ```
>
> პროცესი:
>
> 1. მიიღე მომხმარებლის ტექსტი.
> 2. შექმენი query embedding.
> 3. pgvector-ის საშუალებით მოძებნე ყველაზე რელევანტური რეალური ვაკანსიები.
> 4. საუკეთესო რამდენიმე ვაკანსიის მონაცემები ჩასვი Gemini prompt-ის context-ში.
> 5. Gemini-მ პასუხი უნდა შექმნას მხოლოდ მიწოდებული ვაკანსიების კონტექსტზე დაყრდნობით.
> 6. პასუხთან ერთად დააბრუნე გამოყენებული job IDs.
> 7. მომხმარებლის და ასისტენტის შეტყობინებები შეინახე CHAT_MESSAGES-ში.
>
> თუ შესაბამისი ვაკანსია არ არსებობს, ასისტენტმა არ უნდა გამოიგონოს არარსებული ვაკანსია.
>
> დაამატე system prompt, რომელიც მკაფიოდ უკრძალავს მოდელს არარსებული ვაკანსიების გამოგონებას.

ეს ზუსტად ნაშრომში აღწერილ RAG პროცესს მიჰყვება. Recommendation Service ჯერ რეალურ ვაკანსიებს ეძებს pgvector-ით და შემდეგ აწვდის მათ Gemini-ს კონტექსტად.

---

# ეტაპი 20 — AI Chat UI

## Task 20 — Chat Panel

AI prompt:

> Angular-ის რეკომენდაციების გვერდის მარჯვენა მხარეს დაამატე AI Assistant panel.
>
> დიზაინი მაქსიმალურად დაემსგავსოს CareerProject mockup-ს.
>
> უნდა ჰქონდეს:
>
> * conversation history
> * user message
> * assistant message
> * loading state
> * error state
> * send button
>
> გამოიყენე რეალური:
>
> `POST /api/ai/chat`
>
> თუ პასუხში დაბრუნდა job IDs, მომხმარებელს შესთავაზე შესაბამისი ვაკანსიების გახსნა.
>
> mock AI response აღარ გამოიყენო.

---

# ეტაპი 21 — Redis

## Task 21 — Cache და Rate Limiting

AI prompt:

> პროექტში გამოიყენე Redis ორი მიზნისთვის.
>
> 1. Recommendation cache
>
> კონკრეტული Person-ის რეკომენდაციების შედეგი მოკლე დროით შეინახე Redis-ში.
>
> ProfileUpdated ან JobUpdated event-ის შემთხვევაში შესაბამისი cache გააუქმე.
>
> 2. AI chat rate limiting
>
> ერთ მომხმარებელს განსაზღვრულ პერიოდში ჰქონდეს შეზღუდული რაოდენობის AI request.
>
> ლიმიტები configuration-ში იყოს და არა hardcoded.
>
> Redis-ის გათიშვის შემთხვევაში ძირითადი job browsing არ უნდა შეწყდეს.

---

# ეტაპი 22 — ტესტირება

## Task 22 — Automated Tests

AI prompt:

> CareerProject პროექტს დაამატე automated tests.
>
> აუცილებელია:
>
> ### Unit Tests
>
> * skill overlap
> * hybrid score
> * role validation
> * application duplicate prevention
>
> ### Integration Tests
>
> * PostgreSQL
> * RabbitMQ
> * auth flow
> * Job CRUD
>
> ### E2E
>
> Angular-ისთვის Playwright:
>
> 1. Person registration
> 2. Profile creation
> 3. Company registration
> 4. Vacancy creation
> 5. Recommendation viewing
> 6. Apply
>
> თითო failing test-ის მიზეზი მკაფიოდ უნდა ჩანდეს.

ნაშრომშიც სწორედ unit, integration და Playwright E2E ტესტებია დაგეგმილი, ხოლო recommendation quality-სთვის `precision@k` და `recall@k` არის გათვალისწინებული.

---

# ეტაპი 23 — Production-like გაშვება

## Task 23 — საბოლოო Docker Compose

AI prompt:

> CareerProject-ს ყველა კომპონენტი ჩასვი Docker-ში.
>
> ერთი `docker compose up -d` ბრძანებით უნდა გაეშვას:
>
> * Angular
> * API Gateway
> * UserService
> * JobService
> * RecommendationService
> * NotificationService
> * PostgreSQL + pgvector
> * Redis
> * RabbitMQ
>
> გამოიყენე environment variables.
>
> API keys და passwords repository-ში არ მოხვდეს.
>
> დაამატე health checks.
>
> README-ში აღწერე სუფთა კომპიუტერზე პროექტის გაშვების სრული ინსტრუქცია.

---

# სამუშაოს რეალური თანმიმდევრობა

არ გადახვიდე პირდაპირ AI chat-ზე.

გააკეთე ამ რიგით:

1. Repository / Solution
2. Docker infrastructure
3. Database
4. Authentication
5. Gateway
6. Skills
7. Person Profile
8. Login/Register Angular
9. Profile UI
10. Company
11. Jobs
12. Job UI
13. RabbitMQ
14. Embeddings
15. Matching
16. Recommendations UI
17. Apply
18. Notifications
19. AI RAG
20. AI Chat UI
21. Redis/cache
22. Tests
23. Final Docker deployment

## პირველი რეალური milestone

პირველი milestone დასრულებულად ჩაითვლება, როდესაც:

* [ ] Docker-ში PostgreSQL, Redis და RabbitMQ მუშაობს
* [ ] Angular იხსნება
* [ ] ყველა .NET service ეშვება
* [ ] Person რეგისტრირდება
* [ ] Company რეგისტრირდება
* [ ] Login აბრუნებს JWT-ს
* [ ] User-ის role სწორად მუშაობს
* [ ] მონაცემები PostgreSQL-ში ინახება

**ჯერ მხოლოდ ამ milestone-მდე მივიდეთ.**

ამის შემდეგ უკვე CV/Profile ფუნქციონალზე გადავალთ.
