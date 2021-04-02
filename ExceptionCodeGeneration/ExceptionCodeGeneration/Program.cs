using Microsoft.CodeAnalysis;
using System;

namespace ExceptionCodeGeneration
{

    [Generator]
    internal sealed class JsonToExceptionGenerator
    {

    }


    [Serializable]
    public class RocketApiException : Exception
    {
        public RocketApiException() { }
        public RocketApiException(string message) : base(message) { }
        public RocketApiException(string message, Exception inner) : base(message, inner) { }
        protected RocketApiException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

   
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
