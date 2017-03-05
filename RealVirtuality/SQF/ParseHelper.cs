using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealVirtuality.SQF
{
    internal static class ParseHelper
    {
        internal static readonly char[] Separators = { ';', ' ', '\t' };

        internal static string GetNextToken(StreamReader reader)
        {
            int ic;
            var builder = new StringBuilder();

            while ((ic = reader.Peek()) != -1)
            {
                if (Separators.Contains(((char)ic)))
                {
                    break;
                }
                builder.Append((char)reader.Read());
            }
            return builder.ToString();
        }

        internal static bool CheckExpressionForError(SQFExpression exp, out ParseInfo msg, int startOff, int startLine)
        {
            throw new NotImplementedException();
        }

        internal static SQFDocument ParseSQFDocument(SQFDocument doc, StreamReader reader, IEnumerable<SQFDefinition> Definitions, ref int offset, ref int line, ref int curLineOffset, ref int expStart, ref int expStartLine)
        {
            throw new NotImplementedException();
            /*
            SQFExpression exp = null;
            var wasPrivateStartToken = false;
            var privTokenString = string.Empty;
            var isArray = false;
            while (reader.Peek() != -1)
            {
                if(exp == null)
                {
                    exp = new SQFExpression();
                    expStart = offset;
                    expStartLine = line;
                }
                var c = (char)reader.Peek();
                //Skip whitespaces
                while(c == ' ' || c == '\t')
                {
                    reader.Read();
                    offset++;
                    curLineOffset++;
                    var ic = reader.Peek();
                    if(ic == -1)
                    {
                        c = '\0';
                    }
                    else
                    {
                        c = (char)ic;
                    }
                }

                //Decide what to do
                switch (c)
                {
                    case '\0':
                        //EOF
                        {
                            if (!CheckExpressionForError(exp, out ParseInfo parseInfo, expStart, expStartLine) && parseInfo == default(ParseInfo))
                            {
                                doc.InfoEntries.Add(new ParseInfo() { Offset = offset, Line = line, Column = curLineOffset, Length = 0, RefCode = LogHelper.GetName(LogHelper.EError.EOF_Reached), Level = ParseInfo.EInfoLevel.Error, Text = LogHelper.GetName(LogHelper.EError.EOF_Reached) });
                            }
                        }
                        break;
                    case ';':
                        //End of current expression
                        {
                            reader.Read();
                            offset++;
                            while (reader.Peek() == ';')
                            {
                                offset++;
                                reader.Read();
                            }
                            if (CheckExpressionForError(exp, out ParseInfo parseInfo, expStart, expStartLine))
                            {
                                doc.InfoEntries.Add(parseInfo);
                            }
                            doc.Expressions.Add(exp);
                            exp = null;
                        }
                        break;
                    case '\r':
                        //New Line
                        offset++;
                        reader.Read(); //skip \r
                        offset++;
                        reader.Read(); //skip \n
                        line++;
                        curLineOffset = 0; //Reset offset
                        break;
                    case '"':
                        break;
                    case '[':
                        
                        break;
                    case ']':
                        break;
                    case '=':
                        if(wasPrivateStartToken)
                        {

                        }
                        else
                        {
                            doc.InfoEntries.Add(new ParseInfo() { Offset = offset, Line = line, Column = curLineOffset, Length = 1, RefCode = LogHelper.GetName(LogHelper.EError.InvalidAssignment), Level = ParseInfo.EInfoLevel.Error, Text = LogHelper.GetName(LogHelper.EError.InvalidAssignment) });
                        }
                        break;
                    default:
                        {
                            var token = GetNextToken(reader);
                            offset += token.Length;
                            if (wasPrivateStartToken)
                            {
                                if (string.IsNullOrWhiteSpace(privTokenString))
                                {
                                    privTokenString = token;
                                }
                            }
                            else if (token.Equals("private", StringComparison.InvariantCultureIgnoreCase))
                            {
                                wasPrivateStartToken = true;
                                break;
                            }
                        }
                        break;
                }
            }
                */

        }
    }
}
