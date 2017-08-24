namespace QueryBuilder
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Breakpoint since I'm too lazy to write to the Console right now...
            System.Diagnostics.Debugger.Break();

            using (var queryBuilder = new QueryBuilder())
            {
                var basic = queryBuilder.Build($@"
                    SELECT  *
                    FROM    YourTable
                    WHERE   TheAnswer = { 42 }
                ");

                var collection = queryBuilder.Build($@"
                    SELECT  *
                    FROM    YourTable
                    WHERE   TheAnswer IN ({ new int[] { 1, 2, 3 } })
                ");

                var reuse = queryBuilder.Build($@"
                    SELECT  *
                    FROM    YourTable
                    WHERE   TheAnswer IN ({ new int[] { 1, 2, 3 } })
                    AND     Foo = { 2 }
                ");
            }
        }
    }
}
