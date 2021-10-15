﻿using Filuet.Hardware.CashAcceptors.Abstractions.Enums;
using Filuet.Hardware.CashAcceptors.Abstractions.Models;
using Filuet.Hardware.CashAcceptors.Common.Helpers;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Filuet.Hardware.CashAcceptors.Abstractions.Converters
{
    public class BillJsonConverter : JsonConverter<Denomination>
    {
        public override Denomination Read(
                    ref Utf8JsonReader reader,
                    Type typeToConvert,
                    JsonSerializerOptions options)
        {
            string value = reader.GetString();
            string[] x = value.Trim().Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (x.Length != 2)
                throw new ArgumentException($"Invalid denomination detected {value}");

            if (!uint.TryParse(x[0], out uint amount))
                throw new ArgumentException($"Invalid denomination amount {value}");

            Currency currency = EnumHelpers.GetValueFromCode<Currency>(x[1]);

            return new Denomination(amount, currency);
        }

        public override void Write(
            Utf8JsonWriter writer,
            Denomination note,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(note.ToString());
    }
}