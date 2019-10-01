using System;
using System.Data.Common;
using JetBrains.Annotations;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace NHibernate.ActsAsVersioned.Models
{
    /// <summary>
    /// Custom type mapping. Stores a boolean as either "True" or "False"
    /// in an ANSI string varchar(32). Just for testing.
    /// </summary>
    [Serializable]
    public class CustomType: IUserType
    {
        public SqlType[] SqlTypes => new[] { SqlTypeFactory.GetAnsiString(32)};
        public System.Type ReturnedType => typeof(bool);
        public bool IsMutable => false;

        bool IUserType.Equals(object x, object y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            if (x == null)
            {
                return 0;
            }

            return x.GetHashCode();
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            object obj = NHibernateUtil.String.NullSafeGet(rs, names[0], session); 
            if (obj == null)
            {
                return null;
            }

            if (obj is string s)
            {
                return s.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public void NullSafeSet([NotNull] DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            if (value == null)
            {
                cmd.Parameters[index].Value = DBNull.Value;
            }
            else
            {
                if (value is bool b)
                {
                    cmd.Parameters[index].Value = b.ToString();
                }

                cmd.Parameters[index].Value = "False";
            }
        }

        public object DeepCopy(object value)
        {
            // immutable object, can just return value
            return value;
        }

        public object Replace(object original, object target, object owner)
        {
            // immutable object, can just return original
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            // immutable object, can just return cached
            return cached;
        }

        public object Disassemble(object value)
        {
            // immutable object, can just return value
            return value;
        }
    }
}

