using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Poly.Ioc.Tests
{
    [TestClass]
    public partial class ContainerTest
    {
        private IIocContainer container;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
        }
        [ClassCleanup]
        public static void ClassCleanup()
        {
        }
        [TestInitialize]
        public void TestInitialize()
        {
            container = new IocContainer();
        }
        [TestCleanup]
        public void TestCleanup()
        {
            container.Dispose();
            container = null;
        }

        [TestMethod]
        public void BindTest()
        {
            //Bind ToType
            container.Bind<ITest1, Test1>();
            Assert.IsTrue(container.HasBind<ITest1>());
            var obj1 = container.Resolve<ITest1>();
            var obj2 = container.Resolve<ITest1>();
            Assert.AreEqual(obj1, obj2);
            container.Unbind<ITest1>();
            Assert.IsFalse(container.HasBind<ITest1>());

            //Bind ToType with name
            var bindName = "Test1";
            container.Bind<ITest1, Test1>(false, bindName);
            Assert.IsTrue(container.HasBind<ITest1>(bindName));
            //Assert.IsFalse(container.HasBind<ITest1>());
            obj1 = container.Resolve<ITest1>(bindName);
            obj2 = container.Resolve<ITest1>(bindName);
            Assert.AreNotEqual(obj1, obj2);
            container.Unbind<ITest1>(bindName);
            Assert.IsFalse(container.HasBind<ITest1>(bindName));

            //Bind ToInstance
            var test1 = new Test1 { Value = 10 };
            container.Bind<ITest1>(test1);
            Assert.IsTrue(container.HasBind<ITest1>());
            Assert.AreEqual(test1, container.Resolve<ITest1>());
            container.Unbind<ITest1>();

            //Bind ToFunc
            container.Bind(typeof(ITest1), (c) => new Test1());
            obj1 = container.Resolve<ITest1>();
            obj2 = container.Resolve<ITest1>();
            Assert.AreEqual(obj1, obj2);
            container.Unbind<ITest1>();


            //Inject

        }
        [TestMethod]
        public void ResolveTest()
        {
            //Resolve all
            var test1 = new Test1 { Value = 10 };
            container.Bind<ITest1, Test1>();
            container.Bind<ITest1, Test1>(true);
            container.Bind<ITest1>(test1);
            container.Bind(typeof(ITest1), (c) => new Test1 { Value = 11 }, true);
            var test1s = container.ResolveAll<ITest1>();
            Assert.AreEqual(4, test1s.Count());
            container.Unbind<ITest1>();
        }
        [TestMethod]
        public void InjectTest()
        {
            //Inject
            var test1 = new Test1 { Value = 10 };
            //var test2 = new Test2();
            container.Bind<ITest1>(test1);
            Assert.IsTrue(container.HasBind<ITest1>());
            container.Bind<ITest2, Test2>();
            Assert.IsTrue(container.HasBind<ITest2>());
            Assert.IsTrue(container.TryResolve<ITest2>(out var test21));
            container.Build();
            var test2 = container.Resolve<ITest2>() as Test2;
            Assert.AreEqual(test2, test1.Test2);
            Assert.AreEqual(test1, test2.Test1);

        }
    }

    public interface ITest1
    {
        int Value { get; }
    }
    public class Test1 : ITest1
    {
        [IocInject(Name = "Test2")]
        private ITest2 test2;

        public int Value { get; set; }
        public ITest2 Test2 => test2;
    }
    public interface ITest2
    {
        int Value { get; }
    }
    public class Test2 : ITest2
    {
        private readonly ITest1 test1;
        public int Value { get; set; }
        public ITest1 Test1 => test1;

        [IocInject]
        public Test2(ITest1 test1)
        {
            this.test1 = test1;
        }
    }
}