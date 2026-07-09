# SimpleCalculator

Минималистичный веб-калькулятор на ASP.NET Core MVC для изучения основ фреймворка.

## 🎯 Цель проекта

Образовательный проект, демонстрирующий ключевые концепции ASP.NET Core на простом примере (~200 строк кода).

## ✨ Возможности

- ➕ Четыре математические операции: сложение, вычитание, умножение, деление
- 🖥️ HTML-форма для ввода данных
- 📡 JSON API для программного доступа
- ⚠️ Обработка ошибок (деление на ноль)
- 📝 Логирование всех операций

## 🏗️ Архитектура

```
SimpleCalculator/
├── Program.cs                      # Точка входа, настройка приложения
├── Models/
│   └── CalculationRequest.cs      # Модель запроса
├── Services/
│   └── CalculatorService.cs       # Бизнес-логика вычислений
├── Controllers/
│   └── CalcController.cs          # HTTP-обработчики
└── Views/
    └── Calc/
        └── Index.cshtml            # HTML-форма
```

## 📚 Покрываемые навыки

- ✅ MVC структура (Models, Views, Controllers)
- ✅ Dependency Injection (Singleton lifetime)
- ✅ Роутинг (convention-based и attribute)
- ✅ Model Binding ([FromForm], [FromBody], [FromQuery])
- ✅ Razor View Engine
- ✅ Обработка ошибок (try-catch)
- ✅ Логирование (ILogger)
- ✅ Возврат View и JSON

## 🚀 Быстрый старт

### Требования

- .NET SDK 8.0 или выше

### Запуск

```bash
cd /Users/andreyrubtsov/RiderProjects/SimpleCalculator
dotnet run
```

Откройте в браузере: `https://localhost:5001/calc`

## 📖 API

### GET /calc
Показывает HTML-форму калькулятора.

### POST /calc/calculate
Вычисляет результат через HTML-форму.

**Параметры (form-data):**
- `Number1` (double) — первое число
- `Number2` (double) — второе число
- `Operation` (string) — операция: "+", "-", "*", "/"

**Ответ:** HTML-страница с результатом

### POST /calc/api
Вычисляет результат через JSON API.

**Тело запроса:**
```json
{
  "Number1": 10,
  "Number2": 5,
  "Operation": "+"
}
```

**Ответ (200 OK):**
```json
{
  "success": true,
  "expression": "10 + 5",
  "result": 15.0
}
```

**Ответ (400 Bad Request) при делении на ноль:**
```json
{
  "success": false,
  "error": "Деление на ноль!"
}
```

### GET /calc/history?count=10
Демонстрация query-параметров.

**Параметры:**
- `count` (int, optional) — количество записей (default: 10)

**Ответ:**
```json
{
  "message": "История последних 10 вычислений",
  "note": "В реальном проекте здесь был бы список из БД"
}
```

## 🧪 Примеры использования

### Через браузер

1. Откройте `https://localhost:5001/calc`
2. Введите числа: 10 и 5
3. Выберите операцию: +
4. Нажмите "Вычислить"
5. Результат: `10 + 5 = 15`

### Через curl

```bash
# Вычисление через JSON API
curl -k -X POST https://localhost:5001/calc/api \
  -H "Content-Type: application/json" \
  -d '{"Number1": 20, "Number2": 4, "Operation": "/"}'

# Получить "историю"
curl -k https://localhost:5001/calc/history?count=5
```

## 📝 Документация

Подробное объяснение всех концепций проекта для интернов:

👉 **[GUIDE_FOR_INTERNS.md](GUIDE_FOR_INTERNS.md)**

Этот документ объясняет:
- Основы C# (классы, свойства, исключения)
- Основы ASP.NET Core (middleware, DI, MVC)
- Роутинг и атрибуты
- Model Binding
- Razor View Engine
- Обработка ошибок и логирование

## 🔍 Структура кода

### Program.cs (111 строк)
- Настройка приложения
- Регистрация сервисов в DI
- Настройка middleware конвейера
- Настройка роутинга

### Models/CalculationRequest.cs (95 строк)
- Модель данных для запроса
- Свойства: Number1, Number2, Operation
- Подробные комментарии о свойствах в C#

### Services/CalculatorService.cs (150 строк)
- Бизнес-логика вычислений
- Метод Calculate с switch expression
- Обработка деления на ноль
- Логирование операций

### Controllers/CalcController.cs (280 строк)
- 4 экшена: Index, Calculate, CalculateApi, History
- Демонстрация [FromForm], [FromBody], [FromQuery]
- Возврат View и JSON
- Обработка ошибок (try-catch)

### Views/Calc/Index.cshtml (250 строк)
- HTML-форма с CSS
- Razor синтаксис (@if, @ViewBag)
- Условный рендеринг результата/ошибки
- Подробные комментарии о Razor

## 🛠️ Технологии

- **ASP.NET Core 8.0** — веб-фреймворк
- **C# 12** — язык программирования
- **Razor** — шаблонизатор View
- **Kestrel** — встроенный веб-сервер

## 📦 Зависимости

Все зависимости встроены в .NET SDK, дополнительные пакеты не требуются.

---

**Создано для изучения ASP.NET Core MVC** 🚀
# practice_calculator
