using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using Roguish;


public class Test_Initial
{
    [Test]
    [Arguments(1, 2, 3)]
    [Arguments(-2, 2, 0)]
    public async Task Test_Add(int a, int b, int sum)
    {
        await Assert.That(RootScreen.Add(a, b)).IsEqualTo(sum);
    }
}
