package com.craftinginterpreters.lox;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.URI;
import java.net.URISyntaxException;
import java.nio.charset.Charset;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;

import org.apache.hc.client5.http.classic.methods.HttpGet;
import org.apache.hc.client5.http.classic.methods.HttpPost;
import org.apache.hc.client5.http.entity.UrlEncodedFormEntity;
import org.apache.hc.client5.http.impl.classic.CloseableHttpClient;
import org.apache.hc.client5.http.impl.classic.CloseableHttpResponse;
import org.apache.hc.client5.http.impl.classic.HttpClients;
import org.apache.hc.core5.http.HttpEntity;
import org.apache.hc.core5.http.NameValuePair;
import org.apache.hc.core5.http.ParseException;
import org.apache.hc.core5.http.io.entity.EntityUtils;
import org.apache.hc.core5.http.message.BasicNameValuePair;
import org.apache.hc.core5.net.URIBuilder;

/**
 * An implementation of the "Lox" embedded scripting language. This work was
 * created from tutorials by the original author, Robert Nystrom. Source URL:
 * https://craftinginterpreters.com/
 *
 * @author Victor G. Brusca, Carlo Bruscani, based on the work of Robert
 * Nystrom, Copyright Middlemind Games 05/03/2024
 */
public class Lox {

   public enum HttpCallType {
      GET_BLANK,
      GET_QUERY_PARAMS,
      POST
   }

   private static final Interpreter interpreter = new Interpreter();
   static boolean hadError = false;
   static boolean hadRuntimeError = false;
   static String lastError = null;

   static String globalsFile = null;
   static String globalsScript = null;
   static boolean hasGlobalsFile = false;
   static boolean hasGlobalsScript = false;
   static String lastFile = null;
   static String lastLine = null;

   /**
    * The static main entry point to the program. Expects a valid Lox script
    * file as an argument or no arguments if you want to start a REPL session.
    *
    * @param args
    * @throws IOException
    */
   public static void main(String[] args) throws IOException {
      hasGlobalsFile = false;
      hasGlobalsScript = false;
      for (int i = 0; i < args.length; i++) {
         if (args[i] != null && args[i].equals("-gf")) {
            if (i + 1 < args.length) {
               globalsFile = args[i + 1];
               hasGlobalsFile = true;
            }
         } else if (args[i] != null && args[i].equals("-gs")) {
            if (i + 1 < args.length) {
               globalsScript = args[i + 1];
               hasGlobalsScript = true;
            }
         }
      }

      if (hasGlobalsFile) {
         Lox.log(0, "Found global file to import: " + globalsFile);
         runFile(globalsFile);
      }

      if (hasGlobalsScript) {
         Lox.log(0, "Found global script to import: " + globalsScript);
         run(globalsScript);
      }

      if (args.length >= 1 && args[0] != null && args[0].toLowerCase().equals("-p")) {
         runPrompt();
      } else if (args.length >= 2 && args[0] != null && args[0].toLowerCase().equals("-f") && args[1] != null) {
         runFile(args[1]);
      } else if (args.length >= 2 && args[0] != null && args[0].toLowerCase().equals("-s") && args[1] != null) {
         run(args[1]);
      } else {
         System.out.println("Usage: JavaLox ([-f script file] | [-s script string] | [-p REPL]) & [-gf script file] [-gs script string]");
         System.exit(64);
      }
   }

   public static String getBlank(String url) {
      String resultContent = null;
      HttpGet httpGet = new HttpGet(url);
      try (CloseableHttpClient httpclient = HttpClients.createDefault()) {
         try (CloseableHttpResponse response = httpclient.execute(httpGet)) {
            // Get status code
            System.out.println(response.getVersion()); // HTTP/1.1
            System.out.println(response.getCode()); // 200
            System.out.println(response.getReasonPhrase()); // OK
            HttpEntity entity = response.getEntity();
            // Get response information
            resultContent = EntityUtils.toString(entity);
         }
      } catch (IOException | ParseException e) {
         e.printStackTrace();
      }
      return resultContent;
   }

