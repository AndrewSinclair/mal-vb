public class Eval
{
	private static bool IsMacroCall(MalType ast, MalEnvironment env)
	{

		if (ast is MalList) {
			List<MalType> astList = ((MalList)ast).Value;

			if (astList(0) is MalSymbol) {
				MalSymbol headSymbol = astList(0);

				MalType headVal;

				try {
					headVal = env.Get(headSymbol);
				} catch {
					headVal = null;
				}

				return headVal != null && headVal is MalFunction && ((MalFunction)headVal).IsMacro;
			}
		}

		return false;
	}

	private static MalType MacroExpand(MalType ast, MalEnvironment env)
	{
		while (IsMacroCall(ast, env)) {
			List<MalType> astList = ((MalList)ast).Value;
			MalSymbol headSymbol = astList(0);

			MalType headVal = env.Get(headSymbol);

			List<MalType> astTail = astList.GetRange(1, astList.Count - 1);

			ast = ((MalFunction)headVal).Invoke(astTail);
		}

		return ast;
	}

	private static MalType EvalAst(MalType ast, MalEnvironment env)
	{
		if (ast is MalSymbol) {
			return env.Get(ast);
		} else if (ast is MalList) {
			List<MalType> malTypes = ((MalList)ast).Value.Select(t => Eval(t, env)).ToList;

			return new MalList(malTypes);
		} else if (ast is MalVector) {
			List<MalType> malTypes = ((MalVector)ast).Value.Select(t => Eval(t, env)).ToList;

			return new MalVector(malTypes);
		} else if (ast is MalHashMap) {
			List<MalType> keys = ((MalHashMap)ast).GetKeys.Select(t => Eval(t, env)).ToList;
			List<MalType> pairs = new List<MalType>();

			foreach (MalType key in keys) {
				object val = Eval(((MalHashMap)ast).Get(key), env);
				pairs.Add(key);
				pairs.Add(val);
			}

			return new MalHashMap(pairs);
		} else {
			return ast;
		}
	}

	public static MalType Eval(MalType ast, MalEnvironment env)
	{
		while (true) {
			if (ast is MalList) {
				MalList inputList = (MalList)ast;

				if (inputList.Value.Count == 0) {
					return ast;
				}

				MalSymbol headSymbol = inputList.Value(0) as MalSymbol;
				if (headSymbol != null) {
					ast = MacroExpand(ast, env);

					if (!object.ReferenceEquals(ast is , MalList)) {
						ast = EvalAst(ast, env);
						continue;
					}

					string specialForm = headSymbol.Value;
					if (specialForm == "def!") {
						MalType val = Eval(inputList.Value(2), env);
						env.Set(inputList.Value(1), val);
						return val;
					} else if (specialForm == "let*") {
						MalEnvironment outer = new MalEnvironment(env);

						List<MalType> bindings = ((MalVector)inputList.Value(1)).Value;

						for (i = 0; i <= bindings.Count - 1; i += 2) {
							outer.Set(bindings(i), Eval(bindings(i + 1), outer));
						}

						env = outer;
						ast = inputList.Value(2);
						continue;
					} else if (specialForm == "do") {
						int count = inputList.Value.Count;
						MalList tailList = new MalList(inputList.Value.GetRange(1, count - 2));
						EvalAst(tailList, env);

						ast = inputList.Value.Last;
						continue;
					} else if (specialForm == "if") {
						MalType firstResult = EvalAst(inputList.Value(1), env);
						if (firstResult is MalNil || (firstResult is MalBool && ((MalBool)firstResult).Equals(MalBool.False))) {
							if (inputList.Value.Count == 4) {
								ast = inputList.Value(3);
							} else {
								return MalNil.Instance;
							}
						} else {
							ast = inputList.Value(2);
						}
						continue;
					} else if (specialForm == "fn*") {
						return new MalTcoHelper(inputList.Value(2), inputList.Value(1), env, new MalFunction((List<MalType> input) =>
						{
							MalEnvironment outer = new MalEnvironment(env, (MalVector)inputList.Value(1), new MalList(input));
							return Eval(inputList.Value(2), outer);
						}));
					} else if (specialForm == "quote") {
						return inputList.Value(1);
					} else if (specialForm == "quasiquote") {
						ast = Quasiquote(inputList.Value(1));
						continue;
					} else if (specialForm == "defmacro!") {
						MalType val = Eval(inputList.Value(2), env);
						((MalFunction)val).IsMacro = true;
						env.Set(inputList.Value(1), val);
						return val;
					} else if (specialForm == "macroexpand") {
						return MacroExpand(inputList.Value(1), env);
					}

				}

				MalList evaledList = EvalAst(ast, env);

				MalType head = evaledList.Value(0);
				int length = evaledList.Value.Count;

				if (head is MalFunction) {
					List<MalType> tail = evaledList.Value.GetRange(1, length - 1);

					return ((MalFunction)head).Invoke(tail);
				} else if (head is MalTcoHelper) {
					MalTcoHelper tcoHelper = (MalTcoHelper)head;

					ast = tcoHelper.Ast;
					env = new MalEnvironment(tcoHelper.Env, tcoHelper.Params, new MalList(evaledList.Value.GetRange(1, length - 1)));
					continue;
				}
			} else if (ast is MalVector) {
				MalVector inputVector = (MalVector)ast;

				if (inputVector.Value.Count == 0) {
					return ast;
				} else {
					return EvalAst(ast, env);
				}
			} else if (ast is MalHashMap) {
				MalHashMap inputMap = (MalHashMap)ast;

				if (inputMap.Count == 0) {
					return ast;
				} else {
					List<MalType> keys = inputMap.GetKeys();
					List<MalType> pairs = new List<MalType>();

					foreach (MalType key in keys) {
						pairs.Add(key);
						pairs.Add(Eval(inputMap.Get(key), env));
					}

					return new MalHashMap(pairs);
				}
			} else {
				return EvalAst(ast, env);
			}
		}

		throw new Exception("Code execution should not have gotten here");
	}

	public static bool IsPair(MalType form)
	{
		return (form is MalList && ((MalList)form).Value.Count > 0) || (form is MalVector && ((MalVector)form).Value.Count > 0);
	}

	public static MalType Quasiquote(MalType ast)
	{
		if (IsPair(ast) == false) {
			return new MalList(new List<MalType>({
				new MalSymbol("quote"),
				ast
			}));
		} else if (ast is MalList) {
			List<MalType> astList = ((MalList)ast).Value;

			if (astList(0) is MalSymbol && ((MalSymbol)astList(0)).Value == "unquote") {
				return astList(1);
			} else if (IsPair(astList(0))) {
				MalType astSym = ((MalList)astList(0)).Value(0);

				if (astSym is MalSymbol && ((MalSymbol)astSym).Value == "splice-unquote") {
					return new MalList(new List<MalType>({
						new MalSymbol("concat"),
						astList(1),
						Quasiquote(new MalList(astList.GetRange(1, astList.Count - 1)))
					}));
				} else {
					return ast;
				}
			} else {
				return new MalList(new List<MalType>({
					new MalSymbol("cons"),
					Quasiquote(astList(0)),
					Quasiquote(new MalList(astList.GetRange(1, astList.Count - 1)))
				}));
			}
		} else {
			return ast;
		}
	}
}
