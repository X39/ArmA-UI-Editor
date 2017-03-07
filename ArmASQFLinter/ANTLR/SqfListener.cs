using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using RealVirtuality.SQF.ANTLR.Parser;


namespace RealVirtuality.SQF.ANTLR
{
    public class SqfListener : IsqfListener
    {
        public struct SyntaxError
        {
            public int Line { get; set; }
            public int Col { get; set; }

            public int Length { get; set; }
            public int StartOffset { get; set; }
            public string Message { get; set; }
        }
#if DEBUG
        private readonly SqfNode Original;
#endif
        public SqfNode Current { get; set; }
        public readonly List<SyntaxError> OtherSyntaxErrors;

        public SqfListener()
        {
            this.OtherSyntaxErrors = new List<SyntaxError>();
            this.Current = new SqfCode(null);
#if DEBUG
            this.Original = this.Current;
#endif
        }
    private void EnterGeneric(SqfNode node)
        {
            Current.AddChild(node);
            Current = node;
        }
        private T ExitGeneric<T>(ParserRuleContext ctx) where T : SqfNode
        {
            var node = Current;
            Current = node.GetParent();

            node.StartOffset = ctx.Start.StartIndex;
            node.Line = ctx.Start.Line;
            node.Length = ctx.Stop.StopIndex - node.StartOffset;
            node.Col = ctx.start.Column;

            return node as T;
        }

        #region Enter
        public void EnterEveryRule(ParserRuleContext ctx) { }
        public void EnterSqf([NotNull] sqfParser.SqfContext context) { }

        public void EnterAssignment([NotNull] sqfParser.AssignmentContext context)
        {
            this.EnterGeneric(new SqfAssignment(this.Current));
        }

        public void EnterBinaryexpression([NotNull] sqfParser.BinaryexpressionContext context)
        {
            this.EnterGeneric(new SqfBinaryExpression(this.Current));
        }

        public void EnterCode([NotNull] sqfParser.CodeContext context)
        {
            this.EnterGeneric(new SqfCode(this.Current));
        }

        public void EnterNularexpression([NotNull] sqfParser.NularexpressionContext context)
        {
            this.EnterGeneric(new SqfNullarExpression(this.Current));
        }

        public void EnterOperator([NotNull] sqfParser.OperatorContext context)
        {
            this.EnterGeneric(new SqfOperator(this.Current));
        }

        public void EnterPrimaryexpression([NotNull] sqfParser.PrimaryexpressionContext context)
        {
            this.EnterGeneric(new SqfPrimaryExpression(this.Current));
        }

        public void EnterStatement([NotNull] sqfParser.StatementContext context)
        {
            this.EnterGeneric(new SqfStatement(this.Current));
        }

        public void EnterUnaryexpression([NotNull] sqfParser.UnaryexpressionContext context)
        {
            this.EnterGeneric(new SqfUnaryExpression(this.Current));
        }

        public void EnterVariable([NotNull] sqfParser.VariableContext context)
        {
            this.EnterGeneric(new SqfVariable(this.Current));
        }
        #endregion
        #region Exit
        public void ExitEveryRule(ParserRuleContext ctx) { }
        public void ExitSqf([NotNull] sqfParser.SqfContext context) { }

