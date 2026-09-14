using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic.Exceptions
{
    public class ValidationAppException : Exception
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationAppException(Dictionary<string, string[]> errors) : base("Validation failed")
        {
            Errors = errors;
        }

        public ValidationAppException(string field, string error) : base("Validation failed")
        {
            Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
        }
    }
}
