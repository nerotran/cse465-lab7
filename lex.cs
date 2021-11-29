using System;
using System.Collections.Generic;

[System.Serializable]
public class LexicalErrorException : System.Exception
{
    public LexicalErrorException() { }
    public LexicalErrorException(string message) : base(message) { }
    public LexicalErrorException(string message, System.Exception inner) : base(message, inner) { }
    protected LexicalErrorException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

/**
 * This lab is the FIRST part of a parser lab.  This week we focus on lexing. 
 * Eventually, we will build a parser for a simple language that supports assignments
 *     var = expr
 * Typeing an expression by itself will display the value of the expression
 *
 * Tokens:
 *      ID:  Any letter followed be a sequence of letters and numbers
 *      REAL: An optional sign followed by a sequence of digits, optionally with single decimal point. 
 *      WS:  Whitespace (no tokens generated, this is skipped)
 *      LPAREN, RPAREN, EQUALS:  (, ), and = literals
 *      OP_PLUS, OP_MINUS: + and - literals
 *      OP_MULTIPLY, OP_DIVIDE:  * and / literals
 *      OP_EXPONENT: ** literal (x**2 is "x squared"). 
 *Grammar:
 *      <stmt> ::= <assign> | <expr>
 *      <assign> ::= ID = <expr>
 *      <expr> ::= <term> | <term> + <expr> | <term> - <expr>
 *      <term> ::= <factor> | <factor> * <term> | <factor> / <term>
 *      <factor> ::= <base>**<factor> | <base>
 *      <base> := ID | NUM |  (<expr>)
 */
public class ExpressionParser {
    
    public enum Symbol
    {
        ID, REAL, WS, LPAREN, RPAREN, EQUALS, OP_PLUS, OP_MINUS, OP_MULTIPLY, OP_DIVIDE, OP_EXPONENT
    }

    /**
     * Represents a node in a parse tree. 
     * - Should keep track of the 'text' of the node (the substring under the node)
     * - Should keep track of the line and column when the node begins. 
     * - Should keep track of the children of the node in the parse tree
     * - should keep track of the Symbol (see the enum) corresponding to the node
     * - Tokens are leaf nodes (the array of children should be null)
     * - Needs a constructor with symbol, text, line, and column
     **/
    public class Node{
        public Symbol symbol;
        public int line;
        public int column;
        public string text;
        public Node[] children = null;

        public Node(Symbol symbol, string text, int line, int column ) {
            this.symbol = symbol;
            this.line = line;
            this.column = column;
            this.text = text;
        }
    }

    /**
     * Generator for tokens. 
     * Use 'yield return' to return tokens one at a time.
     **/
    public static IEnumerable<Node> tokenize(System.IO.StreamReader src) 
    {
        int line = 1;
        int column = 1;
        System.Text.StringBuilder lexeme = new System.Text.StringBuilder();

        int state = 0; //start
        int next = 0;
        bool consume = true;

        // Symbol is ttypes[state] when we find the token
        Symbol[] ttypes = {Symbol.WS,           //0
                           Symbol.REAL,         //1
                           Symbol.REAL,         //2
                           Symbol.OP_PLUS,      //3      
                           Symbol.OP_MINUS,     //4
                           Symbol.OP_MULTIPLY,  //5
                           Symbol.OP_EXPONENT,  //6
                           Symbol.OP_DIVIDE,    //7
                           Symbol.EQUALS,       //8
                           Symbol.ID,           //9
                           Symbol.LPAREN,       //10
                           Symbol.RPAREN,       //11
                          };

        while (src.Peek() > -1) {
            char c = (char) src.Peek();
            consume = true;
            // Determine next state

            switch (state) 
            {
                case 0: 
                    switch (c)
                    {
                        case '+':
                            next = 3;
                            break;
                        case '-':
                            next = 4;
                            break;
                        case '*':
                            next = 5;
                            break;
                        case '/':
                            next = 7;
                            break;
                        case '=':
                            next = 8;
                            break;
                        case '(':
                            next = 10;
                            break;
                        case ')':
                            next = 11;
                            break;
                        default:
                            throw new LexicalErrorException($"Invalid character '{c}' at line {line} column {column}");
                    }
                    break;

                case 1: 
                    switch (c)
                    {
                        case '.':
                            next = 2;
                            break;
                        default:
                            next = 0;
                            consume = false;
                            break;
                    }
                    break;

                case 2: 
                    switch (c)
                    {
                        default:
                            next = 0;
                            consume = false;
                            break;
                    }
                    break;

                case 3: 
                case 4: 
                    switch (c)
                    {
                        default:
                            next = 0;
                            consume = false;
                            break;
                    }
                    break;
                case 5: 
                    switch (c)
                    {
                        case '*':
                            next = 6;
                            break;
                        default:
                            next = 0;
                            consume = false;
                            break;
                    }
                    break;
                case 6: case 7: case 8: case 10: case 11:
                    next = 0;
                    consume = false;
                    break;
                case 9: 
                    switch (c)
                    {
                        default:
                            next = 0;
                            consume = false;
                            break;
                    }
                    break;
            } // switching on state
            
            // Add to the lexeme OR output a token and reset the lexeme
            // Consume the next character (unless WS or finished a token)
            if (consume) {
                src.Read();
                if (next != 0) {
                    lexeme.Append(c);
                }
                column += 1;
            } else {
                yield return new Node(ttypes[state], lexeme.ToString(), line, column);
                lexeme.Clear();
            }

            // Update the line counter
            if (c == "\n") {
                line += 1;
                column = 1;
            }

            state = next;
        }

    }

    public static void Main(string[] args){
        try {
            foreach (Node n in tokenize(new System.IO.StreamReader(Console.OpenStandardInput()))){
                Console.WriteLine($"{Enum.GetName(typeof(Symbol), n.symbol),-15}\t{n.text}");
            }
        } catch (LexicalErrorException e){
            Console.WriteLine(e.Message);
        }
    }
}
