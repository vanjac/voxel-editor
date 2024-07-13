using System;
using System.Collections.Generic;

abstract class ConfigParser {
    public delegate void ParseLineFn(string command, string args, int lineNum);
    public static readonly char[] wordSeparators = new char[] { ' ', '\t' };

    public class ConfigException : Exception {
        public ConfigException(string message) : base(message) { }
        public ConfigException(string message, int line)
            : base($"Error on line {line}: {message}") { }
        public ConfigException(Exception inner, int line)
            : base("Exception while reading line " + line, inner) { }
    }
}

class ConfigParser<State> : ConfigParser where State : struct {
    public State state;
    private Stack<State> stateStack = new Stack<State>();

    public void Parse(System.IO.TextReader reader, ParseLineFn callback) {
        int numStates = stateStack.Count;

        string readLine;
        for (int l = 1; (readLine = reader.ReadLine()) != null; l++) {
            string line = readLine.Trim(wordSeparators);

            if (line == "" || line.StartsWith("#")) {
                // ignore
            } else if (line == "{") {
                stateStack.Push(state);
            } else if (line == "}") {
                if (stateStack.Count == 0) {
                    throw new ConfigException("Too many closing braces", l);
                }
                state = stateStack.Pop();
            } else {
                int argsIdx = line.IndexOfAny(wordSeparators);
                if (argsIdx == -1) { argsIdx = line.Length; }
                string command = line.Substring(0, argsIdx);
                string args = line.Substring(argsIdx).Trim(wordSeparators);
                try {
                    callback(command, args, l);
                } catch (ConfigException) {
                    throw;
                } catch (Exception e) {
                    throw new ConfigException(e, l);
                }
            }
        }

        if (numStates != stateStack.Count) {
            throw new ConfigException("Mismatched braces");
        }
    }
}
