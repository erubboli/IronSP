using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using IronSharePoint.Administration;
using NUnit.Framework;

namespace IronSharePoint.Framework.Test
{
    [TestFixture]
    public class IronRuntime_Fixture
    {
        IronRuntime Sut;

        [Test]
        public void Ctor_IntializedWithoutError()
        {
            var registry = IronRegistry.Local;

            registry.Hives.Add();

            registry.Update();
            registry.Uncache();
            registry = IronRegistry.Local;
            foreach (var hiveSetup in registry.Hives)
            {
                System.Console.WriteLine(hiveSetup.Id);
            }
        } 
    }
}
