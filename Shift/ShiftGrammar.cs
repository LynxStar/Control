using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift
{
	public static class ShiftGrammar
	{

		public static string FullGrammar => @$"

{source}
{aspect}

{data}

{library}

{method_forms}

{statement}

{declaration}
{assignment}
{accessor}
{expressionTypes}
{call}




{chains}

{invocation}
	
{literals}

{field}

{type_def}

{type}
{identifier}

{COMMENT}

{STRING}

{KEYWORD_TOKENS}

{SYNTAX_TOKENS}

{LASTCAPTURE_LITERALS}

{_fragments}

{WHITESPACE}
";

		#region Forms

		public static string source => "form source : (aspect)*;";

		public static string aspect => @"
form aspect
	: data
	| library
	;
";

		public static string data => "form data : DATA identifier OPENSBRACKET (field)* CLOSESBRACKET;";

		public static string field => "form field : typeDef SEMICOLON;";

		public static string library => "form library : LIBRARY identifier OPENSBRACKET (method)* CLOSESBRACKET;";

		public static string method_forms => @"
form method : signature block;

form signature : typeDef OPENPARENS (parameters)? CLOSEPARENS;

form parameters : parameter (COMMA parameter)*;

form parameter : typeDef;

form block : OPENSBRACKET (statement SEMICOLON)* CLOSESBRACKET;
";

		public static string statement => @"
form statement 
	: declaration		
	| assignment
	| return_expression
	| call
	;
";

		public static string declaration => @"
form declaration : type identifier (initializer)?;

form initializer : ASSIGNMENT expression;
";

		public static string assignment => "form assignment : accessor ASSIGNMENT expression;";
		public static string accessor => "form accessor : identifier (DOT identifier)*;";

		public static string expressionTypes => @"
form return_expression : RETURN expression;

form expression 
	: unary_expression
	;

form unary_expression
	: primary_expression
	;

form primary_expression
	: parens_expression
	| literal
	| chain
	;

form parens_expression : OPENPARENS (expression)? CLOSEPARENS;
";

		public static string call => "form call : expression invocation;";

		public static string chains => @"
form chain : chain_part (DOT chain_part)*;

form chain_part
	: invocation
	| identifier
	| literal
	;
";

		public static string invocation => @"
form invocation : OPENPARENS (arguments)? CLOSEPARENS;

form arguments : argument (COMMA argument)*;

form argument : expression;
";

		public static string literals => @"
form literal
	: boolean
	| string
	| INTEGER
	;
	
form boolean
	: TRUE
	| FALSE
	;

form string : STRING;
";

		public static string type_def => "form typeDef : type identifier;";

		public static string type => "form type : IDENTIFIER;";
		public static string identifier => "form identifier : IDENTIFIER;";

		#endregion

		#region Tokens

		public static string KEYWORD_TOKENS => @"
token DATA : 'data' WHITESPACE;
token LIBRARY : 'library' WHITESPACE;

token RETURN : 'return';

token TRUE : 'true';
token FALSE : 'false';
";

		public static string SYNTAX_TOKENS => @"
token OPENSBRACKET : '{';
token CLOSESBRACKET : '}';
token OPENPARENS : '(';
token CLOSEPARENS : ')';

token DOT : '.';
token SEMICOLON : ';';
token COMMA : ',';

token ASSIGNMENT : '=';
";

		public static string STRING => "token STRING : `\"((?:\\\\.|[^\\\\\"])*)\"`;";

		public static string LASTCAPTURE_LITERALS => @"
token IDENTIFIER : LETTER (LETTER_OR_DIGIT)*;

token INTEGER : (DIGIT)+;
";

        #endregion

        #region Fragments

		public static string DOUBLESPACE => "fragment DOUBLEQUOTE : '\"';";

		public static string _fragments => $@"
fragment LETTER     : `[a-zA-Z]` ;
fragment DIGIT      : `[0-9]` ;
{DOUBLESPACE}

fragment LETTER_OR_DIGIT
	: LETTER 
	| DIGIT
	;
";

		#endregion

		#region Discards

		public static string COMMENT => "discard COMMENT : `\\/\\/.*?((?=\\n)|(?=\\r\\n)|(?=$))`;";

		public static string WHITESPACE => "discard WHITESPACE : `[ \t\n\r]+` ;";

        #endregion

    }
}
