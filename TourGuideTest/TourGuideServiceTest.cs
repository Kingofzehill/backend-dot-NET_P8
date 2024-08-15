using GpsUtil.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourGuide.Services;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuideTest
{
    public class TourGuideServiceTour : IClassFixture<DependencyFixture>
    {
        private readonly DependencyFixture _fixture;

        public TourGuideServiceTour(DependencyFixture fixture)
        {
            _fixture = fixture;
        }

        public void Dispose()
        {
            _fixture.Cleanup();
        }

        // FIX Perf optimization ==> async&await. 
        [Fact]
        public async Task GetUserLocation()
        {
            _fixture.Initialize(0);
            var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
            var visitedLocation = await _fixture.TourGuideService.TrackUserLocation(user);
            _fixture.TourGuideService.Tracker.StopTracking();

            Assert.Equal(user.UserId, visitedLocation.UserId);
        }

        [Fact]
        public void AddUser()
        {
            _fixture.Initialize(0);
            var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
            var user2 = new User(Guid.NewGuid(), "jon2", "000", "jon2@tourGuide.com");

            _fixture.TourGuideService.AddUser(user);
            _fixture.TourGuideService.AddUser(user2);

            var retrievedUser = _fixture.TourGuideService.GetUser(user.UserName);
            var retrievedUser2 = _fixture.TourGuideService.GetUser(user2.UserName);

            _fixture.TourGuideService.Tracker.StopTracking();

            Assert.Equal(user, retrievedUser);
            Assert.Equal(user2, retrievedUser2);
        }

        [Fact]
        public void GetAllUsers()
        {
            _fixture.Initialize(0);
            var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
            var user2 = new User(Guid.NewGuid(), "jon2", "000", "jon2@tourGuide.com");

            _fixture.TourGuideService.AddUser(user);
            _fixture.TourGuideService.AddUser(user2);

            List<User> allUsers = _fixture.TourGuideService.GetAllUsers();

            _fixture.TourGuideService.Tracker.StopTracking();

            Assert.Contains(user, allUsers);
            Assert.Contains(user2, allUsers);
        }

        // FIX Perf optimization ==> async&await. 
        [Fact]
        public async Task TrackUser()
        {
            _fixture.Initialize();
            var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
            var visitedLocation = await _fixture.TourGuideService.TrackUserLocation(user);

            _fixture.TourGuideService.Tracker.StopTracking();

            Assert.Equal(user.UserId, visitedLocation.UserId);
        }

        // FIX Perf optimization ==> async&await / multiple await. 
        // FIX03 set GetNearbyAttractions test to be played:
        // [Fact(Skip = "Not yet implemented")] ==> [Fact].
        [Fact]
        public async Task GetNearbyAttractions()
        {
            _fixture.Initialize(0);
            // FIX03.1 & 3.2 set ProxmityBuffer from 10 to int.MaxValue.
            // It increases the detection of proximity attractions.
            _fixture.RewardsService.SetProximityBuffer(int.MaxValue);
            // FIX03.6 set ProxmityRange from 200 to int.MaxValue.
            // It increases the detection of proximity attractions.
            _fixture.RewardsService.SetAttractionProximityRange(int.MaxValue);

            var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
            var visitedLocation = await _fixture.TourGuideService.TrackUserLocation(user);

            List<Attraction> attractions = await _fixture.TourGuideService.GetNearByAttractions(visitedLocation);

            _fixture.TourGuideService.Tracker.StopTracking();

            Assert.Equal(5, attractions.Count);
        }

        // See FIX01 & FIX02 for code fixes done for validating GetTripDeals unit test.
        [Fact]
        public void GetTripDeals()
        {
            _fixture.Initialize(0);
            var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
            List<Provider> providers = _fixture.TourGuideService.GetTripDeals(user);

            _fixture.TourGuideService.Tracker.StopTracking();

            Assert.Equal(10, providers.Count);
        }
    }
}
