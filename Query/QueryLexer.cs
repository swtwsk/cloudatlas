//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.7.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Query.g4 by ANTLR 4.7.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.7.2")]
[System.CLSCompliant(false)]
public partial class QueryLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, SELECT=2, ORDER=3, BY=4, ASC=5, DESC=6, WHERE=7, FIRST=8, LAST=9, 
		NULLS=10, ALL=11, AS=12, DISTINCT=13, REGEXP=14, AND=15, OR=16, LPAREN=17, 
		RPAREN=18, LBRACE=19, RBRACE=20, LBRACK=21, RBRACK=22, SEMI=23, COMMA=24, 
		GT=25, LT=26, NOT=27, EQ=28, NEQ=29, LE=30, GE=31, ADD=32, SUB=33, MUL=34, 
		DIV=35, MOD=36, ID=37, INT=38, TRUE=39, FALSE=40, DOUBLE=41, WHITESPACE=42, 
		STRING=43;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "SELECT", "ORDER", "BY", "ASC", "DESC", "WHERE", "FIRST", "LAST", 
		"NULLS", "ALL", "AS", "DISTINCT", "REGEXP", "AND", "OR", "LPAREN", "RPAREN", 
		"LBRACE", "RBRACE", "LBRACK", "RBRACK", "SEMI", "COMMA", "GT", "LT", "NOT", 
		"EQ", "NEQ", "LE", "GE", "ADD", "SUB", "MUL", "DIV", "MOD", "ExponentPart", 
		"Digits", "ID", "INT", "TRUE", "FALSE", "DOUBLE", "WHITESPACE", "STRING"
	};


	public QueryLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public QueryLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "'error'", "'SELECT'", "'ORDER'", "'BY'", "'ASC'", "'DESC'", "'WHERE'", 
		"'FIRST'", "'LAST'", "'NULLS'", "'ALL'", "'AS'", "'DISTINCT'", "'REGEXP'", 
		null, null, "'('", "')'", "'{'", "'}'", "'['", "']'", "';'", "','", "'>'", 
		"'<'", "'!'", "'=='", "'!='", "'<='", "'>='", "'+'", "'-'", "'*'", "'/'", 
		"'%'", null, null, "'true'", "'false'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, "SELECT", "ORDER", "BY", "ASC", "DESC", "WHERE", "FIRST", 
		"LAST", "NULLS", "ALL", "AS", "DISTINCT", "REGEXP", "AND", "OR", "LPAREN", 
		"RPAREN", "LBRACE", "RBRACE", "LBRACK", "RBRACK", "SEMI", "COMMA", "GT", 
		"LT", "NOT", "EQ", "NEQ", "LE", "GE", "ADD", "SUB", "MUL", "DIV", "MOD", 
		"ID", "INT", "TRUE", "FALSE", "DOUBLE", "WHITESPACE", "STRING"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "Query.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static QueryLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x2', '-', '\x132', '\b', '\x1', '\x4', '\x2', '\t', '\x2', 
		'\x4', '\x3', '\t', '\x3', '\x4', '\x4', '\t', '\x4', '\x4', '\x5', '\t', 
		'\x5', '\x4', '\x6', '\t', '\x6', '\x4', '\a', '\t', '\a', '\x4', '\b', 
		'\t', '\b', '\x4', '\t', '\t', '\t', '\x4', '\n', '\t', '\n', '\x4', '\v', 
		'\t', '\v', '\x4', '\f', '\t', '\f', '\x4', '\r', '\t', '\r', '\x4', '\xE', 
		'\t', '\xE', '\x4', '\xF', '\t', '\xF', '\x4', '\x10', '\t', '\x10', '\x4', 
		'\x11', '\t', '\x11', '\x4', '\x12', '\t', '\x12', '\x4', '\x13', '\t', 
		'\x13', '\x4', '\x14', '\t', '\x14', '\x4', '\x15', '\t', '\x15', '\x4', 
		'\x16', '\t', '\x16', '\x4', '\x17', '\t', '\x17', '\x4', '\x18', '\t', 
		'\x18', '\x4', '\x19', '\t', '\x19', '\x4', '\x1A', '\t', '\x1A', '\x4', 
		'\x1B', '\t', '\x1B', '\x4', '\x1C', '\t', '\x1C', '\x4', '\x1D', '\t', 
		'\x1D', '\x4', '\x1E', '\t', '\x1E', '\x4', '\x1F', '\t', '\x1F', '\x4', 
		' ', '\t', ' ', '\x4', '!', '\t', '!', '\x4', '\"', '\t', '\"', '\x4', 
		'#', '\t', '#', '\x4', '$', '\t', '$', '\x4', '%', '\t', '%', '\x4', '&', 
		'\t', '&', '\x4', '\'', '\t', '\'', '\x4', '(', '\t', '(', '\x4', ')', 
		'\t', ')', '\x4', '*', '\t', '*', '\x4', '+', '\t', '+', '\x4', ',', '\t', 
		',', '\x4', '-', '\t', '-', '\x4', '.', '\t', '.', '\x3', '\x2', '\x3', 
		'\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', 
		'\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x5', '\x3', '\x5', '\x3', 
		'\x5', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', 
		'\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\b', 
		'\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', 
		'\t', '\x3', '\t', '\x3', '\t', '\x3', '\t', '\x3', '\t', '\x3', '\t', 
		'\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', 
		'\v', '\x3', '\v', '\x3', '\v', '\x3', '\v', '\x3', '\v', '\x3', '\v', 
		'\x3', '\f', '\x3', '\f', '\x3', '\f', '\x3', '\f', '\x3', '\r', '\x3', 
		'\r', '\x3', '\r', '\x3', '\xE', '\x3', '\xE', '\x3', '\xE', '\x3', '\xE', 
		'\x3', '\xE', '\x3', '\xE', '\x3', '\xE', '\x3', '\xE', '\x3', '\xE', 
		'\x3', '\xF', '\x3', '\xF', '\x3', '\xF', '\x3', '\xF', '\x3', '\xF', 
		'\x3', '\xF', '\x3', '\xF', '\x3', '\x10', '\x3', '\x10', '\x3', '\x10', 
		'\x3', '\x10', '\x3', '\x10', '\x3', '\x10', '\x5', '\x10', '\xB1', '\n', 
		'\x10', '\x3', '\x11', '\x3', '\x11', '\x3', '\x11', '\x3', '\x11', '\x5', 
		'\x11', '\xB7', '\n', '\x11', '\x3', '\x12', '\x3', '\x12', '\x3', '\x13', 
		'\x3', '\x13', '\x3', '\x14', '\x3', '\x14', '\x3', '\x15', '\x3', '\x15', 
		'\x3', '\x16', '\x3', '\x16', '\x3', '\x17', '\x3', '\x17', '\x3', '\x18', 
		'\x3', '\x18', '\x3', '\x19', '\x3', '\x19', '\x3', '\x1A', '\x3', '\x1A', 
		'\x3', '\x1B', '\x3', '\x1B', '\x3', '\x1C', '\x3', '\x1C', '\x3', '\x1D', 
		'\x3', '\x1D', '\x3', '\x1D', '\x3', '\x1E', '\x3', '\x1E', '\x3', '\x1E', 
		'\x3', '\x1F', '\x3', '\x1F', '\x3', '\x1F', '\x3', ' ', '\x3', ' ', '\x3', 
		' ', '\x3', '!', '\x3', '!', '\x3', '\"', '\x3', '\"', '\x3', '#', '\x3', 
		'#', '\x3', '$', '\x3', '$', '\x3', '%', '\x3', '%', '\x3', '&', '\x3', 
		'&', '\x5', '&', '\xE7', '\n', '&', '\x3', '&', '\x3', '&', '\x3', '\'', 
		'\x3', '\'', '\x3', '\'', '\a', '\'', '\xEE', '\n', '\'', '\f', '\'', 
		'\xE', '\'', '\xF1', '\v', '\'', '\x5', '\'', '\xF3', '\n', '\'', '\x3', 
		'(', '\x3', '(', '\a', '(', '\xF7', '\n', '(', '\f', '(', '\xE', '(', 
		'\xFA', '\v', '(', '\x3', ')', '\x3', ')', '\x3', '*', '\x3', '*', '\x3', 
		'*', '\x3', '*', '\x3', '*', '\x3', '+', '\x3', '+', '\x3', '+', '\x3', 
		'+', '\x3', '+', '\x3', '+', '\x3', ',', '\x6', ',', '\x10A', '\n', ',', 
		'\r', ',', '\xE', ',', '\x10B', '\x3', ',', '\x3', ',', '\a', ',', '\x110', 
		'\n', ',', '\f', ',', '\xE', ',', '\x113', '\v', ',', '\x3', ',', '\x3', 
		',', '\x6', ',', '\x117', '\n', ',', '\r', ',', '\xE', ',', '\x118', '\x3', 
		',', '\x6', ',', '\x11C', '\n', ',', '\r', ',', '\xE', ',', '\x11D', '\x3', 
		',', '\x3', ',', '\x5', ',', '\x122', '\n', ',', '\x3', '-', '\x3', '-', 
		'\x3', '-', '\x3', '-', '\x3', '.', '\x3', '.', '\x3', '.', '\x3', '.', 
		'\a', '.', '\x12C', '\n', '.', '\f', '.', '\xE', '.', '\x12F', '\v', '.', 
		'\x3', '.', '\x3', '.', '\x2', '\x2', '/', '\x3', '\x3', '\x5', '\x4', 
		'\a', '\x5', '\t', '\x6', '\v', '\a', '\r', '\b', '\xF', '\t', '\x11', 
		'\n', '\x13', '\v', '\x15', '\f', '\x17', '\r', '\x19', '\xE', '\x1B', 
		'\xF', '\x1D', '\x10', '\x1F', '\x11', '!', '\x12', '#', '\x13', '%', 
		'\x14', '\'', '\x15', ')', '\x16', '+', '\x17', '-', '\x18', '/', '\x19', 
		'\x31', '\x1A', '\x33', '\x1B', '\x35', '\x1C', '\x37', '\x1D', '\x39', 
		'\x1E', ';', '\x1F', '=', ' ', '?', '!', '\x41', '\"', '\x43', '#', '\x45', 
		'$', 'G', '%', 'I', '&', 'K', '\x2', 'M', '\x2', 'O', '\'', 'Q', '(', 
		'S', ')', 'U', '*', 'W', '+', 'Y', ',', '[', '-', '\x3', '\x2', '\n', 
		'\x4', '\x2', 'G', 'G', 'g', 'g', '\x4', '\x2', '-', '-', '/', '/', '\x3', 
		'\x2', '\x33', ';', '\x3', '\x2', '\x32', ';', '\x4', '\x2', '\x43', '\\', 
		'\x63', '|', '\x6', '\x2', '\x32', ';', '\x43', '\\', '\x61', '\x61', 
		'\x63', '|', '\x5', '\x2', '\v', '\f', '\xF', '\xF', '\"', '\"', '\x5', 
		'\x2', '\f', '\f', '\xF', '\xF', '$', '$', '\x2', '\x13D', '\x2', '\x3', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\x5', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '\a', '\x3', '\x2', '\x2', '\x2', '\x2', '\t', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\v', '\x3', '\x2', '\x2', '\x2', '\x2', '\r', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '\xF', '\x3', '\x2', '\x2', '\x2', '\x2', '\x11', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\x13', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '\x15', '\x3', '\x2', '\x2', '\x2', '\x2', '\x17', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '\x19', '\x3', '\x2', '\x2', '\x2', '\x2', '\x1B', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\x1D', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '\x1F', '\x3', '\x2', '\x2', '\x2', '\x2', '!', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '#', '\x3', '\x2', '\x2', '\x2', '\x2', '%', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '\'', '\x3', '\x2', '\x2', '\x2', '\x2', ')', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '+', '\x3', '\x2', '\x2', '\x2', '\x2', '-', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '/', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\x31', '\x3', '\x2', '\x2', '\x2', '\x2', '\x33', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x35', '\x3', '\x2', '\x2', '\x2', '\x2', '\x37', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\x39', '\x3', '\x2', '\x2', '\x2', '\x2', 
		';', '\x3', '\x2', '\x2', '\x2', '\x2', '=', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '?', '\x3', '\x2', '\x2', '\x2', '\x2', '\x41', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x43', '\x3', '\x2', '\x2', '\x2', '\x2', '\x45', '\x3', 
		'\x2', '\x2', '\x2', '\x2', 'G', '\x3', '\x2', '\x2', '\x2', '\x2', 'I', 
		'\x3', '\x2', '\x2', '\x2', '\x2', 'O', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'Q', '\x3', '\x2', '\x2', '\x2', '\x2', 'S', '\x3', '\x2', '\x2', '\x2', 
		'\x2', 'U', '\x3', '\x2', '\x2', '\x2', '\x2', 'W', '\x3', '\x2', '\x2', 
		'\x2', '\x2', 'Y', '\x3', '\x2', '\x2', '\x2', '\x2', '[', '\x3', '\x2', 
		'\x2', '\x2', '\x3', ']', '\x3', '\x2', '\x2', '\x2', '\x5', '\x63', '\x3', 
		'\x2', '\x2', '\x2', '\a', 'j', '\x3', '\x2', '\x2', '\x2', '\t', 'p', 
		'\x3', '\x2', '\x2', '\x2', '\v', 's', '\x3', '\x2', '\x2', '\x2', '\r', 
		'w', '\x3', '\x2', '\x2', '\x2', '\xF', '|', '\x3', '\x2', '\x2', '\x2', 
		'\x11', '\x82', '\x3', '\x2', '\x2', '\x2', '\x13', '\x88', '\x3', '\x2', 
		'\x2', '\x2', '\x15', '\x8D', '\x3', '\x2', '\x2', '\x2', '\x17', '\x93', 
		'\x3', '\x2', '\x2', '\x2', '\x19', '\x97', '\x3', '\x2', '\x2', '\x2', 
		'\x1B', '\x9A', '\x3', '\x2', '\x2', '\x2', '\x1D', '\xA3', '\x3', '\x2', 
		'\x2', '\x2', '\x1F', '\xB0', '\x3', '\x2', '\x2', '\x2', '!', '\xB6', 
		'\x3', '\x2', '\x2', '\x2', '#', '\xB8', '\x3', '\x2', '\x2', '\x2', '%', 
		'\xBA', '\x3', '\x2', '\x2', '\x2', '\'', '\xBC', '\x3', '\x2', '\x2', 
		'\x2', ')', '\xBE', '\x3', '\x2', '\x2', '\x2', '+', '\xC0', '\x3', '\x2', 
		'\x2', '\x2', '-', '\xC2', '\x3', '\x2', '\x2', '\x2', '/', '\xC4', '\x3', 
		'\x2', '\x2', '\x2', '\x31', '\xC6', '\x3', '\x2', '\x2', '\x2', '\x33', 
		'\xC8', '\x3', '\x2', '\x2', '\x2', '\x35', '\xCA', '\x3', '\x2', '\x2', 
		'\x2', '\x37', '\xCC', '\x3', '\x2', '\x2', '\x2', '\x39', '\xCE', '\x3', 
		'\x2', '\x2', '\x2', ';', '\xD1', '\x3', '\x2', '\x2', '\x2', '=', '\xD4', 
		'\x3', '\x2', '\x2', '\x2', '?', '\xD7', '\x3', '\x2', '\x2', '\x2', '\x41', 
		'\xDA', '\x3', '\x2', '\x2', '\x2', '\x43', '\xDC', '\x3', '\x2', '\x2', 
		'\x2', '\x45', '\xDE', '\x3', '\x2', '\x2', '\x2', 'G', '\xE0', '\x3', 
		'\x2', '\x2', '\x2', 'I', '\xE2', '\x3', '\x2', '\x2', '\x2', 'K', '\xE4', 
		'\x3', '\x2', '\x2', '\x2', 'M', '\xF2', '\x3', '\x2', '\x2', '\x2', 'O', 
		'\xF4', '\x3', '\x2', '\x2', '\x2', 'Q', '\xFB', '\x3', '\x2', '\x2', 
		'\x2', 'S', '\xFD', '\x3', '\x2', '\x2', '\x2', 'U', '\x102', '\x3', '\x2', 
		'\x2', '\x2', 'W', '\x121', '\x3', '\x2', '\x2', '\x2', 'Y', '\x123', 
		'\x3', '\x2', '\x2', '\x2', '[', '\x127', '\x3', '\x2', '\x2', '\x2', 
		']', '^', '\a', 'g', '\x2', '\x2', '^', '_', '\a', 't', '\x2', '\x2', 
		'_', '`', '\a', 't', '\x2', '\x2', '`', '\x61', '\a', 'q', '\x2', '\x2', 
		'\x61', '\x62', '\a', 't', '\x2', '\x2', '\x62', '\x4', '\x3', '\x2', 
		'\x2', '\x2', '\x63', '\x64', '\a', 'U', '\x2', '\x2', '\x64', '\x65', 
		'\a', 'G', '\x2', '\x2', '\x65', '\x66', '\a', 'N', '\x2', '\x2', '\x66', 
		'g', '\a', 'G', '\x2', '\x2', 'g', 'h', '\a', '\x45', '\x2', '\x2', 'h', 
		'i', '\a', 'V', '\x2', '\x2', 'i', '\x6', '\x3', '\x2', '\x2', '\x2', 
		'j', 'k', '\a', 'Q', '\x2', '\x2', 'k', 'l', '\a', 'T', '\x2', '\x2', 
		'l', 'm', '\a', '\x46', '\x2', '\x2', 'm', 'n', '\a', 'G', '\x2', '\x2', 
		'n', 'o', '\a', 'T', '\x2', '\x2', 'o', '\b', '\x3', '\x2', '\x2', '\x2', 
		'p', 'q', '\a', '\x44', '\x2', '\x2', 'q', 'r', '\a', '[', '\x2', '\x2', 
		'r', '\n', '\x3', '\x2', '\x2', '\x2', 's', 't', '\a', '\x43', '\x2', 
		'\x2', 't', 'u', '\a', 'U', '\x2', '\x2', 'u', 'v', '\a', '\x45', '\x2', 
		'\x2', 'v', '\f', '\x3', '\x2', '\x2', '\x2', 'w', 'x', '\a', '\x46', 
		'\x2', '\x2', 'x', 'y', '\a', 'G', '\x2', '\x2', 'y', 'z', '\a', 'U', 
		'\x2', '\x2', 'z', '{', '\a', '\x45', '\x2', '\x2', '{', '\xE', '\x3', 
		'\x2', '\x2', '\x2', '|', '}', '\a', 'Y', '\x2', '\x2', '}', '~', '\a', 
		'J', '\x2', '\x2', '~', '\x7F', '\a', 'G', '\x2', '\x2', '\x7F', '\x80', 
		'\a', 'T', '\x2', '\x2', '\x80', '\x81', '\a', 'G', '\x2', '\x2', '\x81', 
		'\x10', '\x3', '\x2', '\x2', '\x2', '\x82', '\x83', '\a', 'H', '\x2', 
		'\x2', '\x83', '\x84', '\a', 'K', '\x2', '\x2', '\x84', '\x85', '\a', 
		'T', '\x2', '\x2', '\x85', '\x86', '\a', 'U', '\x2', '\x2', '\x86', '\x87', 
		'\a', 'V', '\x2', '\x2', '\x87', '\x12', '\x3', '\x2', '\x2', '\x2', '\x88', 
		'\x89', '\a', 'N', '\x2', '\x2', '\x89', '\x8A', '\a', '\x43', '\x2', 
		'\x2', '\x8A', '\x8B', '\a', 'U', '\x2', '\x2', '\x8B', '\x8C', '\a', 
		'V', '\x2', '\x2', '\x8C', '\x14', '\x3', '\x2', '\x2', '\x2', '\x8D', 
		'\x8E', '\a', 'P', '\x2', '\x2', '\x8E', '\x8F', '\a', 'W', '\x2', '\x2', 
		'\x8F', '\x90', '\a', 'N', '\x2', '\x2', '\x90', '\x91', '\a', 'N', '\x2', 
		'\x2', '\x91', '\x92', '\a', 'U', '\x2', '\x2', '\x92', '\x16', '\x3', 
		'\x2', '\x2', '\x2', '\x93', '\x94', '\a', '\x43', '\x2', '\x2', '\x94', 
		'\x95', '\a', 'N', '\x2', '\x2', '\x95', '\x96', '\a', 'N', '\x2', '\x2', 
		'\x96', '\x18', '\x3', '\x2', '\x2', '\x2', '\x97', '\x98', '\a', '\x43', 
		'\x2', '\x2', '\x98', '\x99', '\a', 'U', '\x2', '\x2', '\x99', '\x1A', 
		'\x3', '\x2', '\x2', '\x2', '\x9A', '\x9B', '\a', '\x46', '\x2', '\x2', 
		'\x9B', '\x9C', '\a', 'K', '\x2', '\x2', '\x9C', '\x9D', '\a', 'U', '\x2', 
		'\x2', '\x9D', '\x9E', '\a', 'V', '\x2', '\x2', '\x9E', '\x9F', '\a', 
		'K', '\x2', '\x2', '\x9F', '\xA0', '\a', 'P', '\x2', '\x2', '\xA0', '\xA1', 
		'\a', '\x45', '\x2', '\x2', '\xA1', '\xA2', '\a', 'V', '\x2', '\x2', '\xA2', 
		'\x1C', '\x3', '\x2', '\x2', '\x2', '\xA3', '\xA4', '\a', 'T', '\x2', 
		'\x2', '\xA4', '\xA5', '\a', 'G', '\x2', '\x2', '\xA5', '\xA6', '\a', 
		'I', '\x2', '\x2', '\xA6', '\xA7', '\a', 'G', '\x2', '\x2', '\xA7', '\xA8', 
		'\a', 'Z', '\x2', '\x2', '\xA8', '\xA9', '\a', 'R', '\x2', '\x2', '\xA9', 
		'\x1E', '\x3', '\x2', '\x2', '\x2', '\xAA', '\xAB', '\a', '\x43', '\x2', 
		'\x2', '\xAB', '\xAC', '\a', 'P', '\x2', '\x2', '\xAC', '\xB1', '\a', 
		'\x46', '\x2', '\x2', '\xAD', '\xAE', '\a', '\x63', '\x2', '\x2', '\xAE', 
		'\xAF', '\a', 'p', '\x2', '\x2', '\xAF', '\xB1', '\a', '\x66', '\x2', 
		'\x2', '\xB0', '\xAA', '\x3', '\x2', '\x2', '\x2', '\xB0', '\xAD', '\x3', 
		'\x2', '\x2', '\x2', '\xB1', ' ', '\x3', '\x2', '\x2', '\x2', '\xB2', 
		'\xB3', '\a', 'Q', '\x2', '\x2', '\xB3', '\xB7', '\a', 'T', '\x2', '\x2', 
		'\xB4', '\xB5', '\a', 'q', '\x2', '\x2', '\xB5', '\xB7', '\a', 't', '\x2', 
		'\x2', '\xB6', '\xB2', '\x3', '\x2', '\x2', '\x2', '\xB6', '\xB4', '\x3', 
		'\x2', '\x2', '\x2', '\xB7', '\"', '\x3', '\x2', '\x2', '\x2', '\xB8', 
		'\xB9', '\a', '*', '\x2', '\x2', '\xB9', '$', '\x3', '\x2', '\x2', '\x2', 
		'\xBA', '\xBB', '\a', '+', '\x2', '\x2', '\xBB', '&', '\x3', '\x2', '\x2', 
		'\x2', '\xBC', '\xBD', '\a', '}', '\x2', '\x2', '\xBD', '(', '\x3', '\x2', 
		'\x2', '\x2', '\xBE', '\xBF', '\a', '\x7F', '\x2', '\x2', '\xBF', '*', 
		'\x3', '\x2', '\x2', '\x2', '\xC0', '\xC1', '\a', ']', '\x2', '\x2', '\xC1', 
		',', '\x3', '\x2', '\x2', '\x2', '\xC2', '\xC3', '\a', '_', '\x2', '\x2', 
		'\xC3', '.', '\x3', '\x2', '\x2', '\x2', '\xC4', '\xC5', '\a', '=', '\x2', 
		'\x2', '\xC5', '\x30', '\x3', '\x2', '\x2', '\x2', '\xC6', '\xC7', '\a', 
		'.', '\x2', '\x2', '\xC7', '\x32', '\x3', '\x2', '\x2', '\x2', '\xC8', 
		'\xC9', '\a', '@', '\x2', '\x2', '\xC9', '\x34', '\x3', '\x2', '\x2', 
		'\x2', '\xCA', '\xCB', '\a', '>', '\x2', '\x2', '\xCB', '\x36', '\x3', 
		'\x2', '\x2', '\x2', '\xCC', '\xCD', '\a', '#', '\x2', '\x2', '\xCD', 
		'\x38', '\x3', '\x2', '\x2', '\x2', '\xCE', '\xCF', '\a', '?', '\x2', 
		'\x2', '\xCF', '\xD0', '\a', '?', '\x2', '\x2', '\xD0', ':', '\x3', '\x2', 
		'\x2', '\x2', '\xD1', '\xD2', '\a', '#', '\x2', '\x2', '\xD2', '\xD3', 
		'\a', '?', '\x2', '\x2', '\xD3', '<', '\x3', '\x2', '\x2', '\x2', '\xD4', 
		'\xD5', '\a', '>', '\x2', '\x2', '\xD5', '\xD6', '\a', '?', '\x2', '\x2', 
		'\xD6', '>', '\x3', '\x2', '\x2', '\x2', '\xD7', '\xD8', '\a', '@', '\x2', 
		'\x2', '\xD8', '\xD9', '\a', '?', '\x2', '\x2', '\xD9', '@', '\x3', '\x2', 
		'\x2', '\x2', '\xDA', '\xDB', '\a', '-', '\x2', '\x2', '\xDB', '\x42', 
		'\x3', '\x2', '\x2', '\x2', '\xDC', '\xDD', '\a', '/', '\x2', '\x2', '\xDD', 
		'\x44', '\x3', '\x2', '\x2', '\x2', '\xDE', '\xDF', '\a', ',', '\x2', 
		'\x2', '\xDF', '\x46', '\x3', '\x2', '\x2', '\x2', '\xE0', '\xE1', '\a', 
		'\x31', '\x2', '\x2', '\xE1', 'H', '\x3', '\x2', '\x2', '\x2', '\xE2', 
		'\xE3', '\a', '\'', '\x2', '\x2', '\xE3', 'J', '\x3', '\x2', '\x2', '\x2', 
		'\xE4', '\xE6', '\t', '\x2', '\x2', '\x2', '\xE5', '\xE7', '\t', '\x3', 
		'\x2', '\x2', '\xE6', '\xE5', '\x3', '\x2', '\x2', '\x2', '\xE6', '\xE7', 
		'\x3', '\x2', '\x2', '\x2', '\xE7', '\xE8', '\x3', '\x2', '\x2', '\x2', 
		'\xE8', '\xE9', '\x5', 'M', '\'', '\x2', '\xE9', 'L', '\x3', '\x2', '\x2', 
		'\x2', '\xEA', '\xF3', '\a', '\x32', '\x2', '\x2', '\xEB', '\xEF', '\t', 
		'\x4', '\x2', '\x2', '\xEC', '\xEE', '\t', '\x5', '\x2', '\x2', '\xED', 
		'\xEC', '\x3', '\x2', '\x2', '\x2', '\xEE', '\xF1', '\x3', '\x2', '\x2', 
		'\x2', '\xEF', '\xED', '\x3', '\x2', '\x2', '\x2', '\xEF', '\xF0', '\x3', 
		'\x2', '\x2', '\x2', '\xF0', '\xF3', '\x3', '\x2', '\x2', '\x2', '\xF1', 
		'\xEF', '\x3', '\x2', '\x2', '\x2', '\xF2', '\xEA', '\x3', '\x2', '\x2', 
		'\x2', '\xF2', '\xEB', '\x3', '\x2', '\x2', '\x2', '\xF3', 'N', '\x3', 
		'\x2', '\x2', '\x2', '\xF4', '\xF8', '\t', '\x6', '\x2', '\x2', '\xF5', 
		'\xF7', '\t', '\a', '\x2', '\x2', '\xF6', '\xF5', '\x3', '\x2', '\x2', 
		'\x2', '\xF7', '\xFA', '\x3', '\x2', '\x2', '\x2', '\xF8', '\xF6', '\x3', 
		'\x2', '\x2', '\x2', '\xF8', '\xF9', '\x3', '\x2', '\x2', '\x2', '\xF9', 
		'P', '\x3', '\x2', '\x2', '\x2', '\xFA', '\xF8', '\x3', '\x2', '\x2', 
		'\x2', '\xFB', '\xFC', '\x5', 'M', '\'', '\x2', '\xFC', 'R', '\x3', '\x2', 
		'\x2', '\x2', '\xFD', '\xFE', '\a', 'v', '\x2', '\x2', '\xFE', '\xFF', 
		'\a', 't', '\x2', '\x2', '\xFF', '\x100', '\a', 'w', '\x2', '\x2', '\x100', 
		'\x101', '\a', 'g', '\x2', '\x2', '\x101', 'T', '\x3', '\x2', '\x2', '\x2', 
		'\x102', '\x103', '\a', 'h', '\x2', '\x2', '\x103', '\x104', '\a', '\x63', 
		'\x2', '\x2', '\x104', '\x105', '\a', 'n', '\x2', '\x2', '\x105', '\x106', 
		'\a', 'u', '\x2', '\x2', '\x106', '\x107', '\a', 'g', '\x2', '\x2', '\x107', 
		'V', '\x3', '\x2', '\x2', '\x2', '\x108', '\x10A', '\t', '\x5', '\x2', 
		'\x2', '\x109', '\x108', '\x3', '\x2', '\x2', '\x2', '\x10A', '\x10B', 
		'\x3', '\x2', '\x2', '\x2', '\x10B', '\x109', '\x3', '\x2', '\x2', '\x2', 
		'\x10B', '\x10C', '\x3', '\x2', '\x2', '\x2', '\x10C', '\x10D', '\x3', 
		'\x2', '\x2', '\x2', '\x10D', '\x111', '\a', '\x30', '\x2', '\x2', '\x10E', 
		'\x110', '\t', '\x5', '\x2', '\x2', '\x10F', '\x10E', '\x3', '\x2', '\x2', 
		'\x2', '\x110', '\x113', '\x3', '\x2', '\x2', '\x2', '\x111', '\x10F', 
		'\x3', '\x2', '\x2', '\x2', '\x111', '\x112', '\x3', '\x2', '\x2', '\x2', 
		'\x112', '\x122', '\x3', '\x2', '\x2', '\x2', '\x113', '\x111', '\x3', 
		'\x2', '\x2', '\x2', '\x114', '\x116', '\a', '\x30', '\x2', '\x2', '\x115', 
		'\x117', '\t', '\x5', '\x2', '\x2', '\x116', '\x115', '\x3', '\x2', '\x2', 
		'\x2', '\x117', '\x118', '\x3', '\x2', '\x2', '\x2', '\x118', '\x116', 
		'\x3', '\x2', '\x2', '\x2', '\x118', '\x119', '\x3', '\x2', '\x2', '\x2', 
		'\x119', '\x122', '\x3', '\x2', '\x2', '\x2', '\x11A', '\x11C', '\t', 
		'\x5', '\x2', '\x2', '\x11B', '\x11A', '\x3', '\x2', '\x2', '\x2', '\x11C', 
		'\x11D', '\x3', '\x2', '\x2', '\x2', '\x11D', '\x11B', '\x3', '\x2', '\x2', 
		'\x2', '\x11D', '\x11E', '\x3', '\x2', '\x2', '\x2', '\x11E', '\x11F', 
		'\x3', '\x2', '\x2', '\x2', '\x11F', '\x120', '\a', '\x30', '\x2', '\x2', 
		'\x120', '\x122', '\x5', 'K', '&', '\x2', '\x121', '\x109', '\x3', '\x2', 
		'\x2', '\x2', '\x121', '\x114', '\x3', '\x2', '\x2', '\x2', '\x121', '\x11B', 
		'\x3', '\x2', '\x2', '\x2', '\x122', 'X', '\x3', '\x2', '\x2', '\x2', 
		'\x123', '\x124', '\t', '\b', '\x2', '\x2', '\x124', '\x125', '\x3', '\x2', 
		'\x2', '\x2', '\x125', '\x126', '\b', '-', '\x2', '\x2', '\x126', 'Z', 
		'\x3', '\x2', '\x2', '\x2', '\x127', '\x12D', '\a', '$', '\x2', '\x2', 
		'\x128', '\x12C', '\n', '\t', '\x2', '\x2', '\x129', '\x12A', '\a', '$', 
		'\x2', '\x2', '\x12A', '\x12C', '\a', '$', '\x2', '\x2', '\x12B', '\x128', 
		'\x3', '\x2', '\x2', '\x2', '\x12B', '\x129', '\x3', '\x2', '\x2', '\x2', 
		'\x12C', '\x12F', '\x3', '\x2', '\x2', '\x2', '\x12D', '\x12B', '\x3', 
		'\x2', '\x2', '\x2', '\x12D', '\x12E', '\x3', '\x2', '\x2', '\x2', '\x12E', 
		'\x130', '\x3', '\x2', '\x2', '\x2', '\x12F', '\x12D', '\x3', '\x2', '\x2', 
		'\x2', '\x130', '\x131', '\a', '$', '\x2', '\x2', '\x131', '\\', '\x3', 
		'\x2', '\x2', '\x2', '\x10', '\x2', '\xB0', '\xB6', '\xE6', '\xEF', '\xF2', 
		'\xF8', '\x10B', '\x111', '\x118', '\x11D', '\x121', '\x12B', '\x12D', 
		'\x3', '\b', '\x2', '\x2',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}