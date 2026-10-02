namespace GameOfLife.Tests
{
    internal sealed class SequenceRandom : Random
    {
        private readonly Queue<double> _values;

        /// Summary:
        /// Supplies a fixed sequence so the test does not depend on chance.
        public SequenceRandom(params double[] values)
        {
            _values = new Queue<double>(values);
        }

        /// Summary:
        /// Returns the next predefined random value.
        public override double NextDouble()
        {
            return _values.Dequeue();
        }
    }
}
