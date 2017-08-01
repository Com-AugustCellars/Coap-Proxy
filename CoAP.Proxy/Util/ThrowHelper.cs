﻿/*
 * Copyright (c) 2011-2014, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;
using Com.AugustCellars.CoAP.Proxy;

namespace Com.AugustCellars.CoAP.Util
{
    static class ThrowHelper
    {
        public static Exception TranslationException(String msg)
        {
            return new TranslationException(msg);
        }

        public static Exception ArgumentNull(String paramName)
        {
            return new ArgumentNullException(paramName);
        }

        public static Exception Argument(String paramName, String message)
        {
            return new ArgumentException(message, paramName);
        }
    }
}
