using GoRogue.Random;
using Roguish;
using ShaiRandom.Generators;

namespace Roguish_Tests;

public class TestInitial
{
    private static readonly IEnhancedRandom Rng = GlobalRandom.DefaultRNG;

    [Test]
    [Arguments(1, 3)]
    [Arguments(0, 100)]
    public async Task Test_Rng(int a, int b)
    {
        var list = Enumerable.Repeat(0, 100).Select(_ => Rng.NextInt(a, b)).ToList();
        await Assert.That(list.All(x => x >= a && x < b)).IsTrue();
    }
}