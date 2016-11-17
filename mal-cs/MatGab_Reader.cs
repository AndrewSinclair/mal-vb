public class Reader
{

	public const string keywordPrefix = ChrW(0x29e);
	private List<Token> Tokens {
		get { return m_Tokens; }
		set { m_Tokens = Value; }
	}
	private List<Token> m_Tokens;
	private int Position {
		get { return m_Position; }
		set { m_Position = Value; }
	}
	private int m_Position;

	private Reader(List<Token> tokens)
	{
		this.Tokens = tokens;
		Position = 0;
	}

	public Token Next()
	{
		if (Tokens.Count == 0)
			return null;
		if (Position >= Tokens.Count)
			return null;

		Token obj = Tokens(Position);

		Position += 1;

		return obj;
	}

	public Token Peek()
	{
		if (Tokens.Count == 0)
			return null;
		if (Position >= Tokens.Count)
			return null;

		return Tokens(Position);
	}

	public MalType ReadList(string delimiterType)
	{
		List<MalType> results = new List<MalType>();
		Next();

		while (Peek() != null && Peek().Value != delimiterType) {
			MalType form = ReadForm();
			results.Add(form);

			Next();
		}

		if (Peek() == null)
			throw new EvaluateException("There was an unexpected EOF while reading a list");

		if (delimiterType == ")") {
			return new MalList(results);
		} else if (delimiterType == "]") {
			return new MalVector(results);
		} else if (delimiterType == "}") {
			if (results.Count % 2 != 0)
				throw new EvaluateException("There was an uneven number of forms in the reading of a hashmap");

			return new MalHashMap(results);
		} else {
			throw new EvaluateException("Delimiter Type was not correct!");
		}
	}

	private static string StringFix(string str)
	{
		string @fixed = str.Replace("\\\"", "\"");
		@fixed = @fixed.Replace("\\n", vbCrLf);
		@fixed = @fixed.Replace("\\\\", "\\");
		return @fixed;
	}

	private MalType ReadAtom(string atom)
	{
		return new Reader(new List<Token>({ new Token(atom) })).ReadAtom;
	}

	private MalType ReadAtom()
	{
		Token currToken = Peek();
		string valueStr = currToken.Value;
		int valueInt;
		bool valueBool;
		double valueDbl;

		if (valueStr.StartsWith("\"") && !valueStr.EndsWith("\""))
			throw new EvaluateException("Expected \", got EOF.");

		if (valueStr.StartsWith("\"") && valueStr.EndsWith("\"")) {
			return new MalStr(StringFix(valueStr.Substring(1, valueStr.Length - 2)));
		} else if (valueStr.StartsWith(":")) {
			return new MalKeyword(valueStr.Replace(":", keywordPrefix));
		} else if (int.TryParse(valueStr, valueInt)) {
			return new MalInt(valueInt);
		} else if (bool.TryParse(valueStr, valueBool)) {
			return valueBool ? MalBool.True : MalBool.False;
		} else if (double.TryParse(valueStr, valueDbl)) {
			return new MalDbl(valueDbl);
		} else if (valueStr.Equals("nil")) {
			return MalNil.Instance;
		} else {
			return new MalSymbol(valueStr);
		}
	}

	public MalType ReadForm()
	{
		Token first = Peek();

		if (first.Value == "'") {
			return new MalList(new List<MalType>({
				new MalSymbol("quote"),
				ReadForm()
			}));
		} else if (first.Value == "`") {
			return new MalList(new List<MalType>({
				new MalSymbol("quasiquote"),
				ReadForm()
			}));
		} else if (first.Value == "~") {
			return new MalList(new List<MalType>({
				new MalSymbol("unquote"),
				ReadForm()
			}));
		} else if (first.Value == "~@") {
			return new MalList(new List<MalType>({
				new MalSymbol("splice-unquote"),
				ReadForm()
			}));
		} else if (first.Value == "@") {
			return new MalList(new List<MalType>({
				new MalSymbol("deref"),
				ReadForm()
			}));
		} else if (first.Value == "(") {
			return ReadList(")");
		} else if (first.Value == "[") {
			return ReadList("]");
		} else if (first.Value == "{") {
			return ReadList("}");
		} else {
			return ReadAtom();
		}
	}

	public static List<Token> Tokenizer(string inputLine)
	{
		List<Token> tokenList = new List<Token>();
		const string pattern = "[\\s,]*(~@|[\\[\\]{}()'`~@]|\"(?:[\\\\].|[^\\\\\"])*\"|;.*|[^\\s \\[\\]{}()'\"`~@,;]*)";
		Text.RegularExpressions.Regex regex = new Text.RegularExpressions.Regex(pattern);

		foreach (Text.RegularExpressions.Match match in regex.Matches(inputLine)) {
			string strMatch = (match.Groups(1).Value);

			if (strMatch != null && strMatch != "" && !strMatch.StartsWith(";")) {
				tokenList.Add(new Token(strMatch));
			}
		}

		return tokenList;
	}

	public static MalType ReadStr(string inputLine)
	{
		List<Token> tokens = Tokenizer(inputLine);

		if (tokens.Count == 0)
			throw new NoReadableCodeException("Comment or blankline");

		Reader reader = new Reader(tokens);

		return reader.ReadForm();
	}
}

public class Token
{
	public string Type {
		get { return m_Type; }
		set { m_Type = Value; }
	}
	private string m_Type;
	public string Value {
		get { return m_Value; }
		set { m_Value = Value; }
	}
	private string m_Value;

	private Token(string val)
	{
		Value = val;
		Type = "string";
	}
}
