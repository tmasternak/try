﻿using FluentAssertions;
using MLS.Protocol.Completion;
using Newtonsoft.Json;
using Xunit;

namespace MLS.Protocol.Tests
{
    public class CompletionItemTests
    {
        [Theory]
        [InlineData("{\r\n  \"displayText\": \"BackgroundColor\",\r\n   \"kind\": \"Property\",\r\n \"filterText\": \"BackgroundColor\",\r\n    \"sortText\": \"BackgroundColor\",\r\n  \"insertText\": \"BackgroundColor\",\r\n    \"documentation\": {\r\n        \"value\": \"Gets or sets the background color of the console.\",\r\n       \"isTrusted\": false\r\n    }\r\n}")]
        [InlineData("{\r\n  \"displayText\": \"BackgroundColor\",\r\n   \"kind\": \"Property\",\r\n \"filterText\": \"BackgroundColor\",\r\n    \"sortText\": \"BackgroundColor\",\r\n  \"insertText\": \"BackgroundColor\",\r\n    \"documentation\": \"Gets or sets the background color of the console.\"\r\n}")]
        public void CanDeserializeFromJson(string source)
        {
          
            var ci = JsonConvert.DeserializeObject<CompletionItem>(source);
            ci.Documentation.Should().NotBeNull();
            ci.Documentation.IsTrusted.Should().BeFalse();
            ci.Documentation.Value.Should().Be("Gets or sets the background color of the console.");
        }
    }
}