grammar Query;

program :
  	statement (SEMI statement)*
	;

statement :
  	SELECT sel_item (COMMA sel_item)* (where_clause)? (order_by_clause)?
	;

where_clause : WHERE cond_expr ;

order_by_clause : ORDER BY order_item (COMMA order_item)* ;

order_item :
  	cond_expr (order)? (nulls)?
	;

order : ASC | DESC ;

nulls : NULLS FIRST | NULLS LAST ;

sel_item :
  	(sel_modifier)? cond_expr (AS identifier)? ;

sel_modifier : ALL | DISTINCT ;

cond_expr :
    and_expr (OR and_expr)*
	| error
	;

cond_expr_no_gt :
    and_expr_no_gt (OR and_expr_no_gt)*
	| error
	;

and_expr :
    not_expr (AND not_expr)* ;

and_expr_no_gt :
  	not_expr_no_gt (AND not_expr_no_gt)*
	;

not_expr :
  	bool_expr
	| NOT not_expr
	;

not_expr_no_gt :
  	bool_expr_no_gt
	| NOT not_expr_no_gt
	;

bool_expr :
  	basic_expr rel_op basic_expr
	| basic_expr REGEXP string_const
	| basic_expr
	;

bool_expr_no_gt :
      basic_expr rel_op_no_gt basic_expr
	| basic_expr REGEXP string_const
	| basic_expr
	;

basic_expr :
  	basic_expr ADD fact_expr
	| basic_expr SUB fact_expr
	| fact_expr
	;

fact_expr :
  	fact_expr MUL neg_expr
	| fact_expr DIV neg_expr
	| fact_expr MOD neg_expr
	| neg_expr
	;

neg_expr :
  	SUB neg_expr
	| term_expr
	;

term_expr :
  	identifier
	| identifier LPAREN (expr_list)? RPAREN
	| identifier LPAREN MUL RPAREN
	| string_const
	| bool_const
	| int_const
	| double_const
	| LBRACE RBRACE
	| LBRACK RBRACK
	| LT expr_list_no_gt GT
	| LPAREN cond_expr RPAREN
	| LPAREN statement RPAREN
	;

identifier :
      ID
    ;

string_const :
      STRING
    ;

bool_const :
      TRUE
    | FALSE
    ;

int_const :
      INT
    ;

double_const :
      DOUBLE
    ;

expr_list :
    cond_expr (COMMA cond_expr)* ;

expr_list_no_gt :
	cond_expr_no_gt (COMMA cond_expr_no_gt)* ;

rel_op :
  	rel_op_no_gt
	| GT
	;

rel_op_no_gt :
  	EQ
	| NEQ
	| LT
	| LE
	| GE
	;

// Keywords
SELECT : 'SELECT' ;
ORDER : 'ORDER' ;
BY : 'BY' ;
ASC : 'ASC' ;
DESC : 'DESC' ;
WHERE : 'WHERE' ;
FIRST : 'FIRST' ;
LAST : 'LAST' ;
NULLS : 'NULLS' ;
ALL : 'ALL' ;
AS : 'AS' ;
DISTINCT : 'DISTINCT' ;
REGEXP : 'REGEXP' ;

error : 'error' ; // TODO: CHECK IT

AND : 'AND' | 'and' ;
OR : 'OR' | 'or' ;

// Separators
LPAREN:             '(';
RPAREN:             ')';
LBRACE:             '{';
RBRACE:             '}';
LBRACK:             '[';
RBRACK:             ']';
SEMI:               ';';
COMMA:              ',';
// Operators
GT:                 '>';
LT:                 '<';
NOT:               '!';
EQ:                 '==';
NEQ:                '!=';
LE:                 '<=';
GE:                 '>=';
ADD:                '+';
SUB:                '-';
MUL:                '*';
DIV:                '/';
MOD:                '%';

// fragments
fragment ExponentPart : [eE] [+-]? Digits ;
fragment Digits : '0' | [1-9] [0-9]* ;

ID : [a-zA-Z] [a-zA-Z_0-9]* ;
INT : Digits ;
TRUE : 'true' ;
FALSE : 'false' ;
DOUBLE : 
    [0-9]+ '.' [0-9]* 
  | '.' [0-9]+ 
  | [0-9]+ '.' ExponentPart ;
WHITESPACE : [ \t\r\n] -> skip ;
STRING : '"' (~["\r\n] | '""')* '"' ;  // TODO: check it

