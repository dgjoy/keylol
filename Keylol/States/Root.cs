using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Keylol.States
{
    /// <summary>
    /// 状态树根
    /// </summary>
    public class Root
    {
        public static Task<Root> Get()
        {
            return Task.FromResult(new Root {Sum = 8});
        }

        public static Task<int> GetSum(int a, int b)
        {
            return Task.FromResult(a + b);
        }

        public int Sum { get; set; }
    }
}