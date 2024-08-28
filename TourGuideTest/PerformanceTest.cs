using GpsUtil.Location;
using System.Diagnostics;
using TourGuide.Users;
using Xunit.Abstractions;

namespace TourGuideTest
{
    public class PerformanceTest : IClassFixture<DependencyFixture>
    {
        /*
         * Note on performance improvements:
         * 
         * The number of generated users for high-volume tests can be easily adjusted using this method:
         * 
         *_fixture.Initialize(100000); (for example)
         * 
         * 
         * These tests can be modified to fit new solutions, as long as the performance metrics at the end of the tests remain consistent.
         * 
         * These are the performance metrics we aim to achieve:
         * 
         * highVolumeTrackLocation: 100,000 users within 15 minutes:
         * Assert.True(TimeSpan.FromMinutes(15).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
         *
         * highVolumeGetRewards: 100,000 users within 20 minutes:
         * Assert.True(TimeSpan.FromMinutes(20).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
        */

        private readonly DependencyFixture _fixture;

        private readonly ITestOutputHelper _output;

        public PerformanceTest(DependencyFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }
        // FIX Perf optimization ==> async&await / new List<Task>() + await Task.WhenAll(tasks).
        // FIX06 set HighVolumeGetRewards test to be played.
        // [Fact(Skip = "Delete Skip when you want to pass the test")] ==> [Fact].
        [Fact]
        public async Task HighVolumeTrackLocation()
        {
            //We can rise test users number for testing application performances.
            _fixture.Initialize(100000);

            List<User> allUsers = _fixture.TourGuideService.GetAllUsers();

            Stopwatch stopWatch = new();
            stopWatch.Start();
            sdfjlksdj
            var tasks = new List<Task>();
            foreach (var user in allUsers)
            {
                tasks.Add(_fixture.TourGuideService.TrackUserLocation(user));                
            }
            await Task.WhenAll(tasks);
            stopWatch.Stop();
            _fixture.TourGuideService.Tracker.StopTracking();

            _output.WriteLine($"highVolumeTrackLocation: Time Elapsed: {stopWatch.Elapsed.TotalSeconds} seconds.");

            Assert.True(TimeSpan.FromMinutes(15).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
        }
        // FIX Perf optimization ==> async&await / multiple await / new List<Task>() + await Task.WhenAll(tasks).
        // FIX05 set HighVolumeGetRewards test to be played.
        // [Fact(Skip = "Delete Skip when you want to pass the test")] ==> [Fact].
        [Fact]
        public async Task HighVolumeGetRewards()
        {
            //We can rise test users number for testing application performances.
            _fixture.Initialize(100);

            Stopwatch stopWatch = new();
            stopWatch.Start();

            var attraction = await _fixture.GpsUtil.GetAttractions();
            List<User> allUsers = _fixture.TourGuideService.GetAllUsers();
            
            allUsers.ForEach(u => u.AddToVisitedLocations(new VisitedLocation(u.UserId, attraction[0], DateTime.Now)));            

            var tasks = new List<Task>();
            foreach (var user in allUsers)
            {                
                tasks.Add(_fixture.RewardsService.CalculateRewards(user));                
            }
            await Task.WhenAll(tasks);
            allUsers.ForEach(user => Assert.True(user.UserRewards.Count > 0));

            stopWatch.Stop();
            _fixture.TourGuideService.Tracker.StopTracking();

            _output.WriteLine($"highVolumeGetRewards: Time Elapsed: {stopWatch.Elapsed.TotalSeconds} seconds.");
            Assert.True(TimeSpan.FromMinutes(20).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
        }
    }
}
