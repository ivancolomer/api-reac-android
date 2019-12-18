using Nancy;
using Nancy.Testing;
using NUnit.Framework;
using REAC_AndroidAPI;
using REAC_AndroidAPI.Entities;
using REAC_AndroidAPI.Utils.Responses;
using System.Collections.Generic;

namespace NUnitTestREAC_AndroidAPI
{
    public class Tests
    {
        private DefaultNancyBootstrapper boostrapper;
        private Browser browser;

        [SetUp]
        public void Setup()
        {
            boostrapper = new DefaultNancyBootstrapper();
            browser = new Browser(boostrapper, defaults: to => to.Accept("application/json"));

            TestContext.WriteLine("test setup done");
        }


        [Test]
        public void should_return_ipadress()
        {
            // When
            var result = browser.Get("/api/ipaddress", with => {
                with.HttpRequest();
            }).Result;

            MainResponse<string> response = result.Body.DeserializeJson<MainResponse<string>>();

            // Then
            TestContext.WriteLine(response.Content);
            Assert.IsTrue(response.Content.StartsWith("192.168"));
        }

        [Test]
        public void login_should_return_unique_session_id()
        {
            // When
            var result1 = browser.Get("/api/login", with => {
                with.HttpRequest();
                with.Query("user_name", "prueba1");
                with.Query("password", "HnP0BkORqL08ocPtddb8HQJmx3MH0UXMLG7FoiRDQEA=");
            }).Result;

            var result2 = browser.Get("/api/login", with => {
                with.HttpRequest();
                with.Query("user_name", "prueba1");
                with.Query("password", "HnP0BkORqL08ocPtddb8HQJmx3MH0UXMLG7FoiRDQEA=");
            }).Result;

            MainResponse<string> response1 = result1.Body.DeserializeJson<MainResponse<string>>();
            MainResponse<string> response2 = result2.Body.DeserializeJson<MainResponse<string>>();

            TestContext.WriteLine(response1.Content);
            TestContext.WriteLine(response2.Content);

            // Then
            Assert.AreNotEqual(response1.Content, response2.Content);
        }

        public void login_should_remove_previous_session_id_from_same_user()
        {
            // When
            var result1 = browser.Get("/api/login", with => {
                with.HttpRequest();
                with.Query("user_name", "prueba1");
                with.Query("password", "HnP0BkORqL08ocPtddb8HQJmx3MH0UXMLG7FoiRDQEA=");
            }).Result;

            var result2 = browser.Get("/api/login", with => {
                with.HttpRequest();
                with.Query("user_name", "prueba1");
                with.Query("password", "HnP0BkORqL08ocPtddb8HQJmx3MH0UXMLG7FoiRDQEA=");
            }).Result;

            MainResponse<string> response1 = result1.Body.DeserializeJson<MainResponse<string>>();
            MainResponse<string> response2 = result2.Body.DeserializeJson<MainResponse<string>>();

            var result3 = browser.Get("/api/users", with =>
            {
                with.HttpRequest();
                with.Query("session_id", response1.Content);
            }).Result;

            var result4 = browser.Get("/api/users", with =>
            {
                with.HttpRequest();
                with.Query("session_id", response2.Content);
            }).Result;

            // Then

            Assert.DoesNotThrow(() =>
            {
                MainResponse<byte> response3 = result3.Body.DeserializeJson<MainResponse<byte>>();
            });

            Assert.DoesNotThrow(() =>
            {
                MainResponse<List<User>> response4 = result4.Body.DeserializeJson<MainResponse<List<User>>>();
            });
        }
    }
}