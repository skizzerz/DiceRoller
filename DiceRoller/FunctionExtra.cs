using System;
using System.Collections.Generic;
using System.Text;

namespace Dice
{
    public class FunctionExtra
    {
        /// <summary>
        /// Name of the extra, in all-lowercase
        /// </summary>
        public string ExtraName { get; private set; }

        /// <summary>
        /// Name of the function, in all-lowercase
        /// </summary>
        public string FunctionName { get; private set; }

        private readonly Dictionary<string, string> _multipartFollowers = new Dictionary<string, string>();

        /// <summary>
        /// Name of context-dependent extras that can follow this extra, as a map of
        /// lowercase extra names to lowercase function names
        /// </summary>
        public IReadOnlyDictionary<string, string> MultipartFollowers => _multipartFollowers;

        /// <summary>
        /// Construct a new FunctionExtra
        /// </summary>
        /// <param name="extraName">Extra name</param>
        /// <param name="functionName">Function name this extra is mapped to</param>
        public FunctionExtra(string extraName, string functionName)
        {
            ExtraName = extraName?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(extraName));
            FunctionName = functionName?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(functionName));
        }

        /// <summary>
        /// Multipart extras allow for context-dependent extra names. When a multipart extra is defined,
        /// the context-dependent extras added with this method take precedence over extras defined globally.
        /// </summary>
        /// <param name="extraName">Name of the multipart extra; must be unique for this particular FunctionExtra instance</param>
        /// <param name="functionName">Function to call when this multipart extra is encountered</param>
        public void AddMultipart(string extraName, string functionName)
        {
            if (extraName == null)
            {
                throw new ArgumentNullException(nameof(extraName));
            }

            if (functionName == null)
            {
                throw new ArgumentNullException(nameof(functionName));
            }

            _multipartFollowers.Add(extraName.ToLowerInvariant(), functionName.ToLowerInvariant());
        }
    }
}
