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

ეტაპი 1 დასრულებულია: 6 პროექტი + `.sln`, build succeeds, GitHub-ზეც აიტვირთა. შემდეგი: **ეტაპი 2 — Docker ინფრასტრუქტურა**.
