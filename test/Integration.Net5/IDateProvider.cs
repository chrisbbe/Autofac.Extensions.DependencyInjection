// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Integration.Net5
{
    public interface IDateProvider
    {
        DateTimeOffset GetDate();
    }
}
