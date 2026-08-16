# Kariera

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
- **Frontend:** Angular (`frontend/kariera-web`)
- **Infra:** Docker Compose, Redis, RabbitMQ
- **AI:** Gemini API (embeddings + RAG chat)

## სტრუქტურა

```text
backend/
├── Kariera.ApiGateway/
├── Kariera.UserService/
├── Kariera.JobService/
├── Kariera.RecommendationService/
├── Kariera.NotificationService/
└── Kariera.Shared/
frontend/kariera-web/
docker/
docker-compose.yml
```

## მიმდინარე სტატუსი

ეტაპი 1 დასრულებულია: 6 პროექტი + `.sln`, build succeeds. შემდეგი: **ეტაპი 2 — Docker ინფრასტრუქტურა**.
