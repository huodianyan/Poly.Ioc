# Poly.Ioc
a lightweight dependency injection container for Unity and any C# (or .Net) project.

## Features
- Zero dependencies
- Minimal Container core
- Lightweight and fast
- Bind types, singleton instances, factories
- Instance resolution by type, identifier and complex conditions
- Injection on constructor, fields and properties
- Adapted to all C# game engine

# Installation

# Overview

## What is a DI container?

A *dependency injection container* is a piece of software that handles the resolution of dependencies in objects. It's related to the [dependency injection](http://en.wikipedia.org/wiki/Dependency_injection) and [inversion of control](http://en.wikipedia.org/wiki/Inversion_of_control) design patterns.

The idea is that any dependency an object may need should be resolved by an external entity rather than the own object. Practically speaking, an object should not use `new` to create the objects it uses, having those instances *injected* into it by another object whose sole existence is to resolve dependencies.

So, a *dependency injection container* holds information about dependencies (the *bindings*) that can be injected into another objects by demand (injecting into existing objects) or during resolution (when you are creating a new object of some type).

## Why use a DI container?

In a nutshell, **to decouple your code**.

A DI container, in pair with a good architecture, can ensure [SOLID principles](http://en.wikipedia.org/wiki/SOLID_%28object-oriented_design%29) and help you write better code.

Using such container, you can easily work with abstractions without having to worry about the specifics of each external implementation, focusing just on the code you are writing. It's all related to dependencies: any dependency your code needs is not resolved directly by your code, but externally, allowing your code to deal only with its responsibilities.

As a plus, there are other benefits from using a DI container:

1. **Refactorability**: with your code decoupled, it's easy to refactor it without affecting the entire codebase.
2. **Reusability**: thinking about abstractions allows your code to be even more reusable by making it small and focused on a single responsibility.
3. **Easily change implementations**: given all dependencies are configured in the container, it's easy to change a implementation for a given abstraction. It helps e.g. when implementing generic functionality in a platform specific way.
4. **Testability**: by focusing on abstractions and dependency injection, it's easy to replace implementations with mock objects to test your code.
5. **Improved architecture**: your codebase will be naturally better and more organized because you'll think about the relationships of your code.
6. **Staying sane**: by focusing on small parts of the code and having a consistent architecture, the sanity of the developer is also ensured!

```csharp
public interface ITest1
{
}

public class Test1 : ITest1
{
    [IocInject(Name = "Test2")]
    private ITest2 test2;
    public ITest2 Test2 => test2;
}

public interface ITest2
{
}
public class Test2 : ITest2
{
    private readonly ITest1 test1;
    public ITest1 Test1 => test1;

    [IocInject]
    public Test2(ITest1 test1)
    {
        this.test1 = test1;
    }
}

container = new IocContainer();

container.Bind<ITest1, Test1>();
container.Bind<ITest2, Test2>(false, "Test2");
var test1 = new Test1();
container.Bind<ITest1>(test1);
container.Bind(typeof(ITest1), (c) => new Test1 { Value = 11 });
container.Build();

container.Resolve<ITest1>();
container.Resolve<ITest1>("Test1");
container.ResolveAll<ITest1>();
container.ResolveAll<ITest1>("Test1");

container.Inject(test1);

container.Unbind<ITest1>("Test1");
container.Unbind<ITest1>();

container.Dispose();
```

# License
The software is released under the terms of the [MIT license](./LICENSE.md).

No personal support or any guarantees.

# FAQ

# References

## Documents
- [IoC container solves a problem you might not have but it's a nice problem to have](http://kozmic.net/2012/10/23/ioc-container-solves-a-problem-you-might-not-have-but-its-a-nice-problem-to-have/)
- [IoC Container for Unity3D 每 part 1](http://www.sebaslab.com/ioc-container-for-unity3d-part-1/)
- [IoC Container for Unity3D 每 part 2](http://www.sebaslab.com/ioc-container-for-unity3d-part-2/)
- [The truth behind Inversion of Control 每 Part I 每 Dependency Injection](http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-i-dependency-injection/)
- [The truth behind Inversion of Control 每 Part II 每 Inversion of Control](http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-ii-inversion-of-control/)
- [The truth behind Inversion of Control 每 Part III 每 Entity Component Systems](http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-iii-entity-component-systems/)
- [The truth behind Inversion of Control 每 Part IV 每 Dependency Inversion Principle](http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-iii-entity-component-systems/)
- [From STUPID to SOLID Code!](http://williamdurand.fr/2013/07/30/from-stupid-to-solid-code/)

## Projects
- [microsoft/MinIoC](https://github.com/microsoft/MinIoC)
- [intentor/adic](https://github.com/intentor/adic)

## Benchmarks
- [danielpalme/IocPerformance](https://github.com/danielpalme)
