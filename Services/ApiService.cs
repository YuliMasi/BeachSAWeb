namespace BeachSAWeb.Services
{
    public class ApiService
    {
        public HttpClient ApiBeachStart()
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("https://localhost:7001");

            return client;
        }
    }
}