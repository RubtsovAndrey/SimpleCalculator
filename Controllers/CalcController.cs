using Microsoft.AspNetCore.Mvc;
using SimpleCalculator.Models;
using SimpleCalculator.Services;

namespace SimpleCalculator.Controllers;

// ============================================================================
// КОНТРОЛЛЕР: Обработчик HTTP-запросов
// ============================================================================

// Контроллер - это класс, который принимает HTTP-запросы и возвращает ответы
// Он является "тонким слоем" между HTTP и бизнес-логикой:
// - Принимает данные из запроса (Model Binding)
// - Вызывает сервисы для выполнения логики
// - Формирует ответ (View или JSON)

/// <summary>
/// Контроллер калькулятора
/// </summary>
/// <remarks>
/// Наследуется от Controller - базового класса ASP.NET Core MVC
/// Controller предоставляет методы: View(), Ok(), BadRequest(), и т.д.
/// </remarks>
[Route("calc")]  // Attribute routing: все экшены начинаются с /calc
public class CalcController : Controller
{
    // ========================================================================
    // ПОЛЯ
    // ========================================================================
    
    // Зависимости, которые внедряются через конструктор
    private readonly CalculatorService _calculatorService;
    private readonly ILogger<CalcController> _logger;
    
    // ========================================================================
    // КОНСТРУКТОР
    // ========================================================================
    
    /// <summary>
    /// Конструктор контроллера
    /// </summary>
    /// <param name="calculatorService">Сервис вычислений (внедряется через DI)</param>
    /// <param name="logger">Логгер (внедряется через DI)</param>
    /// <remarks>
    /// DI-контейнер автоматически создаёт контроллер при каждом запросе
    /// и передаёт зарегистрированные зависимости
    /// 
    /// CalculatorService - Singleton (один на всё приложение)
    /// ILogger - встроенный сервис
    /// </remarks>
    public CalcController(CalculatorService calculatorService, ILogger<CalcController> logger)
    {
        _calculatorService = calculatorService;
        _logger = logger;
    }
    
    // ========================================================================
    // ЭКШЕН 1: Показать форму калькулятора (GET)
    // ========================================================================
    
    /// <summary>
    /// GET /calc - показывает HTML-форму калькулятора
    /// </summary>
    /// <returns>Razor View с формой</returns>
    /// <remarks>
    /// [HttpGet] - атрибут, указывающий, что метод обрабатывает GET-запросы
    /// Без параметров в атрибуте = базовый роут контроллера (/calc)
    /// </remarks>
    [HttpGet]
    public IActionResult Index()
    {
        // View() ищет файл Views/Calc/Index.cshtml
        // Путь формируется автоматически:
        // - Views/{ИмяКонтроллера}/{ИмяЭкшена}.cshtml
        // - Views/Calc/Index.cshtml
        //
        // Razor View Engine рендерит .cshtml в HTML и отправляет клиенту
        return View();
    }
    
    // ========================================================================
    // ЭКШЕН 2: Вычислить через HTML-форму (POST)
    // ========================================================================
    
    /// <summary>
    /// POST /calc/calculate - вычисляет результат и возвращает View
    /// </summary>
    /// <param name="request">Данные из HTML-формы</param>
    /// <returns>View с результатом или ошибкой</returns>
    /// <remarks>
    /// [HttpPost("calculate")] - обрабатывает POST на /calc/calculate
    /// [FromForm] - данные приходят из HTML-формы (Content-Type: application/x-www-form-urlencoded)
    /// 
    /// Model Binding автоматически парсит данные формы:
    /// - Поле <input name="Number1"> → request.Number1
    /// - Поле <select name="Operation"> → request.Operation
    /// - Поле <input name="Number2"> → request.Number2
    /// </remarks>
    [HttpPost("calculate")]
    public IActionResult Calculate([FromForm] CalculationRequest request)
    {
        // ====================================================================
        // TRY-CATCH: Обработка исключений
        // ====================================================================
        
        // try - блок кода, который может выбросить исключение
        // catch - блок, который перехватывает исключение и обрабатывает его
        // finally - блок, который выполняется всегда (есть исключение или нет)
        
        try
        {
            // Вызываем сервис для вычисления
            var result = _calculatorService.Calculate(
                request.Number1, 
                request.Number2, 
                request.Operation);
            
            // ================================================================
            // VIEWBAG: Передача данных во View
            // ================================================================
            
            // ViewBag - это динамический объект для передачи данных во View
            // Альтернативы:
            // - ViewData["Result"] = result (словарь, требует приведение типов)
            // - Передать модель: return View(result) (строго типизировано)
            //
            // ViewBag удобен для простых случаев (1-2 значения)
            // Для сложных данных лучше создать отдельную модель View
            ViewBag.Result = result;
            ViewBag.Expression = $"{request.Number1} {request.Operation} {request.Number2} = {result}";
            
            // Возвращаем ту же View (Index.cshtml)
            // View проверит ViewBag.Result и покажет результат
            return View("Index");
        }
        catch (DivideByZeroException ex)
        {
            // Специфичная обработка деления на ноль
            // LogWarning - уровень "Warning" (не ошибка, но требует внимания)
            _logger.LogWarning("Попытка деления на ноль");
            
            // Передаём сообщение об ошибке во View
            ViewBag.Error = ex.Message;
            return View("Index");
        }
        catch (Exception ex)
        {
            // Общая обработка всех остальных исключений
            // LogError - уровень "Error" (что-то пошло не так)
            // Первый аргумент ex - само исключение (включая stack trace)
            _logger.LogError(ex, "Ошибка при вычислении");
            
            ViewBag.Error = "Произошла ошибка";
            return View("Index");
        }
    }
    
