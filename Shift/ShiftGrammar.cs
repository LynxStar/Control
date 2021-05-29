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

{service}

{service_member}

{method_forms}

{statement}

{control_statement}
{declaration}
{assignment}
{accessor}
{expressionTypes}


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
	| service
	;
";

		public static string data => "form data : DATA identifier OPENSBRACKET (field)* CLOSESBRACKET;";

		public static string field => "form field : typeDef SEMICOLON;";

		public static string library => "form library : LIBRARY identifier OPENSBRACKET (method)* CLOSESBRACKET;";

		public static string service => "form service : SERVICE identifier OPENSBRACKET (service_member)* CLOSESBRACKET;";

		public static string service_member => @"
form service_member
	: method
	| field
	| constructor
	;

";

		public static string method_forms => @"
form method : signature block;

form constructor : THIS OPENPARENS (parameters)? CLOSEPARENS block;

form signature : typeDef OPENPARENS (parameters)? CLOSEPARENS;

form parameters : parameter (COMMA parameter)*;

form parameter : typeDef;

form block : OPENSBRACKET (statement)* CLOSESBRACKET;
";

		public static string statement => @"
form statement 
	: control_statement
	| declaration
	| assignment
	| return_expression
	| expression_statement
	;
";

		public static string control_statement => @"
form control_statement
	: if_control
	| while_control;

form if_control : IF OPENPARENS conditional_or_expression CLOSEPARENS block;
form while_control : WHILE OPENPARENS conditional_or_expression CLOSEPARENS block;

";

		public static string declaration => @"
form declaration : typeDef (initializer)? SEMICOLON;

form initializer : ASSIGNMENT expression;
";

		public static string assignment => "form assignment : accessor ASSIGNMENT expression SEMICOLON;";
		public static string accessor => "form accessor : identifier (DOT identifier)*;";

		public static string expressionTypes => @"
form return_expression : RETURN expression SEMICOLON;

form expression_statement : expression SEMICOLON;

form expression 
	: conditional_or_expression
	;

form conditional_or_expression : conditional_and_expression (OR conditional_and_expression)*;

form conditional_and_expression : equality_expression (AND equality_expression)*;

form equality_expression : relational_expression (equality_operator relational_expression)?;

form equality_operator
	: EQUALS
	| NOT_EQUALS
	;

form relational_expression : additive_expression (relational_operator additive_expression)?;

form relational_operator
	: LESS_THAN_OR_EQUAL
	| GREATER_THAN_OR_EQUAL
	| LEFT_CAROT
	| RIGHT_CAROT
	;

form additive_expression : multiplicative_expression (additive_operator multiplicative_expression)*;

form additive_operator 
	: PLUS
	| MINUS
	;

form multiplicative_expression : unary_expression (multiplicative_operator unary_expression)*;


form multiplicative_operator
	: STAR
	| FORWARDSLASH
	;

form unary_expression
	: main_expression
	;

form main_expression : expression_start (expression_chain)*;

form expression_start
	: literal
	| identifier
	| parens_expression
	| new_expression
	| invocation 
	;

form expression_chain
	: member_access 
	| invocation
	;

form member_access : DOT identifier;

form parens_expression : OPENPARENS (expression)? CLOSEPARENS;

form new_expression : NEW identifier invocation;

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
token SERVICE : 'service' WHITESPACE;

token THIS : 'this';

token IF : 'if';
token WHILE : 'while';

token RETURN : 'return';

token NEW : 'new';
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

token OR : '||';
token AND : '&&';

token LESS_THAN_OR_EQUAL : '<=';
token GREATER_THAN_OR_EQUAL : '>=';

token LEFT_CAROT : '<';
token RIGHT_CAROT : '>';

token EQUALS : '==';
token NOT_EQUALS : '!=';

token ASSIGNMENT : '=';

token PLUS : '+';
token MINUS : '-';
token STAR : '*';
token FORWARDSLASH : '/';

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
