# BizSecureDemo 22180092

## Описание
Учебен проект за демонстрация на уязвимости и защити в бизнес уеб приложение.  
Курс: **Информационна сигурност в бизнес приложенията** – УНСС

---

## Демонстрирани уязвимости и защити

| Упражнение | Уязвимост | Статус |
|---|---|---|
| Упр. 1 | IDOR (Insecure Direct Object Reference) | ✅ Фиксирано |
| Упр. 2 | Stored XSS | ✅ Фиксирано |
| Упр. 4 | Brute Force (rate limit + account lockout) | ✅ Фиксирано |
| Упр. 5 | SQL Injection | ✅ Фиксирано |
| Упр. 6 | Replay Attack | ✅ Фиксирано (RequestId nonce) |

---

## Упражнение 1 – IDOR (Insecure Direct Object Reference)

### Какво е IDOR?
Уязвимост, при която потребителят може да достъпи ресурс на друг потребител само като промени ID в URL-а, без допълнителна проверка за права.

**Пример за уязвима заявка:**
```
GET /Orders/Details/3
```
Ако контролерът не проверява собствеността, потребителят вижда чужда поръчка.

### Как е оправено
**`OrdersController.cs` – метод `Details`:**
```csharp
var order = await _db.Orders
    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

if (order == null)
    return NotFound();
```
Поръчката се търси едновременно по `Id` И по `UserId` на логнатия потребител. Ако ID-то принадлежи на друг потребител, резултатът е `null` → `404 Not Found`.

---

## Упражнение 2 – Stored XSS (Cross-Site Scripting)

### Какво е Stored XSS?
Атакуващият записва злонамерен JavaScript в базата данни (напр. в поле за текст). При всяко зареждане на страницата скриптът се изпълнява в браузъра на жертвата.

**Пример за злонамерен вход:**
```html
<script>alert('XSS')</script>
```

### Как е оправено
Razor автоматично escape-ва HTML символи при използване на `@Model.Title`:
```html
<!-- Безопасно – Razor escape-ва < > & " -->
<td>@Model.Title</td>

<!-- Уязвимо – НЕ се използва -->
<td>@Html.Raw(Model.Title)</td>
```
Изходът `<script>alert('XSS')</script>` се рендира като текст `&lt;script&gt;...`, а не като код.

---

## Упражнение 4 – Brute Force

### Какво е Brute Force?
Нападателят автоматично изпраща множество заявки за вход с различни пароли, докато намери правилната.

### Как е оправено

**1. Account Lockout** – след 5 последователни грешни опита акаунтът се заключва за 5 минути:
```csharp
// AccountController.cs
user.FailedLogins++;
if (user.FailedLogins >= 5)
    user.LockoutUntilUtc = DateTime.UtcNow.AddMinutes(5);
```

**2. Rate Limiter** – максимум 5 POST заявки/минута към `/Account/Login` на IP:
```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
    });
});
```

---

## Упражнение 5 – SQL Injection

### Какво е SQL Injection?
Нападателят вмъква SQL код в полета за въвеждане. Ако заявката се конкатенира директно, злонамереният код се изпълнява в базата данни.

**Пример за уязвима заявка:**
```
?query=' OR 1=1 --
```
Резултат при string concatenation: връща ВСИЧКИ записи.

### Как е оправено
**`OrdersController.cs` – метод `Search`** използва параметризирани заявки:
```csharp
// Безопасно – потребителският вход е параметър, не е вграден в SQL
var results = await _db.Orders
    .FromSqlRaw("SELECT * FROM Orders WHERE Title LIKE {0} AND UserId = {1}",
        $"%{query}%", userId)
    .ToListAsync();
```
Параметрите се предават отделно от SQL кода – базата данни ги третира като данни, а не като команди.

---

## Упражнение 6 – Replay Attack

### Какво е Replay Attack?
Атака, при която нападателят прихваща валидна HTTP заявка и я изпраща повторно. Системата я приема като нова легитимна заявка, тъй като няма механизъм за еднократност. Нападателят не е длъжен да знае паролата или да разбива криптирането.

**Пример:** прихваната заявка за теглене на 100 лв., изпратена 5 пъти → 500 лв. изтеглени.

### Уязвимото демо (`/ReplayDemo`)
Симулирана банкова операция за теглене от сметка с начален баланс 1000.

**Защо е уязвимо:**
- Проверява само дали токенът е `SECRET123` и дали има наличност
- `[ValidateAntiForgeryToken]` предпазва от CSRF, но **НЕ** от Replay Attack
- Липсва проверка за еднократност на заявката

### Как се демонстрира (с Burp Suite)
1. Стартирай приложението и влез с акаунт (`alice@test.com` / `Password123!`)
2. Отвори **Burp Suite** → Proxy → конфигурирай браузъра да минава през него
3. Изпрати заявка за теглене от формата
4. В **Proxy History** намери `POST /ReplayDemo/Withdraw`
5. С десен бутон → **Send to Repeater** → натисни **Send** многократно
6. *(Уязвима версия)* Всяко повторение намалява баланса

