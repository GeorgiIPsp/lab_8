namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void FirstTest()
        {
            string mainString = "Hello World";
            string substring = "World";
            bool result = mainString.Contains(substring);
            Assert.True(result);
        }
    }
}