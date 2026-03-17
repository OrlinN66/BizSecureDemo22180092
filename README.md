# BizSecureDemo 22180092

## Описание
Учебен проект за демонстрация на уязвимости и защити в бизнес уеб приложение.  
Курс: **Информационна сигурност в бизнес приложенията** – УНСС

## Демонстрирани уязвимости и защити

| Упражнение | Уязвимост | Статус |
|---|---|---|
| Упр. 1 | IDOR (Insecure Direct Object Reference) | ✅ Фиксирано |
| Упр. 2 | Stored XSS | ✅ Фиксирано |
| Упр. 4 | Brute force (rate limit + account lockout) | ✅ Фиксирано |
| Упр. 5 | SQL Injection | ✅ Фиксирано |
| Упр. 6 | Replay Attack | ⚠️ Демонстрирано (умишлено уязвимо) |

## Упражнение 6 – Replay Attack

### Какво е Replay Attack?
Атака, при която нападателят прихваща валидна HTTP заявка и я изпраща повторно. Системата я приема като нова легитимна заявка, тъй като няма механизъм за еднократност.

### Демото (`/ReplayDemo`)
Симулирана банкова операция за теглене. Контролерът **умишлено не прилага защита** срещу повторни заявки.

**Уязвим код** – `ReplayDemoController.cs`:
- Проверява само дали токенът е `SECRET123` и дали има наличност
- Не проверява дали заявката вече е изпълнена
- `[ValidateAntiForgeryToken]` предпазва от CSRF, но **НЕ** от Replay Attack

### Как се демонстрира (с Burp Suite)
1. Стартирай приложението и влез с акаунт (`alice@test.com` / `Password123!`)
2. Отвори **Burp Suite** → Proxy → изпрати заявка за теглене от браузъра
3. В **Proxy History** намери `POST /ReplayDemo/Withdraw`
4. Изпрати я в **Repeater** и натисни **Send** многократно
5. Всяко повторение намалява баланса – въпреки че оригиналната заявка е само една

### Как се предотвратява
- **Nonce** – случаен токен за еднократна употреба, съхранен в DB/кеш
- **Timestamp** – заявки по-стари от N секунди се отхвърлят
- **RequestId** – уникален идентификатор на заявка, проверяван за дубликати
- **Cryptographic signature** – подпис включващ nonce + timestamp

## Технологии
- .NET 8 / ASP.NET Core MVC
- Entity Framework Core + Microsoft SQL Server (LocalDB)
- Bootstrap 5.3
- Cookie Authentication

## Как да стартирате

### Изисквания
- .NET 8 SDK
- SQL Server или LocalDB

### Настройка на базата данни
Добавете connection string в User Secrets (НЕ в appsettings.json):

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Server=(localdb)\\MSSQLLocalDB;Database=BizSecureDemo22180092;Trusted_Connection=True;TrustServerCertificate=True"
```

### Стартиране
```bash
dotnet ef database update   # Прилага миграциите и създава базата
dotnet run
```

Отворете браузър на `https://localhost:<port>` (портът е изписан в конзолата).

## Структура на проекта
```
BizSecureDemo22180092/
├── Controllers/
│   ├── AccountController.cs   # Register / Login (+ lockout) / Logout
│   ├── HomeController.cs      # Landing page + публични поръчки
│   └── OrdersController.cs    # Create / Details (IDOR fix) / Search (SQL Injection fix)
├── Data/
│   └── AppDbContext.cs
├── Migrations/
├── Models/
│   ├── AppUser.cs             # + FailedLogins, LockoutUntilUtc
│   └── Order.cs
├── ViewModels/
│   ├── RegisterVm.cs
│   ├── LoginVm.cs
│   └── CreateOrderVm.cs
└── Views/
    ├── Account/  Register.cshtml, Login.cshtml
    ├── Home/     Index.cshtml
    └── Orders/   Details.cshtml, SearchResults.cshtml
```

## Бележки за сигурността (резюме)

### Ex 1 – IDOR Fix
`OrdersController.Details` проверява `o.UserId == uid`, за да не позволи достъп до чужди записи.

### Ex 2 – XSS Fix
`Details.cshtml` използва `@Model.Title` (Razor auto-escape) вместо `@Html.Raw(Model.Title)`.

### Ex 4 – Brute Force Fix
- **Account lockout**: след 5 грешни опита акаунтът се заключва за 5 мин.  
- **Rate limiter**: максимум 5 POST заявки/мин към `/Account/Login` (фиксирано прозоречно ограничение).

### Ex 5 – SQL Injection Fix
- **Параметризирани заявки**: `OrdersController.Search` използва параметризирани SQL заявки (`FromSqlRaw` с плейсхолдъри {0}, {1})
- Потребителският вход се подава като параметър, не чрез string concatenation
- Предотвратява SQL Injection атаки като `' OR 1=1 --`
