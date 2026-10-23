using System.ComponentModel.DataAnnotations;

namespace weather_proj.Configuration;

public class AccuWeatherOptions
{
    public const string SectionName = "AccuWeather"; 
    [Required(ErrorMessage = "ApiKey обязателен для работы с сервисом")]
    [MinLength(10, ErrorMessage = "ApiKey слишком короткий")]
    public string ApiKey { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "BaseUrl обязателен")]
    [Url(ErrorMessage = "BaseUrl должен быть корректным URL")]
    public string BaseUrl { get; set; } = "http://dataservice.accuweather.com";
}