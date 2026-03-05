namespace Tests.Entities;

public class LabelNameCacheTest {

    private readonly InoreaderClient _inoreader = A.Fake<InoreaderClient>();
    private readonly LabelNameCache  _cache;

    public LabelNameCacheTest() {
        _cache = new LabelNameCache(_inoreader, TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task AlreadyCanceled() {
        CancellationToken     ct     = new(true);
        LabelNameCache.Labels actual = await _cache.GetLabelNames(ct);
        actual.Folders.Should().BeEmpty();
        actual.Tags.Should().BeEmpty();
    }

}