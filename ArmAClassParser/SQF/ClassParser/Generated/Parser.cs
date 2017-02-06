using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using SQF.ClassParser;



using System;
using NLog;


namespace SQF.ClassParser.Generated
{
    public class Parser {
    	public const int _EOF = 0;
	public const int _T_SCALAR = 1;
	public const int _T_HEX = 2;
	public const int _T_STRING = 3;
	public const int _T_STRINGTABLESTRING = 4;
	public const int _T_IDENT = 5;
	public const int maxT = 17;

        const bool _T = true;
        const bool _x = false;
        const int minErrDist = 2;

        public Scanner scanner;
        public Errors  errors;

        public Token t;    // last recognized token
        public Token la;   // lookahead token
        int errDist = minErrDist;
    FlowDocument doc;
	ConfigEntry Root;

    

        public Parser(Scanner scanner) {
            this.scanner = scanner;
            errors = new Errors();
        }
        
        bool peekCompare(params int[] values)
        {
            Token t = la;
            foreach(int i in values)
            {
                if(i != -1 && t.kind != i)
                {
                    scanner.ResetPeek();
                    return false;
                }
                if (t.next == null)
                    t = scanner.Peek();
                else
                    t = t.next;
            }
            scanner.ResetPeek();
            return true;
        }
        bool peekString(int count, params string[] values)
        {
            Token t = la;
            for(; count > 0; count --)
                t = scanner.Peek();
            foreach(var it in values)
            {
                if(t.val == it)
                {
                    scanner.ResetPeek();
                    return true;
                }
            }
            scanner.ResetPeek();
            return false;
        }
        
        
        void SynErr (int n) {
            if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
            errDist = 0;
        }
        void Warning (string msg) {
            errors.Warning(la.line, la.col, msg);
        }

        public void SemErr (string msg) {
            if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
            errDist = 0;
        }
        
        void Get () {
            for (;;) {
                t = la;
                la = scanner.Scan();
                if (la.kind <= maxT) { ++errDist; break; }
    
                la = t;
            }
        }
        
        void Expect (int n) {
            if (la.kind==n) Get(); else { SynErr(n); }
        }
        
        bool StartOf (int s) {
            return set[s, la.kind];
        }
        
        void ExpectWeak (int n, int follow) {
            if (la.kind == n) Get();
            else {
                SynErr(n);
                while (!StartOf(follow)) Get();
            }
        }


        bool WeakSeparator(int n, int syFol, int repFol) {
            int kind = la.kind;
            if (kind == n) {Get(); return true;}
            else if (StartOf(repFol)) {return false;}
            else {
                SynErr(n);
                while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
                    Get();
                    kind = la.kind;
                }
                return StartOf(syFol);
            }
        }

        
    	void CONFIGFILE() {
		while (la.kind == 6) {
			CONFIG(Root);
		}
	}

	void CONFIG(ConfigEntry parent) {
		var cur = new ConfigEntry(parent); 
		Expect(6);
		Expect(5);
		cur.NameStart = doc.ContentStart.GetPositionAtOffset(t.charPos, LogicalDirection.Forward);
		cur.NameEnd = doc.ContentStart.GetPositionAtOffset(t.charPos + t.val.Length, LogicalDirection.Backward);
		
		if (la.kind == 7) {
			Get();
			Expect(5);
			cur.ParentStart = doc.ContentStart.GetPositionAtOffset(t.charPos, LogicalDirection.Forward);
			cur.ParentEnd = doc.ContentStart.GetPositionAtOffset(t.charPos + t.val.Length, LogicalDirection.Backward);
			
		}
		if (la.kind == 8) {
			Get();
			while (la.kind == 5 || la.kind == 6) {
				if (la.kind == 5) {
					FIELD(cur);
				} else {
					CONFIG(cur);
				}
			}
			Expect(9);
		}
		Expect(10);
	}

	void FIELD(ConfigEntry parent) {
		var cur = new ConfigEntry(parent); 
		Expect(5);
		cur.NameStart = doc.ContentStart.GetPositionAtOffset(t.charPos, LogicalDirection.Forward);
		cur.NameEnd = doc.ContentStart.GetPositionAtOffset(t.charPos + t.val.Length, LogicalDirection.Backward);
		
		if (la.kind == 11) {
			Get();
			Expect(12);
		}
		Expect(13);
		if (la.kind == 8) {
			ARRAY();
			cur.ContentStart = doc.ContentStart.GetPositionAtOffset(t.charPos, LogicalDirection.Forward);
			cur.ContentEnd = doc.ContentStart.GetPositionAtOffset(t.charPos + t.val.Length, LogicalDirection.Backward);
			
		} else if (la.kind == 1 || la.kind == 2) {
			SCALAR();
			cur.ContentStart = doc.ContentStart.GetPositionAtOffset(t.charPos, LogicalDirection.Forward);
			cur.ContentEnd = doc.ContentStart.GetPositionAtOffset(t.charPos + t.val.Length, LogicalDirection.Backward);
			
		} else if (la.kind == 3 || la.kind == 4) {
			STRING();
			cur.ContentStart = doc.ContentStart.GetPositionAtOffset(t.charPos, LogicalDirection.Forward);
			cur.ContentEnd = doc.ContentStart.GetPositionAtOffset(t.charPos + t.val.Length, LogicalDirection.Backward);
			
		} else if (la.kind == 14 || la.kind == 15) {
			BOOLEAN();
			cur.ContentStart = doc.ContentStart.GetPositionAtOffset(t.charPos, LogicalDirection.Forward);
			cur.ContentEnd = doc.ContentStart.GetPositionAtOffset(t.charPos + t.val.Length, LogicalDirection.Backward);
			
		} else if (StartOf(1)) {
			Get();
			cur.ContentStart = doc.ContentStart.GetPositionAtOffset(t.charPos, LogicalDirection.Forward); 
			while (StartOf(2)) {
				Get();
			}
			cur.ContentEnd = doc.ContentStart.GetPositionAtOffset(t.charPos + t.val.Length, LogicalDirection.Backward); 
		} else SynErr(18);
		Expect(10);
	}

