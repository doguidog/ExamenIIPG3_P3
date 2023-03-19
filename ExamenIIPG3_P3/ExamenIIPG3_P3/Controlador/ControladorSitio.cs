using ExamenIIPG3_P3.Modelo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ExamenIIPG3_P3.Controlador
{
    public class ControladorSitio
    {

        private static readonly string URL_SITIOS = "https://elkinhn.online/APIEX/";
        private static HttpClient client = new HttpClient();

        public async static Task<bool> UpdateSitio(Sitio sitio)
        {
            try
            {
                Uri requestUri = new Uri(URL_SITIOS + "actualizarsitio.php");
                var jsonObject = JsonConvert.SerializeObject(sitio);
                var content = new StringContent(jsonObject, Encoding.UTF8, "application/json");
                //var response = await client.PutAsync(requestUri, content);
                var response = await client.PostAsync(requestUri, content);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public static async Task<List<Sitio>> GetAllSite()
        {
            List<Sitio> listBooks = new List<Sitio>();
            try
            {
                var uri = new Uri(URL_SITIOS + "listasitios.php");
                var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    listBooks = JsonConvert.DeserializeObject<List<Sitio>>(content);
                    return listBooks;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return listBooks;
        }

        public async static Task<bool> DeleteSite(string id)
        {
            try
            {
                var uri = new Uri(URL_SITIOS + "eliminarsitio.php?id=" + id);
                var result = await client.GetAsync(uri);
                if (result.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public async static Task<bool> CreateSite(Sitio sitio)
        {
            try
            {
                Uri requestUri = new Uri(URL_SITIOS + "crearsitio.php");
                var jsonObject = JsonConvert.SerializeObject(sitio);
                var content = new StringContent(jsonObject, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(requestUri, content);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

    }
}
