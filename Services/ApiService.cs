namespace BeachSAWeb.Services
{
    public class ApiService
    {
        public HttpClient ApiBeachStart()
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("http://beachsa.somee.com");

            return client;
        }
    }
}