	void ARRAY() {
		Expect(8);
		if (la.kind == 1 || la.kind == 2) {
			SCALAR();
		} else if (la.kind == 3 || la.kind == 4) {
			STRING();
		} else if (la.kind == 14 || la.kind == 15) {
			BOOLEAN();
		} else SynErr(19);
		while (la.kind == 16) {
			Get();
			if (la.kind == 1 || la.kind == 2) {
				SCALAR();
			} else if (la.kind == 3 || la.kind == 4) {
				STRING();
			} else if (la.kind == 14 || la.kind == 15) {
				BOOLEAN();
			} else SynErr(20);
		}
		Expect(9);
	}

	void SCALAR() {
		if (la.kind == 1) {
			Get();
		} else if (la.kind == 2) {
			Get();
		} else SynErr(21);
	}

	void STRING() {
		if (la.kind == 3) {
			Get();
		} else if (la.kind == 4) {
			Get();
		} else SynErr(22);
	}

	void BOOLEAN() {
		if (la.kind == 14) {
			Get();
		} else if (la.kind == 15) {
			Get();
		} else SynErr(23);
	}


    
        public void Parse() {
    		CONFIGFILE();
		Expect(0);

        }
        
        static readonly bool[,] set = {
    		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_T,_T,_T, _x,_T,_T,_T, _T,_T,_x,_x, _T,_T,_x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_T, _T,_T,_T,_T, _T,_T,_x}

        };
    } // end Parser


    public class Errors {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public int count = 0;                                    // number of errors detected
        public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
        public string errMsgFormat = "line {0} col {1}: {2}"; // 0=line, 1=column, 2=text
        public Errors()
        {
        }

        public virtual void SynErr (int line, int col, int n) {
            string s;
            switch (n) {
    			case 0: s = "EOF expected"; break;
			case 1: s = "T_SCALAR expected"; break;
			case 2: s = "T_HEX expected"; break;
			case 3: s = "T_STRING expected"; break;
			case 4: s = "T_STRINGTABLESTRING expected"; break;
			case 5: s = "T_IDENT expected"; break;
			case 6: s = "\"class\" expected"; break;
			case 7: s = "\":\" expected"; break;
			case 8: s = "\"{\" expected"; break;
			case 9: s = "\"}\" expected"; break;
			case 10: s = "\";\" expected"; break;
			case 11: s = "\"[\" expected"; break;
			case 12: s = "\"]\" expected"; break;
			case 13: s = "\"=\" expected"; break;
			case 14: s = "\"true\" expected"; break;
			case 15: s = "\"false\" expected"; break;
			case 16: s = "\",\" expected"; break;
			case 17: s = "??? expected"; break;
			case 18: s = "invalid FIELD"; break;
			case 19: s = "invalid ARRAY"; break;
			case 20: s = "invalid ARRAY"; break;
			case 21: s = "invalid SCALAR"; break;
			case 22: s = "invalid STRING"; break;
			case 23: s = "invalid BOOLEAN"; break;

                default: s = "error " + n; break;
            }
            logger.Error(string.Format(errMsgFormat, line, col, s));
            //errorStream.WriteLine(errMsgFormat, line, col, s);
            count++;
        }

        public virtual void SemErr (int line, int col, string s) {
            logger.Error(string.Format(errMsgFormat, line, col, s));
            //errorStream.WriteLine(errMsgFormat, line, col, s);
            count++;
        }
        
        public virtual void SemErr (string s) {
            logger.Error(s);
            //errorStream.WriteLine(s);
            count++;
        }
        
        public virtual void Warning (int line, int col, string s) {
            logger.Warn(string.Format(errMsgFormat, line, col, s));
            //errorStream.WriteLine(errMsgFormat, line, col, s);
        }
        
        public virtual void Warning(string s) {
            logger.Warn(s);
            //errorStream.WriteLine(s);
        }
    } // Errors


    public class FatalError: Exception {
        public FatalError(string m): base(m) {}
    }
}
