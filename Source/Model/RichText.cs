using System.Globalization;
using System.Text.RegularExpressions;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Model {
    public class RichText : IExposable {
        private string _html;

        public RichText() {
        }

        public RichText(string html) {
            _html = html;
        }

        public string GetWithoutInteractive() {
            if (_html == null) return null;
            return Sanitize(ParseStyle(ParseFont(ParseLineBreak(SanitizeInteractive(_html)))));
        }

        public string Get() {
            if (_html == null) return null;
            return Sanitize(ParseStyle(ParseFont(ParseLineBreak(ParseInteractive(_html)))));
        }

        private static string Sanitize(string str) {
            str = Regex.Replace(str, @"</?p>", "");
            return str.Trim();
        }

        private static string ParseLineBreak(string str) {
            str = Regex.Replace(str, @"</?br>", "\n");
            return str.Trim();
        }

        private static string SanitizeInteractive(string str) {
            str = Regex.Replace(str, @"<span(?: contenteditable=""false"")?>.*?</span>", "");
            return str;
        }

        private static string ParseInteractive(string str) {
            str = Regex.Replace(str, @"<span(?: contenteditable=""false"")?>(.*?)</span>", match => {
                var matchString = match.Groups[1].Value;
                if (matchString.StartsWith("@")) {
                    return ParsePawnMention(matchString);
                }
                
                if (matchString.StartsWith("/")) {
                    return ParseVariableMention(matchString);
                }
                
                Tell.Warn("Found interactive text node with unknown char indicator (full match: " + matchString + ")");
                return "";
            });
            return str;
        }

        private static string ParseVariableMention(string match) {
            return DataBankUtil.GetValueFromVariable(match.Substring(1)).ToString(CultureInfo.InstalledUICulture);
        }

        private static string ParsePawnMention(string match) {
            var split = match.Substring(1).Split(':');
            var pawn = PawnUtil.GetPawnByName(split[0]);
            if (pawn == null) {
                Tell.Warn("Pawn mentioned in text but not found (full match: " + match + ") showing pawn IdName in text...");
                return split[0];
            }

            switch (split[1]) {
                    case "IdName":
                        return split[0];
                    case "FullName":
                        return pawn.Name.ToStringFull;
                    case "ShortName":
                        return pawn.Name.ToStringShort;
                    case "Age":
                        return pawn.ageTracker.AgeNumberString;
                    default:
                        Tell.Warn("Pawn mentioned in text but attribute not recognized (full match: " + match + ") showing pawn IdName in text...");
                        return split[0];
            }
        }
        
        private static string ParseFont(string str) {
            str = Regex.Replace(str, @"</font>", "</size>");
            str = Regex.Replace(str, @"<font size=""5"">", "<size=16>");
            str = Regex.Replace(str, @"<font size=""6"">", "<size=24>");
            str = Regex.Replace(str, @"<font size=""7"">", "<size=40>");
            return str;
        }
        
        private static string ParseStyle(string str) {
            str = Regex.Replace(str, @"<strong>", "<b>");
            str = Regex.Replace(str, @"</strong>", "</b>");
            str = Regex.Replace(str, @"<em>", "<i>");
            str = Regex.Replace(str, @"</em>", "</i>");
            return str;
        }

        public override string ToString() {
            return "RichText: [" + _html + "]";
        }

        public void ExposeData() {
            Scribe_Values.Look(ref _html, "_html");
        }
    }
}