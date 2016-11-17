public class Environment
{
	public static MalEnvironment ReplEnv {
		get { return m_ReplEnv; }
		set { m_ReplEnv = Value; }
	}
	private static MalEnvironment m_ReplEnv;

	public static void InitLoad()
	{
		List<MalSymbol> coreKeys = Core.Ns.Keys.ToList;

		foreach (MalSymbol key in coreKeys) {
			ReplEnv.Set(key, Core.Ns(key));
		}

		ReplEnv.Set(new MalSymbol("eval"), new MalFunction(inputs =>
		{
			MalType ast = inputs(0);
			return Eval.Eval(ast, ReplEnv);
		}));

		Rep("(def! not (fn* [a] (if a false true)))");
		Rep("(def! load-file (fn* [f] (eval (read-string (str \\\"(do \\\"(slurp f) \\\")\\\")))))");
		//Rep("(defmacro! cond (fn* (& xs) (if (> (count xs) 0) (list 'if (first xs) (if (> (count xs) 1) (nth xs 1) (throw \""odd number of forms to cond\"")) (cons 'cond (rest (rest xs)))))))")
		//Rep("(def! *gensym-counter* (atom 0))")
		//Rep("(def! gensym (fn* [] (symbol (str \""G__\"" (swap! *gensym-counter* (fn* [x] (+ 1 x)))))))")
		//Rep("(defmacro! or (fn* (& xs) (if (empty? xs) nil (if (= 1 (count xs)) (first xs) (let* (condvar (gensym)) `(let* (~condvar ~(first xs)) (if ~condvar ~condvar (or ~@(rest xs)))))))))")
	}
}

public class MalEnvironment
{
	private Dictionary<MalType, MalType> Data {
		get { return m_Data; }
		set { m_Data = Value; }
	}
	private Dictionary<MalType, MalType> m_Data;
	private MalEnvironment Outer {
		get { return m_Outer; }
		set { m_Outer = Value; }
	}
	private MalEnvironment m_Outer;

	public MalEnvironment(MalEnvironment outer)
	{
		this.Outer = outer;
	}

	public MalEnvironment(MalEnvironment outer, MalVector binds, MalList exprs)
	{
		bool hasVariadicBinding = binds.Value.Contains(new MalSymbol("&"));
		if ((hasVariadicBinding && binds.Value.Count - 1 != exprs.Value.Count) || (!hasVariadicBinding && binds.Value.Count != exprs.Value.Count)) {
			throw new EvaluateException("Invalid bindings due to uneven arity.");
		}

		this.Outer = outer;

		if (hasVariadicBinding) {
			int variadicIndex = binds.Value.IndexOf(new MalSymbol("&"));

			for (i = 0; i <= variadicIndex; i++) {
				Set(binds.Value(i), exprs.Value(i));
			}

			Set(binds.Value(variadicIndex + 1), new MalList(exprs.Value.GetRange(variadicIndex, exprs.Value.Count)));
		} else {
			for (i = 0; i <= binds.Value.Count - 1; i++) {
				Set(binds.Value(i), exprs.Value(i));
			}
		}
	}

	public void Set(MalType symbol, MalType value)
	{
		Data.Add(symbol, value);
	}

	public MalEnvironment Find(MalType symbol)
	{
		if (Data.ContainsKey(symbol)) {
			return this;
		} else if (Outer != null) {
			return Outer.Find(symbol);
		} else {
			return null;
		}
	}

	public MalType Get(MalType symbol)
	{
		MalEnvironment inner = Find(symbol);

		if (inner == null)
			throw new EvaluateException("Symbol not Found: " + symbol.ToString);

		return inner.Data(symbol);
	}
}
