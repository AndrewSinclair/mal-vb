class Main
{
	public void InitModules()
	{
		Core.InitLoad();
		Environment.InitLoad();
	}

	public string Prompt()
	{
		return Prompt(null);
	}

	public string Prompt(string promptText)
	{
		if (promptText != null) {
			Console.Write(promptText);
		} else {
			Console.Write("user> ");
		}

		return Console.ReadLine();
	}

	public bool ReadExit(string inputLine)
	{
		return inputLine == null || inputLine.ToLower.Trim == "exit" || inputLine.ToLower.Trim == "quit";
	}

	public string Rep(string inputLine)
	{
		MalType ast = Reader.ReadStr(inputLine);
		MalType evalResult = Eval.Eval(ast, Environment.ReplEnv);
		object printOutput = Printer.PrStr(evalResult, true);

		return printOutput;
	}

	private void Main(string[] args)
	{
		string inputLine;
		bool isRunOnce = false;
		bool isExitEncountered = false;

		InitModules();

		if (args != null && args.Count > 0) {
			inputLine = string.Format("(load-file {0})", args(0));

			Environment.ReplEnv.Set(new MalSymbol("*ARGV*"), new MalList(args.ToList.GetRange(1, args.Count - 1).Select(t => Reader.ReadStr(t))));

			isRunOnce = true;
		} else {
			inputLine = Prompt();
		}

		while (!isExitEncountered) {
			try {
				string outputLine = Rep(inputLine);

				Console.WriteLine(outputLine);
			} catch (EvaluateException eex) {
				Console.WriteLine("Error has occurred: " + eex.Message);
			} catch (NoReadableCodeException nrex) {
			//comment or blank line
			} catch (Exception ex) {
				Console.WriteLine("Something unexpected has occured: " + ex.Message);
			}

			if (!isRunOnce) {
				inputLine = Prompt();
				isExitEncountered = ReadExit(inputLine);
			} else {
				isExitEncountered = true;
			}
		}

		Console.WriteLine("Thanks for playing!");
	}

	private class NoReadableCodeException : Exception
	{

		private NoReadableCodeException(string msg)
		{
			base.New(msg);
		}
	}
}
