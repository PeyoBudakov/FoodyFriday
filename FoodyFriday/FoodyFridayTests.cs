using FoodyFriday.Methods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.IO;
using System.Net;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FoodyFriday
{
    public class FoodyFridayTests

    {
        private RestClient client;
        private static string createdFoodId;
        //your link here
        private const string baseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";

        [OneTimeSetUp]
        public void Setup()
        {
            // your credentials
            string token = GetJwtToken("peyopeyo", "peyopeyo");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }
        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;

        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }

        [Test, Order(1)]
        public void CreateANewFood_WithTheRequiredFields_ShouldSucceed()
        {
            //Arrange
            var newFood = new FoodDTO
            {
                Name = "new food",
                Description = "new food description",
                Url = ""
            };

            //Act
            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(newFood);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Content, Does.Contain("foodId"));

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            createdFoodId = responseData.foodId;

            Console.WriteLine(createdFoodId);
        }

        [Test, Order(2)]
        public void EditTheTitleOfTheFoodThatYouCreated_ShouldSucceed()
        {
            //Arrange
            var editedFood = new[]
            { new{
                path = "/name",
                op = "replace",
                value = "edited food name"
            } };

            //Act
            var request = new RestRequest($"/api/Food/Edit/{createdFoodId}", Method.Patch);
            request.AddJsonBody(editedFood);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseData.Msg, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllFoods_ShouldSucceed()
        {
            //Arrange Act
            var request = new RestRequest("/api/Food/All", Method.Get);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseDataArray = JsonSerializer.Deserialize<ApiResponseDTO[]>(response.Content);
            Assert.That(responseDataArray, Is.Not.Empty);
        }

        [Test, Order(4)]

        public void DeleteTheFoodYouEdited_ShouldSucceed()
        {
            //Arrange Act
            var request = new RestRequest($"/api/Food/Delete/{createdFoodId}", Method.Delete);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));

        }

        [Test, Order(5)]
        public void CreateFoodWithoutTheRequiredFields_ShouldFail()
        {
            //Arrange
            var newFood = new FoodDTO
            {
                Name = "",
                Description = "",
                Url = ""
            };

            //Act
            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(newFood);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditANonExistingFood_ShouldFial()
        {
            //Arrange
            var editedFood = new[]
            { new{
                path = "/name",
                op = "replace",
                value = "edited food name"
            } };

            //Act
            var request = new RestRequest($"/api/Food/Edit/666", Method.Patch);
            request.AddJsonBody(editedFood);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No food revues..."));
        }

        [Test, Order(7)]
        public void DeleteANonExistingFood_ShouldFail()
        {
            //Arrange Act
            var request = new RestRequest($"/api/Food/Delete/999", Method.Delete);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this food revue!"));
        }

    }
}