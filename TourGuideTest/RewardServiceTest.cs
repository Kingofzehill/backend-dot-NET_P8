using GpsUtil.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourGuide.Users;
using TourGuide.Utilities;

namespace TourGuideTest;

public class RewardServiceTest : IClassFixture<DependencyFixture>
{
    private readonly DependencyFixture _fixture;

    public RewardServiceTest(DependencyFixture fixture)
    {
        _fixture = fixture;
    }

    // FIX Perf optimization ==> async&await / multiple await.
    [Fact]
    public async Task UserGetRewards()
    {
        _fixture.Initialize(0);
        var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
        var attraction = await _fixture.GpsUtil.GetAttractions();
        user.AddToVisitedLocations(new VisitedLocation(user.UserId, attraction[0], DateTime.Now));
        await _fixture.TourGuideService.TrackUserLocation(user);
        var userRewards = user.UserRewards;
        _fixture.TourGuideService.Tracker.StopTracking();
        Assert.True(userRewards.Count == 1);
    }
    // FIX Perf optimization ==> async&await..
    [Fact]
    public async Task IsWithinAttractionProximity()
    {
        var attraction = await _fixture.GpsUtil.GetAttractions();
        Assert.True(_fixture.RewardsService.IsWithinAttractionProximity(attraction[0], attraction[0]));
    }
    // FIX Perf optimization ==> async&await / multiple await.
    // FIX04 set NearAllAttractions test to be played:
    // [Fact(Skip = "Needs fixed - can throw InvalidOperationException")] ==> [Fact].
    [Fact]
    public async Task NearAllAttractions()
    {
        _fixture.Initialize(1);
        _fixture.RewardsService.SetProximityBuffer(int.MaxValue);

        var user = _fixture.TourGuideService.GetAllUsers().First();
        await _fixture.RewardsService.CalculateRewards(user);
        var userRewards = _fixture.TourGuideService.GetUserRewards(user);
        _fixture.TourGuideService.Tracker.StopTracking();
        var attraction = await _fixture.GpsUtil.GetAttractions();
        Assert.Equal(attraction.Count, userRewards.Count);
    }

}
