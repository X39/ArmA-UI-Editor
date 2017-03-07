using System.Xml.Serialization;

namespace RealVirtuality.SQF
{
    public partial struct SqfDefinition
    {
        public enum EKind
        {
            [XmlEnum]
            Unary = 'u',
            [XmlEnum]
            Binary = 'b',
            [XmlEnum]
            Nullar = 'n',
            [XmlEnum]
            Type = 't'
        }
    }
}
