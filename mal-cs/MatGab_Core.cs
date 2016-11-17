public class Core
{
	public static Dictionary<MalSymbol, MalFunction> Ns {
		get { return m_Ns; }
		set { m_Ns = Value; }
	}
	private static Dictionary<MalSymbol, MalFunction> m_Ns;

	public static void InitLoad()
	{
		Ns = new Dictionary<MalSymbol, MalFunction>();
		Ns.Add(new MalSymbol("+"), new MalFunction(MalIntAggregate((a, b) => a + b)));
		Ns.Add(new MalSymbol("-"), new MalFunction(MalIntAggregate((a, b) => a - b)));
		Ns.Add(new MalSymbol("*"), new MalFunction(MalIntAggregate((a, b) => a * b)));
		Ns.Add(new MalSymbol("/"), new MalFunction(MalIntAggregate((a, b) => a / b)));
		Ns.Add(new MalSymbol("<"), new MalFunction(inputs =>
		{
			if (((MalInt)inputs(0)).Value < ((MalInt)inputs(1)).Value) {
				return MalBool.True;
			} else {
				return MalBool.False;
			}
		}));

		Ns.Add(new MalSymbol("<="), new MalFunction(inputs =>
		{
			if (((MalInt)inputs(0)).Value <= ((MalInt)inputs(1)).Value) {
				return MalBool.True;
			} else {
				return MalBool.False;
			}
		}));

		Ns.Add(new MalSymbol(">"), new MalFunction(inputs =>
		{
			if (((MalInt)inputs(0)).Value > ((MalInt)inputs(1)).Value) {
				return MalBool.True;
			} else {
				return MalBool.False;
			}
		}));

		Ns.Add(new MalSymbol(">="), new MalFunction(inputs =>
		{
			if (((MalInt)inputs(0)).Value >= ((MalInt)inputs(1)).Value) {
				return MalBool.True;
			} else {
				return MalBool.False;
			}
		}));


		Ns.Add(new MalSymbol("list"), new MalFunction(inputs => new MalList(inputs)));
		Ns.Add(new MalSymbol("list?"), new MalFunction(inputs => inputs(0) is MalList ? MalBool.True : MalBool.False));
		Ns.Add(new MalSymbol("empty?"), new MalFunction(inputs => ((MalList)inputs(0)).Value.Count == 0 ? MalBool.True : MalBool.False));
		Ns.Add(new MalSymbol("count"), new MalFunction(inputs => new MalInt(((MalList)inputs(0)).Value.Count)));
		Ns.Add(new MalSymbol("="), new MalFunction(inputs => RecurEqual(inputs) ? MalBool.True : MalBool.False));


		Ns.Add(new MalSymbol("pr-str"), new MalFunction(inputs =>
		{
			List<string> strs = inputs.Select(t => Printer.PrStr(t, true)).ToList;

			return new MalStr(string.Join(" ", strs));
		}));

		Ns.Add(new MalSymbol("str"), new MalFunction(inputs =>
		{
			List<string> strs = inputs.Select(t => Printer.PrStr(t, false)).ToList;

			return new MalStr(string.Join("", strs));
		}));

		Ns.Add(new MalSymbol("prn"), new MalFunction(inputs =>
		{
			List<string> strs = inputs.Select(t => Printer.PrStr(t, true)).ToList;
			string output = string.Join(" ", strs);

			Console.Write(output);

			return MalNil.Instance;
		}));

		Ns.Add(new MalSymbol("println"), new MalFunction(inputs =>
		{
			List<string> strs = inputs.Select(t => Printer.PrStr(t, false)).ToList;
			string output = string.Join(" ", strs);

			Console.WriteLine(output);

			return MalNil.Instance;
		}));

		Ns.Add(new MalSymbol("read-string"), new MalFunction(inputs => Reader.ReadStr(((MalStr)inputs(0)).Value)));

		Ns.Add(new MalSymbol("slurp"), new MalFunction(inputs => new MalStr(System.IO.File.ReadAllText(((MalStr)inputs(0)).Value))));


		Ns.Add(new MalSymbol("atom"), new MalFunction(inputs => new MalAtom(inputs(0))));

		Ns.Add(new MalSymbol("atom?"), new MalFunction(inputs => inputs(0) is MalAtom ? MalBool.True : MalBool.False));

		Ns.Add(new MalSymbol("deref"), new MalFunction(inputs => ((MalAtom)inputs(0)).Value));

		Ns.Add(new MalSymbol("reset!"), new MalFunction(inputs =>
		{
			((MalAtom)inputs(0)).Value = inputs(1);
			return inputs(0);
		}));

		Ns.Add(new MalSymbol("swap!"), new MalFunction(inputs =>
		{
			MalAtom atom = (MalAtom)inputs(0);
			MalFunction f = (MalFunction)inputs(1);
			List<MalType> args = inputs.GetRange(2, inputs.Count - 2);
			MalType prevValue = atom.Value;

			atom.Value = f.Invoke({ prevValue }.ToList.Union(args));
			return atom.Value;
		}));

		Ns.Add(new MalSymbol("cons"), new MalFunction(inputs =>
		{
			MalType x = inputs(0);
			List<MalType> xs;
			if (inputs(1) is MalList) {
				xs = ((MalList)inputs(1)).Value;
			} else if (inputs(1) is MalVector) {
				xs = ((MalVector)inputs(1)).Value;
			} else {
				throw new EvaluateException("Expected either list or vector to cons operator.");
			}

			MalType[] immutableXs = new MalType[xs.Count - 1];
			xs.CopyTo(immutableXs);
			immutableXs.ToList.Insert(0, x);

			return new MalList(immutableXs.ToList);
		}));

		Ns.Add(new MalSymbol("concat"), new MalFunction(inputs =>
		{
			List<List<MalType>> lists = (from input in inputsinput is MalList ? ((MalList)input).Value : ((MalVector)input).Value);

			return new MalList(lists.Aggregate((xss, xs) => xss.Concat(xs)));
		}));

		Ns.Add(new MalSymbol("nth"), new MalFunction(inputs =>
		{
			List<MalType> list;
			if (inputs(0) is MalList) {
				list = ((MalList)inputs(0)).Value;
			} else {
				list = ((MalVector)inputs(0)).Value;
			}

			MalInt index = inputs(1);

			if (index.Value < 0 || index.Value > list.Count)
				throw new EvaluateException("The index was not in the list");

			return inputs(index.Value);
		}));

		Ns.Add(new MalSymbol("first"), new MalFunction(inputs =>
		{
			List<MalType> list;
			if (inputs(0) is MalNil) {
				return MalNil.Instance;
			} else if (inputs(0) is MalList) {
				list = ((MalList)inputs(0)).Value;
			} else {
				list = ((MalVector)inputs(0)).Value;
			}

			if (list.Count == 0)
				return MalNil.Instance;

			return list(0);
		}));

		Ns.Add(new MalSymbol("rest"), new MalFunction(inputs =>
		{
			List<MalType> list;
			if (inputs(0) is MalNil) {
				return MalNil.Instance;
			} else if (inputs(0) is MalList) {
				list = ((MalList)inputs(0)).Value;
			} else {
				list = ((MalVector)inputs(0)).Value;
			}

			int count = list.Count;

			if (count <= 1)
				return MalNil.Instance;

			return new MalList(list.GetRange(1, count - 1));
		}));
	}

	private static bool RecurEqual(List<MalType> inputs)
	{
		MalType first = inputs(0);
		MalType second = inputs(1);

		if (first is MalList && second is MalList) {
			if (((MalList)first).Value.Count == ((MalList)second).Value.Count) {
				List<MalType> firstList = ((MalList)first).Value;
				List<MalType> secondList = ((MalList)second).Value;
				bool isEqual = true;
				int length = firstList.Count;
				int index = 0;

				while (index < length && isEqual) {
					isEqual = RecurEqual(new List<MalType>({
						firstList(index),
						secondList(index)
					}));

					index += 1;
				}

				return isEqual;
			} else {
				return false;
			}
		}

		return first.GetType.Equals(second.GetType) && first.Equals(second);
	}

	public static Func<List<MalType>, MalInt> MalIntAggregate(Func<int, int, int> f, int initial)
	{
		return (List<MalType> xs) => new MalInt(xs.Select(t => ((MalInt)t).Value).Aggregate(initial, f));
	}

	public static Func<List<MalType>, MalInt> MalIntAggregate(Func<int, int, int> f)
	{
		return (List<MalType> xs) => new MalInt(xs.Select(t => ((MalInt)t).Value).Aggregate(f));
	}

	public static Func<List<MalType>, MalDbl> MalDblAggregate(Func<double, double, double> f, double initial)
	{
		return (List<MalType> xs) => new MalDbl(xs.Select(t => ((MalDbl)t).Value).Aggregate(initial, f));
	}

	public static Func<List<MalType>, MalDbl> MalDblAggregate(Func<double, double, double> f)
	{
		return (List<MalType> xs) => new MalDbl(xs.Select(t => ((MalDbl)t).Value).Aggregate(f));
	}

}
