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
                // Basic single-parameter example
                var basic = queryBuilder.Build($@"
                    SELECT  *
                    FROM    YourTable
                    WHERE   TheAnswer = { 42 }
                ");

                // Example demonstrating collections
                var collection = queryBuilder.Build($@"
                    SELECT  *
                    FROM    YourTable
                    WHERE   TheAnswer IN ({ new int[] { 1, 2, 3 } })
                ");

                // Example demonstrating parameter reuse
                var reuse = queryBuilder.Build($@"
                    SELECT  *
                    FROM    YourTable
                    WHERE   TheAnswer IN ({ new int[] { 1, 2, 3 } })
                    AND     Foo = { 2 }
                ");

                // Example demonstrating parameterized and explicitly unparameterized arguments
                var unparameterized = queryBuilder.Build($@"
                    SELECT  Something,
                            { ("Column", false) }
                    FROM    { ("Table", false) }
                    WHERE   TheAnswer = { 42 } 
                    AND     { ("Column", false) } = { ("Rion", false) }
                    OR      1 = { (2, false) }
                ");

                // Example demonstrating unparameterized collections
                var unparameterizedCollections = queryBuilder.Build($@"
                    SELECT  Something,
                    FROM    { ("Table", false) }
                    WHERE   Flavor IN ({ (new string[] { "Vanilla", "Chocolate" }, false) })
                ");
            }
        }
    }
}
