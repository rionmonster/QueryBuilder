# QueryBuilder

A very simple, barebones query builder for building SQL-based queries within C#. The builder itself leverages C#'s string interpolation 
support to recognize items that need to be parameterized, and well - does it. 

## Goals

- **Short, simple syntax** - Focusing on ensuring that queries are readable.
- **Ensuring parameterization** - Parameters are there to keep you protected, and if they are easy to use then you'll probably use them.
- **Flexability** - Ideally, the query builder would be highly configurable and able to export directly to a SqlCommand or other common database access objects.

## Examples

#### Basic Example

This is a trivial example, but hopefully it demonstrates the purpose of the builder. It'll accept a single parameter within the WHERE clause
and behind the scenes it will convert that to a properly named parameter and store the value within the builder itself:

```
using (var queryBuilder = new QueryBuilder())
{
    var basic = queryBuilder.Build($@"
        SELECT  *
        FROM    YourTable
        WHERE   TheAnswer = { 42 }
    ");
}
```

After this code runs, the `basic` variable should look like the following:

```
SELECT  *
FROM    YourTable
WHERE   TheAnswer = @P1
```

And there will be a parameter added to the internal parameters store which has the following mapping as you might expect:

- @P1 = 42.

#### Collections Example

Collections are also supported* and should work roughly as you might expect as seen below:

```
using (var queryBuilder = new QueryBuilder())
{
    var basic = queryBuilder.Build($@"
        SELECT  *
        FROM    YourTable
        WHERE   TheAnswer IN ({ new int[] { 1, 2, 42, 3 } })
    ");
}
```

which would output:

```
SELECT  *
FROM    YourTable
WHERE   TheAnswer IN (@P1, @P2, @P3, @P4)
```

And as you might expect the parameters with their respective values were: 

- @P1 = 1
- @P2 = 2 
- @P3 = 42
- @P4 = 3

> **NOTE:** At present only primatives are supported, but then again I just threw this thing together so you shouldn't be using it anyways.

#### Reuse Example

The builder itself will also keep track of existing parameters to ensure that multiple references to the same parameter aren't unnecessarily
duplicated:

```
using (var queryBuilder = new QueryBuilder())
{
    var basic = queryBuilder.Build($@"
        SELECT  *
        FROM    YourTable
        WHERE   TheAnswer IN ({ new int[] { 1, 2, 42, 3 } })
        AND     TheLoneliestNumber = { 1 }
    ");
}
```

This would generate the following:

```
SELECT  *
FROM    YourTable
WHERE   TheAnswer IN (@P1, @P2, @P3, @P4)
AND     TheLoneliestNumber = @P1
```

Notice the repetition of the @P1 parameter.

## Unparameterized Parameters

Some scenarios involving building dynamic queries within C# may often require the use of parameters that are not parameterized (or that SQL cannot accept as
parameters such as table and column names without the use of dynamic SQL). We can accomplish this through the use of a ValueTuple parameter that accepts both
the value to be parameterized and a boolean to indicate if the value should be parameterized or not:

```
using (var queryBuilder = new QueryBuilder())
{
    var basic = queryBuilder.Build($@"
        SELECT  *
        FROM    {("YourTable", false)}
    ");
}

```

which would result similar to as you might expect dynamic SQL to work:

```
SELECT	*
FROM	YourTable
```

Or a more complex example might look like:

```
using (var queryBuilder = new QueryBuilder())
{
    var basic = queryBuilder.Build($@"
        SELECT  {("YourColumn", false)}
        FROM    SomeTable
		WHERE	TheAnswer IN {(new int[]{ 1, 2, 3}, false)}
		AND		{("SomeOtherColumn", false)} = { 42 }
    ");
}
```

which yields:

```
SELECT	YourColumn
FROM	SomeTable
WHERE	TheAnswer IN (1, 2, 3)
AND		SomeOtherColumn = 42
```

It's worth noting, much as I'll repeat below, that this is super dangerous and ripe for introducing some SQL Injection, which no one wants.

## Status

This project is primarily just a proof of concept at this point and isn't currently planned for any widespread production use, so please don't 
do so and if you do - I don't want to know about it (at least if anything _bad_ happens).
