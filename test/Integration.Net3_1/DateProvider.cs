// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Integration.Net3_1
{
    public class DateProvider : IDateProvider
    {
        public DateTimeOffset GetDate() => DateTimeOffset.UtcNow;
    }
}
