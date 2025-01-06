using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;

namespace Localization
{
    partial struct FontMetadata
    {
        public string family;
        public float fontSize;
        public TextOverflowModes overflowMode;
        public float wordSpacing;
        public float lineSpacing;
        public bool enableWordWrapping;
        public bool extraPadding;
    }

    internal static class TmpTextExtensions
    {
        internal static FontMetadata SerializeMetadata(this TMP_Text t) => new()
        {
            family = t.font?.name,
            fontSize = t.fontSize,
            overflowMode = t.overflowMode,
            wordSpacing = t.wordSpacing,
            lineSpacing = t.lineSpacing,
            enableWordWrapping = t.enableWordWrapping,
            extraPadding = t.extraPadding,
        };

        // see font logic in localization loader
        internal static void DeserializeMetadataExceptFont(this TMP_Text t, FontMetadata metadata)
        {
            t.fontSize = metadata.fontSize;
            t.overflowMode = metadata.overflowMode;
            t.wordSpacing = metadata.wordSpacing;
            t.lineSpacing = metadata.lineSpacing;
            t.enableWordWrapping = metadata.enableWordWrapping;
            t.extraPadding = metadata.extraPadding;
            
            t.SetAllDirty();
        }

        internal static FontMetadata ParseMetadata(this TMP_Text txt, string metadata)
        {
            if (string.IsNullOrWhiteSpace(metadata))
            {
                // fallback to current state
                return txt.SerializeMetadata();
            }

            object data = txt.SerializeMetadata();
            
            foreach(
                var (f, v) in metadata
                    .Split(';')
                    .Where(s => s.Contains(':'))
                    .Select(s => s.Split(":"))
                    .Where(kv => kv.Length == 2 && FontMetadata.Fields.ContainsKey(kv[0]))
                    .Select(kv => (FontMetadata.Fields[kv[0]], kv[1])))
            {
                if (f.FieldType == typeof(string))
                {
                    f.SetValue(data, v);
                }
                else if (f.FieldType == typeof(float))
                {
                    if (float.TryParse(v, out var value))
                    {
                        f.SetValue(data, value);
                    }
                }
                else if (f.FieldType.IsEnum)
                {
                    if (Enum.TryParse(f.FieldType, v, out var value))
                    {
                        f.SetValue(data, value);
                    }
                }
                else if (f.FieldType == typeof(bool))
                {
                    if (int.TryParse(v, out var value))
                    {
                        f.SetValue(data, value == 1);
                    }
                }
            }

            return (FontMetadata) data;
        }
    }

    internal partial struct FontMetadata
    {
        
        internal static readonly SortedDictionary<string, FieldInfo> Fields =
            new(typeof(FontMetadata).GetFields().ToDictionary(f => f.Name, f => f));

        public override string ToString()
        {
            StringBuilder sb = new();

            foreach (var (k, f) in Fields)
            {
                sb.Append(k);
                sb.Append(":");
                
                if (f.FieldType == typeof(bool))
                {
                    var val = f.GetValue(this);
                    if ((bool)val)
                    {
                        sb.Append("1");
                    }
                    else
                    {
                        sb.Append("0");
                    }
                }
                else
                {
                    sb.Append(f.GetValue(this));
                }
                
                sb.Append(";");
            }

            return sb.ToString();
        }

        public static implicit operator string(FontMetadata self) => self.ToString();
    }
}