// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace XRTK.Services.CameraSystem.CameraControl
{
    /// <summary>
    /// Interface to be implemented by camera controller components that should
    /// be selectable in the camera controller dropdown of the <see cref="XRTK.Definitions.MixedRealityCameraProfile"/>.
    /// </summary>
    public interface ICameraController
    {
        /// <summary>
        /// Is the camera controller enabled?
        /// </summary>
        bool IsEnabled { get; }
    }
}