public class Printer
{
	private static string UnfixString(string str)
	{
		string unfixed = str.Replace(vbCrLf, "\\n");
		unfixed = unfixed.Replace("\"", "\\\"");
		return unfixed.Replace("\\\\", "\\");
	}

	public static string PrStr(MalType outputLine, bool printReadably)
	{
		if (outputLine is MalInt) {
			return ((MalInt)outputLine).Value.ToString;
		} else if (outputLine is MalBool) {
			return ((MalBool)outputLine).Value.ToString;
		} else if (outputLine is MalDbl) {
			return ((MalDbl)outputLine).Value.ToString;
		} else if (outputLine is MalStr) {
			if (printReadably) {
				return "\"" + UnfixString(((MalStr)outputLine).Value) + "\"";
			} else {
				return "\"" + ((MalStr)outputLine).Value + "\"";
			}
		} else if (outputLine is MalSymbol) {
			return ((MalSymbol)outputLine).Value;
		} else if (outputLine is MalKeyword) {
			return ((MalKeyword)outputLine).Value.Replace(Reader.keywordPrefix, ":");
		} else if (outputLine is MalNil) {
			return "nil";
		} else if (outputLine is MalList) {
			return "(" + string.Join(" ", (from output in ((MalList)outputLine).ValuePrStr(output, false)).ToList) + ")";
		} else if (outputLine is MalVector) {
			return "[" + string.Join(" ", (from output in ((MalVector)outputLine).ValuePrStr(output, false)).ToList) + "]";
		} else if (outputLine is MalHashMap) {
			return "{" + string.Join(" ", (from key in ((MalHashMap)outputLine).GetKeys((MalHashMap)outputLine).Get(key)MalTypePrStr(key, false) + " " + PrStr(val, false)).ToList) + "}";
		} else if (outputLine is MalFunction) {
			return "#";
		} else if (outputLine is MalTcoHelper) {
			return "tcohelper";
		} else {
			throw new EvaluateException("MalType not recognized");
		}
	}
}
