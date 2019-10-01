using NHibernate.SqlTypes;
using NHibernate.Type;

namespace NHibernate.ActsAsVersioned.Models
{
    public class CustomSimpleType : CharBooleanType
    {
        public CustomSimpleType() : base(new AnsiStringFixedLengthSqlType(3))
        {
        }

        protected override string TrueString => "Yes";
        protected override string FalseString => "No";
        public override string Name => "yes_no_custom";
    }
}
