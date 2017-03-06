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
        public SqfDocument Document { get; set; }
        public SqfNode Current { get; set; }

        public SqfListener()
        {
            this.Document = new SqfDocument();
            this.Current = this.Document.Root = new SqfCode(null);
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
            node.HasPrivateKeyword = context.PRIVATE() != null;
            node.VariableName = context.IDENTIFIER().GetText();
        }

        public void ExitBinaryexpression([NotNull] sqfParser.BinaryexpressionContext context)
        {
            var node = this.ExitGeneric<SqfBinaryExpression>(context);
        }

        public void ExitCode([NotNull] sqfParser.CodeContext context)
        {
            var node = this.ExitGeneric<SqfCode>(context);
        }

        public void ExitNularexpression([NotNull] sqfParser.NularexpressionContext context)
        {
            var node = this.ExitGeneric<SqfNullarExpression>(context);
        }

        public void ExitOperator([NotNull] sqfParser.OperatorContext context)
        {
            var node = this.ExitGeneric<SqfOperator>(context);
        }

        public void ExitPrimaryexpression([NotNull] sqfParser.PrimaryexpressionContext context)
        {
            var node = this.ExitGeneric<SqfPrimaryExpression>(context);
        }

        public void ExitStatement([NotNull] sqfParser.StatementContext context)
        {
            var node = this.ExitGeneric<SqfStatement>(context);
        }

        public void ExitUnaryexpression([NotNull] sqfParser.UnaryexpressionContext context)
        {
            var node = this.ExitGeneric<SqfUnaryExpression>(context);
        }

        public void ExitVariable([NotNull] sqfParser.VariableContext context)
        {
            var node = this.ExitGeneric<SqfVariable>(context);
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
