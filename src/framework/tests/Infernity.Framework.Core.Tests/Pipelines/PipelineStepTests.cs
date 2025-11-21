using Infernity.Framework.Core.Pipelines;

namespace Infernity.Framework.Core.Tests.Pipelines;

[TestFixture]
public class PipelineStepTests
{
    [Test]
    public void StepsShouldBeExecutedInOrder()
    {
        static IPipelineStep<IList<int>> Create(int valueToAdd)
        {
            return new DelegatePipelineStep<IList<int>>(((ints,
                action) =>
            {
                ints.Add(valueToAdd);
                action();
            }));
        }
        
        IReadOnlyList<IPipelineStep<IList<int>>> steps =
        [
            Create(1),
            Create(2),
            Create(3),
            Create(4)
        ];

        var compiled = steps.Compile();
        
        var data = new List<int>();
        
        compiled.Invoke(data);
        
        Assert.That(data, Is.EqualTo(new[] { 1, 2, 3,4 }));
    }
}