﻿using System;

namespace MG.Utils.Exceptions
{
    public class DbUpdateConcurrencyException : InvalidOperationException
    {
        public DbUpdateConcurrencyException(string error, Exception innerException)
            : base(error, innerException)
        {
        }
    }
}