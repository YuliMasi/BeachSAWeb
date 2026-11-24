namespace BeachSAWeb.Services
{
    /// <summary>
    /// Centraliza la dirección del servidor de la Web API.
    /// </summary>
    public class ApiService
    {
        /// <summary>
        /// Crea el cliente HTTP configurado con la dirección base de la API.
        /// </summary>
        /// <returns>Un HttpClient listo para consumir la API.</returns>
        public HttpClient ApiBeachStart()
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("http://beachsa.somee.com");

            return client;
        }
    }
}
