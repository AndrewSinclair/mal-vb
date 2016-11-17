using fun_programming;

public class Types
{
	public abstract class MalType
	{
		public string Name {
			get { return m_Name; }
			set { m_Name = Value; }
		}
		private string m_Name;
	}

	public class MalList : MalType
	{

		public List<MalType> Value {
			get { return m_Value; }
			set { m_Value = Value; }
		}
		private List<MalType> m_Value;

		public MalList(List<MalType> value)
		{
			this.Value = value;
		}
	}

	public class MalVector : MalType
	{

		public List<MalType> Value {
			get { return m_Value; }
			set { m_Value = Value; }
		}
		private List<MalType> m_Value;

		public MalVector(List<MalType> value)
		{
			this.Value = value;
		}
	}

	public class MalHashMap : MalType
	{


		private readonly Dictionary<MalType, MalType> _data = new Dictionary<MalType, MalType>();
		public MalHashMap(List<MalType> value)
		{
			int length = value.Count;

			for (i = 0; i <= length - 1; i += 2) {
				MalType key = value(i);
				MalType val = value(i + 1);
				_data.Add(key, val);
			}
		}

		public MalType Get(MalType key)
		{
			return _data(key);
		}

		public int Count()
		{
			return _data.Count;
		}

		public List<MalType> GetKeys()
		{
			return _data.Keys.ToList;
		}
	}

	public class MalInt : MalType
	{

		public int Value {
			get { return m_Value; }
			set { m_Value = Value; }
		}
		private int m_Value;

		public MalInt(int value)
		{
			this.Value = value;
		}
	}

	public class MalStr : MalType
	{

		public string Value {
			get { return m_Value; }
			set { m_Value = Value; }
		}
		private string m_Value;

		public MalStr(string value)
		{
			this.Value = value;
		}
	}

	public class MalBool : MalType
	{

		public bool Value { get; }

		public static MalBool True {
			get { return m_True; }
			set { m_True = Value; }
		}
		private static MalBool m_True;
		public static MalBool False {
			get { return m_False; }
			set { m_False = Value; }
		}
		private static MalBool m_False;

		private MalBool(bool value)
		{
			this.Value = value;
		}
	}

	public class MalDbl : MalType
	{

		public double Value {
			get { return m_Value; }
			set { m_Value = Value; }
		}
		private double m_Value;

		public MalDbl(double value)
		{
			this.Value = value;
		}
	}

	public class MalSymbol : MalType
	{

		public string Value {
			get { return m_Value; }
			set { m_Value = Value; }
		}
		private string m_Value;

		public MalSymbol(string value)
		{
			this.Value = value;
		}

		public override bool Equals(object other)
		{
			return other is MalType && this.Value == other.Value;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode;
		}
	}

	public class MalKeyword : MalType
	{

		public string Value {
			get { return m_Value; }
			set { m_Value = Value; }
		}
		private string m_Value;

		public MalKeyword(string value)
		{
			this.Value = value;
		}
	}

	public class MalAtom : MalType
	{

		public MalType Value {
			get { return m_Value; }
			set { m_Value = Value; }
		}
		private MalType m_Value;

		public MalAtom(MalType value)
		{
			this.Value = value;
		}
	}

	public class MalNil : MalType
	{


		private static MalNil _instance1;
		public static MalNil Instance {
			get { return _instance1; }
		}

		public MalNil()
		{
			_instance1 = new MalNil();
		}

	}

	public class MalFunction : MalType
	{

		public Func<List<MalType>, MalType> Value {
			get { return m_Value; }
			set { m_Value = Value; }
		}
		private Func<List<MalType>, MalType> m_Value;
		public bool IsMacro {
			get { return m_IsMacro; }
			set { m_IsMacro = Value; }
		}
		private bool m_IsMacro;

		public MalFunction(Func<List<MalType>, MalType> value)
		{
			this.Value = value;
		}

		public MalType Invoke(List<MalType> @params)
		{
			return Value.Invoke(@params);
		}
	}

	public class MalTcoHelper : MalType
	{

		public MalType Ast {
			get { return m_Ast; }
			set { m_Ast = Value; }
		}
		private MalType m_Ast;
		public MalType Params {
			get { return m_Params; }
			set { m_Params = Value; }
		}
		private MalType m_Params;
		public MalEnvironment Env {
			get { return m_Env; }
			set { m_Env = Value; }
		}
		private MalEnvironment m_Env;
		public MalFunction Fn {
			get { return m_Fn; }
			set { m_Fn = Value; }
		}
		private MalFunction m_Fn;

		public MalTcoHelper(MalType ast, MalType @params, MalEnvironment env, MalFunction fn)
		{
			this.Ast = ast;
			this.Params = @params;
			this.Env = env;
			this.Fn = fn;
		}
	}
}