    // ========================================================================
    // ЭКШЕН 3: Вычислить через JSON API (POST)
    // ========================================================================
    
    /// <summary>
    /// POST /calc/api - вычисляет результат и возвращает JSON
    /// </summary>
    /// <param name="request">Данные из JSON</param>
    /// <returns>JSON с результатом или ошибкой</returns>
    /// <remarks>
    /// [FromBody] - данные приходят из тела запроса как JSON (Content-Type: application/json)
    /// 
    /// Model Binding автоматически десериализует JSON в объект:
    /// {"Number1": 10, "Number2": 5, "Operation": "+"} → CalculationRequest
    /// 
    /// Этот экшен демонстрирует разницу между View и JSON API:
    /// - Calculate возвращает HTML (для браузера)
    /// - CalculateApi возвращает JSON (для JavaScript, мобильных приложений)
    /// </remarks>
    [HttpPost("api")]
    public IActionResult CalculateApi([FromBody] CalculationRequest request)
    {
        try
        {
            var result = _calculatorService.Calculate(
                request.Number1, 
                request.Number2, 
                request.Operation);
            
            // ================================================================
            // OK(): Возврат JSON с кодом 200
            // ================================================================
            
            // Ok() - метод из базового класса Controller
            // Возвращает HTTP 200 (OK) и сериализует объект в JSON
            //
            // new { ... } - анонимный тип (anonymous type)
            // Компилятор автоматически создаёт класс с этими свойствами
            //
            // Результат:
            // HTTP/1.1 200 OK
            // Content-Type: application/json
            // {
            //   "success": true,
            //   "expression": "10 + 5",
            //   "result": 15.0
            // }
            return Ok(new
            {
                success = true,
                expression = $"{request.Number1} {request.Operation} {request.Number2}",
                result = result
            });
        }
        catch (DivideByZeroException ex)
        {
            // ================================================================
            // BADREQUEST(): Возврат JSON с кодом 400
            // ================================================================
            
            // BadRequest() возвращает HTTP 400 (Bad Request)
            // Используется для ошибок валидации или некорректных данных
            //
            // Результат:
            // HTTP/1.1 400 Bad Request
            // Content-Type: application/json
            // {
            //   "success": false,
            //   "error": "Деление на ноль!"
            // }
            return BadRequest(new { success = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            // ================================================================
            // STATUSCODE(): Возврат JSON с произвольным кодом
            // ================================================================
            
            // StatusCode(500, ...) возвращает HTTP 500 (Internal Server Error)
            // Используется для непредвиденных ошибок сервера
            //
            // Другие коды:
            // - 200 OK - успех
            // - 201 Created - ресурс создан
            // - 204 No Content - успех без тела ответа
            // - 400 Bad Request - ошибка клиента
            // - 401 Unauthorized - требуется авторизация
            // - 403 Forbidden - доступ запрещён
            // - 404 Not Found - ресурс не найден
            // - 500 Internal Server Error - ошибка сервера
            _logger.LogError(ex, "Ошибка в API");
            return StatusCode(500, new { success = false, error = "Внутренняя ошибка" });
        }
    }
    
    // ========================================================================
    // ЭКШЕН 4: История вычислений (GET с query-параметром)
    // ========================================================================
    
    /// <summary>
    /// GET /calc/history?count=10 - демонстрация query-параметров
    /// </summary>
    /// <param name="count">Количество записей (из query-строки)</param>
    /// <returns>JSON с информацией</returns>
    /// <remarks>
    /// [FromQuery] - параметр берётся из query-строки URL
    /// 
    /// Примеры:
    /// - GET /calc/history → count = 10 (default)
    /// - GET /calc/history?count=5 → count = 5
    /// - GET /calc/history?count=100 → count = 100
    /// 
    /// Значение по умолчанию (= 10) используется, если параметр не передан
    /// </remarks>
    [HttpGet("history")]
    public IActionResult History([FromQuery] int count = 10)
    {
        // В реальном проекте здесь был бы запрос к базе данных:
        // var history = _dbContext.Calculations
        //     .OrderByDescending(c => c.Date)
        //     .Take(count)
        //     .ToList();
        //
        // Для демонстрации просто возвращаем заглушку
        return Ok(new
        {
            message = $"История последних {count} вычислений",
            note = "В реальном проекте здесь был бы список из БД"
        });
    }
}

// ============================================================================
// ИТОГОВАЯ КАРТА РОУТОВ
// ============================================================================

// Контроллер имеет базовый роут [Route("calc")]
// Каждый экшен добавляет свой сегмент:
//
// GET  /calc              → Index()          (показать форму)
// POST /calc/calculate    → Calculate()      (вычислить через форму)
// POST /calc/api          → CalculateApi()   (вычислить через JSON)
// GET  /calc/history      → History()        (получить историю)

// ============================================================================
// АТРИБУТЫ MODEL BINDING
// ============================================================================

// [FromBody]   - данные из тела запроса (JSON)
// [FromForm]   - данные из HTML-формы
// [FromQuery]  - данные из query-строки (?key=value)
// [FromRoute]  - данные из сегмента URL ({id})
// [FromHeader] - данные из HTTP-заголовка

// ============================================================================
// МЕТОДЫ ВОЗВРАТА РЕЗУЛЬТАТА
// ============================================================================

// View()                    - рендерит Razor View (HTML)
// Ok(object)                - HTTP 200 + JSON
// BadRequest(object)        - HTTP 400 + JSON
// NotFound()                - HTTP 404
// StatusCode(code, object)  - произвольный код + JSON
// Redirect(url)             - HTTP 302 (перенаправление)
// File(bytes, contentType)  - возврат файла
