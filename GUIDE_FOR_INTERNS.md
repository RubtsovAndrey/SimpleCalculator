# Руководство для интернов: Проект SimpleCalculator

Этот документ объясняет **все технические решения** в проекте простым языком для начинающих C# разработчиков.

---

## Оглавление

1. [Что такое этот проект](#что-такое-этот-проект)
2. [Архитектура проекта](#архитектура-проекта)
3. [Основы C# в проекте](#основы-c-в-проекте)
4. [Основы ASP.NET Core](#основы-aspnet-core)
5. [Dependency Injection (DI)](#dependency-injection-di)
6. [Model-View-Controller (MVC)](#model-view-controller-mvc)
7. [Роутинг и атрибуты](#роутинг-и-атрибуты)
8. [Model Binding](#model-binding)
9. [Razor View Engine](#razor-view-engine)
10. [Обработка ошибок](#обработка-ошибок)
11. [Логирование](#логирование)
12. [Как запустить проект](#как-запустить-проект)
13. [Как проверить работу](#как-проверить-работу)

---

## Что такое этот проект

**SimpleCalculator** — это минималистичное веб-приложение на ASP.NET Core MVC, которое:
- Показывает HTML-форму для ввода двух чисел и выбора операции (+, -, *, /)
- Вычисляет результат и показывает его на той же странице
- Предоставляет JSON API для вычислений (для JavaScript, мобильных приложений)

**Цель:** Изучить основы ASP.NET Core на простом примере (~200 строк кода).

**Покрываемые навыки:**
- GET и POST запросы
- MVC структура (Models, Views, Controllers)
- Dependency Injection
- Роутинг (convention-based и attribute)
- Model Binding ([FromForm], [FromBody], [FromQuery])
- Razor View (HTML + C#)
- Обработка ошибок (try-catch)
- Логирование
- Возврат View и JSON

---

## Архитектура проекта

```
SimpleCalculator/
│
├── Program.cs                      # Точка входа, настройка приложения
│
├── Models/                         # Модели данных
│   └── CalculationRequest.cs      # Модель запроса (Number1, Number2, Operation)
│
├── Services/                       # Бизнес-логика
│   └── CalculatorService.cs       # Сервис вычислений
│
├── Controllers/                    # Обработчики HTTP-запросов
│   └── CalcController.cs          # Контроллер калькулятора (4 экшена)
│
├── Views/                          # HTML-шаблоны
│   └── Calc/
│       └── Index.cshtml            # Форма калькулятора
│
└── appsettings.json                # Конфигурация (логирование, и т.д.)
```

**Принцип разделения ответственности:**
- **Models** — описывают структуру данных
- **Services** — содержат бизнес-логику (вычисления, работа с БД)
- **Controllers** — принимают HTTP-запросы, вызывают сервисы, возвращают ответы
- **Views** — генерируют HTML для браузера

---

## Основы C# в проекте

### 1. Классы и объекты

**Класс** — это шаблон для создания объектов.

```csharp
// Определение класса
public class CalculatorService
{
    // Поля (данные класса)
    private readonly ILogger _logger;
    
    // Конструктор (вызывается при создании объекта)
    public CalculatorService(ILogger logger)
    {
        _logger = logger;
    }
    
    // Методы (действия класса)
    public double Calculate(double num1, double num2, string operation)
    {
        return num1 + num2;
    }
}

// Создание объекта
var calculator = new CalculatorService(logger);
var result = calculator.Calculate(10, 5, "+");
```

**В проекте:**
- `CalculatorService` — класс сервиса
- `CalcController` — класс контроллера
- `CalculationRequest` — класс модели

### 2. Свойства (Properties)

**Свойство** — это специальный синтаксис для доступа к данным класса.

```csharp
// Старый способ (поле + методы)
private double number1;
public double GetNumber1() { return number1; }
public void SetNumber1(double value) { number1 = value; }

// Современный способ (свойство)
public double Number1 { get; set; }

// Использование
var request = new CalculationRequest();
request.Number1 = 10;  // Вызывает set
var value = request.Number1;  // Вызывает get
```

**Зачем:**
- Короче и читабельнее
- Можно добавить валидацию в get/set
- Model Binding работает только со свойствами

**В проекте:** `CalculationRequest.cs` использует свойства для `Number1`, `Number2`, `Operation`.

### 3. Модификаторы доступа

```csharp
public class MyClass
{
    public int PublicField;        // Доступен везде
    private int PrivateField;      // Доступен только внутри класса
    protected int ProtectedField;  // Доступен в классе и наследниках
}
```

**Правило:** Делай поля `private`, а свойства `public` (инкапсуляция).

### 4. readonly

```csharp
private readonly ILogger _logger;

public MyClass(ILogger logger)
{
    _logger = logger;  // ✅ Можно присвоить в конструкторе
}

public void SomeMethod()
{
    _logger = null;  // ❌ ОШИБКА! readonly нельзя изменить
}
```

**Зачем:** Гарантирует, что значение не будет случайно перезаписано.

### 5. Типы данных

```csharp
int целое = 42;              // Целое число (-2147483648 до 2147483647)
double дробное = 3.14;       // Дробное число (15-16 знаков точности)
string текст = "Hello";      // Строка (текст)
bool флаг = true;            // Логическое значение (true/false)
```

**В проекте:** `double` для чисел (поддерживает дробные результаты деления).

### 6. Исключения (Exceptions)

```csharp
try
{
    // Код, который может выбросить исключение
    var result = 10 / 0;  // DivideByZeroException
}
catch (DivideByZeroException ex)
{
    // Обработка конкретного исключения
    Console.WriteLine("Деление на ноль!");
}
catch (Exception ex)
{
    // Обработка всех остальных исключений
    Console.WriteLine("Что-то пошло не так");
}
finally
{
    // Выполняется всегда (есть исключение или нет)
    Console.WriteLine("Очистка ресурсов");
}
```

**В проекте:** `CalcController.cs` использует try-catch для обработки ошибок вычисления.

### 7. Switch Expression (C# 8.0+)

```csharp
// Старый синтаксис
string result;
switch (operation)
{
    case "+": result = "сложение"; break;
    case "-": result = "вычитание"; break;
    default: result = "неизвестно"; break;
}

// Новый синтаксис (короче)
string result = operation switch
{
    "+" => "сложение",
    "-" => "вычитание",
    _ => "неизвестно"  // _ = default
};
```

**В проекте:** `CalculatorService.Calculate()` использует switch expression.

### 8. Интерполяция строк

```csharp
int x = 10, y = 5;

// Старый способ (конкатенация)
string s1 = "Результат: " + x + " + " + y + " = " + (x + y);

// Новый способ (интерполяция)
string s2 = $"Результат: {x} + {y} = {x + y}";
```

**Зачем:** Читабельнее, меньше ошибок.

**В проекте:** `ViewBag.Expression = $"{request.Number1} {request.Operation} {request.Number2} = {result}";`

---

## Основы ASP.NET Core

### Что такое ASP.NET Core?

**ASP.NET Core** — это фреймворк для создания веб-приложений на C#.

**Основные компоненты:**
1. **Kestrel** — встроенный веб-сервер (принимает HTTP-запросы)
2. **Middleware** — конвейер обработки запросов
3. **MVC** — паттерн для структурирования приложения
4. **Dependency Injection** — система управления зависимостями
5. **Routing** — система маршрутизации (URL → контроллер/экшен)

### Жизненный цикл запроса

```
1. Браузер отправляет запрос: GET https://localhost:5001/calc
2. Kestrel принимает запрос
3. Middleware конвейер:
   - UseHttpsRedirection (редирект HTTP → HTTPS)
   - UseRouting (определяет, какой контроллер вызвать)
   - UseAuthorization (проверяет права доступа)
4. Routing находит: CalcController.Index()
5. DI создаёт контроллер, внедряет зависимости
6. Вызывается экшен Index()
7. Экшен возвращает View()
8. Razor рендерит Index.cshtml в HTML
9. HTML отправляется браузеру
10. Браузер показывает страницу
```

### Program.cs — точка входа

**`Program.cs`** — это первый файл, который выполняется при запуске.

**Структура:**
```csharp
var builder = WebApplication.CreateBuilder(args);  // 1. Создание строителя
builder.Services.AddControllersWithViews();        // 2. Регистрация сервисов
var app = builder.Build();                         // 3. Сборка приложения
app.UseHttpsRedirection();                         // 4. Настройка middleware
app.MapControllerRoute(...);                       // 5. Настройка роутинга
app.Run();                                         // 6. Запуск сервера
```

**Аналогия:** Строительство дома
1. Создаём чертёж (builder)
2. Заказываем материалы (регистрация сервисов)
3. Строим дом (Build)
4. Устанавливаем двери, окна (middleware)
5. Вешаем таблички с номерами комнат (роутинг)
6. Открываем дом для жильцов (Run)

---

## Dependency Injection (DI)

### Что такое DI?

**Dependency Injection** — это паттерн, при котором объекты не создают свои зависимости сами, а получают их извне.

**Без DI (плохо):**
```csharp
public class CalcController
{
    private CalculatorService _service;
    
    public CalcController()
    {
        _service = new CalculatorService();  // ❌ Жёсткая связь
    }
}
```

**Проблемы:**
- Сложно тестировать (нельзя подменить сервис на mock)
- Если конструктор `CalculatorService` изменится, нужно менять все места создания
- Нельзя переиспользовать один экземпляр

**С DI (хорошо):**
```csharp
public class CalcController
{
    private CalculatorService _service;
    
    public CalcController(CalculatorService service)  // ✅ Внедрение через конструктор
    {
        _service = service;
    }
}
```

**Преимущества:**
- Класс явно объявляет свои зависимости
- DI-контейнер автоматически создаёт и передаёт зависимости
- Легко подменить в тестах
- Можно контролировать время жизни (Singleton, Scoped, Transient)

### Регистрация в DI

**`Program.cs`:**
```csharp
builder.Services.AddSingleton<CalculatorService>();
```

**Это означает:**
- Зарегистрировать `CalculatorService` в DI-контейнере
- Lifetime: Singleton (один экземпляр на всё приложение)

### Lifetime (время жизни)

| Lifetime | Когда создаётся | Когда уничтожается | Пример |
|----------|-----------------|---------------------|--------|
| **Singleton** | Один раз при первом запросе | При остановке приложения | Кеш, конфигурация |
| **Scoped** | Один раз на HTTP-запрос | В конце запроса | DbContext (работа с БД) |
| **Transient** | При каждом запросе из контейнера | Сразу после использования | Лёгкие сервисы без состояния |

**В проекте:** `CalculatorService` — Singleton, потому что он stateless (не хранит данные между вызовами).

### Внедрение зависимостей

**Контроллер:**
```csharp
public CalcController(CalculatorService service, ILogger<CalcController> logger)
{
    _service = service;
    _logger = logger;
}
```

**Что происходит:**
1. DI видит: конструктор требует `CalculatorService` и `ILogger`
2. Ищет регистрации в контейнере
3. Создаёт экземпляры (или берёт существующие для Singleton)
4. Вызывает конструктор, передавая зависимости

---

## Model-View-Controller (MVC)

### Что такое MVC?

**MVC** — это паттерн проектирования, который разделяет приложение на три части:

1. **Model** — данные и бизнес-логика
2. **View** — представление (HTML)
3. **Controller** — диспетчер (принимает запросы, вызывает модель, выбирает View)

### Как это работает в проекте

```
Браузер → GET /calc → Routing → CalcController.Index()
                                       ↓
                                 return View()
                                       ↓
                                 Razor рендерит Views/Calc/Index.cshtml
                                       ↓
                                 HTML → Браузер
```

**Пользователь заполняет форму и нажимает "Вычислить":**

```
Браузер → POST /calc/calculate → Routing → CalcController.Calculate(request)
                                                  ↓
                                            _calculatorService.Calculate(...)
                                                  ↓
                                            ViewBag.Result = result
                                                  ↓
                                            return View("Index")
                                                  ↓
                                            Razor рендерит Index.cshtml с результатом
                                                  ↓
                                            HTML → Браузер
```

### Почему контроллер "тонкий"?

**Контроллер не содержит логику вычислений** — он только:
- Принимает данные из запроса (Model Binding)
- Вызывает сервис `_calculatorService.Calculate(...)`
- Формирует ответ (View или JSON)

**Логика вычислений** находится в `CalculatorService`.

**Зачем:**
- Разделение ответственности (контроллер = HTTP, сервис = логика)
- Тестируемость (сервис можно тестировать без HTTP)
- Переиспользование (логику можно вызвать из разных мест)

---

## Роутинг и атрибуты

### Что такое роутинг?

**Роутинг** — это система, которая сопоставляет URL с контроллером и экшеном.

**Пример:**
- URL: `/calc/calculate`
- Контроллер: `CalcController`
- Экшен: `Calculate()`

### Convention-based routing

**`Program.cs`:**
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calc}/{action=Index}/{id?}");
```

**Шаблон:** `{controller}/{action}/{id?}`

**Примеры:**
- `/` → `CalcController.Index()` (defaults)
- `/Calc/Calculate` → `CalcController.Calculate()`
- `/Home/About/5` → `HomeController.About(id: 5)`

### Attribute routing

**`CalcController.cs`:**
```csharp
[Route("calc")]  // Базовый роут контроллера
public class CalcController : Controller
{
    [HttpGet]                // GET /calc
    public IActionResult Index() { ... }
    
    [HttpPost("calculate")]  // POST /calc/calculate
    public IActionResult Calculate(...) { ... }
    
    [HttpPost("api")]        // POST /calc/api
    public IActionResult CalculateApi(...) { ... }
}
```

**Итоговые роуты:**
- `GET /calc` → `Index()`
- `POST /calc/calculate` → `Calculate()`
- `POST /calc/api` → `CalculateApi()`

### HTTP-методы

| Метод | Назначение | Пример |
|-------|------------|--------|
| **GET** | Получить данные (показать страницу) | Открыть форму |
| **POST** | Отправить данные (создать, обновить) | Отправить форму |
| **PUT** | Полностью обновить ресурс | Обновить профиль |
| **DELETE** | Удалить ресурс | Удалить запись |

**В проекте:**
- `GET /calc` — показать форму
- `POST /calc/calculate` — вычислить результат

---

## Model Binding

### Что такое Model Binding?

**Model Binding** — это автоматическое преобразование данных из HTTP-запроса в C# объекты.

**Без Model Binding:**
```csharp
public IActionResult Calculate()
{
    var num1 = double.Parse(Request.Form["Number1"]);
    var num2 = double.Parse(Request.Form["Number2"]);
    var operation = Request.Form["Operation"];
    // ...
}
```

**С Model Binding:**
```csharp
public IActionResult Calculate([FromForm] CalculationRequest request)
{
    // request.Number1, request.Number2, request.Operation уже заполнены!
}
```

### Атрибуты источников данных

| Атрибут | Откуда данные | Пример |
|---------|---------------|--------|
| `[FromBody]` | Тело запроса (JSON) | `{"Number1": 10}` |
| `[FromForm]` | HTML-форма | `<input name="Number1">` |
| `[FromQuery]` | Query-строка | `?count=10` |
| `[FromRoute]` | Сегмент URL | `/calc/history/5` |
| `[FromHeader]` | HTTP-заголовок | `X-API-Key: abc123` |

### Примеры в проекте

**1. [FromForm] — HTML форма**

```csharp
[HttpPost("calculate")]
public IActionResult Calculate([FromForm] CalculationRequest request)
```

**Запрос:**
```
POST /calc/calculate
Content-Type: application/x-www-form-urlencoded

Number1=10&Number2=5&Operation=%2B
```

**Результат:** `request.Number1 = 10`, `request.Number2 = 5`, `request.Operation = "+"`

**2. [FromBody] — JSON**

```csharp
[HttpPost("api")]
public IActionResult CalculateApi([FromBody] CalculationRequest request)
```

**Запрос:**
```
POST /calc/api
Content-Type: application/json

{"Number1": 10, "Number2": 5, "Operation": "+"}
```

**Результат:** тот же объект `CalculationRequest`

**3. [FromQuery] — Query-параметры**

```csharp
[HttpGet("history")]
public IActionResult History([FromQuery] int count = 10)
```

**Запрос:** `GET /calc/history?count=5`

**Результат:** `count = 5`

### Как работает Model Binding

1. ASP.NET смотрит на атрибут (`[FromForm]`, `[FromBody]`, и т.д.)
2. Читает данные из соответствующего источника
3. Парсит данные (JSON → объект, форма → объект)
4. Создаёт экземпляр модели: `new CalculationRequest()`
5. Заполняет свойства: `Number1 = 10`, `Number2 = 5`, ...
6. Передаёт объект в параметр экшена

---

## Razor View Engine

### Что такое Razor?

**Razor** — это шаблонизатор, который позволяет смешивать HTML и C# код.

**Синтаксис:**
- `@` — переключение в C# режим
- `@variable` — вывод переменной
- `@{ код }` — блок C# кода
- `@if`, `@for`, `@foreach` — управляющие конструкции

### Примеры из проекта

**1. Вывод переменной**

```html
<h2>Счёт: @ViewBag.Result</h2>
```

**Компилируется в:**
```csharp
Write("<h2>Счёт: ");
Write(ViewBag.Result);
Write("</h2>");
```

**2. Условный рендеринг**

```html
@if (ViewBag.Result != null)
{
    <div class="result">
        Результат: @ViewBag.Expression
    </div>
}
```

**Компилируется в:**
```csharp
if (ViewBag.Result != null)
{
    Write("<div class='result'>");
    Write("Результат: ");
    Write(ViewBag.Expression);
    Write("</div>");
}
```

**3. HTML-форма**

```html
<form method="post" action="/calc/calculate">
    <input type="number" name="Number1" required>
    <select name="Operation">
        <option value="+">Сложение</option>
    </select>
    <input type="number" name="Number2" required>
    <button type="submit">Вычислить</button>
</form>
```

**Важно:** Атрибуты `name` должны совпадать со свойствами модели:
- `name="Number1"` → `request.Number1`
- `name="Operation"` → `request.Operation`

### ViewBag vs Model

**ViewBag** — динамический объект для передачи данных:
```csharp
// Контроллер
ViewBag.Result = 15;

// View
@ViewBag.Result
```

**Model** — строго типизированная модель:
```csharp
// Контроллер
return View(calculatorService);

// View
@model CalculatorService
@Model.Score
```

**В проекте:** используется ViewBag для простоты (1-2 значения).

---

## Обработка ошибок

### Try-Catch

```csharp
try
{
    // Код, который может выбросить исключение
    var result = _calculatorService.Calculate(num1, num2, operation);
    return Ok(new { result });
}
catch (DivideByZeroException ex)
{
    // Обработка конкретной ошибки
    return BadRequest(new { error = ex.Message });
}
catch (Exception ex)
{
    // Обработка всех остальных ошибок
    _logger.LogError(ex, "Ошибка");
    return StatusCode(500, new { error = "Внутренняя ошибка" });
}
```

### Коды статусов HTTP

| Код | Название | Когда использовать |
|-----|----------|-------------------|
| 200 | OK | Успешный запрос |
| 400 | Bad Request | Ошибка валидации, некорректные данные |
| 401 | Unauthorized | Требуется авторизация |
| 403 | Forbidden | Доступ запрещён |
| 404 | Not Found | Ресурс не найден |
| 500 | Internal Server Error | Ошибка сервера |

**В проекте:**
- `Ok(...)` → 200
- `BadRequest(...)` → 400 (деление на ноль)
- `StatusCode(500, ...)` → 500 (непредвиденная ошибка)

---

## Логирование

### Уровни логирования

| Уровень | Когда использовать | Пример |
|---------|-------------------|--------|
| **Trace** | Детальная отладка | "Вход в метод Calculate" |
| **Debug** | Отладочная информация | "Значение переменной x = 10" |
| **Information** | Обычные события | "Вычисление: 10 + 5" |
| **Warning** | Предупреждения | "Попытка деления на ноль" |
| **Error** | Ошибки | "Исключение при вычислении" |
| **Critical** | Критические ошибки | "База данных недоступна" |

### Структурированное логирование

**Плохо (конкатенация):**
```csharp
_logger.LogInformation("Вычисление: " + num1 + " + " + num2);
```

**Хорошо (placeholders):**
```csharp
_logger.LogInformation("Вычисление: {Num1} + {Num2}", num1, num2);
```

**Преимущества:**
- Системы мониторинга сохраняют `Num1` и `Num2` как отдельные поля
- Можно фильтровать: "покажи все вычисления, где Num1 > 100"
- Производительность: строка не форматируется, если уровень логирования выше

### Настройка в appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Что это значит:**
- `Default: Information` — показывать Information и выше (Warning, Error, Critical)
- `Microsoft.AspNetCore: Warning` — для встроенных логов ASP.NET показывать только Warning и выше

---

## Как запустить проект

### 1. Убедитесь, что установлен .NET SDK

```bash
dotnet --version
```

Должна быть версия 8.0 или выше.

### 2. Перейдите в папку проекта

```bash
cd /Users/andreyrubtsov/RiderProjects/SimpleCalculator
```

### 3. Запустите приложение

```bash
dotnet run
```

### 4. Откройте в браузере

Консоль покажет адрес, например:
```
Now listening on: https://localhost:5001
```

Откройте: `https://localhost:5001/calc`

---

## Как проверить работу

### 1. Проверить GET (HTML форма)

**Браузер:** `https://localhost:5001/calc`

Должна открыться форма калькулятора.

### 2. Проверить POST (форма)

Заполните форму:
- Первое число: `10`
- Операция: `+`
- Второе число: `5`
- Нажмите "Вычислить"

Результат: `10 + 5 = 15`

### 3. Проверить деление на ноль

- Первое число: `10`
- Операция: `/`
- Второе число: `0`
- Нажмите "Вычислить"

Ошибка: `Деление на ноль!`

### 4. Проверить JSON API (через curl)

```bash
curl -k -X POST https://localhost:5001/calc/api \
  -H "Content-Type: application/json" \
  -d '{"Number1": 20, "Number2": 4, "Operation": "/"}'
```

Ответ:
```json
{
  "success": true,
  "expression": "20 / 4",
  "result": 5.0
}
```

### 5. Проверить query-параметры

```bash
curl -k https://localhost:5001/calc/history?count=5
```

Ответ:
```json
{
  "message": "История последних 5 вычислений",
  "note": "В реальном проекте здесь был бы список из БД"
}
```

---

## Итоговая карта навыков

| Навык | Где в проекте |
|-------|---------------|
| **Классы и объекты** | `CalculatorService`, `CalcController`, `CalculationRequest` |
| **Свойства** | `CalculationRequest.Number1`, `Number2`, `Operation` |
| **Конструкторы** | `CalcController(CalculatorService service, ...)` |
| **Методы** | `CalculatorService.Calculate(...)` |
| **Исключения** | `try-catch` в `CalcController` |
| **Switch expression** | `CalculatorService.Calculate()` |
| **Интерполяция строк** | `$"{num1} {operation} {num2}"` |
| **DI регистрация** | `builder.Services.AddSingleton<CalculatorService>()` |
| **DI внедрение** | Конструкторы контроллера и сервиса |
| **MVC структура** | Models, Views, Controllers |
| **Роутинг** | `[Route("calc")]`, `[HttpGet]`, `[HttpPost("calculate")]` |
| **Model Binding** | `[FromForm]`, `[FromBody]`, `[FromQuery]` |
| **Razor** | `Views/Calc/Index.cshtml` |
| **ViewBag** | `ViewBag.Result`, `ViewBag.Error` |
| **Возврат View** | `return View()` |
| **Возврат JSON** | `return Ok(new { ... })` |
| **Коды статусов** | `Ok()`, `BadRequest()`, `StatusCode(500)` |
| **Логирование** | `_logger.LogInformation(...)`, `LogError(...)` |

---

## Следующие шаги

После изучения этого проекта попробуйте:

1. **Добавить новую операцию** (например, возведение в степень)
2. **Добавить историю вычислений** (сохранять в `List<string>`)
3. **Добавить валидацию** (атрибуты `[Range]`, `[Required]`)
4. **Создать enum для операций** вместо строки
5. **Добавить тесты** (unit-тесты для `CalculatorService`)
6. **Подключить базу данных** (Entity Framework Core)

---

## Полезные ресурсы

- [Официальная документация ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [C# для начинающих](https://docs.microsoft.com/dotnet/csharp)
- [Dependency Injection в ASP.NET Core](https://docs.microsoft.com/aspnet/core/fundamentals/dependency-injection)
- [Razor синтаксис](https://docs.microsoft.com/aspnet/core/mvc/views/razor)

---

**Удачи в изучении ASP.NET Core! 🚀**
