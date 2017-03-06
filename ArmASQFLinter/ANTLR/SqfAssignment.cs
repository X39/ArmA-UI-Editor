using RealVirtuality.SQF.ANTLR.Parser;

namespace RealVirtuality.SQF
{
    public class SqfAssignment : SqfNode
    {
        public SqfAssignment(SqfNode parent) : base(parent)
        {
        }

        public sqfParser.AssignmentContext Context { get; internal set; }
        public bool HasPrivateKeyword { get; internal set; }
        public string VariableName { get; internal set; }
    }
}