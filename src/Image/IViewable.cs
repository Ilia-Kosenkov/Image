﻿using System;

namespace ImageCore
{
    public interface IViewable
    {
        ReadOnlySpan<byte> GetByteView();
        bool BitwiseEquals(IImage other);
    }
}