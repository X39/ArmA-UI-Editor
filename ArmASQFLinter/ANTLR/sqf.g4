/*
 * MIT License
 * 
 * Copyright (c) 2017 Marco Silipo aka X39
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

grammar sqf;
options
{
    language = cs;
}


sqf: code;

fragment LOWERCASE: [a-z];
fragment UPPERCASE: [A-Z];
fragment DIGIT: [0-9];
fragment LETTER: (LOWERCASE | UPPERCASE);
fragment HEXADIGIT: (DIGIT | [a-f] | [A-F]);
fragment ANY: .;
PUNCTUATION: '||' | '&&' | '==' | '>=' | '<=' | '!=' | '*' | '/' | '>>' | '+' | '-';
PRIVATE: [pP][rR][iI][vV][aA][tT][eE];
WS: [ \t\r\n]+ -> skip;
INSTRUCTION: '//@' .*? '\n';
INLINECOMMENT: '//' .*? '\n' -> skip;
BLOCKCOMMENT: '/*' .*? '*/' -> skip;
PREPROCESSOR: '#' .*? '\n' -> skip;
STRING: '"' ( ANY | '""' )*? '"' | '\'' ( ANY | '\'\'' )*? '\'';
NUMBER: ('0x' | '$') HEXADIGIT+ |  '-'? DIGIT+ ( '.' DIGIT+ )?;
IDENTIFIER: (LETTER | '_') (LETTER | DIGIT | '_')*;


code:
        statement (';' statement)*
    |
    ;
statement:
             assignment
         |   binaryexpression
         |
         ;
assignment:
			IDENTIFIER '=' binaryexpression
          |	PRIVATE IDENTIFIER '=' binaryexpression
          ;
binaryexpression:
                    primaryexpression
                |	binaryexpression operator binaryexpression 
                ;
primaryexpression: 
                     NUMBER
                 |   unaryexpression
                 |   nularexpression
                 |   variable
                 |   STRING
                 |   '{' code '}'
                 |   '(' binaryexpression ')'
                 |   '[' ( binaryexpression ( ',' binaryexpression )* )? ']'
                 ;
nularexpression:
                   operator
               ;
unaryexpression:
                   operator primaryexpression
               ;
variable:
            IDENTIFIER
        ;
operator:
            IDENTIFIER
		|	PRIVATE
        |   PUNCTUATION
        |   PUNCTUATION PUNCTUATION
        ;