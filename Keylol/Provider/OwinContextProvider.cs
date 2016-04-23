using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Microsoft.Owin;

namespace Keylol.Provider
{
    /// <summary>
    /// 提供 IOwinContexn 对象
    /// </summary>
    public class OwinContextProvider
    {
        /// <summary>
        /// 当前 OWIN Context
        /// </summary>
        public IOwinContext Current
        {
            get
            {
                var context = (IOwinContext) CallContext.LogicalGetData("IOwinContext");
                if (context == null)
                {
                    try
                    {
                        context = HttpContext.Current.Request.GetOwinContext();
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                return context;
            }
        }
    }
}