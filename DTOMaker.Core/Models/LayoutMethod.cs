namespace DTOMaker.Models;

/// <summary>
/// The entity's member memory layout method. This need only be defined when using
/// DTO source generators that require or support memory layout, such as MemBlocks.
/// </summary>
public enum LayoutMethod : int
{
    /// <summary>
    /// No member memory layout is defined.
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Member memory layout is explicitly defined by the model designer. This is
    /// only used by DTO source generators that support memory layout, such as 
    /// MemBlocks. When this method is used: a) the entity block length must be
    /// explicitly set; b) each member must have a [MemberLayout] attribute that
    /// defines its unique and immutable location (offset and length) within the
    /// entity memory block; c) each member data type must be a non-nullable value
    /// type or fixed length array of these.
    /// </summary>
    Explicit = 1,

    /// <summary>
    /// Member memory layout is automatic and is assigned sequentially in member
    /// sequence order. It is important that the member sequence and data type do
    /// not ever change, to ensure consistent backward compatibility. With this
    /// method, the entity memory block length can be limited, or left undefined
    /// to allow automatic growth. Note that as each member is assigned a memory
    /// location, an additional hidden control byte is also assigned, resulting
    /// in total entity memory block size being larger than the sum of the sizes
    /// of the declared member data types.
    /// </summary>
    SequentialV1 = 2,
}
