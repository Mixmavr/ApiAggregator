namespace ApiAggregator.Models
{
    public class WeatherModel
    {
        public string Name { get; set; }
        public MainWeather Main { get; set; }
    }

    public class MainWeather
    {
        public double Temp { get; set; }
        public double Feels_Like { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
    }

}
