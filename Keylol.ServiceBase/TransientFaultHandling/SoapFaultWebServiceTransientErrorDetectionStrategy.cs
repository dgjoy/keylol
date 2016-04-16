using System;
using System.ServiceModel;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Keylol.ServiceBase.TransientFaultHandling
{
    /// <summary>
    /// A transient error detection strategy for a SOAP-based web service - everything but a FaultException is a transient failure.
    /// </summary>
    public class SoapFaultWebServiceTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        /// <summary>
        /// Determines whether the specified exception represents a transient failure that can be compensated by a retry.
        /// </summary>
        /// <param name="ex">The exception object to be verified.</param>
        /// <returns>
        /// true if the specified exception is considered as transient; otherwise, false.
        /// </returns>
        public bool IsTransient(Exception ex)
        {
            return !(ex is FaultException);
        }
    }
}