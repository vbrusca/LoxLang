# LoxLang
A C# and Java interpreter for the Lox programming language by Robert Nystrom. Easily use it as an embedded scripting language for your programs.

Special thanks to Mr. Nystrom, the website for the text this project was based on can be found here:
[Crafting Interpreters](https://craftinginterpreters.com/)

This implementation of Lox doesn't support many extended features. The language is simple in nature. This implementation supports the following unique implementation differences.

1. Support for 'null' along side 'nil'.
2. Command line/programmatic support for processing a 'globals' file that contains Lox source code that drives entries into the global environment.
3. Command line/programmatic support for processing a 'globals' script string that contains Lox source code that drives entries into the global environment.
4. Programmatic support for injecting a list of key, value pairs into the global environment.
5. Support for Array, List, and Dict data structures with basic get/set/add/length functionality depending on the data structure.
6. Support for multi-line comments with nested multi-line comments.
7. Support for a generic, easily extensible function called "sys" that's first argument is a string indicating what code to run. Used as a quick an easy way to integrate the Lox interpreter into an application environment.
8. Arbitrary number of function arguments via special handling of an arity value of -1.
9. **Support for retrieving the script to execute from a URL.
10. **Support for retrieving a globals script to run from a URL.
11. **Support for returning a global variable after script execution to a URL.

** With this support Lox can act as a data driven execution subsystem in part of a larger application where the language has been integrated to some extent using globals, "sys", or other customizations.

# Developers
Victor G. Brusca
<br/>Carlo Bruscani

# CLI Arguments
An example of the CLI arguments are as follows:

<pre>
Usage: JavaLox  ([-f script file] | [-s script string] | [-u script url] | [-p REPL]) & [-gf script file] [-gs script string] [-gu script url] [-ru url] [-gv global variable name to respond with]

Where:
-f = Run a Lox script.
-s = Run a Lox program in a string.
-p = Runs a Lox REPL.
-u = Runs a script from a URL, expect a script JSON = { "script":"some Lox code here" }.
-gf = A Lox file script to process, loading globals, before running the intended script.
-gs = A Lox string script to process, loading globals, before running the intended script.
-gu = A Lox URL script to process, loading globals, before running the intended script, expect a script JSON = { "script":"some Lox code here" }.
-ru = A URL to send a execution response message to.
-gv = The global variable to return after script execution, doesn't support Objects.
</pre>

## Example CLI Call
<pre>
CsLox -f "C:\FILES\OIT_LAPTOP_BACKUP\DOCUMENTS\GitHub\LoxLang\test.lox" -gs "var GBL_BASE_NAT_LOG = 2.71828;" -gf "C:\FILES\OIT_LAPTOP_BACKUP\DOCUMENTS\GitHub\LoxLang\globals.lox"

CsLox -u https://localhost:7109/getScript -gs "var GBL_BASE_NAT_LOG = 2.71828;" -gf "C:\FILES\OIT_LAPTOP_BACKUP\DOCUMENTS\GitHub\LoxLang\globals.lox" -gu https://localhost:7109/getGlobal -ru https://localhost:7109/setAnswer -gv urlGlobal
</pre>

## Testing URL Functionality
You can test URL functionality by using the CsLoxTestServer project, Visual Studio, and either Lox interpretter. You'll have to add a certificate to the JRE by using a command similar to this run as administrator in the current JRE bin dir.

<pre>
keytool -import -trustcacerts -alias LOX_LOCAL -file "C:\Users\brusc\Downloads\localhost.pem | .cer" -keystore "C:\Program Files\Java\jdk-21\lib\security\cacerts" -storepass LOX_LOCAL            
</pre>

## Example Output from a URL Test

<pre>
run:
[line 0] Log: Found global file to import: C:\FILES\OIT_LAPTOP_BACKUP\DOCUMENTS\GitHub\LoxLang\globals.lox
[line 0] Log: Found global script to import: var GBL_BASE_NAT_LOG = 2.71828;
[line 0] Log: Found global script to import: https://localhost:7109/getGlobal
SLF4J: Failed to load class "org.slf4j.impl.StaticLoggerBinder".
SLF4J: Defaulting to no-operation (NOP) logger implementation
SLF4J: See http://www.slf4j.org/codes.html#StaticLoggerBinder for further details.
HTTP/1.1
200
OK
[line 0] Log: Run URL Response: '{ "script":"var urlGlobal = true;"}'
HTTP/1.1
200
OK
[line 0] Log: Run URL Response: '{ "script":"urlGlobal = false;
print urlGlobal;"}'
false
HTTP/1.1
200
OK
[line 0] Log: Return URL Response: '{ "msg":"Found answer: 'false, urlGlobal'" }'
BUILD SUCCESSFUL (total time: 1 second)   
</pre>

# Errata
<ul>
   <li>The C# version of Lox uses Visual Studio.</li>
   <li>The Java version uses NetBeans.</li>
   <li>There are some hardcoded paths in the project configuration for easy testing. You'll have to adjust these to match your environment if you expect to test the interpreter via IDE debugging.</li>
   <li>** The test file, "test.lox", expects the globals from the file "globals.lox" and the CLI global "GBL_BASE_NAT_LOG" to be injected before the test script is executed. **</li>
</ul>

# Lox Data Structures
The following are added data structures that are simple but useful.

## Arrays
<pre>
var array = Array(3);
print array.length;    // Prints out "3".
array.set(1, "new");   // Sets the value at array index 1 to "new"
print array.get(1);    // Prints the value at index 1, "new".
</pre>

## Lists
<pre>
var list = List();
print list.length;     // Prints out "0".
list.add("new");       // Adds a new value, "new", into the list.
list.add("old");       // Adds a new value, "old", into the list.
list.set(0, "newer");  // Sets the value at position 0 to "newer"
print list.get(0);     // Prints the value at index 0, "newer".
</pre>

## Dict
<pre>
var dct = Dict();
dct.set("hello", "world");  // Sets the key "hello" equal to "world"
dct.set(2, "test");         // Sets the key 2 equal to "test"
print dct.length;           // Prints out "2"
print dct.get(2);           // Prints out "test"
print dct.get("hello");     // Prints out "world"
</pre>

# Extensibility
To demonstrate the extensibility we'll look at the Java version but the C# version is almost identical.

<pre>
public Interpreter(HashMap&lt;Object, HandleLoxCallables&gt; handleCalls, HandleLoxGlobals externalGlobals) {
   this.handleCalls = handleCalls;
   this.externalGlobals = externalGlobals;
   initialize();
}

private void initialize() {
   environment = globals;
   globals.define("clock", new Clock());
   globals.define("Array", new Array());
   globals.define("List", new LinkedList());
   globals.define("Dict", new Dict());      

   externalFunctions = new ExternalLoxCallable(handleCalls);
   globals.define("sys", externalFunctions);
   globals.define("GBL_NUM_MIN", Double.MAX_VALUE * -1.0);       //-1.7976931348623157E+308
   globals.define("GBL_NUM_MAX", Double.MAX_VALUE);              // 1.7976931348623157E+308

   if (externalGlobals != null) {
      externalGlobals.defineGlobals(globals);
   }
}
</pre>

Starting with the external globals, if defined the "defineGlobals" method is called and passed a reference to the globals environment.

<pre>
public void defineGlobals(Environment loxGlobals) {
   if (externalGlobals != null) {
      for (String key : externalGlobals.keySet()) {
         loxGlobals.define(key, externalGlobals.get(key));
      }
   }
}
</pre>

If an instance of the "ExternalLoxGlobal" class is provided to the Interpreter class at instantiation the extra globals are injected into the global environment.

<pre>
externalFunctions = new ExternalLoxCallable(handleCalls);
globals.define("sys", externalFunctions);  
</pre>

If a HashMap of - function name, LoxCallable pairs - are provided the Interpreter's constructor then you can call a "sys" function with any number of arguments that gets handled like so.

<pre>
public Object call(Interpreter interpreter, List&lt;Object&gt; arguments) {
   if (handleCalls != null && arguments.size() >= 1 && handleCalls.containsKey(arguments.get(0))) {
      HandleLoxCallables handleCall = handleCalls.get(arguments.get(0));
      return handleCall.call(arguments.size() - 1, interpreter, arguments);
   }
   return null;  
}
</pre>

The first argument is the name of the "sys" function to run. The entire set of arguments is then passed onto the "HandleLoxCallables" instance for the given string and with an arity of -1. Note the first argument defining what subfunction to run is not removed from the arguments even though the arity is the argument length minus one. 

<pre>
interface HandleLoxCallables {
   public Object call(int arity, Interpreter interpreter, List&lt;Object&gt; arguments);
}
</pre>

You can use this to quickly extend the "sys" function by adding new code to the class without having to define a more complex structure of classes.
