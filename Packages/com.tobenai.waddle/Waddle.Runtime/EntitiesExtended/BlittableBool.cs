using System;

namespace Waddle.Runtime.EntitiesExtended
{
    /// <summary>
    /// Boolean backed by a byte, to allow for blitting (mem-copy)
    /// </summary>
    [Serializable]
    public struct BlittableBool
    {
        #region Variables
        /// <summary>
        /// Value of Bool
        /// </summary>
        public byte Value;
        #endregion
 
        #region Constructors
        /// <summary>
        /// Constructs a BlittableBool from a Byte.
        /// <para>
        /// Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is neither 0 nor 1
        /// </para>
        /// </summary>
        /// <param name="value">Value to set. Must be 0 or 1</param>
        public BlittableBool(byte value)
        {
            if (value != 0 && value != 1)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid value for BlittableBool. Value must be 0 or 1");
            Value = value;
        }
        /// <summary>
        /// Constructs a BlittableBool from a Boolean
        /// </summary>
        /// <param name="value">Value to set</param>
        public BlittableBool(bool value)
        {
            Value = value ? (byte)1 : (byte)0;
        }
        #endregion
 
        #region Operators
        /// <summary>
        /// Implicit cast from BlittableBool to Boolean
        /// </summary>
        /// <param name="bb">BlittableBool to cast</param>
        public static implicit operator bool(BlittableBool bb)
        {
            return bb.Value == 1;
        }
        /// <summary>
        /// Implicit cast from Boolean to BlittableBool
        /// </summary>
        /// <param name="b">Boolean to cast</param>
        public static implicit operator BlittableBool(bool b)
        {
            return new BlittableBool(b);
        }
        #endregion
    }
}