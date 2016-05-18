using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQF.ClassParser;



using System;


namespace SQF.ClassParser
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

    

        public SQF.ClassParser.File Base { get; set;}
        
        public Parser(Scanner scanner, Action<string> LoggerAction) {
            this.scanner = scanner;
            errors = new Errors(LoggerAction);
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
		Data data; 
		if(Base == null)
		{
		   Base = new SQF.ClassParser.File();
		}
		StringList PathList = new StringList();
		
		CONFIG(out data, Base, PathList);
		Base[data] = data; 
		while (la.kind == 6) {
			CONFIG(out data, Base, PathList);
			Base[data] = data; 
		}
	}

	void CONFIG(out Data data, ConfigClass parent, StringList PathList) {
		ConfigClass cc = new ConfigClass(); data = new Data(cc); Data d; 
		Expect(6);
		Expect(5);
		if(parent.ContainsKey(t.val))
		{
		   SemErr("Class is already defined in current scope");
		}
		data.Name = t.val;
		
		if (la.kind == 7) {
			Get();
			Expect(5);
			ConfigClass c = parent;
			var tmpList = new StringList(PathList);
			Data tmpData = null;
			do
			{
			   StringBuilder sb = new StringBuilder();
			   foreach (var it in tmpList)
			   {
			       sb.Append('/');
			       sb.Append(it);
			   };
			   sb.Append('/');
			   sb.Append(t.val);
			   tmpData = this.Base[sb.ToString()];
			   if (tmpList.Count == 0)
			       break;
			   tmpList.RemoveAt(tmpList.Count - 1);
			} while (tmpData == null || !tmpData.IsClass);
			
			if (c != null)
			{
			   cc.Parent = tmpData;
			}
			else
			{
			   SemErr("Parent is not yet existing");
			};
			
		}
		PathList.Add(data.Name); 
		if (la.kind == 8) {
			Get();
			while (la.kind == 5 || la.kind == 6) {
				if (la.kind == 5) {
					FIELD(out d);
					data.Class[d] = d; 
				} else {
					CONFIG(out d, data.Class, PathList);
					data.Class[d] = d; 
				}
			}
			Expect(9);
		}
		Expect(10);
		PathList.RemoveAt(PathList.Count - 1); 
	}

	void FIELD(out Data data) {
		data = null; bool isArray = false; 
		Expect(5);
		string name = t.val; 
		if (la.kind == 11) {
			Get();
			Expect(12);
			isArray = true; 
		}
		Expect(13);
		if (la.kind == 8) {
			ARRAY(out data);
			if(!isArray) SemErr("Invalid field syntax: Missing [] at field name"); 
		} else if (la.kind == 1 || la.kind == 2) {
			SCALAR(out data);
			if(isArray) SemErr("Invalid field syntax: Located [] at field name"); 
		} else if (la.kind == 3 || la.kind == 4) {
			STRING(out data);
			if(isArray) SemErr("Invalid field syntax: Located [] at field name"); 
		} else if (la.kind == 14 || la.kind == 15) {
			BOOLEAN(out data);
			if(isArray) SemErr("Invalid field syntax: Located [] at field name"); 
		} else SynErr(18);
		Expect(10);
		if(data != null) data.Name = name; 
	}

	void ARRAY(out Data data) {
		data = null; Data tmp; 
		Expect(8);
		if (StartOf(1)) {
			if (la.kind == 1 || la.kind == 2) {
				var list = new List<double>(); 
				SCALAR(out tmp);
				list.Add(tmp.Number); 
				while (la.kind == 16) {
					Get();
					SCALAR(out tmp);
					list.Add(tmp.Number); 
				}
				data = new Data(list); 
			} else if (la.kind == 3 || la.kind == 4) {
				var list = new StringList(); 
				STRING(out tmp);
				list.Add(tmp.String); 
				while (la.kind == 16) {
					Get();
					STRING(out tmp);
					list.Add(tmp.String); 
				}
				data = new Data(list); 
			} else {
				var list = new List<bool>(); 
				BOOLEAN(out tmp);
				list.Add(tmp.Boolean); 
				while (la.kind == 16) {
					Get();
					BOOLEAN(out tmp);
					list.Add(tmp.Boolean); 
				}
				data = new Data(list); 
			}
		}
		Expect(9);
	}

	void SCALAR(out Data data) {
		data = null; 
		if (la.kind == 1) {
			Get();
			data = new Data(Double.Parse(t.val, System.Globalization.CultureInfo.InvariantCulture)); 
		} else if (la.kind == 2) {
			Get();
			data = new Data((double)Convert.ToInt32(t.val.Substring(2), 16)); 
		} else SynErr(19);
	}

	void STRING(out Data data) {
		string content = default(string); 
		if (la.kind == 3) {
			Get();
			content = t.val.Substring(1, t.val.Length - 2); 
		} else if (la.kind == 4) {
			Get();
			content = t.val.Substring(1); 
		} else SynErr(20);
		data = new Data(content);
		
	}

	void BOOLEAN(out Data data) {
		bool flag = false; 
		if (la.kind == 14) {
			Get();
			flag = true; 
		} else if (la.kind == 15) {
			Get();
		} else SynErr(21);
		data = new Data(flag);
		
	}



        public void Parse() {
            la = new Token();
            la.val = "";		
            Get();
    		CONFIGFILE();
		Expect(0);

        }
        
        static readonly bool[,] set = {
    		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x}

        };
    } // end Parser


    public class Errors {
        public int count = 0;                                    // number of errors detected
        public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
        public string errMsgFormat = "line {0} col {1}: {2}"; // 0=line, 1=column, 2=text
        public Action<string> LoggerAction;
        public Errors(Action<string> LoggerAction)
        {
            this.LoggerAction = LoggerAction;
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
			case 19: s = "invalid SCALAR"; break;
			case 20: s = "invalid STRING"; break;
			case 21: s = "invalid BOOLEAN"; break;

                default: s = "error " + n; break;
            }
            Logger.Instance.log(Logger.LogLevel.ERROR, String.Format(errMsgFormat, line, col, s), this.LoggerAction);
            //errorStream.WriteLine(errMsgFormat, line, col, s);
            count++;
        }

        public virtual void SemErr (int line, int col, string s) {
            Logger.Instance.log(Logger.LogLevel.ERROR, String.Format(errMsgFormat, line, col, s), this.LoggerAction);
            //errorStream.WriteLine(errMsgFormat, line, col, s);
            count++;
        }
        
        public virtual void SemErr (string s) {
            Logger.Instance.log(Logger.LogLevel.ERROR, s, this.LoggerAction);
            //errorStream.WriteLine(s);
            count++;
        }
        
        public virtual void Warning (int line, int col, string s) {
            Logger.Instance.log(Logger.LogLevel.WARNING, String.Format(errMsgFormat, line, col, s));
            //errorStream.WriteLine(errMsgFormat, line, col, s);
        }
        
        public virtual void Warning(string s) {
            Logger.Instance.log(Logger.LogLevel.WARNING, s);
            //errorStream.WriteLine(s);
        }
    } // Errors


    public class FatalError: Exception {
        public FatalError(string m): base(m) {}
    }
}
