using SimpleCalculator.Services;

// ============================================================================
// ШАБЛОН BUILDER: Создание и настройка веб-приложения
// ============================================================================

// WebApplicationBuilder - это объект-строитель, который позволяет настроить
// приложение ДО его запуска. Он инициализирует:
// - Configuration (чтение appsettings.json, переменных окружения)
// - Logging (система логирования)
// - Services (контейнер Dependency Injection)
var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// РЕГИСТРАЦИЯ СЕРВИСОВ В DI-КОНТЕЙНЕРЕ
// ============================================================================

// AddControllersWithViews() регистрирует все необходимые сервисы для работы MVC:
// - Контроллеры (обработчики HTTP-запросов)
// - Razor View Engine (для рендеринга .cshtml файлов в HTML)
// - Model Binding (автоматический парсинг данных из запроса в C# объекты)
// - Валидация моделей
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // WriteIndented = true делает JSON красивым с отступами (для отладки)
        // Без этого: {"success":true,"result":15}
        // С этим:    {
        //              "success": true,
        //              "result": 15
        //            }
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Регистрация нашего сервиса CalculatorService в DI-контейнере
// AddSingleton означает: создать ОДИН экземпляр на всё приложение
// Альтернативы:
// - AddScoped: новый экземпляр на каждый HTTP-запрос
// - AddTransient: новый экземпляр при каждом запросе из контейнера
// Мы выбрали Singleton, потому что CalculatorService не хранит состояние
// (stateless) и может безопасно использоваться всеми запросами одновременно
builder.Services.AddSingleton<CalculatorService>();

// ============================================================================
// СБОРКА ПРИЛОЖЕНИЯ
// ============================================================================

// Build() "замораживает" конфигурацию и создаёт готовое приложение
// После этого нельзя добавлять новые сервисы в builder.Services
var app = builder.Build();

// ============================================================================
// НАСТРОЙКА MIDDLEWARE КОНВЕЙЕРА
// ============================================================================

// Middleware - это компоненты, которые обрабатывают HTTP-запросы
// Они выстраиваются в конвейер (pipeline):
// Запрос → Middleware1 → Middleware2 → ... → Контроллер → ... → Ответ

// В режиме Production (не Development) включаем обработку ошибок
if (!app.Environment.IsDevelopment())
{
    // UseExceptionHandler перехватывает необработанные исключения
    // и показывает пользователю красивую страницу ошибки
    app.UseExceptionHandler("/Error");
    
    // HSTS (HTTP Strict Transport Security) заставляет браузер
    // всегда использовать HTTPS вместо HTTP (безопасность)
    app.UseHsts();
}

// UseHttpsRedirection автоматически перенаправляет HTTP → HTTPS
// Например: http://localhost:5000 → https://localhost:5001
app.UseHttpsRedirection();

// UseStaticFiles позволяет отдавать статические файлы из папки wwwroot/
// (css, js, картинки) без обработки контроллером
app.UseStaticFiles();

// UseRouting включает систему маршрутизации
// Она анализирует URL и определяет, какой контроллер/экшен вызвать
app.UseRouting();

// UseAuthorization проверяет права доступа (атрибуты [Authorize])
// В этом проекте не используется, но оставлен для будущего расширения
app.UseAuthorization();

// ============================================================================
// НАСТРОЙКА РОУТИНГА
// ============================================================================

// MapControllerRoute настраивает convention-based routing
// Шаблон: {controller=Calc}/{action=Index}/{id?}
// Это означает:
// - URL "/" → CalcController.Index() (defaults)
// - URL "/Calc/Calculate" → CalcController.Calculate()
// - URL "/Calc/History/5" → CalcController.History(id: 5)
// 
// {id?} - знак вопроса означает "опциональный параметр"
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calc}/{action=Index}/{id?}");

// ============================================================================
// ЗАПУСК ПРИЛОЖЕНИЯ
// ============================================================================

// Run() запускает веб-сервер Kestrel и начинает слушать HTTP-запросы
// Поток блокируется здесь до остановки приложения (Ctrl+C)
app.Run();
