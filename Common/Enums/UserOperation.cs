using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Enums;

/// <summary>
/// Enumerates the types of operations that can be performed on a user.
/// </summary>
public enum UserOperation {
    /// <summary>
    /// Represents the operation of viewing user details.
    /// </summary>
    View,
    /// <summary>
    /// Represents the operation of editing user details.
    /// </summary>
    Edit,
    /// <summary>
    /// Represents the operation of deleting a user.
    /// </summary>
    Delete
}
