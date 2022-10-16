using System.Reflection;
using System.Text.RegularExpressions;

namespace OpenStore.Data.EntityFramework.Extensions;

/// <summary>
/// This represents source of the data to go into a method/ctor. It to allow injection of DbContext into access methods
/// </summary>
public enum MatchSources
{
    Property,
    DbContext
}

/// <summary>
/// This holds the information on a match between a name/type and a propertyInfo, with a score to set how it did
/// This is used by a name matcher to try to match method/ctor properties to a set of properties in a class
/// </summary>
public class PropertyMatch
{
    /// <summary>
    /// Use "score >= PerfectMatchValue" to check if there is a perfect match
    /// </summary>
    public const double PerfectMatchValue = 0.99999;

    /// <summary>
    /// Use "score lessThanOrEqual NoMatchAtAll" to check if there is a perfect match
    /// </summary>
    public const double NoMatchAtAll = 0.00001;

    /// <summary>
    /// These are the type match values. It is very imporant that they go from zero to 3, as the match maths relise on this
    /// </summary>
    public enum TypeMatchLevels
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        NoMatch = 0,
        Match = 3
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// True if the NameMatcher said the names matched
    /// </summary>
    public bool NameMatched { get; }

    /// <summary>
    /// Set by the NameMatcher to the level of type match
    /// </summary>
    public TypeMatchLevels TypeMatch { get; }

    /// <summary>
    /// The PropertyInfo that the name/type was matched to
    /// </summary>
    public PropertyInfo PropertyInfo { get; }

    /// <summary>
    /// The MatchSource says whether the source of the match is the property or an injection of the DbContext
    /// </summary>
    public MatchSources MatchSource { get; }

    /// <summary>
    /// This holds the type of the injected type (some form of DbContext), otherwise null
    /// </summary>
    public Type NonPropertyMatchType { get; }

    /// <summary>
    /// A Score of 1 means a perfect match. 
    /// </summary>
    public double Score => (NameMatched ? 0.7 : 0.0) + ((int)TypeMatch / 10.0);

    /// <summary>
    /// This is the ctor to create a PropertyMatch
    /// </summary>
    /// <param name="nameMatched"></param>
    /// <param name="typeMatch"></param>
    /// <param name="propertyInfo"></param>
    public PropertyMatch(bool nameMatched, TypeMatchLevels typeMatch, PropertyInfo propertyInfo)
        : this(nameMatched, typeMatch, propertyInfo, MatchSources.Property, null)
    {
    }

    internal PropertyMatch(bool nameMatched, TypeMatchLevels typeMatch, PropertyInfo propertyInfo,
        MatchSources matchSource, Type nonPropertyMatchType)
    {
        NameMatched = nameMatched;
        TypeMatch = typeMatch;
        if ((int)TypeMatch > 3)
            throw new InvalidOperationException("The TypeMatchLevels must run from 0 to 3, 3 being a perfect match.");
        PropertyInfo = propertyInfo;
        MatchSource = matchSource;
        NonPropertyMatchType = nonPropertyMatchType;
    }

    /// <summary>
    /// This returns useful information on a match
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string matchInfo = "wrong name";
        if (Score >= PropertyMatch.PerfectMatchValue)
            matchInfo = $"{PropertyInfo.PropertyType.Name} {PropertyInfo.Name}";
        else if (Score <= NoMatchAtAll)
            matchInfo = "nothing matches";
        else if (!NameMatched)
            matchInfo = "Name not match, but type is " + TypeMatch.ToString().SplitPascalCase();

        return matchInfo;
    }
}

/// <summary>
/// This is the default name/type matching method. You can replace it with your own matcher by 
/// setting the value of the NameMatcher property in the <see cref="GenericServicesConfig"/> class
/// and then providing that at startup 
/// </summary>
public static class DefaultNameMatcher
{
    /// <summary>
    /// This matches the name and type to the name/type in a <see cref="PropertyInfo"/>
    /// As method/ctor parameters normally start with a lower case character and properties start with an upper case character
    /// the method ensures the name provided has its first character as an upper case
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public static PropertyMatch MatchCamelAndPascalName(string name, Type type, PropertyInfo propertyInfo)
    {
        //The first item could be a method name, which starts with a lower case
        var nameMatched = name.FirstCharToUpper() == propertyInfo.Name;
        //I have only done a simple match - someone can do a better match for collections etc.
        var typeMatch = type == propertyInfo.PropertyType
            ? PropertyMatch.TypeMatchLevels.Match
            : PropertyMatch.TypeMatchLevels.NoMatch;
        return new PropertyMatch(nameMatched, typeMatch, propertyInfo);
    }

    //thanks to https://stackoverflow.com/questions/3565015/bestpractice-transform-first-character-of-a-string-into-lower-case
    private static string FirstCharToUpper(this string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentNullException(nameof(input));
        return input.First().ToString().ToUpper() + input.Substring(1);
    }
}

internal static class SplitterExtension
{
    private static readonly Regex Reg = new Regex("([a-z,0-9](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", RegexOptions.Compiled);

    /// <summary>
    /// This splits up a string based on capital letters
    /// e.g. "MyAction" would become "My Action" and "My10Action" would become "My10 Action"
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string SplitPascalCase(this string str)
    {
        return Reg.Replace(str, "$1 ");
    }
}