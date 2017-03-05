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
fragment PUNCTUATION: '||' | '&&' | '==' | '>=' | '<=' | '!=' | '*' | '/' | '>>' | '+' | '-';
fragment PRIVATE: [pP][rR][iI][vV][aA][tT][eE];
fragment WS: [ \t\r\n]+ -> skip;
fragment INLINECOMMENT: '//' .*? '\n' -> channel(HIDDEN);
fragment BLOCKCOMMENT: '/*' .*? '*/' -> channel(HIDDEN);


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
              identifier '=' binaryexpression
          |  PRIVATE identifier '=' binaryexpression
          ;
binaryexpression:
                    primaryexpression
                |    binaryexpression operator binaryexpression 
                ;
primaryexpression: 
                     number
                 |   unaryexpression
                 |   nularexpression
                 |   variable
                 |   string
                 |   '{' code '}'
                 |   '(' binaryexpression ')'
                 |   '[' ( binaryexpression ( ',' binaryexpression )* )?
                 ;
nularexpression:
                   operator
               ;
unaryexpression:
                   operator primaryexpression
               ;
variable:
            identifier
        ;
operator:
            identifier
        |   PUNCTUATION
        |   PUNCTUATION PUNCTUATION
        ;
identifier:
              (LETTER | '_') (LETTER | DIGIT | '_')*
          ;
number:
          ('0x' | '$') HEXADIGIT+
      |  '-'? DIGIT+ ( '.' DIGIT+ )?
      ;
string:
          '"' ( ANY | '""' )*? '"'
      |   '\'' ( ANY | '\'\'' )*? '\''
      ;