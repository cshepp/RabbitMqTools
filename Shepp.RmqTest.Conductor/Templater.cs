
using System.Collections.Generic;

namespace Shepp.RmqTest.Conductor
{
    public static class Templater
    {
        public static string Replace(string corpus, IDictionary<string, string> tokens)
        {
            foreach (var token in tokens)
            {
                corpus = corpus.Replace(token.Key, token.Value);
            }

            return corpus;
        }
    }
}