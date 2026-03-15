using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects.DataClasses;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

//*******************************************************************************
/// <summary>
/// Extension methods for the String class.
/// </summary>
//*******************************************************************************
public static class StringExtensions {

  //*******************************************************************************
  /// <summary>
  /// Add the functionality to append an file extension to the string making sure 
  /// there is only one "." between the two strings.
  /// </summary>
  //*******************************************************************************
  public static string AppendFileExtension(this string value, string extension) {
    value += "." + extension;

    // replace double dots
    value = value.Replace("..", ".");

    return value;
  }

  //*******************************************************************************
  /// <summary>
  /// Extracts the part between beginDelimiter and endDelimiter
  /// </summary>
  /// <returns>The extracted string</returns>
  //*******************************************************************************
  public static string Extract(this String value, String beginDelimiter, String endDelimiter) {
    String result = "";
    int pos1 = value.IndexOf(beginDelimiter) + beginDelimiter.Length;
    int pos2 = value.IndexOf(endDelimiter, pos1);
    if (pos2 > pos1) {
      result = value.Substring(pos1, pos2 - pos1);
    }
    return result;
  }

  //*******************************************************************************************************************
  /// <summary>
  /// Gets the value for named entity key
  /// </summary>
  //*******************************************************************************************************************
  public static object GetKeyValueByName(this EntityObject entity, string keyName) {
    foreach (EntityKeyMember member in entity.EntityKey.EntityKeyValues) {
      if (member.Key == keyName) {
        return member.Value;
      }
    }
    throw new ArgumentException("The key name provided does not match a member of the EntityKey");
  }

  //*******************************************************************************
  /// <summary>
  /// Returns the String as a byte array
  /// </summary>
  //*******************************************************************************
  public static Byte[] AsByteArray(this String value) {
    Byte[] result;
    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
    result = encoding.GetBytes(value);
    return result;
  }

  //*******************************************************************************
  /// <summary>
  /// Add the functionality to append a path to the string making sure 
  /// there is only one backslash between the two strings.
  /// </summary>
  //*******************************************************************************
  public static string AppendPath(this string value, string pathToAppend) {
    value = value.RemoveLastBackslash();

    pathToAppend = pathToAppend.RemoveLeadingBackslash();

    value += "\\" + pathToAppend;

    return value;
  }

  //*******************************************************************************************************************
  /// <summary>
  /// Replace all linebreaks (\r\n or \n) with a blank.
  /// </summary>
  /// <returns>String without line breaks</returns>
  //*******************************************************************************************************************
  public static string ReplaceLineBreaks(this string value) {
    value = value.Replace("\r\n", " ");
    value = value.Replace("\n", " ");
    value = value.Replace("\r", " "); // BugNr: 	#4185
    return value;
  }

  //*******************************************************************************
  /// <summary>
  /// Removes the backslash from the end of the string
  /// </summary>
  /// <param name="s">This</param>
  /// <returns>String without a backslash in the end</returns>
  //*******************************************************************************
  public static string RemoveLastBackslash(this string value) {
    if (value.EndsWith("\\")) {
      value = value.Remove(value.Length - 1, 1);
    }

    return value;
  }

  //*******************************************************************************
  /// <summary>
  /// Removes the a leading backslash from the string
  /// </summary>
  /// <param name="s">This</param>
  /// <returns>String without a backslash in the end</returns>
  //*******************************************************************************
  public static string RemoveLeadingBackslash(this string value) {
    if (value.StartsWith("\\")) {
      value = value.Remove(0, 1);
    }

    return value;
  }

  //*******************************************************************************
  /// <summary>
  /// Converts a serialized list with \r\n or \n\r separated items to a List<string>
  /// </summary>
  /// <param name="value">serialized list</param>
  /// <returns>List of strings</returns>
  //*******************************************************************************
  public static List<string> ToList(this string value) {
    List<string> result = new List<string>();
    string tmpValue = value;

    tmpValue = tmpValue.Replace("\r\n", "\n");
    tmpValue = tmpValue.Replace("\n\r", "\n");
    string[] lines = tmpValue.Split('\n');
    foreach (string oneLine in lines) {
      result.Add(oneLine);
    }

    return result;
  }

