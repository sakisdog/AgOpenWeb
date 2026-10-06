using AgOpenWeb.Models;
using AgOpenWeb.Models.Timing;
using AgOpenWeb.Services;

namespace AgOpenWeb.Services.Tests;

[TestFixture]
public class GpsServiceTests
{
    private GpsService _service = null!;
    private TestClock _clock = null!;

    [SetUp]
    public void SetUp()
    {
        _clock = new TestClock();
        Clock.Set(_clock);
        _service = new GpsService();
    }

    [TearDown]
    public void TearDown()
    {
        Clock.Reset();
    }

    [Test]
    public void IsGpsDataOk_NoDataReceived_ReturnsFalse()
    {
        Assert.That(_service.IsGpsDataOk(), Is.False);
    }

    [Test]
    public void IsGpsDataOk_AfterValidUpdate_ReturnsTrue()
    {
        _service.UpdateGpsData(new GpsData { IsValid = true });
        Assert.That(_service.IsGpsDataOk(), Is.True);
    }

    [Test]
    public void IsConnected_AfterValidUpdate_IsTrue()
    {
        _service.UpdateGpsData(new GpsData { IsValid = true });
        Assert.That(_service.IsConnected, Is.True);
    }

    [Test]
    public void IsConnected_AfterInvalidUpdate_IsFalse()
    {
        _service.UpdateGpsData(new GpsData { IsValid = false });
        Assert.That(_service.IsConnected, Is.False);
    }

    [Test]
    public void IsGpsDataOk_SetsDisconnectedOnTimeout()
    {
        _service.UpdateGpsData(new GpsData { IsValid = true });
        Assert.That(_service.IsConnected, Is.True);

        // Before 2000ms timeout
        _clock.AdvanceMs(1900);
        Assert.That(_service.IsGpsDataOk(), Is.True);
        Assert.That(_service.IsConnected, Is.True);

        // Past 2000ms timeout (1900 + 200 = 2100ms)
        _clock.AdvanceMs(200);
        bool ok = _service.IsGpsDataOk();
        Assert.That(ok, Is.False);
        Assert.That(_service.IsConnected, Is.False);
    }

    [Test]
    public void IsConnected_RecoverAfterNewData()
    {
        _service.UpdateGpsData(new GpsData { IsValid = true });
        _clock.AdvanceMs(2100);
        _service.IsGpsDataOk();
        Assert.That(_service.IsConnected, Is.False);

        _service.UpdateGpsData(new GpsData { IsValid = true });
        Assert.That(_service.IsConnected, Is.True);
        Assert.That(_service.IsGpsDataOk(), Is.True);
    }

    [Test]
    public void Start_SetsConnected()
    {
        _service.Start();
        Assert.That(_service.IsConnected, Is.True);
    }

    [Test]
    public void Stop_ClearsConnected()
    {
        _service.Start();
        _service.Stop();
        Assert.That(_service.IsConnected, Is.False);
    }
}
