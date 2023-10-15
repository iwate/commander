using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iwate.Commander.Tests
{
    public class PathResolverTest
    {
        [Fact]
        public void Parse()
        {
            var src = new InvokeRequestBase
            {
                Id = "test",
                Command = "TestCommand",
                InvokedBy = "xunit",
                Partition = null
            };
            var pr = new CommandStoragePathResolver("queue", "state");
            var path = pr.GetQueuePath(src);
            var success = pr.TryParseQueue(path, out var parsed);

            Assert.True(success);
            Assert.Equivalent(src, parsed);
        }
    }
}
