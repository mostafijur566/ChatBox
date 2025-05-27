using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Responses
{
    public class ResponseWithStatuscode
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public List<String>? Errors { get; set; }

        public ResponseWithStatuscode(int statusCode, string message, List<string>? errors = null)
        {
            StatusCode = statusCode;
            Message = message;
            Errors = errors;
        }
    }
}