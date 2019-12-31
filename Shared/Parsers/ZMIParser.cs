using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Shared.Model;

namespace Shared.Parsers
{
    public static class ZMIParser
    {
        public static bool TryParseZMI(TextReader textReader, ref ZMI root)
        {
            string line;

            ZMI current = null;
            
            while ((line = textReader.ReadLine()) != null)
            {
                if (root == null && !line.StartsWith('/'))
                    return false;

                if (line.StartsWith('/')) // zone-line
                {
                    if (!TryParseZoneLine(line, root, out current))
                        return false;

                    if (root == null)
                        root = current;
                    
                    continue;
                }

                if (!TryParseAttributeLine(line, out var attribute, out var created))
                    return false;
                
                current?.Attributes.AddOrChange(attribute, created);
            }

            return true;
        }

        public static bool TryParseZoneLine(string line, ZMI root, out ZMI result)
        {
            var lastSlash = line.LastIndexOf('/');
            ZMI father = null;
            result = null;

            if (lastSlash == -1)
                return false;

            var prefix = line.Substring(0, lastSlash);
            if (root != null && !root.TrySearch(prefix, out father))
                return false;
            
            result = new ZMI(father);
            father?.Sons.Add(result);
            return true;
        }

        public static bool TryParseAttributeLine(string line, out Attribute attribute, out Value value)
        {
            line = line.TrimStart(' ');

            attribute = null;
            value = null;

            var splitByColon = line.Split(':', 2);
            if (splitByColon.Length != 2)
                return false;
            var attributeName = splitByColon[0].Trim(' ');
            var splitByEqualSign = splitByColon[1].Split('=', 2);
            if (splitByEqualSign.Length != 2)
                return false;
            var type = splitByEqualSign[0].Trim(' ');
            var valueString = splitByEqualSign[1].Trim(' ');

            if (!ValueFactory.TryCreateValue(type, valueString, out value))
                return false;
            
            attribute = new Attribute(attributeName);
            
            return true;
        }
    }

    public static class ValueFactory
    {
        public static bool TryCreateValue(string typeString, string valueString, out Value value)
        {
            value = null;

            var isNull = valueString.Equals("NULL") || valueString.Equals("null") || valueString.Equals("");

            switch (typeString)
            {
                case "integer":
                    if (isNull)
                    {
                        value = new ValueInt(null);
                        return true;
                    }
                    if (!long.TryParse(valueString, out var intResult))
                        return false;
                    value = new ValueInt(intResult);
                    return true;
                case "string":
                    if (isNull)
                    {
                        value = new ValueString(null);
                        return true;
                    }
                    value = new ValueString(valueString.Trim('\"'));
                    return true;
                case "time":
                    if (isNull)
                    {
                        value = new ValueTime(null as RefStruct<long>);
                        return true;
                    }
                    value = new ValueTime(valueString);
                    return true;
                case "double":
                    if (isNull)
                    {
                        value = new ValueDouble(null);
                        return true;
                    }
                    if (!double.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleResult))
                        return false;
                    value = new ValueDouble(doubleResult);
                    return true;
                case "boolean":
                    if (isNull)
                    {
                        value = new ValueBoolean(null);
                        return true;
                    }
                    if (!bool.TryParse(valueString, out var booleanResult))
                        return false;
                    value = new ValueBoolean(booleanResult);
                    return true;
                case "duration":
                    if (isNull)
                    {
                        value = new ValueDuration(null as RefStruct<long>);
                        return true;
                    }
                    value = new ValueDuration(valueString);
                    return true;
                case "contact":
                    if (isNull)
                    {
                        value = new ValueContact(null, null);
                        return true;
                    }

                    var split = valueString.Split(' ');
                    if (split.Length != 2 && split.Length != 3)
                        return false;
                    if (!IPAddress.TryParse(split[1], out var address))
                        return false;
                    var port = 555;
                    if (split.Length == 3 && !int.TryParse(split[2], out port))
                        return false;
                    if (!split[0].StartsWith("/"))
                        split[0] = '/' + split[0];
                    value = new ValueContact(new PathName(split[0]), address, port);
                    return true;
            }

            var splitByOf = typeString.Split(" of ", 2);
            if (splitByOf.Length != 2)
                return false;
            var splitBySpace = splitByOf[1].Split(' ', 2);
            if (splitBySpace.Length != 2)
                return false;

            if (!int.TryParse(splitBySpace[0], out var count))
                return false;

            // based on StackOverflow answer: https://stackoverflow.com/a/3147901
            var collectionStrings = Regex.Split(valueString.Trim('{', '}', '[', ']'), ",(?=(?:[^']*'[^']*')*[^']*$)")
                .Select(s => s.Trim(' ')).ToList();
            if (collectionStrings.Count != count)
            {
                if (count != 0 && (collectionStrings.Count != 1 || !string.IsNullOrEmpty(collectionStrings[0])))
                    return false;
                collectionStrings = new List<string>(count);
            }

            var elements = new List<Value>(count);
            var elementTypeString = splitBySpace[1];
            foreach (var element in collectionStrings)
            {
                if (!TryCreateValue(elementTypeString, element, out var elementVal))
                    return false;
                elements.Add(elementVal);
            }

            if (!TryGetPrimitiveAttributeType(elementTypeString, out var elementType))
                return false;

            switch (splitByOf[0])
            {
                case "set":
                    value = new ValueSet(elements.ToHashSet(), elementType);
                    return true;
                case "list":
                    value = new ValueList(elements, elementType);
                    return true;
            }

            return false;
        }

        private static bool TryGetPrimitiveAttributeType(string typeString, out AttributeType attributeType)
        {
            switch (typeString)
            {
                case "integer":
                    attributeType = AttributeTypePrimitive.Integer;
                    return true;
                case "string":
                    attributeType = AttributeTypePrimitive.String;
                    return true;
                case "time":
                    attributeType = AttributeTypePrimitive.Time;
                    return true;
                case "double":
                    attributeType = AttributeTypePrimitive.Double;
                    return true;
                case "boolean":
                    attributeType = AttributeTypePrimitive.Boolean;
                    return true;
                case "duration":
                    attributeType = AttributeTypePrimitive.Duration;
                    return true;
                case "contact":
                    attributeType = AttributeTypePrimitive.Contact;
                    return true;
                case "null":
                    attributeType = AttributeTypePrimitive.Null;
                    return true;
            }

            attributeType = null;
            return false;
        }
    }
}