   public static String getQueryParams(String url, List<NameValuePair> nvps) {
      String resultContent = null;
      HttpGet httpGet = new HttpGet(url);

      try {
         URI uri = new URIBuilder(new URI(url)).addParameters(nvps).build();
         httpGet.setUri(uri);
      } catch (URISyntaxException e) {
         throw new RuntimeException(e);
      }

      try (CloseableHttpClient httpclient = HttpClients.createDefault()) {
         try (CloseableHttpResponse response = httpclient.execute(httpGet)) {
            System.out.println(response.getVersion()); // HTTP/1.1
            System.out.println(response.getCode()); // 200
            System.out.println(response.getReasonPhrase()); // OK
            HttpEntity entity = response.getEntity();
            resultContent = EntityUtils.toString(entity);
         }
      } catch (IOException | ParseException e) {
         e.printStackTrace();
      }
      return resultContent;
   }

   public static String post(String url, List<NameValuePair> nvps) {
      String result = null;
      HttpPost httpPost = new HttpPost(url);
      httpPost.setEntity(new UrlEncodedFormEntity(nvps));
      try (CloseableHttpClient httpclient = HttpClients.createDefault()) {
         try (CloseableHttpResponse response = httpclient.execute(httpPost)) {
            System.out.println(response.getVersion()); // HTTP/1.1
            System.out.println(response.getCode()); // 200
            System.out.println(response.getReasonPhrase()); // OK

            HttpEntity entity = response.getEntity();
            result = EntityUtils.toString(entity);
            // Ensure that the stream is fully consumed
            EntityUtils.consume(entity);
         }
      } catch (IOException | ParseException e) {
         e.printStackTrace();
      }
      return result;
   }

   private static String runUrl(String url, HttpCallType callType, List<NameValuePair> data) {
      if (callType == HttpCallType.GET_BLANK) {
         return getBlank(url);
      } else if (callType == HttpCallType.GET_QUERY_PARAMS) {
         return getQueryParams(url, data);
      } else if (callType == HttpCallType.POST) {
         return post(url, data);
      } else {
         return null;
      }
   }

   private static void runFile(String path) throws IOException {
      lastFile = path;
      byte[] bytes = Files.readAllBytes(Paths.get(path));
      run(new String(bytes, Charset.defaultCharset()));

      // Indicate an error in the exit code.
      if (hadError) {
         System.exit(64);
      }
      if (hadRuntimeError) {
         System.exit(70);
      }
   }

   private static void runPrompt() throws IOException {
      InputStreamReader input = new InputStreamReader(System.in);
      BufferedReader reader = new BufferedReader(input);

      //Control-D to exit
      for (;;) {
         System.out.println("> ");
         String line = reader.readLine();
         if (line == null || line.toLowerCase().equals("exit") || line.toLowerCase().equals("bye")) {
            break;
         }

         try {
            run(line);
         } catch (Exception e) {
            //do nothing
         }

         hadError = false;
         hadRuntimeError = false;
      }
   }

   private static void run(String source) {
      lastLine = source;
      Scanner scanner = new Scanner(source);
      List<Token> tokens = scanner.scanTokens();

      Parser parser = new Parser(tokens);
      List<Stmt> statements = parser.parse();

      // Stop if there was a syntax error.
      if (hadError) {
         return;
      }

      Resolver resolver = new Resolver(interpreter);
      resolver.resolve(statements);

      // Stop if there was a resolution error.
      if (hadError) {
         return;
      }

      interpreter.interpret(statements);
   }

   static void log(int line, String message) {
      System.out.println("[line " + line + "] Log: " + message);
   }

   static void error(int line, String message) {
      report(line, "", message);
   }

   private static void report(int line, String where, String message) {
      lastError = "[line " + line + "] Error" + where + ": " + message;
      System.err.println(lastError);
      hadError = true;
   }

   static void error(Token token, String message) {
      if (token.type == TokenType.EOF) {
         report(token.line, " at end", message);
      } else {
         report(token.line, " at '" + token.lexeme + "'", message);
      }
   }

   static void runtimeError(RuntimeError error) {
      String msg = error.getMessage() + "\n[line " + error.token.line + "]";
      System.err.println(msg);
      hadRuntimeError = true;
   }
}
