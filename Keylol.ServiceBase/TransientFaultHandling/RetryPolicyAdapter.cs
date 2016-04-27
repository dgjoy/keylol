using System;
using ChannelAdam.TransientFaultHandling;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Keylol.ServiceBase.TransientFaultHandling
{
    /// <summary>
    ///     An adapter for the Microsoft Transient Fault Handling Core <see cref="RetryPolicy" />.
    /// </summary>
    public class RetryPolicyAdapter : IRetryPolicyFunction
    {
        private readonly RetryPolicy _retryPolicy;

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RetryPolicyAdapter" /> class.
        /// </summary>
        /// <param name="retryPolicy">The retry policy.</param>
        public RetryPolicyAdapter(RetryPolicy retryPolicy)
        {
            _retryPolicy = retryPolicy;
        }

        #endregion Constructors

        /// <summary>
        ///     Executes the specified function.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function.</param>
        /// <returns>The result of the function.</returns>
        public TResult Execute<TResult>(Func<TResult> func)
        {
            return _retryPolicy != null ? _retryPolicy.ExecuteAction(func) : func.Invoke();
        }

        #region Public Static Methods

        /// <summary>
        ///     Creates an adapter from a Microsoft Practices <see cref="RetryPolicy" />.
        /// </summary>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <returns>A <see cref="RetryPolicyAdapter" />.</returns>
        public static RetryPolicyAdapter CreateFrom(RetryPolicy retryPolicy)
        {
            return new RetryPolicyAdapter(retryPolicy);
        }

        #endregion Public Static Methods

        #region Operators

        /// <summary>
        ///     Performs an implicit conversion from <see cref="RetryPolicy" /> to <see cref="RetryPolicyAdapter" />.
        /// </summary>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        public static implicit operator RetryPolicyAdapter(RetryPolicy retryPolicy)
        {
            return new RetryPolicyAdapter(retryPolicy);
        }

        #endregion Operators
    }
}