### Как е оправено
Приложена е защита с **RequestId (nonce)**:

**`ReplayDemoController.cs`:**
```csharp
private static readonly HashSet<string> _usedRequestIds = new();

// При всяко GET генерираме нов уникален RequestId
var vm = new ReplayDemoVm
{
    RequestId = Guid.NewGuid().ToString(),
    ...
};

// При POST проверяваме дали вече е използван
if (string.IsNullOrEmpty(vm.RequestId) || !_usedRequestIds.Add(vm.RequestId))
{
    TempData["Message"] = "Duplicate request detected. This request has already been processed.";
    return RedirectToAction(nameof(Index));
}
```

**Принцип на работа:**
1. При всяко зареждане на страницата сървърът генерира нов `RequestId` (GUID)
2. `RequestId` се вгражда като hidden поле във формата
3. При POST сървърът опитва да добави `RequestId` в `HashSet<string>`
4. `HashSet.Add()` връща `false` ако вече съществува → заявката се отхвърля
5. Повторно изпратената заявка (replay) носи същия `RequestId` → **блокирана**

**Резултат при тест с Burp Suite Repeater:**  
Всяко повторно изпращане връща `"Duplicate request detected"` – балансът не се променя.

### Генерални техники за защита срещу Replay Attack
| Техника | Описание | Приложено |
|---|---|---|
| **Nonce / RequestId** | Уникален токен за еднократна употреба, съхранен в сървъра | ✅ |
| **Timestamp** | Заявки по-стари от N секунди се отхвърлят | ❌ |
| **Cryptographic signature** | HMAC подпис включващ nonce + timestamp | ❌ |

---

## Технологии
- .NET 10 / ASP.NET Core MVC
- Entity Framework Core 9 + Microsoft SQL Server (LocalDB)
- Bootstrap 5.3
- Cookie Authentication

## Тестови акаунти
| Email | Парола |
|---|---|
| `alice@test.com` | `Password123!` |
| `bob@test.com` | `Password456!` |

## Как да стартирате

### Изисквания
- .NET 10 SDK
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
Отворете браузър на `http://localhost:5220`.

## Структура на проекта
```
BizSecureDemo22180092/
├── Controllers/
│   ├── AccountController.cs      # Register / Login (+ lockout) / Logout
│   ├── HomeController.cs         # Landing page
│   ├── OrdersController.cs       # Create / Details (IDOR fix) / Search (SQLi fix)
│   └── ReplayDemoController.cs   # Replay Attack демо + fix
├── Data/
│   └── AppDbContext.cs
├── Migrations/
├── Models/
│   ├── AppUser.cs                # + FailedLogins, LockoutUntilUtc
│   └── Order.cs
├── ViewModels/
│   ├── RegisterVm.cs
│   ├── LoginVm.cs
│   ├── CreateOrderVm.cs
│   └── ReplayDemoVm.cs
└── Views/
    ├── Account/      Register.cshtml, Login.cshtml
    ├── Home/         Index.cshtml
    ├── Orders/       Details.cshtml, SearchResults.cshtml
    ├── ReplayDemo/   Index.cshtml
    └── Shared/       _Layout.cshtml
```


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
5. Всяко повторение ще връща **"Duplicate request detected"** – балансът не се променя

### Как е оправено
Приложена е защита с **RequestId (nonce)**:

**`ReplayDemoController.cs`**:
```csharp
private static readonly HashSet<string> _usedRequestIds = new();

// При всяко GET генерираме нов уникален RequestId
RequestId = Guid.NewGuid().ToString()

// При POST проверяваме дали вече е използван
if (string.IsNullOrEmpty(vm.RequestId) || !_usedRequestIds.Add(vm.RequestId))
{
    TempData["Message"] = "Duplicate request detected. This request has already been processed.";
    return RedirectToAction(nameof(Index));
}
```

**Принцип на работа:**
1. При всяко зареждане на страницата сървърът генерира нов `RequestId` (GUID)
2. `RequestId` се вгражда като hidden поле във формата
3. При POST сървърът се опитва да добави `RequestId` в HashSet-а
4. `HashSet.Add()` връща `false` ако елементът вече съществува → заявката се отхвърля
5. Повторно изпратената заявка (replay) носи същия `RequestId` → **блокирана**

**Защо работи срещу Replay Attack:**  
Дори нападателят да прихване и препрати точно същата заявка, `RequestId`-ът в нея вече е бил регистриран при първото изпълнение и сървърът го отхвърля.

### Как се предотвратява (генерални техники)
- **Nonce / RequestId** ✅ *приложено* – уникален токен за еднократна употреба
- **Timestamp** – заявки по-стари от N секунди се отхвърлят
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
    ├── Account/      Register.cshtml, Login.cshtml
    ├── Home/         Index.cshtml
    ├── Orders/       Details.cshtml, SearchResults.cshtml
    ├── ReplayDemo/   Index.cshtml
    └── Shared/       _Layout.cshtml
```