  //*******************************************************************************
  /// <summary>
  /// ExtensionMethod: Separates string into single item using given separator
  /// </summary>
  //*******************************************************************************
  public static List<string> Explode(this string value, string separator, int count = Int32.MaxValue) {
    if (String.IsNullOrEmpty(value)) {
      return new List<string>();
    }

    return value.Split(new string[] { separator }, count, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
  }

  //*******************************************************************************
  /// <summary>
  /// ExtensionMethod: Returns a string that is never null.
  /// Null strings will be returned as an empty string
  /// </summary>
  /// <returns>A string that is never null</returns>
  //*******************************************************************************
  public static string NeverNull(this string value) {
    return value == null ? string.Empty : value;
  }

  //*******************************************************************************
  /// <summary>
  /// Extension method for the String class.
  /// Like FoxPro Replicate
  /// </summary>
  /// <param name="s">This</param>
  /// <returns>The string</returns>
  //*******************************************************************************
  public static string Replicate(this string value, char character, Int32 number) {
    StringBuilder sb = new StringBuilder(value, value.Length + number);
    for (Int32 i = 0; i < number; i++) {
      sb.Append(character);
    }

    return sb.ToString();
  }

  //*******************************************************************************************************************
  /// <summary>
  /// Reverses the string.
  /// E.g.: Hello -> olleH
  /// </summary>
  /// <returns>Reversed string</returns>
  //*******************************************************************************************************************
  public static string Reverse(this string s) {
    char[] c = s.ToCharArray();
    Array.Reverse(c);
    return new string(c);
  }


  //*******************************************************************************************************************
  /// <summary>
  /// Checks if the given path is a file
  /// </summary>
  /// <param name="path">Path to be checked</param>
  /// <returns>True, if the given path is a file otherwise false</returns>
  //*******************************************************************************************************************
  public static bool IsFile(this string path) {
    // Get the attributes for the path
    FileAttributes attr = File.GetAttributes(path);

    // Detect whether its a directory or file
    if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
      return false;
    }
    else {
      return true;
    }
  }

  //*******************************************************************************************************************
  /// <summary>
  /// Checks if the given path is a directory
  /// </summary>
  /// <param name="path">Path to be checked</param>
  /// <returns>True, if the given path is a directory otherwise false</returns>
  //*******************************************************************************************************************
  public static bool IsDirectory(this string path) {
    // Get the attributes for the path
    FileAttributes attr = File.GetAttributes(path);

    // Detect whether its a directory or file
    if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
      return true;
    }
    else {
      return false;
    }
  }

  //*******************************************************************************************************************
  /// <summary>
  /// Returns a string that contains only the allowed characters passed in the allowedCharacters parameter.
  /// If the replaceWith parameter is passed, the character, that are not allowed, ar replaced with this value
  /// </summary>
  /// <param name="allowedCharacters">List of allowed characters</param>
  /// <param name="replaceWith">Characters that shall replace not allowed characters</param>
  //*******************************************************************************************************************
  public static String OnlyAllowedCharacters(this string s, string allowedCharacters, string replaceWith = "") {
    string result = String.Empty;
    for (int i = 0; i < s.Length; i++) {
      if (Regex.IsMatch(s.Substring(i, 1), @"^[" + allowedCharacters + "]*$")) {
        result += s.Substring(i, 1);
      }
      else {
        if (replaceWith != String.Empty) {
          result += replaceWith;
        }
      }
    }

    return result;
  }

  //*******************************************************************************************************************
  /// <summary>
  /// Return true if the search string is found using the given comparer
  /// </summary>
  //*******************************************************************************************************************
  public static bool Contains(this string source, string toCheck, StringComparison comp) {
    return source.IndexOf(toCheck, comp) >= 0;
  }

  /// <summary>
  /// indents the specified string by the specified indentation
  /// </summary>
  public static string Indent(this string source, string indentation) {
    using (var sr = new StringReader(source)) {
      using (var sw = new StringWriter()) {
        string line;
        while ((line = sr.ReadLine()) != null) {
          sw.Write(indentation);
          sw.WriteLine(line);
        }
        sw.Flush();
        return sw.GetStringBuilder().ToString();
      }
    }
  }

  /// <summary>
  /// Removes <paramref name="stringToRemove" /> from the end of this string.
  /// Throws ArgumentException if this string does not end with <paramref name="stringToRemove" />.
  /// </summary>
  public static string RemoveFromEnd(this string s, string stringToRemove) {
    if (s == null)
      return null;
    if (string.IsNullOrEmpty(stringToRemove))
      return s;
    if (!s.EndsWith(stringToRemove))
      throw new ArgumentException(string.Format("{0} does not end with {1}", s, stringToRemove));
    return s.Substring(0, s.Length - stringToRemove.Length);
  }

  public static bool IsNullOrEmpty(this string value) {
    return string.IsNullOrEmpty(value);
  }

  public static bool IsNullOrWhiteSpace(this string value) {
    return string.IsNullOrWhiteSpace(value);
  }

}