        public void ExitAssignment([NotNull] sqfParser.AssignmentContext context)
        {
            var node = this.ExitGeneric<SqfAssignment>(context);
            node.Context = context;
            node.HasPrivateKeyword = context.IDENTIFIER(1) != null;
            if(node.HasPrivateKeyword)
            {
                if(!context.IDENTIFIER(0).GetText().Equals("private", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.OtherSyntaxErrors.Add(new SyntaxError()
                    {
                        StartOffset = context.Start.StartIndex,
                        Col = context.Start.Column,
                        Length = context.GetText().Length,
                        Line = context.Start.Line,
                        Message = "Expected { 'private' }"
                    });
                    context.AddErrorNode(context.IDENTIFIER(0).Symbol);
                }
                node.VariableName = context.IDENTIFIER(1).GetText();
            }
            else
            {
                node.VariableName = context.IDENTIFIER(0).GetText();
            }

            if (node.Children.Count == 0)
            {
                this.OtherSyntaxErrors.Add(new SyntaxError()
                {
                    StartOffset = context.Start.StartIndex,
                    Col = context.Start.Column,
                    Length = context.GetText().Length,
                    Line = context.Start.Line,
                    Message = "Unknown Error"
                });
                context.AddErrorNode(context.Start);
            }
            else
            {
                node.AssignedExpression = node.Children[0];
            }
        }

        public void ExitBinaryexpression([NotNull] sqfParser.BinaryexpressionContext context)
        {
            var node = this.ExitGeneric<SqfBinaryExpression>(context);
            if(context.BINARY() == null)
            {
                node.RemoveFromTree();
            }
            else
            {
                node.LValue = node.Children[0];
                node.Operation = context.BINARY().GetText();
                node.RValue = node.Children[1];
            }
        }

        public void ExitCode([NotNull] sqfParser.CodeContext context)
        {
            var node = this.ExitGeneric<SqfCode>(context);
        }

        public void ExitNularexpression([NotNull] sqfParser.NularexpressionContext context)
        {
            var node = this.ExitGeneric<SqfNullarExpression>(context);
            node.RemoveFromTree();
        }

        public void ExitOperator([NotNull] sqfParser.OperatorContext context)
        {
            var node = this.ExitGeneric<SqfOperator>(context);
            node.Operator = context.GetText();
        }

        public void ExitPrimaryexpression([NotNull] sqfParser.PrimaryexpressionContext context)
        {
            var node = this.ExitGeneric<SqfPrimaryExpression>(context);

            if (context.NUMBER() != null)
            {
                var value = new SqfNumber(node.GetParent());
                var numberContext = context.NUMBER();
                value.Value = numberContext.GetText();
                value.StartOffset = node.StartOffset;
                value.Length = node.Length;
                value.Col = node.Col;
                node.GetParent().Children[node.GetParent().Children.IndexOf(node)] = value;
            }
            else if(context.STRING() != null)
            {
                var value = new SqfString(node.GetParent());
                var numberContext = context.STRING();
                value.Value = numberContext.GetText();
                value.StartOffset = node.StartOffset;
                value.Length = node.Length;
                value.Col = node.Col;
                node.GetParent().Children[node.GetParent().Children.IndexOf(node)] = value;
            }
            else
            {
                if (context.GetToken(sqfLexer.EDGYOPEN, 0) != null)
                {//Array
                    SqfNode arr = new SqfArray(node.GetParent());
                    arr.StartOffset = node.StartOffset;
                    arr.Length = node.Length;
                    arr.Col = node.Col;
                    node.GetParent().Children[node.GetParent().Children.IndexOf(node)] = arr;
                    arr.Children = node.Children;
                }
                else
                {
                    node.RemoveFromTree();
                }
            }
        }

        public void ExitStatement([NotNull] sqfParser.StatementContext context)
        {
            var node = this.ExitGeneric<SqfStatement>(context);
            node.RemoveFromTree();
        }

        public void ExitUnaryexpression([NotNull] sqfParser.UnaryexpressionContext context)
        {
            var node = this.ExitGeneric<SqfUnaryExpression>(context);
            node.Operator = node.Children[0];
            node.Expression = node.Children[1];
        }

        public void ExitVariable([NotNull] sqfParser.VariableContext context)
        {
            var node = this.ExitGeneric<SqfVariable>(context);
            node.Identifier = context.GetText();
        }
        #endregion

        public void VisitErrorNode(IErrorNode node)
        {
            
        }

        public void VisitTerminal(ITerminalNode node)
        {
            
        }
    }
}
